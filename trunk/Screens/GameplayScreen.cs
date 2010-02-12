#region File Description
//-----------------------------------------------------------------------------
// GameplayScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Runtime;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
#endregion

namespace GameStateManagement
{
    /// <summary>
    /// This screen implements the actual game logic. It is just a
    /// placeholder to get the idea across: you'll probably want to
    /// put some more interesting gameplay in here!
    /// </summary>
    public class GameplayScreen : GameScreen
    {
        #region Fields

        GraphicsDeviceManager graphics;
        /*SpriteBatch spriteBatch;

        public SpriteBatch SpriteBatch
        {
            get { return spriteBatch; }
        }*/

        //time interval for crystal generation
        float time = 0;

        //Sound stuff!
        AudioEngine audioEngine;
        WaveBank waveBank;
        protected SoundBank soundBank;
        
        const int MAX_WINX = 800;
        const int MAX_WINY = 750;

        Sprite myBackground;
        Sprite myBackground2;
        Jeep Player1;
        Reticle turretReticle;

        LinkedList<Obstacle> obstacleList;
        LinkedList<Obstacle> recycledObstacles;

        LinkedList<Enemy> enemyList;
        LinkedList<Enemy> recycledEnemies;

        Obstacle smallObstacleLoader; // Used to load the first instance of a small obstacle
        //Obstacle smallDestroyedObstacleLoader;
        Enemy enemyLoader;

        HealthBar HBar;

        //enemy object and position info
        private EnemyStats RAPTOR_STATS;

        ExplosionPS explosion;
        ExplosionSmokePS explosion_smoke;
        MuzzleFlashPS muzzleflash;
        DirtCloudPS dirt;
        BloodPS blood;
        RockPS rock_smoke;
        RockPiecePS rock_piece;
        LogPS log_shard;

        // Used for collision detection
        private Rectangle boundingBox1;
        private Rectangle boundingBox2;

        // Defined to prevent overlapping when obstacles are randomly generated
        const int UNIT_OBJECT_WIDTH = 50;
        const int UNIT_OBJECT_HEIGHT = 50;

        private const double OBSTACLE_PLACEMENT_ODDS = 0.01;
        private const double OBSTACLE_ROCK_ODDS = 0.80;
        //private const double OBSTACLE_POND_ODDS = 0.25;
        private const double OBSTACLE_LOG_ODDS = 0.20;

        private const double ENEMY_PLACEMENT_ODDS = 0.005;
        private const double ENEMY_DINO1_ODDS = 1.0;
        private const double ENEMY_DINO2_ODDS = 0.0;

        private const int ENEMY_SPAWN_Y = 900;

        private const int BULLET_COLLISION_RADIUS = 5;

        #region Random Numbers and Helper functions
        private static Random random = new Random();    //Particle random number generator
        public static Random Random
        {
            get { return random; }
        }

        //  a handy little function that gives a random float between two
        // values. This will be used in several places in the sample, in particilar in
        // ParticleSystem.InitializeParticle.
        public static float RandomBetween(float min, float max)
        {
            return min + (float)random.NextDouble() * (max - min);
        }
        #endregion

        Random randNumGenerator; // Used for various randomly generated events, like obstacle placement

        float previousY;

        private Vector2 SCROLL_SPEED = new Vector2(0, 350);
        private Vector2 SCROLL_DIR = new Vector2(0, 1);

        ContentManager content;
        SpriteFont gameFont;
        Game game;

        Vector2 playerPosition = new Vector2(100, 100);
        Vector2 enemyPosition = new Vector2(100, 100);

        //Random random = new Random();

        #endregion

        #region Initialization


        /// <summary>
        /// Constructor.
        /// </summary>
        public GameplayScreen(ScreenManager sm)
        {
            this.ScreenManager = sm;
            //ScreenManager.Game.Content.RootDirectory = "Content";

            // create the particle systems and add them to the components list.
            // 
            explosion = new ExplosionPS(ScreenManager.Game, 2, ScreenManager.SpriteBatch);
            ScreenManager.Game.Components.Add(explosion);

            explosion_smoke = new ExplosionSmokePS(ScreenManager.Game, 1, ScreenManager.SpriteBatch);
            ScreenManager.Game.Components.Add(explosion_smoke);

            muzzleflash = new MuzzleFlashPS(ScreenManager.Game, 1, ScreenManager.SpriteBatch);
            ScreenManager.Game.Components.Add(muzzleflash);

            dirt = new DirtCloudPS(ScreenManager.Game, 1, ScreenManager.SpriteBatch);
            ScreenManager.Game.Components.Add(dirt);

            blood = new BloodPS(ScreenManager.Game, 1, ScreenManager.SpriteBatch);
            ScreenManager.Game.Components.Add(blood);

            rock_smoke = new RockPS(ScreenManager.Game, 1, ScreenManager.SpriteBatch);
            ScreenManager.Game.Components.Add(rock_smoke);

            rock_piece = new RockPiecePS(ScreenManager.Game, 1, ScreenManager.SpriteBatch);
            ScreenManager.Game.Components.Add(rock_piece);

            log_shard = new LogPS(ScreenManager.Game, 1, ScreenManager.SpriteBatch);
            ScreenManager.Game.Components.Add(log_shard);

            // Initialize struct for setting enemy raptor stats
            this.RAPTOR_STATS.maxSpeed = 3.0f;
            this.RAPTOR_STATS.turnSpeed = 0.05f;
            this.RAPTOR_STATS.chaseDistance = 2000.0f;
            this.RAPTOR_STATS.caughtDistance = 60.0f;
            this.RAPTOR_STATS.hysteresis = 15.0f;

            // initialize sound manager objects
            audioEngine = new AudioEngine("Content/gameAudio.xgs");
            waveBank = new WaveBank(audioEngine, "Content/Wave Bank.xwb");
            soundBank = new SoundBank(audioEngine, "Content/Sound Bank.xsb");

            TransitionOnTime = TimeSpan.FromSeconds(1.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);

            this.Initialize();
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected void Initialize()
        {
            myBackground = new Sprite();
            myBackground2 = new Sprite();

            Player1 = new Jeep();
            //give the Jeep a reference to the muzzleflash component
            Player1.muzz = muzzleflash;
            Player1.dirt_cloud = dirt;
            //Player1.Scale = 0.5f;

            HBar = new HealthBar(this.ScreenManager.Game, Player1);

            turretReticle = new Reticle();

            this.previousY = 0;

            obstacleList = new LinkedList<Obstacle>();
            recycledObstacles = new LinkedList<Obstacle>();

            enemyList = new LinkedList<Enemy>();
            recycledEnemies = new LinkedList<Enemy>();

            smallObstacleLoader = new Obstacle("obstacle_small");
            //smallDestroyedObstacleLoader = new Obstacle("obstacle_small_destroyed");

            enemyLoader = new Enemy("raptor", this.RAPTOR_STATS);

            randNumGenerator = new Random();
        }

        /// <summary>
        /// Load graphics content for the game.
        /// </summary>
        public override void LoadContent()
        {
            if (content == null)
                content = new ContentManager(ScreenManager.Game.Services, "Content");

            gameFont = content.Load<SpriteFont>("gamefont");

            // Create a new SpriteBatch, which can be used to draw textures.
            //spriteBatch = new SpriteBatch(ScreenManager.Game.GraphicsDevice);

            //main game screen content
            myBackground.LoadContent(content, "Long_background");
            myBackground.Position = new Vector2(0, 0);

            myBackground2.LoadContent(content, "Long_background");
            myBackground2.Position = new Vector2(0, myBackground2.Position.Y - myBackground2.Size.Height);

            Player1.LoadContent(content);

            HBar.LoadContent(content);

            turretReticle.LoadContent(content);

            // Start the sound!
            soundBank.PlayCue("Dino Escape Main Loop");

            smallObstacleLoader.LoadContent(content);
            //smallDestroyedObstacleLoader.LoadContent(content);
            enemyLoader.LoadContent(content);

            // A real game would probably have more content than this sample, so
            // it would take longer to load. We simulate that by delaying for a
            // while, giving you a chance to admire the beautiful loading screen.
            Thread.Sleep(1000);

            // once the load has finished, we use ResetElapsedTime to tell the game's
            // timing mechanism that we have just finished a very long frame, and that
            // it should not try to catch up.
            ScreenManager.Game.ResetElapsedTime();
        }


        /// <summary>
        /// Unload graphics content used by the game.
        /// </summary>
        public override void UnloadContent()
        {
            content.Unload();
        }


        #endregion

        #region Update and Draw

        protected void generateCrystals(float yVal, GameTime theGameTime)
        {
            float dt = (float)theGameTime.ElapsedGameTime.TotalSeconds;
            time += dt;
            if (time > 20)
            {
                LinkedListNode<Obstacle> obNode;
                if (recycledObstacles.Count == 0)
                {
                    obNode = new LinkedListNode<Obstacle>(new Obstacle("crystal"));
                    time = 0.0f;
                }
                else
                {
                    obNode = recycledObstacles.First;
                    recycledObstacles.RemoveFirst();
                    obNode.Value.AssetName = "crystal";
                    obNode.Value.type = (ObType)3;
                    time = 0.0f;
                }
                obNode.Value.Position.X = RandomBetween(0.0f, 800.0f);
                obNode.Value.Position.Y = yVal;
                obNode.Value.LoadContent(content);
                obstacleList.AddLast(obNode);
            }
        }

        protected void generateNewRow(float yVal, GameTime theGameTime)
        {
            //obstacleList.AddLast(new LinkedListNode<List<Obstacle>>(new List<Obstacle>()));
            double randomNum;

            for (int i = 0; i < MAX_WINX / UNIT_OBJECT_WIDTH; i++)
            {
                randomNum = randNumGenerator.NextDouble();

                if (randomNum < OBSTACLE_PLACEMENT_ODDS)
                {
                    LinkedListNode<Obstacle> obNode;
                    if (recycledObstacles.Count == 0)
                    {
                        obNode = new LinkedListNode<Obstacle>(new Obstacle("obstacle_small"));
                    }
                    else
                    {
                        obNode = recycledObstacles.First;
                        recycledObstacles.RemoveFirst();
                        obNode.Value.AssetName = "obstacle_small";
                        obNode.Value.type = (ObType)1;
                    }

                    if (randomNum < OBSTACLE_LOG_ODDS * OBSTACLE_PLACEMENT_ODDS)
                    {
                        obNode.Value.AssetName = "obstacle_log";
                        obNode.Value.type = (ObType)2;
                        i++;
                        i += 2; // A log is triple-wide, so it takes an additional 2 places
                    }

                    obNode.Value.Position.X = i * UNIT_OBJECT_WIDTH;
                    obNode.Value.Position.Y = yVal;
                    obNode.Value.LoadContent(content);
                    obstacleList.AddLast(obNode);
                }
            }
        }

        protected void generateNewEnemies(float yVal)
        {
            double randomNum;

            for (int i = 0; i < MAX_WINX / UNIT_OBJECT_WIDTH; i++)
            {
                randomNum = randNumGenerator.NextDouble();
                if (randomNum < ENEMY_PLACEMENT_ODDS)
                {
                    LinkedListNode<Enemy> enemyNode;
                    if (recycledEnemies.Count == 0)
                    {
                        enemyNode = new LinkedListNode<Enemy>(new Enemy("raptor", this.RAPTOR_STATS));
                    }
                    else
                    {
                        enemyNode = recycledEnemies.First;
                        recycledEnemies.RemoveFirst();
                        enemyNode.Value.AssetName = "raptor";
                        enemyNode.Value.state = EnemyAiState.Chasing;
                        enemyNode.Value.stats = this.RAPTOR_STATS;
                    }
                    /*
                    if (randomNum < ENEMY_DINO2_ODDS * ENEMY_PLACEMENT_ODDS)
                    {
                        enemyNode.Value.AssetName = "obstacle_log";
                        i++;
                    }
                    */
                    enemyNode.Value.Position.X = i * UNIT_OBJECT_WIDTH;
                    enemyNode.Value.Position.Y = yVal;
                    enemyNode.Value.LoadContent(content);
                    enemyList.AddLast(enemyNode);
                }
            }
        }

        // Recycle any obstacles that are no longer in view
        protected void garbageCollectObstacles()
        {
            LinkedListNode<Obstacle> obstacleListNode = obstacleList.First;
            LinkedListNode<Obstacle> nextNode = null;
            while (obstacleListNode != null)
            {
                if (obstacleListNode.Value.Position.Y > MAX_WINY)
                {
                    nextNode = obstacleListNode.Next;
                    obstacleList.Remove(obstacleListNode);
                    recycledObstacles.AddLast(obstacleListNode);
                    obstacleListNode = nextNode;
                }
                else
                {
                    obstacleListNode = obstacleListNode.Next;
                }
            }
        }

        // Tests collisions between the jeep and other in-game elements
        protected void processObstacleCollisions(GameTime theGameTime)
        {
            // Update obstacles based on scrolling background
            LinkedListNode<Obstacle> obstacleMatrixNode = obstacleList.First;
            LinkedListNode<Obstacle> nextNode = null;
            while (obstacleMatrixNode != null)
            {
                Obstacle currentOb = obstacleMatrixNode.Value;
                nextNode = obstacleMatrixNode.Next;
                //boundingBox1 = new Rectangle((int)Player1.Position.X, (int)Player1.Position.Y, Player1.Source.Width, Player1.Source.Height);
                boundingBox1.X = (int)Player1.Position.X;
                boundingBox1.Y = (int)Player1.Position.Y;
                boundingBox1.Width = Player1.Source.Width;
                boundingBox1.Height = Player1.Source.Height;
                //boundingBox2 = new Rectangle((int)currentOb.Position.X, (int)currentOb.Position.Y, currentOb.Source.Width, currentOb.Source.Height);
                boundingBox2.X = (int)currentOb.Position.X;
                boundingBox2.Y = (int)currentOb.Position.Y;
                boundingBox2.Width = currentOb.Source.Width;
                boundingBox2.Height = currentOb.Source.Height;

                if (boundingBox1.Intersects(boundingBox2)) // Jeep collided with an obstacle
                {
                    obstacleList.Remove(obstacleMatrixNode);
                    recycledObstacles.AddLast(obstacleMatrixNode);

                    Player1.Jeep_health -= 5;
                    Player1.Jeep_health = (int)MathHelper.Clamp(Player1.Jeep_health, 0.0f, 100.0f);

                    //make big explosion
                    Vector2 where;
                    where.X = currentOb.Position.X + currentOb.Source.Width / 2;
                    where.Y = currentOb.Position.Y + currentOb.Source.Height / 2;
                    if (currentOb.type == (ObType)1)
                    {
                        rock_piece.AddParticles(where);
                        rock_smoke.AddParticles(where);
                    }
                    else if (currentOb.type == (ObType)2)
                    {
                        log_shard.AddParticles(where);
                    }
                    else
                    {
                        rock_piece.AddParticles(where);
                        rock_smoke.AddParticles(where);
                    }
                }
                obstacleMatrixNode = nextNode;
            }
        }

        // Tests collisions between the jeep and enemies
        protected void processEnemyCollisions()
        {
            LinkedListNode<Enemy> enemyListNode = enemyList.First;
            LinkedListNode<Enemy> nextNode = null;
            while (enemyListNode != null)
            {
                if (enemyListNode.Value.state == EnemyAiState.Caught)
                {
                    //soundBank.PlayCue("hit");
                    Player1.Jeep_health -= 10;
                    Player1.Jeep_health = (int)MathHelper.Clamp(Player1.Jeep_health, 0.0f, 100.0f);

                    //add blood splash
                    Vector2 where = Vector2.Zero;
                    where.X = enemyListNode.Value.Position.X + enemyListNode.Value.Size.Height;
                    where.Y = enemyListNode.Value.Position.Y - enemyListNode.Value.Size.Width;
                    blood.AddParticles(where);

                    enemyList.Remove(enemyListNode);
                    recycledEnemies.AddLast(enemyListNode);
                }

                enemyListNode = nextNode;
            }
        }

        private void processBulletHits()
        {
            LinkedListNode<Enemy> enemyListNode = enemyList.First;
            LinkedListNode<Enemy> nextNode = null;
            while (enemyListNode != null)
            {
                nextNode = enemyListNode.Next;
                for (int i = 0; i < Player1.bullets.Count; i++)
                {
                    if (Player1.bullets[i].visible)
                    {
                        if (enemyListNode.Value.collidesWith(Player1.bullets[i].Position.X, Player1.bullets[i].Position.Y, BULLET_COLLISION_RADIUS))
                        {
                            enemyList.Remove(enemyListNode);
                            recycledEnemies.AddLast(enemyListNode);

                            //add blood splash
                            Vector2 where = Vector2.Zero;
                            where.X = enemyListNode.Value.Position.X + enemyListNode.Value.Size.Height;
                            where.Y = enemyListNode.Value.Position.Y - enemyListNode.Value.Size.Width;
                            blood.AddParticles(where);

                            Player1.bullets[i].visible = false; // Bullet destroyed by hitting enemy

                            break;
                        }
                    }
                }
                enemyListNode = nextNode;
            }
        }

        /// <summary>
        /// Updates the state of the game. This method checks the GameScreen.IsActive
        /// property, so the game will stop updating when the pause menu is active,
        /// or if you tab away to a different application.
        /// </summary>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                       bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            if (IsActive)
            {
                //// Allows the game to exit
                //if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                //    this.Exit();
                //if (Keyboard.GetState(PlayerIndex.One).IsKeyDown(Keys.Escape))
                //    this.Exit();

                //Update Health Bar
                HBar.Update(gameTime);

                Vector2 distanceTravelled = SCROLL_SPEED * SCROLL_DIR * (float)gameTime.ElapsedGameTime.TotalSeconds;
                myBackground.Position += distanceTravelled;
                myBackground2.Position += distanceTravelled;

                // Perform garbage collection on obstacles
                garbageCollectObstacles();

                // Update obstacles based on scrolling background
                LinkedListNode<Obstacle> obstacleListNode = obstacleList.First;
                while (obstacleListNode != null)
                {
                    obstacleListNode.Value.Position += distanceTravelled;
                    obstacleListNode = obstacleListNode.Next;
                }

                //Create crystal if it's time
                generateCrystals(this.previousY - UNIT_OBJECT_HEIGHT, gameTime);

                // Create a new row of obstacles and enemies, if needed
                if ((this.previousY + distanceTravelled.Y) >= UNIT_OBJECT_HEIGHT)
                {
                    this.previousY = (this.previousY + distanceTravelled.Y) % UNIT_OBJECT_HEIGHT;
                    generateNewRow(this.previousY - UNIT_OBJECT_HEIGHT, gameTime);
                    generateNewEnemies(ENEMY_SPAWN_Y);
                }
                else
                {
                    this.previousY += distanceTravelled.Y;
                }

                // Update enemies
                LinkedListNode<Enemy> enemyListNode = enemyList.First;
                while (enemyListNode != null)
                {
                    enemyListNode.Value.Update(gameTime, Player1.Position + new Vector2(Player1.Size.Width / 2.0f, Player1.Size.Height / 2.0f));
                    enemyListNode = enemyListNode.Next;
                }

                //these if-statements shuffle the pictures as they go out of view

                if (myBackground2.Position.Y > MAX_WINY)
                    myBackground2.Position.Y = (-myBackground2.Size.Height);
                if (myBackground.Position.Y > MAX_WINY)
                    myBackground.Position.Y = (-myBackground.Size.Height);

                Player1.Update(gameTime);

                // Perform collision detection
                processObstacleCollisions(gameTime);
                processEnemyCollisions();
                processBulletHits();

                turretReticle.Update(gameTime);
            }
        }


        /// <summary>
        /// Lets the game respond to player input. Unlike the Update method,
        /// this will only be called when the gameplay screen is active.
        /// </summary>
        public override void HandleInput(InputState input)
        {
            if (input == null)
                throw new ArgumentNullException("input");

            // Look up inputs for the active player profile.
            int playerIndex = (int)ControllingPlayer.Value;

            KeyboardState keyboardState = input.CurrentKeyboardStates[playerIndex];
            GamePadState gamePadState = input.CurrentGamePadStates[playerIndex];

            // The game pauses either if the user presses the pause button, or if
            // they unplug the active gamepad. This requires us to keep track of
            // whether a gamepad was ever plugged in, because we don't want to pause
            // on PC if they are playing with a keyboard and have no gamepad at all!
            bool gamePadDisconnected = !gamePadState.IsConnected &&
                                       input.GamePadWasConnected[playerIndex];

            if (input.IsPauseGame(ControllingPlayer) || gamePadDisconnected)
            {
                ScreenManager.AddScreen(new PauseMenuScreen(), ControllingPlayer);
            }
            else
            {
                // Otherwise move the player position.
                Vector2 movement = Vector2.Zero;

                if (keyboardState.IsKeyDown(Keys.Left))
                    movement.X--;

                if (keyboardState.IsKeyDown(Keys.Right))
                    movement.X++;

                if (keyboardState.IsKeyDown(Keys.Up))
                    movement.Y--;

                if (keyboardState.IsKeyDown(Keys.Down))
                    movement.Y++;

                Vector2 thumbstick = gamePadState.ThumbSticks.Left;

                movement.X += thumbstick.X;
                movement.Y -= thumbstick.Y;

                if (movement.Length() > 1)
                    movement.Normalize();

                playerPosition += movement * 2;
            }
        }


        /// <summary>
        /// Draws the gameplay screen.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            GCLatencyMode oldGCMode = GCSettings.LatencyMode;
            GCSettings.LatencyMode = GCLatencyMode.LowLatency;

            // This game has a blue background. Why? Because!
            ScreenManager.GraphicsDevice.Clear(ClearOptions.Target,
                                               Color.CornflowerBlue, 0, 0);

            // Our player and enemy are both actually just text strings.
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;

            spriteBatch.Begin();

            // Draw scrolling background
            myBackground.Draw(spriteBatch);
            myBackground2.Draw(spriteBatch);

            // Draw obstacles
            LinkedListNode<Obstacle> obstacleListNode = obstacleList.First;
            while (obstacleListNode != null)
            {
                obstacleListNode.Value.Draw(spriteBatch);
                obstacleListNode = obstacleListNode.Next;
            }

            // Draw player
            Player1.Draw(spriteBatch);

            // Draw enemies
            LinkedListNode<Enemy> enemyListNode = enemyList.First;
            while (enemyListNode != null)
            {
                enemyListNode.Value.Draw(spriteBatch);
                enemyListNode = enemyListNode.Next;
            }

            // Draw reticle
            turretReticle.Draw(spriteBatch);

            HBar.Draw(spriteBatch);

            spriteBatch.End();

            // If the game is transitioning on or off, fade it out to black.
            if (TransitionPosition > 0)
                ScreenManager.FadeBackBufferToBlack(255 - TransitionAlpha);

            base.Draw(gameTime);

            GCSettings.LatencyMode = oldGCMode;
        }


        #endregion
    }
}

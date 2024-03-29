#region File Description
//-----------------------------------------------------------------------------
// GameplayScreen.cs
//
// Adapted From :
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

namespace DinoEscape
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

        //Scoring stuff...
        int Score = 0;
        string score_string = "0";
        string crystal_col;
        string crystal_need;
        Vector2 score_pos = new Vector2(10, 20);
        Vector2 crystal_pos = new Vector2(10, 70);

        //Crystal Collection
        int crystals_collected = 0;
        int crystals_needed = 3;
        bool winScreenLoaded = false;

        //time interval for crystal generation
        float time = 0;

        /*
        //Sound stuff!
        AudioEngine audioEngine;
        WaveBank waveBank;
        protected SoundBank soundBank;
        */

        public static SoundControl soundController = new SoundControl();

        const int MAX_WINX = 800;
        const int MAX_WINY = 750;

        //Scrolling Background Sprites
        Sprite myBackground;
        Sprite myBackground2;
        Sprite winBackground;

        //Player Sprite
        Jeep Player1;

        //Player Reticule
        Reticle turretReticle;

        //List of obstacles
        LinkedList<Obstacle> obstacleList;
        LinkedList<Obstacle> recycledObstacles;

        //List of Enemies
        LinkedList<Enemy> enemyList;
        LinkedList<Enemy> recycledEnemies;

        Obstacle smallObstacleLoader; // Used to load the first instance of a small obstacle
        Enemy enemyLoader;

        HealthBar HBar;

        //Scoring Text Sprites
        SpriteFont score_font;
        SpriteFont crystal_font;

        //enemy object and position info
        private EnemyStats RAPTOR_STATS;

        //Particle Effects
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

        private const double OBSTACLE_PLACEMENT_ODDS = 0.012;    //0.01
        private const double OBSTACLE_ROCK_ODDS = 0.80;
        //private const double OBSTACLE_POND_ODDS = 0.25;
        private const double OBSTACLE_LOG_ODDS = 0.20;

        private const double ENEMY_PLACEMENT_ODDS = 0.008;   //0.005
        private const double ENEMY_DINO1_ODDS = 1.0;
        private const double ENEMY_DINO2_ODDS = 0.0;

        private const int ENEMY_SPAWN_Y = MAX_WINY + 300;

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
            explosion = new ExplosionPS(ScreenManager.Game, 1, ScreenManager.SpriteBatch);
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

            /*
            // initialize sound manager objects
            audioEngine = new AudioEngine("Content/gameAudio.xgs");
            waveBank = new WaveBank(audioEngine, "Content/Wave Bank.xwb");
            soundBank = new SoundBank(audioEngine, "Content/Sound Bank.xsb");
            */

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
            //init background sprites
            myBackground = new Sprite();
            myBackground2 = new Sprite();
            winBackground = new Sprite();

            //init text sprites
            score_font = this.ScreenManager.Font2;
            crystal_font = this.ScreenManager.Font2;

            //init player sprite
            Player1 = new Jeep();
            //give the Jeep a reference to the muzzleflash component
            Player1.muzz = muzzleflash;
            //give the Jeep a reference to the dirtcloud component
            Player1.dirt_cloud = dirt;

            //init health bar
            HBar = new HealthBar(this.ScreenManager.Game, Player1);

            //init reticule
            turretReticle = new Reticle();

            this.previousY = 0;

            //init obstacle lists
            obstacleList = new LinkedList<Obstacle>();
            recycledObstacles = new LinkedList<Obstacle>();

            //init enemy lists
            enemyList = new LinkedList<Enemy>();
            recycledEnemies = new LinkedList<Enemy>();

            smallObstacleLoader = new Obstacle("obstacle_small");
            //smallDestroyedObstacleLoader = new Obstacle("obstacle_small_destroyed");

            enemyLoader = new Enemy("raptor", this.RAPTOR_STATS);

            //init random number generator
            randNumGenerator = new Random();

            this.winScreenLoaded = false;
        }

        /// <summary>
        /// Load graphics content for the game.
        /// </summary>
        public override void LoadContent()
        {
            if (content == null)
                content = new ContentManager(ScreenManager.Game.Services, "Content");

            gameFont = content.Load<SpriteFont>("pericles");

            //main game screen content
            myBackground.LoadContent(content, "Long_background");
            myBackground.Position = new Vector2(0, 0);

            myBackground2.LoadContent(content, "Long_background");
            myBackground2.Position = new Vector2(0, myBackground2.Position.Y - myBackground2.Size.Height);

            winBackground.LoadContent(content, "end_screen");

            Player1.LoadContent(content);

            HBar.LoadContent(content);

            turretReticle.LoadContent(content);

            // Start the sound!
            GameplayScreen.soundController.PlayMusic("Dino Escape Main Loop");
            //GameScreen.soundBank.PlayCue("Dino Escape Main Loop");

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

        // This function generates a new crystal every 20 seconds at a random X
        // position.  It also checks to make sure that the crystal is not placed
        // on top of an already existing obstacle.
        protected void generateCrystals(float yVal, GameTime theGameTime)
        {
            float dt = (float)theGameTime.ElapsedGameTime.TotalSeconds;
            time += dt;
            if (time > 20)
            {
                LinkedListNode<Obstacle> obNode;
                if (recycledObstacles.Count == 0)
                {
                    //if no objects are recyclible, then make a new one
                    obNode = new LinkedListNode<Obstacle>(new Obstacle("crystal"));
                    //reset time
                    time = 0.0f;
                }
                else
                {
                    //use an old obstacle again
                    obNode = recycledObstacles.First;
                    recycledObstacles.RemoveFirst();
                    obNode.Value.AssetName = "crystal";
                    obNode.Value.type = ObType.Crystal;
                    //reset time
                    time = 0.0f;
                }
                obNode.Value.Position.X = RandomBetween(0.0f, 800.0f);
                obNode.Value.Position.Y = yVal;

                LinkedListNode<Obstacle> currNode = obstacleList.First;
                LinkedListNode<Obstacle> nextNode = null;
                while (currNode != null)
                {
                    nextNode = currNode.Next;
                    if (currNode.Value.Position.Y == obNode.Value.Position.Y)
                    {
                        obstacleList.Remove(currNode);
                        recycledObstacles.AddLast(currNode);
                    }
                    currNode = nextNode;
                }

                obNode.Value.LoadContent(content);
                obstacleList.AddLast(obNode);
            }
        }

        // This function generates a new row of obstacles by placing them randomly within
        // the bounds of the screen.  The frequency of their appearance is based completely 
        // on the probability constants set above.
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
                        obNode.Value.type = ObType.Rock;
                    }

                    if (randomNum < OBSTACLE_LOG_ODDS * OBSTACLE_PLACEMENT_ODDS)
                    {
                        obNode.Value.AssetName = "obstacle_log";
                        obNode.Value.type = ObType.Log;
                        i += 2; // A log is triple-wide, so it takes an additional 2 places
                    }

                    obNode.Value.Position.X = i * UNIT_OBJECT_WIDTH;
                    obNode.Value.Position.Y = yVal;
                    obNode.Value.LoadContent(content);
                    obstacleList.AddLast(obNode);
                }
            }
        }

        // This function works in very much the same way that generateNewRow works.
        // The only real difference is it generates enemies rather than obstacles.
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

                    //make Particle effect depending on the type of object hit...
                    //update score as well...
                    if (currentOb.type != ObType.Crystal)
                    {
                        Score -= 489;    //not so arbitrary number
                        if (Score < 0) Score = 0;
                    }
                    Vector2 where;
                    where.X = currentOb.Position.X + currentOb.Source.Width / 2;
                    where.Y = currentOb.Position.Y + currentOb.Source.Height / 2;
                    if (currentOb.type == ObType.Rock)
                    {
                        rock_piece.AddParticles(where);
                        rock_smoke.AddParticles(where);
                        Player1.Jeep_health -= 5;
                        Player1.Jeep_health = (int)MathHelper.Clamp(Player1.Jeep_health, 0.0f, 100.0f);
                    }
                    else if (currentOb.type == ObType.Log)
                    {
                        log_shard.AddParticles(where);
                        Player1.Jeep_health -= 5;
                        Player1.Jeep_health = (int)MathHelper.Clamp(Player1.Jeep_health, 0.0f, 100.0f);
                    }
                    else if (currentOb.type == ObType.Crystal)
                    {
                        Score += 9001;  
                        //crystal particles
                        crystals_collected++;
                        soundBank.PlayCue("collect");
                    }
                    else
                    {
                        rock_piece.AddParticles(where);
                        rock_smoke.AddParticles(where);
                    }


                    //check if dead
                    if (Player1.Jeep_health == 0)
                    {
                        //make big explosion
                        Vector2 whereat;
                        whereat.X = Player1.Position.X + Player1.Size.Width / 2;
                        whereat.Y = Player1.Position.Y + Player1.Size.Height / 2;
                        explosion.AddParticles(whereat);
                        //ScreenManager.RemoveScreen(this);
                        this.ScreenState = ScreenState.Hidden;
                        //destroy the soundbank to kill the music
                        soundBank.Dispose();
                        //make new soundbank to play new sound
                        //soundBank = new SoundBank(audioEngine, "Content/Sound Bank.xsb");
                        GameplayScreen.soundController.StopMusic("Dino Escape Main Loop");
                        //soundBank.PlayCue("gameover");
                        GameplayScreen.soundController.Play("gameover");
                        //call function which loads the GameOver screen and then takes you
                        //back to the main menu...
                        GameOver.Load(ScreenManager, null, new BackgroundScreen(), new MainMenuScreen());
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
                    Player1.Jeep_health -= 10;
                    Player1.Jeep_health = (int)MathHelper.Clamp(Player1.Jeep_health, 0.0f, 100.0f);
                    if (Player1.Jeep_health == 0)
                    {
                        Vector2 whereat;
                        whereat.X = Player1.Position.X + Player1.Size.Width / 2;
                        whereat.Y = Player1.Position.Y + Player1.Size.Height / 2;
                        explosion.AddParticles(whereat);
                        //ScreenManager.RemoveScreen(this);
                        this.ScreenState = ScreenState.Hidden;
                        GameplayScreen.soundController.StopMusic("Dino Escape Main Loop");
                        //soundBank = new SoundBank(audioEngine, "Content/Sound Bank.xsb");
                        GameplayScreen.soundController.Play("gameover");
                        GameOver.Load(ScreenManager, null, new BackgroundScreen(), new MainMenuScreen());
                    }

                    Score -= 489;
                    if (Score < 0) Score = 0;
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
                            Score += 1000;

                            //add blood splash
                            Vector2 where = enemyListNode.Value.getCenter();
                            blood.AddParticles(where);

                            enemyList.Remove(enemyListNode);
                            recycledEnemies.AddLast(enemyListNode);

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
                Vector2 distanceTravelled = SCROLL_SPEED * SCROLL_DIR * (float)gameTime.ElapsedGameTime.TotalSeconds;

                //Update Health Bar
                HBar.Update(gameTime);

                // Perform garbage collection on obstacles
                garbageCollectObstacles();

                // Update obstacles based on scrolling background
                LinkedListNode<Obstacle> obstacleListNode = obstacleList.First;
                while (obstacleListNode != null)
                {
                    obstacleListNode.Value.Position += distanceTravelled;
                    obstacleListNode = obstacleListNode.Next;
                }

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

                //Create crystal if it's time
                generateCrystals(this.previousY - UNIT_OBJECT_HEIGHT, gameTime);

                // Update enemies
                LinkedListNode<Enemy> enemyListNode = enemyList.First;
                while (enemyListNode != null)
                {
                    enemyListNode.Value.Update(gameTime, Player1.Position + new Vector2(Player1.Size.Width / 2.0f, Player1.Size.Height / 2.0f), content);
                    enemyListNode = enemyListNode.Next;
                }

                //these if-statements shuffle the pictures as they go out of view
                if (crystals_collected >= crystals_needed)
                {
                    //enough crystals have been collected to engage end-game
                    if (!this.winScreenLoaded)
                    {
                        //set the initial position of the win screen
                        winBackground.Position = new Vector2(0, Math.Min(myBackground.Position.Y, myBackground2.Position.Y) - winBackground.Source.Height);
                        this.winScreenLoaded = true;
                    }
                    else
                    {
                        if (winBackground.Position.Y < 0)
                        {
                            //continue scrolling the win screen until you reach the goal
                            winBackground.Position += distanceTravelled;
                        }
                        else
                        {
                            obstacleList.Clear(); //clear all obstacles once you reach the goal
                            Player1.Position -= distanceTravelled; //keep the jeep moving forward
                            GameplayScreen.soundController.StopMusic("Dino Escape Main Loop");
                            //soundBank = new SoundBank(audioEngine, "Content/Sound Bank.xsb");
                            GameplayScreen.soundController.PlayMusic("Dino Escape End Theme");
                            if (Player1.Position.Y < 240)
                            {
                                //load the completion screen
                                GameWon.Load(ScreenManager, false, null, new BackgroundScreen(), new MainMenuScreen());
                            }
                        }
                    }
                }

                myBackground.Position += distanceTravelled;
                myBackground2.Position += distanceTravelled;

                if (myBackground2.Position.Y > MAX_WINY)
                    myBackground2.Position.Y = (-myBackground2.Size.Height);
                if (myBackground.Position.Y > MAX_WINY)
                    myBackground.Position.Y = (-myBackground.Size.Height);

                Player1.Update(gameTime);

                // Perform collision detection
                processObstacleCollisions(gameTime);
                processEnemyCollisions();
                processBulletHits();

                //Update Score and Crystal count
                score_string = Score.ToString();
                crystal_col = crystals_collected.ToString();
                crystal_need = crystals_needed.ToString();

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
            if (this.winScreenLoaded)
            {
                winBackground.Draw(spriteBatch);
            }

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

            // Draw Healthbar
            HBar.Draw(spriteBatch);

            // Draw scores
            spriteBatch.DrawString(score_font, "Score: " + score_string, score_pos, Color.White);
            spriteBatch.DrawString(crystal_font, "Crystals: " + crystal_col + " / " + crystal_need, crystal_pos, Color.Yellow);

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

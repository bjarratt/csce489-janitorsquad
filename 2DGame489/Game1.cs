using System;
using System.Runtime;
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

namespace _2DGame489
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        public SpriteBatch SpriteBatch
        {
            get { return spriteBatch; }
        }

        const int MAX_WINX = 800;
        const int MAX_WINY = 600;

        Sprite myBackground;
        Sprite myBackground2;
        Jeep Player1;
        Reticle turretReticle;

        LinkedList<Obstacle> obstacleList;
        LinkedList<Obstacle> recycledObstacles;

        LinkedList<Enemy> enemyList;
        LinkedList<Enemy> recycledEnemies;

        Obstacle smallObstacleLoader; // Used to load the first instance of a small obstacle
        Obstacle smallDestroyedObstacleLoader;
        Enemy enemyLoader;

        HealthBar HBar;

        //enemy object and position info
        private EnemyStats RAPTOR_STATS;
        //Texture2D enemyTexture;
        //Vector2 enemyTextureCenter;
        //Vector2 enemyPosition;
        //EnemyAiState enemyState = EnemyAiState.Wander;
        //float enemyOrientation;
        //Vector2 enemyWanderDirection;
        /*
        //enums and state info for enemy
        /// <summary>
        /// EnemyAiState is used to keep track of what the enemy is currently doing.
        /// </summary>
        enum EnemyAiState
        {
            // chasing the jeep
            Chasing,
            // the enemy has gotten close enough to the jeep that it can stop chasing it
            Caught,
            // the enemy can't "see" the jeep, and is wandering around.
            Wander
        }*/
        /*
        // how fast can the enemy move?
        const float MaxEnemySpeed = 4.0f;

        // how fast can he turn?
        const float EnemyTurnSpeed = 0.10f;

        // this value controls the distance at which the enemy will start to chase the
        // jeep.
        const float EnemyChaseDistance = 350.0f;

        // EnemyCaughtDistance controls the distance at which the enemy will stop because
        // he has "caught" the jeep.
        const float EnemyCaughtDistance = 60.0f;

        // this constant is used to avoid hysteresis
        const float EnemyHysteresis = 15.0f;
         */

        ExplosionPS explosion;
        ExplosionSmokePS explosion_smoke;
        MuzzleFlashPS muzzleflash;
        DirtCloudPS dirt;
        GrassPS grass;

        // Used for collision detection
        private Rectangle boundingBox1;
        private Rectangle boundingBox2;

        // Defined to prevent overlapping when obstacles are randomly generated
        const int UNIT_OBJECT_WIDTH = 50;
        const int UNIT_OBJECT_HEIGHT = 50;

        private const double OBSTACLE_PLACEMENT_ODDS = 0.015;
        private const double OBSTACLE_ROCK_ODDS = 0.80;
        //private const double OBSTACLE_POND_ODDS = 0.25;
        private const double OBSTACLE_LOG_ODDS = 0.20;

        private const double ENEMY_PLACEMENT_ODDS = 0.005;
        private const double ENEMY_DINO1_ODDS = 1.0;
        private const double ENEMY_DINO2_ODDS = 0.0;

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
        
        //screen management elements
        private enum Screen
        {
            Title,
            Main,
            Inventory,
            Menu
        }
        Screen mCurrentScreen = Screen.Title;

        private enum MenuOptions
        {
            Resume,
            Inventory,
            ExitGame
        }
        MenuOptions mCurrentMenuOption = MenuOptions.Resume;

        Texture2D mTitleScreen;
        Texture2D mMainScreen;
        Texture2D mMenu;
        Texture2D mMenuOptions;
        Texture2D bg;

        KeyboardState mPreviousKeyboardState;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            // create the particle systems and add them to the components list.
            // 
            explosion = new ExplosionPS(this, 2);
            Components.Add(explosion);

            explosion_smoke = new ExplosionSmokePS(this, 1);
            Components.Add(explosion_smoke);

            muzzleflash = new MuzzleFlashPS(this, 1);
            Components.Add(muzzleflash);

            dirt = new DirtCloudPS(this, 1);
            Components.Add(dirt);

            grass = new GrassPS(this, 1);
            Components.Add(grass);

            // Initialize struct for setting enemy raptor stats
            this.RAPTOR_STATS.maxSpeed = 3.0f;
            this.RAPTOR_STATS.turnSpeed = 0.05f;
            this.RAPTOR_STATS.chaseDistance = 2000.0f;
            this.RAPTOR_STATS.caughtDistance = 60.0f;
            this.RAPTOR_STATS.hysteresis = 15.0f;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            myBackground = new Sprite();
            myBackground2 = new Sprite();

            Player1 = new Jeep();
            //give the Jeep a reference to the muzzleflash component
            Player1.muzz = muzzleflash;
            Player1.dirt_cloud = dirt;
            Player1.grass = grass;
            //Player1.Scale = 0.5f;

            HBar = new HealthBar(this, Player1);

            turretReticle = new Reticle();

            this.previousY = 0;

            obstacleList = new LinkedList<Obstacle>();
            recycledObstacles = new LinkedList<Obstacle>();

            enemyList = new LinkedList<Enemy>();
            recycledEnemies = new LinkedList<Enemy>();

            smallObstacleLoader = new Obstacle("obstacle_small");
            smallDestroyedObstacleLoader = new Obstacle("obstacle_small_destroyed");

            enemyLoader = new Enemy("raptor", this.RAPTOR_STATS);

            randNumGenerator = new Random();

            Viewport vp = graphics.GraphicsDevice.Viewport;

            //enemyPosition = new Vector2(vp.Width / 4, vp.Height / 2);

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            //add all menu content
            mTitleScreen = Content.Load<Texture2D>("Title");
            mMainScreen = Content.Load<Texture2D>("MainScreen");
            mMenu = Content.Load<Texture2D>("Menu");
            mMenuOptions = Content.Load<Texture2D>("MenuOptions2");
            bg = Content.Load<Texture2D>("bg");

            //main game screen content
            myBackground.LoadContent(this.Content, "background");
            myBackground.Position = new Vector2(0, 0);

            myBackground2.LoadContent(this.Content, "background");
            myBackground2.Position = new Vector2(0, myBackground2.Position.Y - myBackground2.Size.Height);

            Player1.LoadContent(this.Content);

            HBar.LoadContent(this.Content); 

            /*tankTexture = Content.Load<Texture2D>("Tank");
            tankTextureCenter =
                new Vector2(tankTexture.Width / 2, tankTexture.Height / 2);*/

            //enemyTexture = Content.Load<Texture2D>("raptor");
            //enemyTextureCenter =
            //    new Vector2(enemyTexture.Width / 2, enemyTexture.Height / 2);

            turretReticle.LoadContent(this.Content);

            smallObstacleLoader.LoadContent(this.Content);
            smallDestroyedObstacleLoader.LoadContent(this.Content);
            enemyLoader.LoadContent(this.Content);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        protected void generateNewRow(float yVal)
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
                    }

                    if (randomNum < OBSTACLE_LOG_ODDS * OBSTACLE_PLACEMENT_ODDS)
                    {
                        obNode.Value.AssetName = "obstacle_log";
                        i++;
                    }

                    obNode.Value.Position.X = i * UNIT_OBJECT_WIDTH;
                    obNode.Value.Position.Y = yVal;
                    obNode.Value.LoadContent(this.Content);
                    obstacleList.AddLast(obNode);
                }
            }
        }

        protected void generateNewEnemies(float yVal)
        {
            //obstacleList.AddLast(new LinkedListNode<List<Obstacle>>(new List<Obstacle>()));
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
                    enemyNode.Value.LoadContent(this.Content);
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
                    //Obstacle ob = new Obstacle("obstacle_small_destroyed");
                    //currentOb.AssetName = "obstacle_small_destroyed";
                    //currentOb.LoadContent(this.Content);
                    obstacleList.Remove(obstacleMatrixNode);
                    recycledObstacles.AddLast(obstacleMatrixNode);

                    // TODO: Put damage updating function call here
                    Player1.Jeep_health -= 5;
                    Player1.Jeep_health = (int)MathHelper.Clamp(Player1.Jeep_health, 0.0f, 100.0f);

                    //make big explosion
                    Vector2 where;
                    where.X = currentOb.Position.X + currentOb.Source.Width / 2;
                    where.Y = currentOb.Position.Y + currentOb.Source.Height / 2;
                    explosion.AddParticles(where);
                    explosion_smoke.AddParticles(where);
                }
                obstacleMatrixNode = nextNode;
            }
        }

        // Tests collisions between the jeep and enemies
        protected void processEnemyCollisions()
        {
            // Update enemies based on scrolling background
            LinkedListNode<Enemy> enemyListNode = enemyList.First;
            LinkedListNode<Enemy> nextNode = null;
            while (enemyListNode != null)
            {
                if (enemyListNode.Value.state == EnemyAiState.Caught)
                {
                    Player1.Jeep_health -= 10;
                    Player1.Jeep_health = (int)MathHelper.Clamp(Player1.Jeep_health, 0.0f, 100.0f);

                    enemyList.Remove(enemyListNode);
                    recycledEnemies.AddLast(enemyListNode);
                }

                enemyListNode = nextNode;
            }
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();
            if (Keyboard.GetState(PlayerIndex.One).IsKeyDown(Keys.Escape))
                this.Exit();
            
            //Menu screen logic
            KeyboardState aKeyboardState = Keyboard.GetState();

            // UpdateEnemy will run the AI code that controls the enemy's movement...
            //UpdateEnemy();

            //Update Health Bar
            HBar.Update(gameTime);

            switch (mCurrentScreen)
            {
                case Screen.Title:
                    {
                        //If the user presses the "Enter" key while on the Title screen, start the game
                        //by switching the current state to the Main Screen
                        if (aKeyboardState.IsKeyDown(Keys.Enter) == true)
                        {
                            mCurrentScreen = Screen.Main;
                        }
                        else if (GamePad.GetState(PlayerIndex.One).Buttons.X == ButtonState.Pressed)
                            mCurrentScreen = Screen.Main;
                        break;
                    }
                case Screen.Main:
                    {
                        //If the user presses the "Space" key while in the main game screen, bring
                        //up the Menu options by switching the current state to Menu
                        if (aKeyboardState.IsKeyDown(Keys.Space) == true)
                        {
                            mCurrentScreen = Screen.Menu;
                        }
                        break;
                    }
                case Screen.Menu:
                    {
                        //Move the currently highlighted menu option 
                        //up and down depending on what key the user has pressed
                        if (aKeyboardState.IsKeyDown(Keys.Down) == true && mPreviousKeyboardState.IsKeyDown(Keys.Down) == false)
                        {
                            //Move selection down
                            switch (mCurrentMenuOption)
                            {
                                case MenuOptions.Resume:
                                    {
                                        mCurrentMenuOption = MenuOptions.ExitGame;
                                        break;
                                    }
                            }

                        }

                        if (aKeyboardState.IsKeyDown(Keys.Up) == true && mPreviousKeyboardState.IsKeyDown(Keys.Up) == false)
                        {
                            //Move selection up
                            switch (mCurrentMenuOption)
                            {
                                case MenuOptions.ExitGame:
                                    {
                                        mCurrentMenuOption = MenuOptions.Resume;
                                        break;
                                    }
                            }
                        }

                        //If the user presses the "Enter" key, move the state to the 
                        //appropriate game state based on the current selection
                        if (aKeyboardState.IsKeyDown(Keys.Enter) == true)
                        {
                            switch (mCurrentMenuOption)
                            {
                                //Return to the Main game screen and close the menu
                                case MenuOptions.Resume:
                                    {
                                        mCurrentScreen = Screen.Main;
                                        break;
                                    }
                                //Exit the game
                                case MenuOptions.ExitGame:
                                    {
                                        this.Exit();
                                        break;
                                    }
                            }

                            //Reset the selected menu option to Resume
                            mCurrentMenuOption = MenuOptions.Resume;
                        }
                        break;
                    }
            }

            // Store the Keyboard state
            mPreviousKeyboardState = aKeyboardState;
            
            //Update scrolling background
            //Vector2 speed = new Vector2(0, SCROLL_SPEED);
            //Vector2 dir = new Vector2(0, 1);        //background movement is in Y direction only
            Vector2 distanceTravelled = SCROLL_SPEED * SCROLL_DIR * (float)gameTime.ElapsedGameTime.TotalSeconds;
            myBackground.Position += distanceTravelled;
            myBackground2.Position += distanceTravelled;
            
            // Perform garbage collection on obstacles
            garbageCollectObstacles();

            /*// Perform garbage collection on enemies
            garbageCollectEnemies();*/

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
                generateNewRow(this.previousY - UNIT_OBJECT_HEIGHT);
                generateNewEnemies(900);
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

                // Once we've finished that, we'll use the ClampToViewport helper function
                // to clamp the enemy's position so that it stays on the screen.
                //enemyPosition = ClampToViewport(enemyPosition);
            }

            /*// Update enemies based on scrolling background
            LinkedListNode<Enemy> enemyListNode = enemyList.First;
            while (enemyListNode != null)
            {
                enemyListNode.Value.Position += distanceTravelled;
                enemyListNode = enemyListNode.Next;
            }*/

            //these if-statements shuffle the pictures as they go out of view
            if (myBackground2.Position.Y > MAX_WINY)
                myBackground2.Position.Y = -myBackground2.Size.Height;
            if (myBackground.Position.Y > MAX_WINY)
                myBackground.Position.Y = -myBackground.Size.Height;

            Player1.Update(gameTime);

            // Perform collision detection
            processObstacleCollisions(gameTime);
            processEnemyCollisions();
            
            turretReticle.Update(gameTime);

            if (mCurrentScreen == Screen.Main)
            {
                base.Update(gameTime);
            }
        }

        //helps the enemy stay on the screen
        private Vector2 ClampToViewport(Vector2 vector)
        {
            Viewport vp = graphics.GraphicsDevice.Viewport;
            vector.X = MathHelper.Clamp(vector.X, vp.X, vp.X + vp.Width);
            vector.Y = MathHelper.Clamp(vector.Y, vp.Y, vp.Y + vp.Height);
            return vector;
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GCLatencyMode oldGCMode = GCSettings.LatencyMode;
            GCSettings.LatencyMode = GCLatencyMode.LowLatency;
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();

            //Draw all screen elements
            switch (mCurrentScreen)
            {
                case Screen.Title:
                    {
                        spriteBatch.Draw(mTitleScreen, new Rectangle(0, 0, this.Window.ClientBounds.Width, this.Window.ClientBounds.Height), Color.White);
                        break;
                    }

                case Screen.Main:
                    {
                        // Draw scrolling background
                        myBackground.Draw(this.spriteBatch);
                        myBackground2.Draw(this.spriteBatch);

                        // Draw obstacles
                        LinkedListNode<Obstacle> obstacleListNode = obstacleList.First;
                        while (obstacleListNode != null)
                        {
                            obstacleListNode.Value.Draw(this.spriteBatch);
                            obstacleListNode = obstacleListNode.Next;
                        }

                        // Draw player
                        Player1.Draw(this.spriteBatch);

                        // Draw enemies
                        LinkedListNode<Enemy> enemyListNode = enemyList.First;
                        while (enemyListNode != null)
                        {
                            enemyListNode.Value.Draw(this.spriteBatch);
                            enemyListNode = enemyListNode.Next;
                        }

                        // Draw reticle
                        turretReticle.Draw(this.spriteBatch);

                        HBar.Draw(this.spriteBatch);

                        break;
                    }

                case Screen.Menu:
                    {
                        //draw background to be displayed under menu screen
                        myBackground.Draw(this.spriteBatch);
                        spriteBatch.Draw(bg, new Rectangle(0, 0, MAX_WINX, MAX_WINY), Color.Black);
                        spriteBatch.Draw(mMenu, new Rectangle(this.Window.ClientBounds.Width / 2 - mMenu.Width / 2, this.Window.ClientBounds.Height / 2 - mMenu.Height / 2, mMenu.Width, mMenu.Height), Color.White);

                        switch (mCurrentMenuOption)
                        {
                            case MenuOptions.Resume:
                                {
                                    spriteBatch.Draw(mMenuOptions, new Rectangle(this.Window.ClientBounds.Width / 2 - 100, this.Window.ClientBounds.Height / 2 - mMenu.Height / 2 + 50, 250, 50), new Rectangle(0, 0, 250, 50), Color.Gold);
                                    spriteBatch.Draw(mMenuOptions, new Rectangle(this.Window.ClientBounds.Width / 2 - 100, this.Window.ClientBounds.Height / 2 - mMenu.Height / 2 + 100, 250, 50), new Rectangle(0, 50, 250, 50), Color.White);
                                    break;
                                }
                            case MenuOptions.ExitGame:
                                {
                                    spriteBatch.Draw(mMenuOptions, new Rectangle(this.Window.ClientBounds.Width / 2 - 100, this.Window.ClientBounds.Height / 2 - mMenu.Height / 2 + 50, 250, 50), new Rectangle(0, 0, 250, 50), Color.White);
                                    spriteBatch.Draw(mMenuOptions, new Rectangle(this.Window.ClientBounds.Width / 2 - 100, this.Window.ClientBounds.Height / 2 - mMenu.Height / 2 + 100, 250, 50), new Rectangle(0, 50, 250, 50), Color.Gold);
                                    break;
                                }
                        }
                        break;
                    }
            }

            spriteBatch.End();

            base.Draw(gameTime);

            GCSettings.LatencyMode = oldGCMode;
        }
    }
}

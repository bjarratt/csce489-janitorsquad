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
        Texture2D enemyTexture;
        Vector2 enemyTextureCenter;
        Vector2 enemyPosition;
        EnemyAiState enemyState = EnemyAiState.Wander;
        float enemyOrientation;
        Vector2 enemyWanderDirection;

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
        }

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

        // this constant is used to avoid hysteresis, which is common in ai programming.
        // see the doc for more details.
        const float EnemyHysteresis = 15.0f;

        ExplosionPS explosion;
        ExplosionSmokePS explosion_smoke;
        MuzzleFlashPS muzzleflash;
        DirtCloudPS dirt;
        GrassPS grass;

        // Used for collision detection
        private Rectangle boundingBox1;
        private Rectangle boundingBox2;

        // Defined to prevent overlapping when obstacles are randomly generated
        const int UNIT_OBSTACLE_WIDTH = 50;
        const int UNIT_OBSTACLE_HEIGHT = 50;

        private const double OBSTACLE_PLACEMENT_ODDS = 0.015;
        private const double OBSTACLE_ROCK_ODDS = 0.80;
        //private const double OBSTACLE_POND_ODDS = 0.25;
        private const double OBSTACLE_LOG_ODDS = 0.20;

        private const double ENEMY_PLACEMENT_ODDS = 0.005;
        private const double ENEMY_DINO1_ODDS = 0.80;
        private const double ENEMY_DINO2_ODDS = 0.20;

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

            enemyLoader = new Enemy("Tank");

            randNumGenerator = new Random();

            Viewport vp = graphics.GraphicsDevice.Viewport;

            enemyPosition = new Vector2(vp.Width / 4, vp.Height / 2);

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

            enemyTexture = Content.Load<Texture2D>("Tank");
            enemyTextureCenter =
                new Vector2(enemyTexture.Width / 2, enemyTexture.Height / 2);

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

            for (int i = 0; i < MAX_WINX / UNIT_OBSTACLE_WIDTH; i++)
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

                    obNode.Value.Position.X = i * UNIT_OBSTACLE_WIDTH;
                    obNode.Value.Position.Y = yVal;
                    obNode.Value.LoadContent(this.Content);
                    obstacleList.AddLast(obNode);
                }
            }
        }

        /*protected void generateNewEnemies(float yVal)
        {
            //obstacleList.AddLast(new LinkedListNode<List<Obstacle>>(new List<Obstacle>()));
            double randomNum;

            for (int i = 0; i < MAX_WINX / UNIT_OBSTACLE_WIDTH; i++)
            {
                randomNum = randNumGenerator.NextDouble();
                if (randomNum < ENEMY_PLACEMENT_ODDS)
                {
                    LinkedListNode<Enemy> enemyNode;
                    if (recycledObstacles.Count == 0)
                    {
                        enemyNode = new LinkedListNode<Enemy>(new Enemy("Tank"));
                    }
                    else
                    {
                        enemyNode = recycledEnemies.First;
                        recycledObstacles.RemoveFirst();
                        enemyNode.Value.AssetName = "Tank";
                    }

                    if (randomNum < ENEMY_DINO2_ODDS * ENEMY_PLACEMENT_ODDS)
                    {
                        enemyNode.Value.AssetName = "obstacle_log";
                        i++;
                    }

                    enemyNode.Value.Position.X = i * UNIT_OBSTACLE_WIDTH;
                    enemyNode.Value.Position.Y = yVal;
                    enemyNode.Value.LoadContent(this.Content);
                    enemyList.AddLast(enemyNode);
                }
            }
        }*/

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

        /*// Recycle any enemies that are no longer in view
        protected void garbageCollectEnemies()
        {
            LinkedListNode<Enemy> enemyListNode = enemyList.First;
            LinkedListNode<Enemy> nextNode = null;
            while (enemyListNode != null)
            {
                if (enemyListNode.Value.Position.Y > MAX_WINY)
                {
                    nextNode = enemyListNode.Next;
                    enemyList.Remove(enemyListNode);
                    recycledEnemies.AddLast(enemyListNode);
                    enemyListNode = nextNode;
                }
                else
                {
                    enemyListNode = enemyListNode.Next;
                }
            }
        }*/

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
                    Player1.Jeep_health -= 10;
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

       /* // Tests collisions between the jeep and other in-game elements
        protected void processEnemyCollisions()
        {
            // Update enemies based on scrolling background
            LinkedListNode<Enemy> enemyMatrixNode = enemyList.First;
            LinkedListNode<Enemy> nextNode = null;
            while (enemyMatrixNode != null)
            {
                Enemy currentEn = enemyMatrixNode.Value;
                nextNode = enemyMatrixNode.Next;
                //boundingBox1 = new Rectangle((int)Player1.Position.X, (int)Player1.Position.Y, Player1.Source.Width, Player1.Source.Height);
                boundingBox1.X = (int)Player1.Position.X;
                boundingBox1.Y = (int)Player1.Position.Y;
                boundingBox1.Width = Player1.Source.Width;
                boundingBox1.Height = Player1.Source.Height;
                //boundingBox2 = new Rectangle((int)currentEn.Position.X, (int)currentEn.Position.Y, currentEn.Source.Width, currentEn.Source.Height);
                boundingBox2.X = (int)currentEn.Position.X;
                boundingBox2.Y = (int)currentEn.Position.Y;
                boundingBox2.Width = currentEn.Source.Width;
                boundingBox2.Height = currentEn.Source.Height;

                if (boundingBox1.Intersects(boundingBox2)) // Jeep collided with an enemy
                {
                    //Enemy en = new Enemy("Tank");
                    //currentEn.AssetName = "dead_dino";
                    //currentEn.LoadContent(this.Content);
                    enemyList.Remove(enemyMatrixNode);
                    recycledEnemies.AddLast(enemyMatrixNode);

                    // TODO: Put damage updating function call here

                    //make big explosion
                    //Vector2 where;
                    //where.X = currentEn.Position.X + currentEn.Source.Width / 2;
                    //where.Y = currentEn.Position.Y + currentEn.Source.Height / 2;
                   // explosion.AddParticles(where);
                }
                enemyMatrixNode = nextNode;
            }
        }*/

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
            UpdateEnemy();

            //Update Health Bar
            HBar.Update(gameTime);

            // Once we've finished that, we'll use the ClampToViewport helper function
            // to clamp the enemy's position so that it stays on the screen.
            enemyPosition = ClampToViewport(enemyPosition);

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

            // Create a new row of obstacles if needed
            if ((this.previousY + distanceTravelled.Y) >= UNIT_OBSTACLE_HEIGHT)
            {
                this.previousY = (this.previousY + distanceTravelled.Y) % UNIT_OBSTACLE_HEIGHT;
                generateNewRow(this.previousY - UNIT_OBSTACLE_HEIGHT);
            }
            else
            {
                this.previousY += distanceTravelled.Y;
            }

            /*// Update enemies based on scrolling background
            LinkedListNode<Enemy> enemyListNode = enemyList.First;
            while (enemyListNode != null)
            {
                enemyListNode.Value.Position += distanceTravelled;
                enemyListNode = enemyListNode.Next;
            }

            // Create a new row of enemies if needed
            if ((this.previousY + distanceTravelled.Y) >= UNIT_OBSTACLE_HEIGHT)
            {
                this.previousY = (this.previousY + distanceTravelled.Y) % UNIT_OBSTACLE_HEIGHT;
                generateNewRow(this.previousY - UNIT_OBSTACLE_HEIGHT);
            }
            else
            {
                this.previousY += distanceTravelled.Y;
            }*/

            //these if-statements shuffle the pictures as they go out of view
            if (myBackground2.Position.Y > MAX_WINY)
                myBackground2.Position.Y = -myBackground2.Size.Height;
            if (myBackground.Position.Y > MAX_WINY)
                myBackground.Position.Y = -myBackground.Size.Height;

            Player1.Update(gameTime);

            // Perform collision detection
            processObstacleCollisions(gameTime);

            /*//Perform collision detection
            processEnemyCollisions(); */
            
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
        /// UpdateEnemy runs the AI code that will update the enemy's orientation and
        /// position. The enemy has three states: chase, caught and idle.
        /// </summary>
        private void UpdateEnemy()
        {
     
            // First we have to use the current state to decide what the thresholds are
            // for changing state, as described in the doc.

            float enemyChaseThreshold = EnemyChaseDistance;
            float enemyCaughtThreshold = EnemyCaughtDistance;
            // if the enemy is idle, he prefers to stay idle. we do this by making the
            // chase distance smaller, so the enemy will be less likely to begin chasing
            // the jeep.
            if (enemyState == EnemyAiState.Wander)
            {
                enemyChaseThreshold -= EnemyHysteresis / 2;
            }
            // similarly, if the enemy is active, he prefers to stay active. we
            // accomplish this by increasing the range of values that will cause the
            // enemy to go into the active state.
            else if (enemyState == EnemyAiState.Chasing)
            {
                enemyChaseThreshold += EnemyHysteresis / 2;
                enemyCaughtThreshold -= EnemyHysteresis / 2;
            }
            // the same logic is applied to the finished state.
            else if (enemyState == EnemyAiState.Caught)
            {
                enemyCaughtThreshold += EnemyHysteresis / 2;
            }

            // Second, now that we know what the thresholds are, we compare the enemy's 
            // distance from the jeep against the thresholds to decide what the enemy's
            // current state is.
            float distanceFromJeep = Vector2.Distance(enemyPosition, Player1.Position);
            if (distanceFromJeep > enemyChaseThreshold)
            {
                // if the enemy is far away from the jeep, it should idle
                enemyState = EnemyAiState.Wander;
            }
            else if (distanceFromJeep > enemyCaughtThreshold)
            {
                enemyState = EnemyAiState.Chasing;
            }
            else
            {
                enemyState = EnemyAiState.Caught;
            }

            // Third, once we know what state we're in, act on that state.
            float currentEnemySpeed;
            if (enemyState == EnemyAiState.Chasing)
            {
                // the enemy wants to chase the jeep, so it will just use the TurnToFace
                // function to turn towards the jeep's position. Then, when the enemy
                // moves forward, he will chase the jeep.
                enemyOrientation = TurnToFace(enemyPosition, Player1.Position, enemyOrientation,
                    EnemyTurnSpeed);
                currentEnemySpeed = MaxEnemySpeed;
            }
            else if (enemyState == EnemyAiState.Wander)
            {
                // call the wander function for the enemy
                Wander(enemyPosition, ref enemyWanderDirection, ref enemyOrientation,
                    EnemyTurnSpeed);
                currentEnemySpeed = .25f * MaxEnemySpeed;
            }
            else
            {
                // if the enemy catches the jeep, it should stop.
                // Otherwise it will run right by, then spin around and
                // try to catch it all over again. The end result is that it will kind
                // of "run laps" around the jeep, which looks funny, but is not what
                // we're after.
                currentEnemySpeed = 0.0f;
            }

            // this calculation is also important; we construct a heading
            // vector based on the enemy's orientation, and then make the enemy move along
            // that heading.
            Vector2 heading = new Vector2(
                (float)Math.Cos(enemyOrientation), (float)Math.Sin(enemyOrientation));
            enemyPosition += heading * currentEnemySpeed;
        }

        /// <summary>
        /// Wander contains functionality for the enemy, and does just what its name implies: makes them wander around the
        /// screen. The specifics of the function are described in more detail in the
        /// accompanying doc.
        /// </summary>
        /// <param name="position">the position of the character that is wandering
        /// </param>
        /// <param name="wanderDirection">the direction that the character is currently
        /// wandering. this parameter is passed by reference because it is an input and
        /// output parameter: Wander accepts it as input, and will update it as well.
        /// </param>
        /// <param name="orientation">the character's orientation. this parameter is
        /// also passed by reference and is an input/output parameter.</param>
        /// <param name="turnSpeed">the character's maximum turning speed.</param>
        private void Wander(Vector2 position, ref Vector2 wanderDirection,
            ref float orientation, float turnSpeed)
        {
            // The wander effect is accomplished by having the character aim in a random
            // direction. Every frame, this random direction is slightly modified.
            // Finally, to keep the characters on the center of the screen, we have them
            // turn to face the screen center. The further they are from the screen
            // center, the more they will aim back towards it.

            // the first step of the wander behavior is to use the random number
            // generator to offset the current wanderDirection by some random amount.
            // .25 is a bit of a magic number, but it controls how erratic the wander
            // behavior is. Larger numbers will make the characters "wobble" more,
            // smaller numbers will make them more stable. we want just enough
            // wobbliness to be interesting without looking odd.
            wanderDirection.X +=
                MathHelper.Lerp(-.25f, .25f, (float)random.NextDouble());
            wanderDirection.Y +=
                MathHelper.Lerp(-.25f, .25f, (float)random.NextDouble());

            // we'll renormalize the wander direction, ...
            if (wanderDirection != Vector2.Zero)
            {
                wanderDirection.Normalize();
            }
            // ... and then turn to face in the wander direction. We don't turn at the
            // maximum turning speed, but at 15% of it. Again, this is a bit of a magic
            // number: it works well for this sample, but feel free to tweak it.
            orientation = TurnToFace(position, position + wanderDirection, orientation,
                .15f * turnSpeed);


            // next, we'll turn the characters back towards the center of the screen, to
            // prevent them from getting stuck on the edges of the screen.
            Vector2 screenCenter = Vector2.Zero;
            screenCenter.X = graphics.GraphicsDevice.Viewport.Width / 2;
            screenCenter.Y = graphics.GraphicsDevice.Viewport.Height / 2;

            // Here we are creating a curve that we can apply to the turnSpeed. This
            // curve will make it so that if we are close to the center of the screen,
            // we won't turn very much. However, the further we are from the screen
            // center, the more we turn. At most, we will turn at 30% of our maximum
            // turn speed. This too is a "magic number" which works well for the sample.
            // Feel free to play around with this one as well: smaller values will make
            // the characters explore further away from the center, but they may get
            // stuck on the walls. Larger numbers will hold the characters to center of
            // the screen. If the number is too large, the characters may end up
            // "orbiting" the center.
            float distanceFromScreenCenter = Vector2.Distance(screenCenter, position);
            float MaxDistanceFromScreenCenter =
                Math.Min(screenCenter.Y, screenCenter.X);

            float normalizedDistance =
                distanceFromScreenCenter / MaxDistanceFromScreenCenter;

            float turnToCenterSpeed = .3f * normalizedDistance * normalizedDistance *
                turnSpeed;

            // once we've calculated how much we want to turn towards the center, we can
            // use the TurnToFace function to actually do the work.
            orientation = TurnToFace(position, screenCenter, orientation,
                turnToCenterSpeed);
        }

        /// <summary>
        /// Calculates the angle that an object should face, given its position, its
        /// target's position, its current angle, and its maximum turning speed.
        /// </summary>
        private static float TurnToFace(Vector2 position, Vector2 faceThis,
            float currentAngle, float turnSpeed)
        {
            // consider this diagram:
            //         B 
            //        /|
            //      /  |
            //    /    | y
            //  / o    |
            // A--------
            //     x
            // 
            // where A is the position of the object, B is the position of the target,
            // and "o" is the angle that the object should be facing in order to 
            // point at the target. we need to know what o is. using trig, we know that
            //      tan(theta)       = opposite / adjacent
            //      tan(o)           = y / x
            // if we take the arctan of both sides of this equation...
            //      arctan( tan(o) ) = arctan( y / x )
            //      o                = arctan( y / x )
            // so, we can use x and y to find o, our "desiredAngle."
            // x and y are just the differences in position between the two objects.
            float x = faceThis.X - position.X;
            float y = faceThis.Y - position.Y;

            // we'll use the Atan2 function. Atan will calculates the arc tangent of 
            // y / x for us, and has the added benefit that it will use the signs of x
            // and y to determine what cartesian quadrant to put the result in.
            // http://msdn2.microsoft.com/en-us/library/system.math.atan2.aspx
            float desiredAngle = (float)Math.Atan2(y, x);

            // so now we know where we WANT to be facing, and where we ARE facing...
            // if we weren't constrained by turnSpeed, this would be easy: we'd just 
            // return desiredAngle.
            // instead, we have to calculate how much we WANT to turn, and then make
            // sure that's not more than turnSpeed.

            // first, figure out how much we want to turn, using WrapAngle to get our
            // result from -Pi to Pi ( -180 degrees to 180 degrees )
            float difference = WrapAngle(desiredAngle - currentAngle);

            // clamp that between -turnSpeed and turnSpeed.
            difference = MathHelper.Clamp(difference, -turnSpeed, turnSpeed);

            // so, the closest we can get to our target is currentAngle + difference.
            // return that, using WrapAngle again.
            return WrapAngle(currentAngle + difference);
        }

        /// <summary>
        /// Returns the angle expressed in radians between -Pi and Pi.
        /// <param name="radians">the angle to wrap, in radians.</param>
        /// <returns>the input value expressed in radians from -Pi to Pi.</returns>
        /// </summary>
        private static float WrapAngle(float radians)
        {
            while (radians < -MathHelper.Pi)
            {
                radians += MathHelper.TwoPi;
            }
            while (radians > MathHelper.Pi)
            {
                radians -= MathHelper.TwoPi;
            }
            return radians;
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
                        //draw scrolling background
                        myBackground.Draw(this.spriteBatch);
                        myBackground2.Draw(this.spriteBatch);

                        // draw the enemy
                        spriteBatch.Draw(enemyTexture, enemyPosition, null, Color.White,
                            enemyOrientation, enemyTextureCenter, 1.0f, SpriteEffects.None, 0.0f);
                        

                        /*// Draw enemies
                        LinkedListNode<Enemy> enemyListNode = enemyList.First;
                        while (enemyListNode != null)
                        {
                            enemyListNode.Value.Draw(this.spriteBatch);
                            enemyListNode = enemyListNode.Next;
                        } */

                        // Draw obstacles
                        LinkedListNode<Obstacle> obstacleListNode = obstacleList.First;
                        while (obstacleListNode != null)
                        {
                            obstacleListNode.Value.Draw(this.spriteBatch);
                            obstacleListNode = obstacleListNode.Next;
                        }

                        //draw player
                        Player1.Draw(this.spriteBatch);

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

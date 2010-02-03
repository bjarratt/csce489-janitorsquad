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
        Obstacle smallObstacleLoader; // Used to load the first instance of a small obstacle
        Obstacle smallDestroyedObstacleLoader;

        ExplosionPS explosion;

        // Used for collision detection
        private Rectangle boundingBox1;
        private Rectangle boundingBox2;

        // Defined to prevent overlapping when obstacles are randomly generated
        const int UNIT_OBSTACLE_WIDTH = 50;
        const int UNIT_OBSTACLE_HEIGHT = 50;

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
        //Texture2D mInventoryScreen;
        Texture2D mMenu;
        Texture2D mMenuOptions;
        Texture2D bg;

        KeyboardState mPreviousKeyboardState;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            // create the particle systems and add them to the components list.
            // we should never see more than four explosions at once
            explosion = new ExplosionPS(this, 1);
            Components.Add(explosion);
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
            //Player1.Scale = 0.5f;

            turretReticle = new Reticle();

            this.previousY = 0;

            obstacleList = new LinkedList<Obstacle>();
            recycledObstacles = new LinkedList<Obstacle>();

            smallObstacleLoader = new Obstacle("obstacle_small");
            smallDestroyedObstacleLoader = new Obstacle("obstacle_small_destroyed");

            randNumGenerator = new Random();

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
            //mInventoryScreen = Content.Load<Texture2D>("Inventory");
            mMenu = Content.Load<Texture2D>("Menu");
            mMenuOptions = Content.Load<Texture2D>("MenuOptions2");
            bg = Content.Load<Texture2D>("bg");

            //main game screen content
            myBackground.LoadContent(this.Content, "background");
            myBackground.Position = new Vector2(0, 0);

            myBackground2.LoadContent(this.Content, "background");
            myBackground2.Position = new Vector2(0, myBackground2.Position.Y - myBackground2.Size.Height);

            Player1.LoadContent(this.Content);

            turretReticle.LoadContent(this.Content);

            smallObstacleLoader.LoadContent(this.Content);
            smallDestroyedObstacleLoader.LoadContent(this.Content);
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

            const double OBSTACLE_PLACEMENT_ODDS = 0.015;

            for (int i = 0; i < MAX_WINX / UNIT_OBSTACLE_WIDTH; i++)
            {
                if (randNumGenerator.NextDouble() < OBSTACLE_PLACEMENT_ODDS)
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

                    obNode.Value.Position.X = i * UNIT_OBSTACLE_WIDTH;
                    obNode.Value.Position.Y = yVal;
                    obNode.Value.LoadContent(this.Content);
                    obstacleList.AddLast(obNode);
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
        protected void processObstacleCollisions()
        {
            // Update obstacles based on scrolling background
            LinkedListNode<Obstacle> obstacleMatrixNode = obstacleList.First;
            while (obstacleMatrixNode != null)
            {
                Obstacle currentOb = obstacleMatrixNode.Value;
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
                    currentOb.AssetName = "obstacle_small_destroyed";
                    currentOb.LoadContent(this.Content);

                    // TODO: Put damage updating function call here

                    //make big explosion
                    Vector2 where;
                    where.X = currentOb.Position.X + currentOb.Source.Width / 2;
                    where.Y = currentOb.Position.Y + currentOb.Source.Height / 2;
                    explosion.AddParticles(where);
                }
                obstacleMatrixNode = obstacleMatrixNode.Next;
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
            switch (mCurrentScreen)
            {
                case Screen.Title:
                    {
                        //If the user presses the "X" key while on the Title screen, start the game
                        //by switching the current state to the Main Screen
                        if (aKeyboardState.IsKeyDown(Keys.X) == true)
                        {
                            mCurrentScreen = Screen.Main;
                        }
                        break;
                    }
                case Screen.Main:
                    {
                        //If the user presses the "P" key while in the main game screen, bring
                        //up the Menu options by switching the current state to Menu
                        if (aKeyboardState.IsKeyDown(Keys.P) == true)
                        {
                            mCurrentScreen = Screen.Menu;
                        }
                        break;
                    }
                /*case Screen.Inventory:
                    {
                        //If the user presses the "X" key while in the Inventory screen, close
                        //the inventory screen and resume the game by switching the current state
                        //to the Main screen
                        if (aKeyboardState.IsKeyDown(Keys.X) == true && mPreviousKeyboardState.IsKeyDown(Keys.X) == false)
                        {
                            mCurrentScreen = Screen.Main;
                        }
                        break;
                    }*/
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
                                /*case MenuOptions.Inventory:
                                    {
                                        mCurrentMenuOption = MenuOptions.ExitGame;
                                        break;
                                    }*/
                            }

                        }

                        if (aKeyboardState.IsKeyDown(Keys.Up) == true && mPreviousKeyboardState.IsKeyDown(Keys.Up) == false)
                        {
                            //Move selection up
                            switch (mCurrentMenuOption)
                            {
                                /* case MenuOptions.Inventory:
                                     {
                                         mCurrentMenuOption = MenuOptions.Resume;
                                         break;
                                     }*/
                                case MenuOptions.ExitGame:
                                    {
                                        mCurrentMenuOption = MenuOptions.Resume;
                                        break;
                                    }
                            }
                        }

                        //If the user presses the "X" key, move the state to the 
                        //appropriate game state based on the current selection
                        if (aKeyboardState.IsKeyDown(Keys.X) == true)
                        {
                            switch (mCurrentMenuOption)
                            {
                                //Return to the Main game screen and close the menu
                                case MenuOptions.Resume:
                                    {
                                        mCurrentScreen = Screen.Main;
                                        break;
                                    }
                                //Open the Inventory screen
                                /*&case MenuOptions.Inventory:
                                    {
                                        mCurrentScreen = Screen.Inventory;
                                        break;
                                    }*/
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
            
            // Update scrolling background
            //Vector2 speed = new Vector2(0, SCROLL_SPEED);
            //Vector2 dir = new Vector2(0, 1);        //background movement is in Y direction only
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


            //these if-statements shuffle the pictures as they go out of view
            if (myBackground2.Position.Y > MAX_WINY)
                myBackground2.Position.Y = -myBackground2.Size.Height;
            if (myBackground.Position.Y > MAX_WINY)
                myBackground.Position.Y = -myBackground.Size.Height;

            Player1.Update(gameTime);

            // Perform collision detection
            processObstacleCollisions();

            turretReticle.Update(gameTime);

            base.Update(gameTime);
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
                                    //spriteBatch.Draw(mMenuOptions, new Rectangle(this.Window.ClientBounds.Width / 2 - 100, this.Window.ClientBounds.Height / 2 - mMenu.Height / 2 + 150, 250, 50), new Rectangle(0, 100, 250, 50), Color.White);
                                    break;
                                }

                            /*case MenuOptions.Inventory:
                                {
                                    spriteBatch.Draw(mMenuOptions, new Rectangle(this.Window.ClientBounds.Width / 2 - 100, this.Window.ClientBounds.Height / 2 - mMenu.Height / 2 + 50, 250, 50), new Rectangle(0, 0, 250, 50), Color.White);
                                    spriteBatch.Draw(mMenuOptions, new Rectangle(this.Window.ClientBounds.Width / 2 - 100, this.Window.ClientBounds.Height / 2 - mMenu.Height / 2 + 100, 250, 50), new Rectangle(0, 50, 250, 50), Color.Gold);
                                    spriteBatch.Draw(mMenuOptions, new Rectangle(this.Window.ClientBounds.Width / 2 - 100, this.Window.ClientBounds.Height / 2 - mMenu.Height / 2 + 150, 250, 50), new Rectangle(0, 100, 250, 50), Color.White);
                                    break;
                                }*/

                            case MenuOptions.ExitGame:
                                {
                                    spriteBatch.Draw(mMenuOptions, new Rectangle(this.Window.ClientBounds.Width / 2 - 100, this.Window.ClientBounds.Height / 2 - mMenu.Height / 2 + 50, 250, 50), new Rectangle(0, 0, 250, 50), Color.White);
                                    spriteBatch.Draw(mMenuOptions, new Rectangle(this.Window.ClientBounds.Width / 2 - 100, this.Window.ClientBounds.Height / 2 - mMenu.Height / 2 + 100, 250, 50), new Rectangle(0, 50, 250, 50), Color.Gold);
                                    //spriteBatch.Draw(mMenuOptions, new Rectangle(this.Window.ClientBounds.Width / 2 - 100, this.Window.ClientBounds.Height / 2 - mMenu.Height / 2 + 150, 250, 50), new Rectangle(0, 100, 250, 50), Color.Gold);
                                    break;
                                }
                        }
                        break;
                    }

                /*case Screen.Inventory:
                    {
                        spriteBatch.Draw(mInventoryScreen, new Rectangle(0, 0, this.Window.ClientBounds.Width, this.Window.ClientBounds.Height), Color.White);
                        break;
                    }*/
            }

            spriteBatch.End();

            base.Draw(gameTime);

            GCSettings.LatencyMode = oldGCMode;
        }
    }
}

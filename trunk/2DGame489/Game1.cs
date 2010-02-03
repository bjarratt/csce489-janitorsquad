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

        //enemy object and position info
        Texture2D tankTexture;
        Vector2 tankTextureCenter;
        Vector2 tankPosition;
        TankAiState tankState = TankAiState.Wander;
        float tankOrientation;
        Vector2 tankWanderDirection;

        //enums and state info for enemy
        /// <summary>
        /// TankAiState is used to keep track of what the tank is currently doing.
        /// </summary>
        enum TankAiState
        {
            // chasing the cat
            Chasing,
            // the tank has gotten close enough that the cat that it can stop chasing it
            Caught,
            // the tank can't "see" the cat, and is wandering around.
            Wander
        }

        // how fast can the tank move?
        const float MaxTankSpeed = 4.0f;

        // how fast can he turn?
        const float TankTurnSpeed = 0.10f;

        // this value controls the distance at which the tank will start to chase the
        // cat.
        const float TankChaseDistance = 350.0f;

        // TankCaughtDistance controls the distance at which the tank will stop because
        // he has "caught" the cat.
        const float TankCaughtDistance = 60.0f;

        // this constant is used to avoid hysteresis, which is common in ai programming.
        // see the doc for more details.
        const float TankHysteresis = 15.0f;

        ExplosionPS explosion;

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
            explosion = new ExplosionPS(this, 2);
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

            Viewport vp = graphics.GraphicsDevice.Viewport;

            tankPosition = new Vector2(vp.Width / 4, vp.Height / 2);

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

            tankTexture = Content.Load<Texture2D>("Tank");
            tankTextureCenter =
                new Vector2(tankTexture.Width / 2, tankTexture.Height / 2);

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

                    //make big explosion
                    Vector2 where;
                    where.X = currentOb.Position.X + currentOb.Source.Width / 2;
                    where.Y = currentOb.Position.Y + currentOb.Source.Height / 2;
                    explosion.AddParticles(where);
                }
                obstacleMatrixNode = nextNode;
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

            // UpdateTank will run the AI code that controls the tank's movement...
            UpdateTank();

            // Once we've finished that, we'll use the ClampToViewport helper function
            // to clamp everyone's position so that they stay on the screen.
            tankPosition = ClampToViewport(tankPosition);

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

        //helps the tank stay on the screen
        private Vector2 ClampToViewport(Vector2 vector)
        {
            Viewport vp = graphics.GraphicsDevice.Viewport;
            vector.X = MathHelper.Clamp(vector.X, vp.X, vp.X + vp.Width);
            vector.Y = MathHelper.Clamp(vector.Y, vp.Y, vp.Y + vp.Height);
            return vector;
        }

        /// <summary>
        /// UpdateTank runs the AI code that will update the tank's orientation and
        /// position. It is very similar to UpdateMouse, but is slightly more
        /// complicated: where mouse only has two states, idle and active, the Tank has
        /// three.
        /// </summary>
        private void UpdateTank()
        {
            // However, the tank's behavior is more complicated than the mouse's, and so
            // the decision making process is a little different. 

            // First we have to use the current state to decide what the thresholds are
            // for changing state, as described in the doc.

            float tankChaseThreshold = TankChaseDistance;
            float tankCaughtThreshold = TankCaughtDistance;
            // if the tank is idle, he prefers to stay idle. we do this by making the
            // chase distance smaller, so the tank will be less likely to begin chasing
            // the cat.
            if (tankState == TankAiState.Wander)
            {
                tankChaseThreshold -= TankHysteresis / 2;
            }
            // similarly, if the tank is active, he prefers to stay active. we
            // accomplish this by increasing the range of values that will cause the
            // tank to go into the active state.
            else if (tankState == TankAiState.Chasing)
            {
                tankChaseThreshold += TankHysteresis / 2;
                tankCaughtThreshold -= TankHysteresis / 2;
            }
            // the same logic is applied to the finished state.
            else if (tankState == TankAiState.Caught)
            {
                tankCaughtThreshold += TankHysteresis / 2;
            }

            // Second, now that we know what the thresholds are, we compare the tank's 
            // distance from the cat against the thresholds to decide what the tank's
            // current state is.
            float distanceFromCat = Vector2.Distance(tankPosition, Player1.Position);
            if (distanceFromCat > tankChaseThreshold)
            {
                // just like the mouse, if the tank is far away from the cat, it should
                // idle.
                tankState = TankAiState.Wander;
            }
            else if (distanceFromCat > tankCaughtThreshold)
            {
                tankState = TankAiState.Chasing;
            }
            else
            {
                tankState = TankAiState.Caught;
            }

            // Third, once we know what state we're in, act on that state.
            float currentTankSpeed;
            if (tankState == TankAiState.Chasing)
            {
                // the tank wants to chase the cat, so it will just use the TurnToFace
                // function to turn towards the cat's position. Then, when the tank
                // moves forward, he will chase the cat.
                tankOrientation = TurnToFace(tankPosition, Player1.Position, tankOrientation,
                    TankTurnSpeed);
                currentTankSpeed = MaxTankSpeed;
            }
            else if (tankState == TankAiState.Wander)
            {
                // wander works just like the mouse's.
                Wander(tankPosition, ref tankWanderDirection, ref tankOrientation,
                    TankTurnSpeed);
                currentTankSpeed = .25f * MaxTankSpeed;
            }
            else
            {
                // this part is different from the mouse. if the tank catches the cat, 
                // it should stop. otherwise it will run right by, then spin around and
                // try to catch it all over again. The end result is that it will kind
                // of "run laps" around the cat, which looks funny, but is not what
                // we're after.
                currentTankSpeed = 0.0f;
            }

            // this calculation is also just like the mouse's: we construct a heading
            // vector based on the tank's orientation, and then make the tank move along
            // that heading.
            Vector2 heading = new Vector2(
                (float)Math.Cos(tankOrientation), (float)Math.Sin(tankOrientation));
            tankPosition += heading * currentTankSpeed;
        }

        /// <summary>
        /// Wander contains functionality that is shared between both the mouse and the
        /// tank, and does just what its name implies: makes them wander around the
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

                        // draw the tank
                        spriteBatch.Draw(tankTexture, tankPosition, null, Color.White,
                            tankOrientation, tankTextureCenter, 1.0f, SpriteEffects.None, 0.0f);

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

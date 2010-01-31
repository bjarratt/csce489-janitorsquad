using System;
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

        const int MAX_WINX = 800;
        const int MAX_WINY = 600;

        Sprite myBackground;
        Sprite myBackground2;
        Jeep Player1;
        Reticle turretReticle;
        LinkedList<List<Obstacle>> obstacleMatrix;
        Obstacle smallObstacleLoader; // Used to load the first instance of a small obstacle

        // Defined to prevent overlapping when obstacles are randomly generated
        const int UNIT_OBSTACLE_WIDTH = 50;
        const int UNIT_OBSTACLE_HEIGHT = 50;

        Random randNumGenerator; // Used for various randomly generated events, like obstacle placement

        float previousY;

        const int SCROLL_SPEED = 300;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
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

            /*
            for (int i = 0; i < (MAX_WINY / UNIT_OBSTACLE_HEIGHT) + 1; i++)
            {
                obstacleMatrix.AddLast(new LinkedListNode<List<Obstacle>>(new List<Obstacle>()));
            }
             */
            obstacleMatrix = new LinkedList<List<Obstacle>>();

            smallObstacleLoader = new Obstacle("obstacle_small", 0);

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

            myBackground.LoadContent(this.Content, "background");
            myBackground.Position = new Vector2(0, 0);

            myBackground2.LoadContent(this.Content, "background");
            myBackground2.Position = new Vector2(0, myBackground2.Position.Y - myBackground2.Size.Height);

            Player1.LoadContent(this.Content);

            turretReticle.LoadContent(this.Content);

            smallObstacleLoader.LoadContent(this.Content);
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
            obstacleMatrix.AddLast(new LinkedListNode<List<Obstacle>>(new List<Obstacle>()));

            const double OBSTACLE_PLACEMENT_ODDS = 0.015;

            for (int i = 0; i < MAX_WINX / UNIT_OBSTACLE_WIDTH; i++)
            {
                if (randNumGenerator.NextDouble() < OBSTACLE_PLACEMENT_ODDS)
                {
                    Obstacle ob = new Obstacle("obstacle_small",0);
                    ob.Position.X = i * UNIT_OBSTACLE_WIDTH;
                    ob.Position.Y = yVal;
                    ob.LoadContent(this.Content);
                    obstacleMatrix.Last.Value.Add(ob);
                }
            }
        }

        // Remove any obstacles that are no longer in view
        protected void garbageCollectObstacles()
        {
            LinkedListNode<List<Obstacle>> obstacleMatrixNode = obstacleMatrix.First;
            LinkedListNode<List<Obstacle>> nextNode = null;
            while (obstacleMatrixNode != null)
            {
                if (obstacleMatrixNode.Value == null ||
                    obstacleMatrixNode.Value.Count == 0 ||
                    obstacleMatrixNode.Value[0].Position.Y > MAX_WINY)
                {
                    nextNode = obstacleMatrixNode.Next;
                    obstacleMatrix.Remove(obstacleMatrixNode);
                    obstacleMatrixNode = nextNode;
                }
                else
                {
                    obstacleMatrixNode = obstacleMatrixNode.Next;
                }
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
            
            //Scrolling Background Update stuff
            Vector2 speed = new Vector2(0, SCROLL_SPEED);
            Vector2 dir = new Vector2(0, 1);        //background movement is in Y direction only
            Vector2 distanceTravelled = speed * dir * (float)gameTime.ElapsedGameTime.TotalSeconds;
            myBackground.Position += distanceTravelled;
            myBackground2.Position += distanceTravelled;

            // Perform garbage collection on obstacles
            garbageCollectObstacles();

            // Update obstacles based on scrolling background
            LinkedListNode<List<Obstacle>> obstacleMatrixNode = obstacleMatrix.First;
            while (obstacleMatrixNode != null)
            {
                // Iterate through the obstacles in the given row
                for (int i = 0; i < obstacleMatrixNode.Value.Count; i++)
                {
                    obstacleMatrixNode.Value[i].Position += distanceTravelled;
                }
                obstacleMatrixNode = obstacleMatrixNode.Next;
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

            turretReticle.Update(gameTime);

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();

            // Draw scrolling background
            myBackground.Draw(this.spriteBatch);
            myBackground2.Draw(this.spriteBatch);

            // Draw obstacles
            LinkedListNode<List<Obstacle>> obstacleMatrixNode = obstacleMatrix.First;
            while (obstacleMatrixNode != null)
            {
                // Iterate through the obstacles in the given row
                for (int i = 0; i < obstacleMatrixNode.Value.Count; i++)
                {
                    obstacleMatrixNode.Value[i].Draw(this.spriteBatch);
                }
                obstacleMatrixNode = obstacleMatrixNode.Next;
            }

            // Draw player
            Player1.Draw(this.spriteBatch);

            turretReticle.Draw(this.spriteBatch);

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}

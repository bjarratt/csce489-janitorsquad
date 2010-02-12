using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace GameStateManagement
{
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

    struct EnemyStats
    {
        public float chaseDistance;
        public float caughtDistance;
        public float hysteresis;
        public float maxSpeed;
        public float turnSpeed;
    }

    class Enemy : Sprite
    {
        // Enemy collision detection is handled by a list of circles, starting from the head
        private static int[] COLLISION_RADII = { 10, 10, 10, 20, 30, 20, 20, 20 };
        private static int COLLISION_RADII_LEN = COLLISION_RADII.Count<int>();

        private const int COLLISION_START_X = 140;
        private const int COLLISION_END_X = 15;
        private const int COLLISION_Y = 25;

        private Vector2 firstCollisionCircleCenter;
        private Vector2 lastCollisionCircleCenter;

        private Vector2 toLastCircle; // Vector pointing to lastCollisionCircleCenter

        private bool boundingCirclesUpdated;

        private int currentAnimationFrame; // Determines when to move the dino's legs

        private List<string> runningAnimationFrames;

        private const int FRAMES_IN_RUN_CYCLE = 24; // Must be divisible by the number of animation frames (four, in this case)

        public EnemyStats stats;

        public EnemyAiState state;

        public Enemy(string assetName, EnemyStats stats)
        {
            this.AssetName = assetName;
            this.stats = stats;
            this.state = EnemyAiState.Chasing;

            this.boundingCirclesUpdated = false;

            this.firstCollisionCircleCenter = new Vector2();
            this.lastCollisionCircleCenter = new Vector2();
            this.toLastCircle = new Vector2();

            this.currentAnimationFrame = 0;
            runningAnimationFrames = new List<string>();
            runningAnimationFrames.Add("raptor");
            runningAnimationFrames.Add("raptor_left");
            runningAnimationFrames.Add("raptor");
            runningAnimationFrames.Add("raptor_right");
        }

        public void LoadContent(ContentManager theContentManager)
        {
            base.LoadContent(theContentManager, this.AssetName);
            this.Scale = 1.0f;
            this.is_projectile = false;
            this.boundingCirclesUpdated = false;
        }

        public void Update(GameTime theGameTime, Vector2 jeepPosition, ContentManager theContentManager)
        {
            if (this.currentAnimationFrame % (FRAMES_IN_RUN_CYCLE / runningAnimationFrames.Count) == 0)
            {
                this.AssetName = runningAnimationFrames[this.currentAnimationFrame / (FRAMES_IN_RUN_CYCLE / runningAnimationFrames.Count)];
                this.LoadContent(theContentManager, this.AssetName);
            }

            this.currentAnimationFrame = (this.currentAnimationFrame + 1) % FRAMES_IN_RUN_CYCLE;

            // First we have to use the current state to decide what the thresholds are
            // for changing state, as described in the doc.

            float enemyChaseThreshold = this.stats.chaseDistance;
            float enemyCaughtThreshold = this.stats.caughtDistance;
            // if the enemy is idle, he prefers to stay idle. we do this by making the
            // chase distance smaller, so the enemy will be less likely to begin chasing
            // the jeep.
            if (this.state == EnemyAiState.Wander)
            {
                enemyChaseThreshold -= this.stats.hysteresis / 2;
            }
            // similarly, if the enemy is active, he prefers to stay active. we
            // accomplish this by increasing the range of values that will cause the
            // enemy to go into the active state.
            else if (this.state == EnemyAiState.Chasing)
            {
                enemyChaseThreshold += this.stats.hysteresis / 2;
                enemyCaughtThreshold -= this.stats.hysteresis / 2;
            }
            // the same logic is applied to the finished state.
            else if (this.state == EnemyAiState.Caught)
            {
                enemyCaughtThreshold += this.stats.hysteresis / 2;
            }

            // Second, now that we know what the thresholds are, we compare the enemy's 
            // distance from the jeep against the thresholds to decide what the enemy's
            // current state is.
            float distanceFromJeep = Vector2.Distance(this.Position, jeepPosition) - this.Size.Width;
            if (distanceFromJeep > enemyChaseThreshold)
            {
                // if the enemy is far away from the jeep, it should idle
                this.state = EnemyAiState.Wander;
            }
            else if (distanceFromJeep > enemyCaughtThreshold)
            {
                this.state = EnemyAiState.Chasing;
            }
            else
            {
                this.state = EnemyAiState.Caught;
            }

            // Third, once we know what state we're in, act on that state.
            float currentEnemySpeed;
            if (this.state == EnemyAiState.Chasing)
            {
                // the enemy wants to chase the jeep, so it will just use the TurnToFace
                // function to turn towards the jeep's position. Then, when the enemy
                // moves forward, he will chase the jeep.
                this.rotation = TurnToFace(jeepPosition);
                currentEnemySpeed = this.stats.maxSpeed;
            }
            else if (this.state == EnemyAiState.Wander)
            {
                // call the wander function for the enemy
                /*Wander(enemyPosition, ref enemyWanderDirection, ref enemyOrientation,
                    EnemyTurnSpeed);
                currentEnemySpeed = .25f * MaxEnemySpeed;*/
                currentEnemySpeed = 0.0f;
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
                (float)Math.Cos(this.rotation), (float)Math.Sin(this.rotation));
            this.Position += heading * currentEnemySpeed;

            this.boundingCirclesUpdated = false;
        }

        public bool collidesWith(float x, float y, int radius)
        {
            // If necessary, the rotated location of the head and tail collision circles are calculated.
            // This allows the location of the middle circles to be extrapolated from their radii.
            if (!this.boundingCirclesUpdated)
            {
                double cosAngle = Math.Cos(this.rotation);
                double sinAngle = Math.Sin(this.rotation);
                double column3row1 = this.Position.X + (this.Position.Y * sinAngle) - (this.Position.X * cosAngle);
                double column3row2 = this.Position.Y - (this.Position.Y * cosAngle) - (this.Position.X * sinAngle);

                this.firstCollisionCircleCenter.X = (float)((cosAngle * (Position.X + COLLISION_START_X)) - (sinAngle * (Position.Y + COLLISION_Y)) + column3row1);
                this.firstCollisionCircleCenter.Y = (float)((sinAngle * (Position.X + COLLISION_START_X)) + (cosAngle * (Position.Y + COLLISION_Y)) + column3row2);

                this.lastCollisionCircleCenter.X = (float)((cosAngle * (Position.X + COLLISION_END_X)) - (sinAngle * (Position.Y + COLLISION_Y)) + column3row1);
                this.lastCollisionCircleCenter.Y = (float)((sinAngle * (Position.X + COLLISION_END_X)) + (cosAngle * (Position.Y + COLLISION_Y)) + column3row2);

                this.toLastCircle = this.lastCollisionCircleCenter - this.firstCollisionCircleCenter;
                this.toLastCircle.Normalize();

                this.boundingCirclesUpdated = true;
            }

            Vector2 currentPosition = this.firstCollisionCircleCenter;
            double distanceSquared;
            double radiiSquared;

            // Start from the head and move towards the tail
            for (int i = 0; i < COLLISION_RADII_LEN - 1; i++)
            {
                distanceSquared = (currentPosition.X - x) * (currentPosition.X - x) + (currentPosition.Y - y) * (currentPosition.Y - y);
                radiiSquared = (COLLISION_RADII[i] + radius) * (COLLISION_RADII[i] + radius);

                if (radiiSquared > distanceSquared)
                {
                    return true; // Bullet hit enemy
                }

                currentPosition += (this.toLastCircle * (COLLISION_RADII[i] + COLLISION_RADII[i+1]));
            }

            // Last circle done separately to avoid an out-of-range exception by calculating currentPosition
            distanceSquared = (currentPosition.X - x) * (currentPosition.X - x) + (currentPosition.Y - y) * (currentPosition.Y - y);
            radiiSquared = (COLLISION_RADII[COLLISION_RADII_LEN - 1] + radius) * (COLLISION_RADII[COLLISION_RADII_LEN - 1] + radius);

            if (radiiSquared > distanceSquared)
            {
                return true; // Bullet hit enemy
            }

            return false; // Bullet did not hit enemy
        }

        public override void Draw(SpriteBatch theSpriteBatch)
        {
            base.Draw(theSpriteBatch);
        }

        /// <summary>
        /// Calculates the angle that an object should face, given its position, its
        /// target's position, its current angle, and its maximum turning speed.
        /// </summary>
        private float TurnToFace(Vector2 faceThis)
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
            float x = faceThis.X - this.Position.X;
            float y = faceThis.Y - this.Position.Y;

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
            float difference = WrapAngle(desiredAngle - this.rotation);

            // clamp that between -turnSpeed and turnSpeed.
            difference = MathHelper.Clamp(difference, -this.stats.turnSpeed, this.stats.turnSpeed);

            // so, the closest we can get to our target is currentAngle + difference.
            // return that, using WrapAngle again.
            return this.WrapAngle(this.rotation + difference);
        }

        /// <summary>
        /// Returns the angle expressed in radians between -Pi and Pi.
        /// <param name="radians">the angle to wrap, in radians.</param>
        /// <returns>the input value expressed in radians from -Pi to Pi.</returns>
        /// </summary>
        private float WrapAngle(float radians)
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

        /*
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
        }*/
    }
}

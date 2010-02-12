using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameStateManagement
{
    class DirtCloudPS : ParticleSystem
    {
        public DirtCloudPS(Game game, int howManyEffects, SpriteBatch spriteBatch)
            : base(game, howManyEffects, spriteBatch)
        {
            base.scroll_speed = 300;
        }

        /// <summary>
        /// Set up the constants 
        /// </summary>
        protected override void InitializeConstants()
        {
            textureFilename = "dirtsmoke";

            // pretty slow
            minInitialSpeed = 20;
            maxInitialSpeed = 120;

            minAcceleration = 0;
            maxAcceleration = 0;

            minLifetime = 0.3f;
            maxLifetime = 0.5f;

            minScale = 0.2f;
            maxScale = 0.3f;

            minNumParticles = 8;
            maxNumParticles = 10;

            minRotationSpeed = -MathHelper.PiOver4;
            maxRotationSpeed = MathHelper.PiOver4;

            // alpha blending is very good at creating smoke effects.
            spriteBlendMode = SpriteBlendMode.AlphaBlend;

            DrawOrder = AlphaBlendDrawOrder;
        }

        /// <summary>
        /// PickRandomDirection is overriden so that we can make the particles always 
        /// move have an initial velocity pointing down.
        /// </summary>
        /// <returns>a random direction which points down.</returns>
        protected override Vector2 PickRandomDirection()
        {
            // Point the particles somewhere between 260 and 280 degrees.
            // tweak this to make the dirt have more or less spread.
            float radians = GameplayScreen.RandomBetween(
                MathHelper.ToRadians(200), MathHelper.ToRadians(320));

            Vector2 direction = Vector2.Zero;
            
            direction.X = (float)Math.Cos(radians);
            direction.Y = -(float)Math.Sin(radians);
            return direction;
        }

        /// <summary>
        /// InitializeParticle is overridden to add the appearance of wind.
        /// </summary>
        /// <param name="p">the particle to set up</param>
        /// <param name="where">where the particle should be placed</param>
        protected override void InitializeParticle(Particle p, Vector2 where)
        {
            base.InitializeParticle(p, where);

            // the base is mostly good, but we want to simulate a little bit of wind
            // heading down.
            p.Acceleration.Y += GameplayScreen.RandomBetween(10, 50);
        }
    }
}

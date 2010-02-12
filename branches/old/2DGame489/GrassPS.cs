using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace _2DGame489
{
    class GrassPS : ParticleSystem
    {
        public GrassPS(Game1 game, int howManyEffects)
            : base(game, howManyEffects)
        {
            base.scroll_speed = 300;
        }

        /// <summary>
        /// Set up the constants 
        /// </summary>
        protected override void InitializeConstants()
        {
            textureFilename = "grass";

            minInitialSpeed = 20;
            maxInitialSpeed = 120;

            minAcceleration = 0;
            maxAcceleration = 0;

            minLifetime = 0.1f;
            maxLifetime = 0.3f;

            minScale = 0.05f;
            maxScale = 0.2f;

            minNumParticles = 4;
            maxNumParticles = 6;

            minRotationSpeed = -MathHelper.PiOver4 / 2;
            maxRotationSpeed = MathHelper.PiOver4 / 2;

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
            float radians = Game1.RandomBetween(
                MathHelper.ToRadians(260), MathHelper.ToRadians(280));

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

            p.Acceleration.Y += Game1.RandomBetween(10, 50);
        }
    }
}

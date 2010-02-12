using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DinoEscape
{
    class RockPS : ParticleSystem
    {
        public RockPS(Game game, int howManyEffects, SpriteBatch spriteBatch)
            : base(game, howManyEffects, spriteBatch)
        {
            base.scroll_speed = 200;
        }

        /// <summary>
        /// Set up the constants 
        /// </summary>
        protected override void InitializeConstants()
        {
            textureFilename = "rocksmoke";

            //smoke is slow
            minInitialSpeed = 20;
            maxInitialSpeed = 200;

            //accelerates away from explosion
            minAcceleration = -10;
            maxAcceleration = -50;

            //longer lived
            minLifetime = 1.0f;
            maxLifetime = 1.5f;

            minScale = 0.2f;
            maxScale = 0.5f;

            //thin smoke
            minNumParticles = 6;
            maxNumParticles = 8;

            //average rotation
            minRotationSpeed = -MathHelper.PiOver4;
            maxRotationSpeed = MathHelper.PiOver4;

            spriteBlendMode = SpriteBlendMode.AlphaBlend;

            DrawOrder = AlphaBlendDrawOrder;
        }

        /// <summary>
        /// InitializeParticle is overridden to add the appearance of wind.
        /// </summary>
        /// <param name="p">the particle to set up</param>
        /// <param name="where">where the particle should be placed</param>
        protected override void InitializeParticle(Particle p, Vector2 where)
        {
            base.InitializeParticle(p, where);

            p.Acceleration.Y += GameplayScreen.RandomBetween(10, 50);
        }
    }
}


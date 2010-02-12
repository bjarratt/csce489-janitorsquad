using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DinoEscape
{
    class ExplosionSmokePS : ParticleSystem
    {
        public ExplosionSmokePS(Game game, int howManyEffects, SpriteBatch spriteBatch)
            : base(game, howManyEffects, spriteBatch)
        {
            base.scroll_speed = 200;    //give it downward drift
        }

        /// <summary>
        /// Set up the constants 
        /// </summary>
        protected override void InitializeConstants()
        {
            textureFilename = "smoke";

            // slower than the explosion
            minInitialSpeed = 20;
            maxInitialSpeed = 200;

            // negative so it moves away from initial velocity and slows down
            minAcceleration = -10;
            maxAcceleration = -50;

            // smoke lives longer
            minLifetime = 1.0f;
            maxLifetime = 2.5f;

            minScale = 1.0f;
            maxScale = 2.0f;

            minNumParticles = 5;
            maxNumParticles = 10;

            minRotationSpeed = -MathHelper.PiOver4;
            maxRotationSpeed = MathHelper.PiOver4;

            spriteBlendMode = SpriteBlendMode.AlphaBlend;

            DrawOrder = AlphaBlendDrawOrder;
        }

        protected override void InitializeParticle(Particle p, Vector2 where)
        {
            base.InitializeParticle(p, where);

            p.Acceleration = -p.Velocity / p.Lifetime;
        }
    }
}

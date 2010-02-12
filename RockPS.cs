using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameStateManagement
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

            // pretty slow
            minInitialSpeed = 20;
            maxInitialSpeed = 200;

            minAcceleration = -10;
            maxAcceleration = -50;

            minLifetime = 1.0f;
            maxLifetime = 1.5f;

            minScale = 0.2f;
            maxScale = 0.5f;

            minNumParticles = 6;
            maxNumParticles = 8;

            minRotationSpeed = -MathHelper.PiOver4;
            maxRotationSpeed = MathHelper.PiOver4;

            // alpha blending is very good at creating smoke effects.
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

            // the base is mostly good, but we want to simulate a little bit of wind
            // heading down.
            p.Acceleration.Y += GameplayScreen.RandomBetween(10, 50);
        }
    }
}


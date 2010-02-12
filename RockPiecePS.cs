using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameStateManagement
{
    class RockPiecePS : ParticleSystem
    {
        public RockPiecePS(Game game, int howManyEffects, SpriteBatch spriteBatch)
            : base(game, howManyEffects, spriteBatch)
        {
            base.scroll_speed = 300;
        }

        /// <summary>
        /// Set up the constants 
        /// </summary>
        protected override void InitializeConstants()
        {
            textureFilename = "rockpiece";

            minInitialSpeed = 100;
            maxInitialSpeed = 500;

            minAcceleration = 0;
            maxAcceleration = 0;

            minLifetime = 0.25f;
            maxLifetime = 0.4f;

            minScale = 0.5f;
            maxScale = 1.2f;

            minNumParticles = 15;
            maxNumParticles = 20;

            minRotationSpeed = -MathHelper.PiOver2;
            maxRotationSpeed = MathHelper.PiOver2;

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


﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DinoEscape
{
    class MuzzleFlashPS : ParticleSystem
    {
        public MuzzleFlashPS(Game game, int howManyEffects, SpriteBatch spriteBatch)
            : base(game, howManyEffects, spriteBatch)
        {
            base.scroll_speed = 0;
        }

        /// <summary>
        /// Set up the constants 
        /// </summary>
        protected override void InitializeConstants()
        {
            textureFilename = "explosion";

            //stationary effect
            minInitialSpeed = 0;
            maxInitialSpeed = 0;

            minAcceleration = 0;
            maxAcceleration = 0;

            //extremely short lived... it's a flash
            minLifetime = 0.05f;
            maxLifetime = 0.1f;

            //extremely small version of the picture used to make fire effects
            minScale = .08f;
            maxScale = .1f;

            //very few particles since they are all on top of each other
            minNumParticles = 3;
            maxNumParticles = 4;

            minRotationSpeed = 0;
            maxRotationSpeed = 0;

            spriteBlendMode = SpriteBlendMode.Additive;

            DrawOrder = AdditiveDrawOrder;
        }

        protected override void InitializeParticle(Particle p, Vector2 where)
        {
            base.InitializeParticle(p, where);

            p.Acceleration = -p.Velocity / p.Lifetime;
        }
    }
}

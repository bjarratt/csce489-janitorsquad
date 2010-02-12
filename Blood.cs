using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

namespace GameStateManagement
{
    class BloodPS : ParticleSystem
    {
        public BloodPS(Game game, int howManyEffects, SpriteBatch spriteBatch)
            : base(game, howManyEffects, spriteBatch)
        {
            base.scroll_speed = 300;
        }

        //sound stuff
        AudioEngine audioEngine;
        WaveBank waveBank;
        SoundBank soundBank;

        /// <summary>
        /// Set up the constants 
        /// </summary>
        protected override void InitializeConstants()
        {
            textureFilename = "blood";

            // pretty slow
            minInitialSpeed = 100;
            maxInitialSpeed = 500;

            minAcceleration = 0;
            maxAcceleration = 0;

            minLifetime = 0.25f;
            maxLifetime = 0.5f;

            minScale = 1.0f;
            maxScale = 1.5f;

            minNumParticles = 25;
            maxNumParticles = 30;

            minRotationSpeed = 0;
            maxRotationSpeed = 0;

            // alpha blending is very good at creating smoke effects.
            spriteBlendMode = SpriteBlendMode.AlphaBlend;

            //load audio
            audioEngine = new AudioEngine("Content/gameAudio.xgs");
            waveBank = new WaveBank(audioEngine, "Content/Wave Bank.xwb");
            soundBank = new SoundBank(audioEngine, "Content/Sound Bank.xsb");

            DrawOrder = AlphaBlendDrawOrder;
        }

        /// <summary>
        /// InitializeParticle is overridden to add the appearance of wind.
        /// </summary>
        /// <param name="p">the particle to set up</param>
        /// <param name="where">where the particle should be placed</param>
        protected override void InitializeParticle(Particle p, Vector2 where)
        {
            soundBank.PlayCue("dino_death");
            base.InitializeParticle(p, where);

            // the base is mostly good, but we want to simulate a little bit of wind
            // heading down.
            p.Acceleration.Y += GameplayScreen.RandomBetween(10, 50);
        }
    }
}


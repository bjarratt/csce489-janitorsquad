using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

namespace DinoEscape
{
    class LogPS : ParticleSystem
    {
        public LogPS(Game game, int howManyEffects, SpriteBatch spriteBatch)
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
            textureFilename = "logshard";

            minInitialSpeed = 100;
            maxInitialSpeed = 500;

            minAcceleration = 0;
            maxAcceleration = 0;

            minLifetime = 0.3f;
            maxLifetime = 0.5f;

            minScale = 0.6f;
            maxScale = 1.0f;

            minNumParticles = 20;
            maxNumParticles = 25;

            minRotationSpeed = -MathHelper.Pi * 4;
            maxRotationSpeed = MathHelper.Pi * 4;

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
            soundBank.PlayCue("log_break");
            base.InitializeParticle(p, where);

            // the base is mostly good, but we want to simulate a little bit of wind
            // heading down.
            p.Acceleration.Y += GameplayScreen.RandomBetween(10, 50);
        }
    }
}


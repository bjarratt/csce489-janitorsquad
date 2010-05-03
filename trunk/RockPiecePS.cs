using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

namespace DinoEscape
{
    class RockPiecePS : ParticleSystem
    {
        public RockPiecePS(Game game, int howManyEffects, SpriteBatch spriteBatch)
            : base(game, howManyEffects, spriteBatch)
        {
            base.scroll_speed = 300;
        }

        /*
        //sound stuff
        AudioEngine audioEngine;
        WaveBank waveBank;
        SoundBank soundBank;
        */

        /// <summary>
        /// Set up the constants 
        /// </summary>
        protected override void InitializeConstants()
        {
            textureFilename = "rockpiece";

            //rock explosion... high variance
            minInitialSpeed = 100;
            maxInitialSpeed = 500;

            minAcceleration = 0;
            maxAcceleration = 0;

            //short lived
            minLifetime = 0.25f;
            maxLifetime = 0.4f;

            //a bit of variance in size
            minScale = 0.5f;
            maxScale = 1.2f;

            //lots of particles
            minNumParticles = 15;
            maxNumParticles = 20;

            //more rotation
            minRotationSpeed = -MathHelper.PiOver2;
            maxRotationSpeed = MathHelper.PiOver2;

            spriteBlendMode = SpriteBlendMode.AlphaBlend;

            /*
            //load audio
            audioEngine = new AudioEngine("Content/gameAudio.xgs");
            waveBank = new WaveBank(audioEngine, "Content/Wave Bank.xwb");
            soundBank = new SoundBank(audioEngine, "Content/Sound Bank.xsb");
            */

            DrawOrder = AlphaBlendDrawOrder;
        }

        /// <summary>
        /// InitializeParticle is overridden to add the appearance of wind.
        /// </summary>
        /// <param name="p">the particle to set up</param>
        /// <param name="where">where the particle should be placed</param>
        protected override void InitializeParticle(Particle p, Vector2 where)
        {
            //soundBank.PlayCue("rock_smash");
            GameplayScreen.soundController.Play("rock_smash");
            base.InitializeParticle(p, where);

            p.Acceleration.Y += GameplayScreen.RandomBetween(10, 50);
        }
    }
}


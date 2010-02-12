using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

namespace DinoEscape
{
    class ExplosionPS : ParticleSystem
    {
        public ExplosionPS(Game game, int howManyEffects, SpriteBatch spriteBatch)
            : base(game, howManyEffects, spriteBatch)
        {
            base.scroll_speed = 0;
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
            textureFilename = "explosion";

            //Lots of variance with high speed for explosions 
            minInitialSpeed = 40;
            maxInitialSpeed = 500;

            //no acceleration
            minAcceleration = 0;
            maxAcceleration = 0;

            //short to medium lifetime
            minLifetime = .5f;
            maxLifetime = 1.0f;

            //lots of variance in size to make each one unpredictable in shape
            minScale = .3f;
            maxScale = 1.0f;

            //lots of particles
            minNumParticles = 20;
            maxNumParticles = 25;

            //moderate rotation
            minRotationSpeed = -MathHelper.PiOver4;
            maxRotationSpeed = MathHelper.PiOver4;

            //additive blending for fire effects
            spriteBlendMode = SpriteBlendMode.Additive;

            //load audio
            audioEngine = new AudioEngine("Content/gameAudio.xgs");
            waveBank = new WaveBank(audioEngine, "Content/Wave Bank.xwb");
            soundBank = new SoundBank(audioEngine, "Content/Sound Bank.xsb");

            DrawOrder = AdditiveDrawOrder;
        }

        protected override void InitializeParticle(Particle p, Vector2 where)
        {
            soundBank.PlayCue("explosion");
            base.InitializeParticle(p, where);
            
            p.Acceleration = -p.Velocity / p.Lifetime;
        }
    }
}

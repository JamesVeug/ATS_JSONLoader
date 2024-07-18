using System.Collections.Generic;
using TinyJson;

namespace ATS_JSONLoader.Sounds
{
    public class SoundCollection : IInitializable
    {
        public List<Sound> sounds = new List<Sound>();

        public void Initialize()
        {
            sounds = new List<Sound>();
        }
    }
    
    public class RacialSounds : IInitializable
    {
        public SoundCollection PositiveSounds = new SoundCollection();
        public SoundCollection NegativeSounds = new SoundCollection();
        public SoundCollection NeutralSounds = new SoundCollection();
        
        public void Initialize()
        {
            PositiveSounds = new SoundCollection();
            NegativeSounds = new SoundCollection();
            NeutralSounds = new SoundCollection();
        }
    }

    public class Sound
    {
        public string soundPath;
        public float? volume = 1f;
    }
}
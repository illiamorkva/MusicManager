using System.Collections.Generic;

namespace MusicManager
{
    class MusicID3Tag
    {
        public byte[] Tagid = new byte[3];
        public byte[] Title = new byte[30];
        public byte[] Artist = new byte[30];
        public byte[] Album = new byte[30];
        public byte[] Year = new byte[4];
        public byte[] Genre = new byte[1];
        public byte[] Duration = new byte[1];
    }

    public class Sound
    {
        public string Name { get; set; }
        public string Author { get; set; }
        public string Genre { get; set; }
        public string Path { get; set; }
        public string Format { get; set; }
        public string Year { get; set; }
    }

    class Music
    {
        private List<Sound> collSounds;

        public Music()
        {
            collSounds = new List<Sound>();
        }

        public void AddMusic(Sound sound)
        {
            collSounds.Add(sound);
        }

        public List<Sound> GetSounds
        {
            get
            {
                return collSounds;
            }

        }

        public void ClearCollection()
        {
            collSounds.Clear();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace OpenSlots
{
    public class MediaLibrary
    {
#pragma warning disable CA2211 // Non-constant fields should not be visible
        public static Dictionary<string, SpriteFont> Fonts = new();
        public static Dictionary<string, Color> Colors = new();
        public static Dictionary<string, SoundEffect> SoundFX = new();
        public static Dictionary<string, Texture2D> Backgrounds = new();
        public static Dictionary<Reel.Tokens, Texture2D> Tokens = new();
        public static Dictionary<string, Texture2D> Images = new();
#pragma warning restore CA2211 // This is the stupidest message I've ever had to suppress.
    }
}

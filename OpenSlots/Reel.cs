using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ml = OpenSlots.MediaLibrary;

namespace OpenSlots
{
    public class Reel
    {
        public Rectangle Area;
        public Screen1 Parent;
        public int WheelIndex;

        public enum Tokens
        {
            Seven,
            TripleSeven,
            Cherries,
            Bar,
            DoubleBar,
            TripleBar,
            LuckyDuck
        }
        public List<Tokens> Wheel;
        public float WheelRotation;
        public int UpperIndex;
        public int MiddleIndex;
        public int LowerIndex;
        public bool WheelSpinning;
        private float decelerationSpeed = 0.08f;
        private float spinVelocity = 0.17f;

        public Reel(Rectangle loc, List<Tokens> WheelLayout, Screen1 Game, int WheelNumber)
        {
            Wheel = WheelLayout;
            Area = loc;
            WheelSpinning = false;
            WheelRotation = 0.0f;
            MiddleIndex = 0;
            Parent = Game;
            WheelIndex = WheelNumber;
        }

        public void Update()
        {
            if (WheelSpinning)
            {
                if (Parent.LongSpinStarted)
                    spinVelocity += 0.001f;
                else
                    spinVelocity = 0.17f;
                // Decrement the wheel rotation to scroll in the opposite direction
                WheelRotation -= spinVelocity + WheelIndex * 0.01f;

                // Normalize WheelRotation
                while (WheelRotation < 0)
                    WheelRotation += Wheel.Count;

                UpperIndex = (int)Math.Floor(WheelRotation) - 1;
                if (UpperIndex >= Wheel.Count)
                    UpperIndex = 0;
                if (UpperIndex == -1)
                    UpperIndex = Wheel.Count - 1;

                MiddleIndex = (int)Math.Floor(WheelRotation);
                if (MiddleIndex >= Wheel.Count)
                    MiddleIndex = 0;

                LowerIndex = (int)Math.Floor(WheelRotation) + 1;
                if (LowerIndex >= Wheel.Count)
                    LowerIndex = 0;
            }
            else if (WheelRotation % 1 != 0) // Check if wheel needs to be aligned
            {
                spinVelocity = 0.17f;
                // Calculate the fractional part of WheelRotation
                float fractionalPart = WheelRotation % 1;

                // If the fractional part is not negligible, decelerate the wheel to align the token
                if (fractionalPart > decelerationSpeed)
                {
                    WheelRotation -= decelerationSpeed;
                }
                else
                {
                    // Align the wheel with the nearest token
                    WheelRotation = (int)WheelRotation;
                }

                // Update CurrentIndex after deceleration
                UpperIndex = (int)Math.Floor(WheelRotation) - 1;
                if (UpperIndex >= Wheel.Count)
                    UpperIndex = 0;
                if (UpperIndex == -1)
                    UpperIndex = Wheel.Count - 1;

                MiddleIndex = (int)Math.Floor(WheelRotation);
                if (MiddleIndex >= Wheel.Count)
                    MiddleIndex = 0;

                LowerIndex = (int)Math.Floor(WheelRotation) + 1;
                if (LowerIndex >= Wheel.Count)
                    LowerIndex = 0;
            }
        }

        public void Draw(SpriteBatch gfx)
        {
            // Define the spacing between tokens.
            float tokenSpacing = Area.Height * 0.3f; // Adjusted value for closer tokens

            // Adjust yPos to account for tokenSpacing and make the transition smooth.
            // The formula ensures yPos is consistent as CurrentIndex changes.
            float yPos = Area.Y + tokenSpacing * (MiddleIndex - WheelRotation) + (Area.Height / 3);

            // Draw each token starting from the current index
            for (int i = -1; i < Wheel.Count; i++)
            {
                int tokenIndex = (MiddleIndex + i) % Wheel.Count;
                Rectangle destinationRect = new Rectangle(
                    (int)(Area.X - (150 * (spinVelocity - 0.17f))),
                    (int)((yPos + (i * tokenSpacing)) - (150 * (spinVelocity - 0.17f))), // Adjusted Y position for this token
                    (int)(Area.Width + (300 * (spinVelocity - 0.17f))),
                    (int)(Area.Width + (300 * (spinVelocity - 0.17f)))); // Assuming square tokens

                // Draw the token using its texture
                if (tokenIndex == -1)
                    tokenIndex = Wheel.Count - 1;
                bool draw = true;
                if (Parent.WinningLines.Contains(1))
                {
                    if (i == -1)
                        draw = true ? true : false;
                    if (i == 0)
                        draw = (DateTime.Now - Parent.LineFlash) > TimeSpan.FromMilliseconds(550);
                    if (i == 1)
                        draw = true ? true : false;
                }
                if (Parent.WinningLines.Contains(2))
                {
                    if (i == -1)
                        draw = (DateTime.Now - Parent.LineFlash) > TimeSpan.FromMilliseconds(600);
                    if (i == 0)
                        draw = true ? true : false;
                    if (i == 1)
                        draw = true ? true : false;
                }
                if (Parent.WinningLines.Contains(3))
                {
                    if (i == -1)
                        draw = true ? true : false;
                    if (i == 0)
                        draw = true ? true : false;
                    if (i == 1)
                            draw = (DateTime.Now - Parent.LineFlash) > TimeSpan.FromMilliseconds(650);
                }
                if (Parent.WinningLines.Contains(4))
                {
                    if (WheelIndex == 1)
                    {
                        if (i == -1)
                            draw = true ? true : false;    
                        if (i == 0)
                            draw = true ? true : false;
                        if (i == 1)
                            draw = (DateTime.Now - Parent.LineFlash) > TimeSpan.FromMilliseconds(700);
                    }
                    if (WheelIndex == 2)
                    {
                        if (i == -1)
                            draw = true ? true : false;
                        if (i == 0)
                            draw = (DateTime.Now - Parent.LineFlash) > TimeSpan.FromMilliseconds(700);
                        if (i == 1)
                            draw = true ? true : false;
                    }
                    if (WheelIndex == 3)
                    {
                        if (i == -1)
                            draw = (DateTime.Now - Parent.LineFlash) > TimeSpan.FromMilliseconds(700);
                        if (i == 0)
                            draw = true ? true : false;
                        if (i == 1)
                            draw = true ? true : false;
                    }
                }
                if (Parent.WinningLines.Contains(5))
                {
                    if (WheelIndex == 3)
                    {
                        if (i == -1)
                            draw = true ? true : false;
                        if (i == 0)
                            draw = true ? true : false;
                        if (i == 1)
                            draw = (DateTime.Now - Parent.LineFlash) > TimeSpan.FromMilliseconds(700);
                    }
                    if (WheelIndex == 2)
                    {
                        if (i == -1)
                            draw = true ? true : false;
                        if (i == 0)
                            draw = (DateTime.Now - Parent.LineFlash) > TimeSpan.FromMilliseconds(700);
                        if (i == 1)
                            draw = true ? true : false;
                    }
                    if (WheelIndex == 1)
                    {
                        if (i == -1)
                            draw = (DateTime.Now - Parent.LineFlash) > TimeSpan.FromMilliseconds(700);
                        if (i == 0)
                            draw = true ? true : false;
                        if (i == 1)
                            draw = true ? true : false;
                    }
                }

                if (Wheel[tokenIndex] == Tokens.LuckyDuck)
                {
                    double time = DateTime.Now.Millisecond / 1000.0; // Convert milliseconds to seconds
                    double wave = Math.Cos(2 * Math.PI * time); // Complete a cycle every second

                    // Adjust the wave's amplitude and offset to suit your needs
                    int amplitude = 25; // Example amplitude value
                    wave *= amplitude; // Scale the wave

                    // Apply the wave pattern to modify the rectangle's properties
                    destinationRect.X -= (int)(wave / 2);
                    destinationRect.Y -= (int)(wave / 2);
                    destinationRect.Width += (int)wave;
                    destinationRect.Height += (int)wave;
                }

                if (draw)
                    gfx.Draw(ml.Tokens[Wheel[tokenIndex]],
                        destinationRect,
                        Color.White);
            }
            gfx.DrawString(ml.Fonts["debug"], Enum.GetName(typeof(Reel.Tokens), Wheel[UpperIndex]), new Vector2(Area.X, Area.Y), Color.Red);
            gfx.DrawString(ml.Fonts["debug"], Enum.GetName(typeof(Reel.Tokens), Wheel[MiddleIndex]), new Vector2(Area.X, Area.Y+20), Color.Red);
            gfx.DrawString(ml.Fonts["debug"], Enum.GetName(typeof(Reel.Tokens), Wheel[LowerIndex]), new Vector2(Area.X, Area.Y+40), Color.Red);

        }
    }
}

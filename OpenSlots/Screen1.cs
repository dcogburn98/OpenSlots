using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Screens;
using MonoGame;

using ml = OpenSlots.MediaLibrary;

namespace OpenSlots
{
    public class Screen1 : GameScreen
    {
        public enum GameState
        {
            InGame,
            SmallWinAnimation,
            BigWinAnimation
        }
        public GameState CurrentState;

        public Random RNG = new Random();
        private GraphicsDeviceManager _graphics;
        private new Game1 Game => (Game1)base.Game;
        private SpriteBatch _spriteBatch => Game._spriteBatch;

        private Texture2D Overlay;
        private SoundEffectInstance SpinSong;
        private SoundEffectInstance FreeSpinBell;
        private SoundEffectInstance DuckSong;
        private SoundEffectInstance LongSpinSong;

        public List<Reel> Reels = new List<Reel>();
        public List<PayTable> PayTables = new List<PayTable>();
        public bool StoppingReels = false;
        public bool AllReelsStopped = true;
        public DateTime SpinStartTime;
        public DateTime SpinStopTime;
        public bool Spinning = false;
        public bool BetUpPressed = false;
        public bool BetDownPressed = false;
        public bool LineUpPressed = false;
        public bool LineDownPressed = false;
        public bool LongSpinStarted = false;

        public int Credits = 1000;
        public int Bet = 10;
        public int Lines = 3;
        public int LastWinAmount = 0;
        public int FreeSpins = 0;
        public bool OnFreeSpin = false;
        public DateTime TimeToNextFreeCredit;

        #region Animation Variables
        public List<int> WinningLines = new List<int>();
        public DateTime LineFlash = DateTime.Now;
        public bool ChangedLines = false;

        public DateTime SmallWinAnimationStart;
        public int SmallWinBoardDelay = 500;
        public int CountUpWinningsDelay = 2500;
        #endregion

        public Screen1(Game game) : base(game)
        {
            _graphics = Game._graphics;
            Content.RootDirectory = "Content";
            //_graphics.ToggleFullScreen();
            Game.IsMouseVisible = true;
        }

        public override void Initialize()
        {
            var bar = Reel.Tokens.Bar;
            var doubleBar = Reel.Tokens.DoubleBar;
            var tripleBar = Reel.Tokens.TripleBar;
            var cherries = Reel.Tokens.Cherries;
            var seven = Reel.Tokens.Seven;
            var tripleSeven = Reel.Tokens.TripleSeven;
            var luckyDuck = Reel.Tokens.LuckyDuck;

            Overlay = Content.Load<Texture2D>("Overlay");

            int ReelWidth = (int)(GraphicsDevice.Viewport.Width / 5.0f);
            int ReelPadding = (int)(GraphicsDevice.Viewport.Width * 0.0833333f);
            Reels.Add(new Reel(new Rectangle(ReelPadding, GraphicsDevice.Viewport.Height / 2, ReelWidth, GraphicsDevice.Viewport.Height / 2), new List<Reel.Tokens> {
                luckyDuck,
                seven,
                bar,
                cherries,
                doubleBar,
                tripleSeven,
                tripleBar,
                bar,
                luckyDuck,
                doubleBar,
                seven,
                bar,
                tripleBar,
                seven,
                bar,
                cherries,
                bar,
                doubleBar,
                cherries,
                tripleBar,
                seven,
                doubleBar,
                tripleBar,
                cherries,
                tripleBar,
                doubleBar,
                cherries,
                bar,
                tripleSeven,
                doubleBar,
                tripleSeven,
                seven,
                doubleBar,
                bar,
                tripleSeven,
                tripleBar,
                doubleBar,
                cherries,
                luckyDuck,
                bar
            }, this, 1));
            Reels.Add(new Reel(new Rectangle(GraphicsDevice.Viewport.Width / 2 - (ReelWidth / 2) - 5, GraphicsDevice.Viewport.Height / 2, ReelWidth, GraphicsDevice.Viewport.Height / 2), new List<Reel.Tokens> {
                luckyDuck,
                tripleBar,
                bar,
                luckyDuck,
                doubleBar,
                seven,
                bar,
                tripleBar,
                seven,
                bar,
                cherries,
                bar,
                doubleBar,
                cherries,
                tripleBar,
                seven,
                bar,
                cherries,
                doubleBar,
                tripleSeven,
                tripleBar,
                seven,
                doubleBar,
                cherries,
                tripleBar,
                doubleBar,
                cherries,
                bar,
                tripleSeven,
                doubleBar,
                tripleSeven,
                seven,
                doubleBar,
                bar,
                tripleSeven,
                tripleBar,
                doubleBar,
                cherries,
                luckyDuck,
                bar
            }, this, 2));
            Reels.Add(new Reel(new Rectangle(GraphicsDevice.Viewport.Width - (ReelWidth + (int)(ReelPadding * 1.125f)), GraphicsDevice.Viewport.Height / 2, ReelWidth, GraphicsDevice.Viewport.Height / 2), new List<Reel.Tokens> {
                luckyDuck,
                tripleBar,
                bar,
                cherries,
                bar,
                doubleBar,
                cherries,
                seven,
                bar,
                cherries,
                doubleBar,
                tripleSeven,
                tripleBar,
                seven,
                doubleBar,
                luckyDuck,
                doubleBar,
                seven,
                bar,
                tripleBar,
                seven,
                bar,
                tripleBar,
                cherries,
                tripleBar,
                doubleBar,
                cherries,
                bar,
                bar,
                tripleSeven,
                doubleBar,
                tripleSeven,
                seven,
                doubleBar,
                tripleSeven,
                tripleBar,
                doubleBar,
                luckyDuck,
                bar,
                cherries
            }, this, 3));

            #region Pay Tables
            PayTables.Add(new PayTable(new Reel.Tokens[] { bar, bar, bar }, 3));
            PayTables.Add(new PayTable(new Reel.Tokens[] { doubleBar, doubleBar, doubleBar }, 10));
            PayTables.Add(new PayTable(new Reel.Tokens[] { tripleBar, tripleBar, tripleBar }, 15));
            PayTables.Add(new PayTable(new Reel.Tokens[] { cherries, cherries, cherries }, 20));
            PayTables.Add(new PayTable(new Reel.Tokens[] { seven, seven, seven }, 30));
            PayTables.Add(new PayTable(new Reel.Tokens[] { tripleSeven, tripleSeven, tripleSeven }, 50));
            PayTables.Add(new PayTable(new Reel.Tokens[] { luckyDuck, luckyDuck, luckyDuck }, 250));

            int anyBarMultiplier = 1;
            PayTables.Add(new PayTable(new Reel.Tokens[] { bar, bar, doubleBar }, anyBarMultiplier));
            PayTables.Add(new PayTable(new Reel.Tokens[] { bar, bar, tripleBar }, anyBarMultiplier));
            PayTables.Add(new PayTable(new Reel.Tokens[] { bar, doubleBar, bar }, anyBarMultiplier));
            PayTables.Add(new PayTable(new Reel.Tokens[] { bar, doubleBar, doubleBar }, anyBarMultiplier));
            PayTables.Add(new PayTable(new Reel.Tokens[] { bar, doubleBar, tripleBar }, anyBarMultiplier));
            PayTables.Add(new PayTable(new Reel.Tokens[] { bar, tripleBar, bar }, anyBarMultiplier));
            PayTables.Add(new PayTable(new Reel.Tokens[] { bar, tripleBar, doubleBar }, anyBarMultiplier));
            PayTables.Add(new PayTable(new Reel.Tokens[] { bar, tripleBar, tripleBar }, anyBarMultiplier));

            PayTables.Add(new PayTable(new Reel.Tokens[] { doubleBar, bar, bar }, anyBarMultiplier));
            PayTables.Add(new PayTable(new Reel.Tokens[] { doubleBar, bar, doubleBar }, anyBarMultiplier));
            PayTables.Add(new PayTable(new Reel.Tokens[] { doubleBar, bar, tripleBar }, anyBarMultiplier));
            PayTables.Add(new PayTable(new Reel.Tokens[] { doubleBar, doubleBar, bar }, anyBarMultiplier));
            PayTables.Add(new PayTable(new Reel.Tokens[] { doubleBar, doubleBar, tripleBar }, anyBarMultiplier));
            PayTables.Add(new PayTable(new Reel.Tokens[] { doubleBar, tripleBar, bar }, anyBarMultiplier));
            PayTables.Add(new PayTable(new Reel.Tokens[] { doubleBar, tripleBar, doubleBar }, anyBarMultiplier));
            PayTables.Add(new PayTable(new Reel.Tokens[] { doubleBar, tripleBar, tripleBar }, anyBarMultiplier));

            PayTables.Add(new PayTable(new Reel.Tokens[] { tripleBar, bar, bar }, anyBarMultiplier));
            PayTables.Add(new PayTable(new Reel.Tokens[] { tripleBar, bar, doubleBar }, anyBarMultiplier));
            PayTables.Add(new PayTable(new Reel.Tokens[] { tripleBar, bar, tripleBar }, anyBarMultiplier));
            PayTables.Add(new PayTable(new Reel.Tokens[] { tripleBar, doubleBar, bar }, anyBarMultiplier));
            PayTables.Add(new PayTable(new Reel.Tokens[] { tripleBar, doubleBar, doubleBar }, anyBarMultiplier));
            PayTables.Add(new PayTable(new Reel.Tokens[] { tripleBar, doubleBar, tripleBar }, anyBarMultiplier));
            PayTables.Add(new PayTable(new Reel.Tokens[] { tripleBar, tripleBar, bar }, anyBarMultiplier));
            PayTables.Add(new PayTable(new Reel.Tokens[] { tripleBar, tripleBar, doubleBar }, anyBarMultiplier));
            #endregion

            TimeToNextFreeCredit = DateTime.Now.AddMinutes(60);
            CurrentState = GameState.InGame;
            base.Initialize();
        }

        public override void LoadContent()
        {

            ml.Tokens.Add(Reel.Tokens.Cherries, Content.Load<Texture2D>("Cherry"));
            ml.Tokens.Add(Reel.Tokens.Seven, Content.Load<Texture2D>("Seven"));
            ml.Tokens.Add(Reel.Tokens.TripleSeven, Content.Load<Texture2D>("TripleSeven"));
            ml.Tokens.Add(Reel.Tokens.Bar, Content.Load<Texture2D>("Bar"));
            ml.Tokens.Add(Reel.Tokens.DoubleBar, Content.Load<Texture2D>("DoubleBar"));
            ml.Tokens.Add(Reel.Tokens.TripleBar, Content.Load<Texture2D>("TripleBar"));
            ml.Tokens.Add(Reel.Tokens.LuckyDuck, Content.Load<Texture2D>("luckyduck"));

            ml.Backgrounds.Add("bg", Content.Load<Texture2D>("bg"));
            ml.Backgrounds.Add("duckpond", Content.Load<Texture2D>("duckpond"));

            ml.Images.Add("bigwinboard", Content.Load<Texture2D>("Big Win Board"));
            ml.Images.Add("smallwinboard", Content.Load<Texture2D>("Small Win Board"));

            ml.Fonts.Add("debug", Content.Load<SpriteFont>("Debug"));
            ml.Fonts.Add("font", Content.Load<SpriteFont>("Font"));
            ml.Fonts.Add("vegas", Content.Load<SpriteFont>("BigWinFont"));

            ml.SoundFX.Add("coin", Content.Load<SoundEffect>("Coin Insert"));
            ml.SoundFX.Add("spin", Content.Load<SoundEffect>("spinsong"));
            ml.SoundFX.Add("bell", Content.Load<SoundEffect>("bell"));
            ml.SoundFX.Add("kaching", Content.Load<SoundEffect>("chaching"));
            ml.SoundFX.Add("clang", Content.Load<SoundEffect>("clang"));
            ml.SoundFX.Add("bigwinsound", Content.Load<SoundEffect>("Big Win Sound"));
            ml.SoundFX.Add("ducksong", Content.Load<SoundEffect>("ducksong"));
            ml.SoundFX.Add("longspin", Content.Load<SoundEffect>("longspin"));
            ml.SoundFX.Add("quack", Content.Load<SoundEffect>("quack"));
            ml.SoundFX.Add("slamstop", Content.Load<SoundEffect>("slamstop"));

            SpinSong = ml.SoundFX["spin"].CreateInstance();
            FreeSpinBell = ml.SoundFX["bell"].CreateInstance();
            DuckSong = ml.SoundFX["ducksong"].CreateInstance();
            LongSpinSong = ml.SoundFX["longspin"].CreateInstance();

            ml.SoundFX["coin"].Play();

            base.LoadContent();
        }

        public override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.FromNonPremultiplied(133, 44, 103, 255));

            _spriteBatch.Begin();
            foreach (Reel reel in Reels)
            {
                reel.Draw(_spriteBatch);
            }

            _spriteBatch.Draw(ml.Backgrounds["bg"], new Rectangle(0, GraphicsDevice.Viewport.Bounds.Height / 2, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Bounds.Height / 2), Color.White);
            _spriteBatch.Draw(ml.Backgrounds["duckpond"], new Rectangle(0, 0, GraphicsDevice.Viewport.Bounds.Width, GraphicsDevice.Viewport.Bounds.Height / 2), Color.White);

            if (FreeSpins > 0 || OnFreeSpin)
            {
                _spriteBatch.Draw(Overlay, GraphicsDevice.Viewport.Bounds, Color.Red);
            }

            _spriteBatch.DrawString(ml.Fonts["font"], Credits.ToString(), new Vector2(GraphicsDevice.Viewport.Width / 6, GraphicsDevice.Viewport.Height - 74), Color.White);
            _spriteBatch.DrawString(ml.Fonts["font"], LastWinAmount.ToString(), new Vector2(GraphicsDevice.Viewport.Width * 0.49f, GraphicsDevice.Viewport.Height - 74), Color.White);
            _spriteBatch.DrawString(ml.Fonts["font"], Lines.ToString(), new Vector2(GraphicsDevice.Viewport.Width / 6 * 4, GraphicsDevice.Viewport.Height - 74), Color.White);
            _spriteBatch.DrawString(ml.Fonts["font"], (Bet * Lines).ToString(), new Vector2(GraphicsDevice.Viewport.Width / 6 * 5, GraphicsDevice.Viewport.Height - 74), Color.White);
            _spriteBatch.DrawString(ml.Fonts["font"], "Time Until Free Credit: " + (DateTime.Now - TimeToNextFreeCredit).ToString("mm':'ss"), new Vector2(10, 10), Color.White);

            _spriteBatch.DrawString(ml.Fonts["debug"], GamePad.GetState(PlayerIndex.One).Buttons.ToString(), new Vector2(10, 70), Color.Red);

            #region Draw the paylines
            if (ChangedLines)
            {
                if ((DateTime.Now - LineFlash).Milliseconds % 1000 > 500)
                {
                    if (Lines > 0)
                    {
                        _spriteBatch.DrawLine(
                            new Vector2(Reels[0].Area.X, Reels[0].Area.Y + (Reels[0].Area.Height / 2)),
                            new Vector2(Reels[1].Area.X + (Reels[1].Area.Width / 2), Reels[1].Area.Y + (Reels[1].Area.Height / 2)),
                            Color.FromNonPremultiplied(0, 128, 200, 255), 12);
                        _spriteBatch.DrawLine(
                            new Vector2(Reels[1].Area.X + (Reels[1].Area.Width / 2), Reels[1].Area.Y + (Reels[1].Area.Height / 2)),
                            new Vector2(Reels[2].Area.X + Reels[2].Area.Width, Reels[2].Area.Y + (Reels[2].Area.Height / 2)),
                            Color.FromNonPremultiplied(0, 128, 200, 255), 12);
                    }
                    if (Lines > 1)
                    {
                        _spriteBatch.DrawLine(
                            new Vector2(Reels[0].Area.X, Reels[0].Area.Y + (Reels[0].Area.Height / 5)),
                            new Vector2(Reels[1].Area.X + (Reels[1].Area.Width / 2), Reels[1].Area.Y + (Reels[1].Area.Height / 5)),
                            Color.DeepPink, 12);
                        _spriteBatch.DrawLine(
                            new Vector2(Reels[1].Area.X + (Reels[1].Area.Width / 2), Reels[1].Area.Y + (Reels[1].Area.Height / 5)),
                            new Vector2(Reels[2].Area.X + Reels[2].Area.Width, Reels[2].Area.Y + (Reels[2].Area.Height / 5)),
                            Color.DeepPink, 12);
                    }
                    if (Lines > 2)
                    {
                        _spriteBatch.DrawLine(
                             new Vector2(Reels[0].Area.X, Reels[0].Area.Y + (Reels[0].Area.Height * 0.8f)),
                             new Vector2(Reels[1].Area.X + (Reels[1].Area.Width / 2), Reels[1].Area.Y + (Reels[1].Area.Height * 0.8f)),
                             Color.Red, 12);
                        _spriteBatch.DrawLine(
                            new Vector2(Reels[1].Area.X + (Reels[1].Area.Width / 2), Reels[1].Area.Y + (Reels[1].Area.Height * 0.8f)),
                            new Vector2(Reels[2].Area.X + Reels[2].Area.Width, Reels[2].Area.Y + (Reels[2].Area.Height * 0.8f)),
                            Color.Red, 12);
                    }
                    if (Lines > 3)
                    {
                        _spriteBatch.DrawLine(
                            new Vector2(Reels[0].Area.X + (Reels[2].Area.Width * 0.2f), Reels[0].Area.Y + (Reels[0].Area.Height * 0.85f)),
                            new Vector2(Reels[1].Area.X + (Reels[1].Area.Width / 2), Reels[1].Area.Y + (Reels[1].Area.Height * 0.5f)),
                            Color.Orange, 12);
                        _spriteBatch.DrawLine(
                            new Vector2(Reels[1].Area.X + (Reels[1].Area.Width / 2), Reels[1].Area.Y + (Reels[1].Area.Height * 0.5f)),
                            new Vector2(Reels[2].Area.X + (Reels[2].Area.Width * 0.7f), Reels[2].Area.Y + (Reels[2].Area.Height * 0.15f)),
                            Color.Orange, 12);
                    }
                    if (Lines > 4)
                    {
                        _spriteBatch.DrawLine(
                            new Vector2(Reels[0].Area.X + (Reels[2].Area.Width * 0.2f), Reels[0].Area.Y + (Reels[0].Area.Height * 0.15f)),
                            new Vector2(Reels[1].Area.X + (Reels[1].Area.Width / 2), Reels[1].Area.Y + (Reels[1].Area.Height * 0.5f)),
                            Color.LimeGreen, 12);
                        _spriteBatch.DrawLine(
                            new Vector2(Reels[1].Area.X + (Reels[1].Area.Width / 2), Reels[1].Area.Y + (Reels[1].Area.Height * 0.5f)),
                            new Vector2(Reels[2].Area.X + (Reels[2].Area.Width * 0.7f), Reels[2].Area.Y + (Reels[2].Area.Height * 0.85f)),
                            Color.LimeGreen, 12);
                    }
                }
            }
            else if (WinningLines.Count > 0)
            {
                if ((DateTime.Now - LineFlash).Milliseconds % 500 > 250)
                {
                    if (WinningLines.Contains(1))
                    {
                        _spriteBatch.DrawLine(
                            new Vector2(Reels[0].Area.X, Reels[0].Area.Y + (Reels[0].Area.Height / 2)),
                            new Vector2(Reels[1].Area.X + (Reels[1].Area.Width / 2), Reels[1].Area.Y + (Reels[1].Area.Height / 2)),
                            Color.FromNonPremultiplied(0, 128, 200, 255), 12);
                        _spriteBatch.DrawLine(
                            new Vector2(Reels[1].Area.X + (Reels[1].Area.Width / 2), Reels[1].Area.Y + (Reels[1].Area.Height / 2)),
                            new Vector2(Reels[2].Area.X + Reels[2].Area.Width, Reels[2].Area.Y + (Reels[2].Area.Height / 2)),
                            Color.FromNonPremultiplied(0, 128, 200, 255), 12);
                    }
                    if (WinningLines.Contains(2))
                    {
                        _spriteBatch.DrawLine(
                            new Vector2(Reels[0].Area.X, Reels[0].Area.Y + (Reels[0].Area.Height / 5)),
                            new Vector2(Reels[1].Area.X + (Reels[1].Area.Width / 2), Reels[1].Area.Y + (Reels[1].Area.Height / 5)),
                            Color.DeepPink, 12);
                        _spriteBatch.DrawLine(
                            new Vector2(Reels[1].Area.X + (Reels[1].Area.Width / 2), Reels[1].Area.Y + (Reels[1].Area.Height / 5)),
                            new Vector2(Reels[2].Area.X + Reels[2].Area.Width, Reels[2].Area.Y + (Reels[2].Area.Height / 5)),
                            Color.DeepPink, 12);
                    }
                    if (WinningLines.Contains(3))
                    {
                        _spriteBatch.DrawLine(
                             new Vector2(Reels[0].Area.X, Reels[0].Area.Y + (Reels[0].Area.Height * 0.8f)),
                             new Vector2(Reels[1].Area.X + (Reels[1].Area.Width / 2), Reels[1].Area.Y + (Reels[1].Area.Height * 0.8f)),
                             Color.Red, 12);
                        _spriteBatch.DrawLine(
                            new Vector2(Reels[1].Area.X + (Reels[1].Area.Width / 2), Reels[1].Area.Y + (Reels[1].Area.Height * 0.8f)),
                            new Vector2(Reels[2].Area.X + Reels[2].Area.Width, Reels[2].Area.Y + (Reels[2].Area.Height * 0.8f)),
                            Color.Red, 12);
                    }
                    if (WinningLines.Contains(4))
                    {
                        _spriteBatch.DrawLine(
                            new Vector2(Reels[0].Area.X + (Reels[2].Area.Width * 0.2f), Reels[0].Area.Y + (Reels[0].Area.Height * 0.85f)),
                            new Vector2(Reels[1].Area.X + (Reels[1].Area.Width / 2), Reels[1].Area.Y + (Reels[1].Area.Height * 0.5f)),
                            Color.Orange, 12);
                        _spriteBatch.DrawLine(
                            new Vector2(Reels[1].Area.X + (Reels[1].Area.Width / 2), Reels[1].Area.Y + (Reels[1].Area.Height * 0.5f)),
                            new Vector2(Reels[2].Area.X + (Reels[2].Area.Width * 0.7f), Reels[2].Area.Y + (Reels[2].Area.Height * 0.15f)),
                            Color.Orange, 12);
                    }
                    if (WinningLines.Contains(5))
                    {
                        _spriteBatch.DrawLine(
                            new Vector2(Reels[0].Area.X + (Reels[2].Area.Width * 0.2f), Reels[0].Area.Y + (Reels[0].Area.Height * 0.15f)),
                            new Vector2(Reels[1].Area.X + (Reels[1].Area.Width / 2), Reels[1].Area.Y + (Reels[1].Area.Height * 0.5f)),
                            Color.LimeGreen, 12);
                        _spriteBatch.DrawLine(
                            new Vector2(Reels[1].Area.X + (Reels[1].Area.Width / 2), Reels[1].Area.Y + (Reels[1].Area.Height * 0.5f)),
                            new Vector2(Reels[2].Area.X + (Reels[2].Area.Width * 0.7f), Reels[2].Area.Y + (Reels[2].Area.Height * 0.85f)),
                            Color.LimeGreen, 12);
                    }
                }
            }
            //if ((ChangedLines || WinningLines.Contains(1)) && (DateTime.Now - LineWinFlash) > TimeSpan.FromMilliseconds(0) && (DateTime.Now - LineWinFlash) < TimeSpan.FromMilliseconds(250)) 
            //{
            //    _spriteBatch.DrawLine(
            //        new Vector2(Reels[0].Area.X, Reels[0].Area.Y + (Reels[0].Area.Height / 2)),
            //        new Vector2(Reels[1].Area.X + (Reels[1].Area.Width / 2), Reels[1].Area.Y + (Reels[1].Area.Height / 2)),
            //        Color.FromNonPremultiplied(0, 128, 200, 255), 12);
            //    _spriteBatch.DrawLine(
            //        new Vector2(Reels[1].Area.X + (Reels[1].Area.Width / 2), Reels[1].Area.Y + (Reels[1].Area.Height / 2)),
            //        new Vector2(Reels[2].Area.X + Reels[2].Area.Width, Reels[2].Area.Y + (Reels[2].Area.Height / 2)),
            //        Color.FromNonPremultiplied(0, 128, 200, 255), 12);
            //}
            //if (((ChangedLines && Lines > 1) || WinningLines.Contains(2)) && (DateTime.Now - LineWinFlash) > TimeSpan.FromMilliseconds(250) && (DateTime.Now - LineWinFlash) < TimeSpan.FromMilliseconds(500)) //Second payline
            //{
            //    _spriteBatch.DrawLine(
            //        new Vector2(Reels[0].Area.X, Reels[0].Area.Y + (Reels[0].Area.Height / 5)),
            //        new Vector2(Reels[1].Area.X + (Reels[1].Area.Width / 2), Reels[1].Area.Y + (Reels[1].Area.Height / 5)),
            //        Color.DeepPink, 12);
            //    _spriteBatch.DrawLine(
            //        new Vector2(Reels[1].Area.X + (Reels[1].Area.Width / 2), Reels[1].Area.Y + (Reels[1].Area.Height / 5)),
            //        new Vector2(Reels[2].Area.X + Reels[2].Area.Width, Reels[2].Area.Y + (Reels[2].Area.Height / 5)),
            //        Color.DeepPink, 12);
            //}
            //if (((ChangedLines && Lines > 2) || WinningLines.Contains(3)) && (DateTime.Now - LineWinFlash) > TimeSpan.FromMilliseconds(500) && (DateTime.Now - LineWinFlash) < TimeSpan.FromMilliseconds(750)) //Third payline
            //{
            //    _spriteBatch.DrawLine(
            //        new Vector2(Reels[0].Area.X, Reels[0].Area.Y + (Reels[0].Area.Height * 0.8f)),
            //        new Vector2(Reels[1].Area.X + (Reels[1].Area.Width / 2), Reels[1].Area.Y + (Reels[1].Area.Height * 0.8f)),
            //        Color.Red, 12);
            //    _spriteBatch.DrawLine(
            //        new Vector2(Reels[1].Area.X + (Reels[1].Area.Width / 2), Reels[1].Area.Y + (Reels[1].Area.Height * 0.8f)),
            //        new Vector2(Reels[2].Area.X + Reels[2].Area.Width, Reels[2].Area.Y + (Reels[2].Area.Height * 0.8f)),
            //        Color.Red, 12);
            //}
            //if (((ChangedLines && Lines > 3) || WinningLines.Contains(4)) && (DateTime.Now - LineWinFlash) > TimeSpan.FromMilliseconds(750) && (DateTime.Now - LineWinFlash) < TimeSpan.FromMilliseconds(1000)) //Fourth payline
            //{
            //    _spriteBatch.DrawLine(
            //        new Vector2(Reels[0].Area.X + (Reels[2].Area.Width * 0.2f), Reels[0].Area.Y + (Reels[0].Area.Height * 0.85f)),
            //        new Vector2(Reels[1].Area.X + (Reels[1].Area.Width / 2), Reels[1].Area.Y + (Reels[1].Area.Height * 0.5f)),
            //        Color.Orange, 12);
            //    _spriteBatch.DrawLine(
            //        new Vector2(Reels[1].Area.X + (Reels[1].Area.Width / 2), Reels[1].Area.Y + (Reels[1].Area.Height * 0.5f)),
            //        new Vector2(Reels[2].Area.X + (Reels[2].Area.Width * 0.7f), Reels[2].Area.Y + (Reels[2].Area.Height * 0.15f)),
            //        Color.Orange, 12);
            //}
            //if (((ChangedLines && Lines > 4) || WinningLines.Contains(5)) && (DateTime.Now - LineWinFlash) > TimeSpan.FromMilliseconds(1000) && (DateTime.Now - LineWinFlash) < TimeSpan.FromMilliseconds(1250)) //Fifth payline
            //{
            //    _spriteBatch.DrawLine(
            //        new Vector2(Reels[0].Area.X + (Reels[2].Area.Width * 0.2f), Reels[0].Area.Y + (Reels[0].Area.Height * 0.15f)),
            //        new Vector2(Reels[1].Area.X + (Reels[1].Area.Width / 2), Reels[1].Area.Y + (Reels[1].Area.Height * 0.5f)),
            //        Color.LimeGreen, 12);
            //    _spriteBatch.DrawLine(
            //        new Vector2(Reels[1].Area.X + (Reels[1].Area.Width / 2), Reels[1].Area.Y + (Reels[1].Area.Height * 0.5f)),
            //        new Vector2(Reels[2].Area.X + (Reels[2].Area.Width * 0.7f), Reels[2].Area.Y + (Reels[2].Area.Height * 0.85f)),
            //        Color.LimeGreen, 12);
            //}
            #endregion

            if (CurrentState == GameState.SmallWinAnimation)
            {
                _spriteBatch.Draw(Overlay, GraphicsDevice.Viewport.Bounds, Color.Black);
                int animProgress = (int)((DateTime.Now - SmallWinAnimationStart).TotalMilliseconds);
                int smallWinBoardSize = Math.Min(animProgress, (int)(GraphicsDevice.Viewport.Width * 0.55f));
                if (animProgress < SmallWinBoardDelay)
                    _spriteBatch.Draw(ml.Images["smallwinboard"], new Rectangle(
                        GraphicsDevice.Viewport.Width / 2 - (smallWinBoardSize / 2),
                        GraphicsDevice.Viewport.Height / 3 - (smallWinBoardSize / 2),
                        smallWinBoardSize, smallWinBoardSize), Color.White);
                else
                    _spriteBatch.DrawString(ml.Fonts["vegas"], LastWinAmount.ToString("c"),
                        new Vector2(GraphicsDevice.Viewport.Width / 2 - (ml.Fonts["vegas"].MeasureString(LastWinAmount.ToString("c")).X / 2), GraphicsDevice.Viewport.Height * 0.66f),
                        Color.Gold);

            }

            _spriteBatch.End();
        }

        public override void Update(GameTime gameTime)
        {
            if ((DateTime.Now - LineFlash) > TimeSpan.FromMilliseconds(1000))
                LineFlash = DateTime.Now;

            if (CurrentState == GameState.InGame)
            {
                if ((TimeToNextFreeCredit - DateTime.Now) < TimeSpan.FromSeconds(0))
                {
                    Credits += 100;
                    TimeToNextFreeCredit = DateTime.Now.AddMinutes(30);
                }
                if ((DateTime.Now - SpinStartTime) > TimeSpan.FromSeconds(3.5d) && FreeSpins == 0 && Spinning && !OnFreeSpin)
                {
                    LongSpinSong.Play();
                    LongSpinStarted = true;
                }
                if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                    Game.Exit();

                //Change the bet
                if ((GamePad.GetState(0).Buttons.LeftShoulder == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Up)) && !Spinning)
                {
                    if (!BetUpPressed)
                    {
                        if (Bet >= 1000)
                            Bet += 100;
                        else if (Bet >= 500)
                            Bet += 25;
                        else if (Bet >= 100)
                            Bet += 10;
                        else if (Bet >= 10)
                            Bet += 5;
                        else if (Bet >= 1)
                            Bet += 1;

                        BetUpPressed = true;
                    }
                }
                else
                    BetUpPressed = false;
                if ((GamePad.GetState(0).Buttons.RightShoulder == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Down)) && !Spinning)
                {
                    if (!BetDownPressed)
                    {
                        if (Bet > 1000)
                            Bet -= 100;
                        else if (Bet > 500)
                            Bet -= 25;
                        else if (Bet > 100)
                            Bet -= 10;
                        else if (Bet > 10)
                            Bet -= 5;
                        else if (Bet > 1)
                            Bet -= 1;

                        BetDownPressed = true;
                    }
                }
                else
                    BetDownPressed = false;

                //Change the paylines
                if ((GamePad.GetState(0).Buttons.Start == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.NumPad8)) && !Spinning && Lines < 5)
                {
                    if (!LineUpPressed)
                    {
                        Lines++;
                        LineUpPressed = true;
                        ChangedLines = true;
                    }
                }
                else
                    LineUpPressed = false;
                if ((GamePad.GetState(0).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.NumPad2)) && !Spinning && Lines > 1)
                {
                    if (!LineDownPressed)
                    {
                        Lines--;
                        LineDownPressed = true;
                        ChangedLines = true;
                    }
                }
                else
                    LineDownPressed = false;

                //Do Free Spins
                if (FreeSpins > 0 && (DateTime.Now - SpinStopTime) > TimeSpan.FromMilliseconds(500) && !OnFreeSpin)
                {
                    WinningLines.Clear();
                    OnFreeSpin = true;
                    FreeSpins--;
                    AllReelsStopped = false;
                    foreach (Reel reel in Reels)
                        reel.WheelSpinning = true;
                    SpinStartTime = DateTime.Now;
                    StoppingReels = true;
                    FreeSpinBell.Play();
                    DuckSong.Play();
                    Spinning = true;
                }
                
                //Spin on space press
                if ((GamePad.GetState(0).Buttons.Y == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Space)) && (DateTime.Now - SpinStopTime) > TimeSpan.FromMilliseconds(500) && FreeSpins == 0 && Credits >= Bet * Lines)
                {
                    if (!StoppingReels)
                        StoppingReels = true;

                    if (AllReelsStopped)
                    {
                        ChangedLines = false;
                        WinningLines.Clear();
                        LastWinAmount = 0;
                        ml.SoundFX["coin"].Play();
                        Credits -= Bet * Lines;
                        System.Threading.Thread.Sleep(600);
                        SpinSong.Play();

                        AllReelsStopped = false;
                        foreach (Reel reel in Reels)
                            reel.WheelSpinning = true;

                        SpinStartTime = DateTime.Now;
                        Spinning = true;
                    }
                }

                //Spinning Ticks
                if (StoppingReels && (DateTime.Now - SpinStartTime) > TimeSpan.FromSeconds(1))
                {
                    if (OnFreeSpin && (DateTime.Now - SpinStartTime) < TimeSpan.FromSeconds(2))
                        goto SkipStop;
                    if (LongSpinStarted && (DateTime.Now - SpinStartTime) < TimeSpan.FromSeconds(10) && (Reels[0].WheelSpinning || Reels[1].WheelSpinning || Reels[2].WheelSpinning))
                        goto SkipStop;

                    if (Reels[0].WheelSpinning)
                    {
                        if (RNG.Next(50) > 42)
                        {
                            Reels[0].WheelSpinning = false;
                            if (Reels[0].Wheel[Reels[0].UpperIndex] == Reel.Tokens.LuckyDuck ||
                                Reels[0].Wheel[Reels[0].MiddleIndex] == Reel.Tokens.LuckyDuck ||
                                Reels[0].Wheel[Reels[0].LowerIndex] == Reel.Tokens.LuckyDuck)
                            {
                                ml.SoundFX["clang"].Play();
                            }
                        }
                    }
                    else if (Reels[1].WheelSpinning)
                    {
                        if (RNG.Next(50) > 48)
                        {
                            Reels[1].WheelSpinning = false;
                            if (Reels[1].Wheel[Reels[1].UpperIndex] == Reel.Tokens.LuckyDuck ||
                                Reels[1].Wheel[Reels[1].MiddleIndex] == Reel.Tokens.LuckyDuck ||
                                Reels[1].Wheel[Reels[1].LowerIndex] == Reel.Tokens.LuckyDuck)
                            {
                                ml.SoundFX["clang"].Play();
                            }
                        }
                    }
                    else if (Reels[2].WheelSpinning)
                    {
                        if (RNG.Next(100) > (!LongSpinStarted ? 96 : 98))
                        {
                            Reels[2].WheelSpinning = false;
                            if (Reels[2].Wheel[Reels[2].UpperIndex] == Reel.Tokens.LuckyDuck ||
                                Reels[2].Wheel[Reels[2].MiddleIndex] == Reel.Tokens.LuckyDuck ||
                                Reels[2].Wheel[Reels[2].LowerIndex] == Reel.Tokens.LuckyDuck)
                            {
                                ml.SoundFX["clang"].Play();
                            }
                        }
                    }
                    else
                    {
                        if (RNG.Next(0, 10) > 5) SpinSong.Stop(); else SpinSong.Pause();

                        LongSpinSong.Stop();
                        if (FreeSpins == 0)
                        {
                            FreeSpinBell.Stop();
                            DuckSong.Stop();
                            Spinning = false;
                        }
                        if (OnFreeSpin && FreeSpins == 0)
                            ml.SoundFX["kaching"].Play();

                        SpinStopTime = DateTime.Now;
                        AllReelsStopped = true;
                        StoppingReels = false;
                        OnFreeSpin = false;

                        Reel.Tokens[] Line1 = new Reel.Tokens[3] { Reels[0].Wheel[Reels[0].MiddleIndex], Reels[1].Wheel[Reels[1].MiddleIndex], Reels[2].Wheel[Reels[2].MiddleIndex] };
                        Reel.Tokens[] Line2 = new Reel.Tokens[3] { Reels[0].Wheel[Reels[0].UpperIndex], Reels[1].Wheel[Reels[1].UpperIndex], Reels[2].Wheel[Reels[2].UpperIndex] };
                        Reel.Tokens[] Line3 = new Reel.Tokens[3] { Reels[0].Wheel[Reels[0].LowerIndex], Reels[1].Wheel[Reels[1].LowerIndex], Reels[2].Wheel[Reels[2].LowerIndex] };
                        Reel.Tokens[] Line4 = new Reel.Tokens[3] { Reels[0].Wheel[Reels[0].LowerIndex], Reels[1].Wheel[Reels[1].MiddleIndex], Reels[2].Wheel[Reels[2].UpperIndex] };
                        Reel.Tokens[] Line5 = new Reel.Tokens[3] { Reels[0].Wheel[Reels[0].UpperIndex], Reels[1].Wheel[Reels[1].MiddleIndex], Reels[2].Wheel[Reels[2].LowerIndex] };

                        if (PayTables.FirstOrDefault(el => (el.Line[0] == Line1[0] && el.Line[1] == Line1[1] && el.Line[2] == Line1[2])) != default)
                        {
                            int currentWin = Bet * PayTables.FirstOrDefault(el => (el.Line[0] == Line1[0] && el.Line[1] == Line1[1] && el.Line[2] == Line1[2])).Multiplier;
                            LastWinAmount += currentWin;
                            Credits += currentWin;
                            WinningLines.Add(1);
                        }
                        if (Lines > 1)
                        {
                            if (PayTables.FirstOrDefault(el => (el.Line[0] == Line2[0] && el.Line[1] == Line2[1] && el.Line[2] == Line2[2])) != default)
                            {
                                int currentWin = Bet * PayTables.FirstOrDefault(el => (el.Line[0] == Line2[0] && el.Line[1] == Line2[1] && el.Line[2] == Line2[2])).Multiplier;
                                LastWinAmount += currentWin;
                                Credits += currentWin;
                                WinningLines.Add(2);
                            }
                        }
                        if (Lines > 2)
                        {
                            if (PayTables.FirstOrDefault(el => (el.Line[0] == Line3[0] && el.Line[1] == Line3[1] && el.Line[2] == Line3[2])) != default)
                            {
                                int currentWin = Bet * PayTables.FirstOrDefault(el => (el.Line[0] == Line3[0] && el.Line[1] == Line3[1] && el.Line[2] == Line3[2])).Multiplier;
                                LastWinAmount += currentWin;
                                Credits += currentWin;
                                WinningLines.Add(3);
                            }
                        }
                        if (Lines > 3)
                        {
                            if (PayTables.FirstOrDefault(el => (el.Line[0] == Line4[0] && el.Line[1] == Line4[1] && el.Line[2] == Line4[2])) != default)
                            {
                                int currentWin = Bet * PayTables.FirstOrDefault(el => (el.Line[0] == Line4[0] && el.Line[1] == Line4[1] && el.Line[2] == Line4[2])).Multiplier;
                                LastWinAmount += currentWin;
                                Credits += currentWin;
                                WinningLines.Add(4);
                            }
                        }
                        if (Lines > 4)
                        {
                            if (PayTables.FirstOrDefault(el => (el.Line[0] == Line5[0] && el.Line[1] == Line5[1] && el.Line[2] == Line5[2])) != default)
                            {
                                int currentWin = Bet * PayTables.FirstOrDefault(el => (el.Line[0] == Line5[0] && el.Line[1] == Line5[1] && el.Line[2] == Line5[2])).Multiplier;
                                LastWinAmount += currentWin;
                                Credits += currentWin;
                                WinningLines.Add(5);
                            }
                        }

                        int luckyDucks = Line1.Count(el => el == Reel.Tokens.LuckyDuck);
                        if (Lines > 1)
                            luckyDucks += Line2.Count(el => el == Reel.Tokens.LuckyDuck);
                        if (Lines > 2)
                            luckyDucks += Line3.Count(el => el == Reel.Tokens.LuckyDuck);

                        if (luckyDucks > 0)
                        {
                            int currentWin = (luckyDucks * (Bet * 2)) * luckyDucks;
                            LastWinAmount += currentWin;
                            Credits += currentWin;
                        }

                        if (LongSpinStarted)
                        {
                            if (LastWinAmount == 0)
                                ml.SoundFX["quack"].Play();
                            else
                                ml.SoundFX["slamstop"].Play();
                        }
                            
                        LongSpinStarted = false;

                        if (luckyDucks == 1)
                        {
                            if (RNG.Next(0, 10) > 8)
                            {
                                FreeSpins += RNG.Next(1, 5);
                                Spinning = true;
                            }
                        }
                        if (luckyDucks == 2)
                        {
                            if (RNG.Next(0, 10) > 7)
                            {
                                FreeSpins += RNG.Next(2, 6);
                                Spinning = true;
                            }
                        }
                        if (luckyDucks == 3)
                        {
                            if (RNG.Next(0, 10) > 6)
                            {
                                FreeSpins += RNG.Next(3, 7);
                                Spinning = true;
                            }
                        }
                        if (luckyDucks == 4)
                        {
                            if (RNG.Next(0, 10) > 5)
                            {
                                FreeSpins += RNG.Next(4, 8);
                                Spinning = true;
                            }
                        }
                        if (luckyDucks == 5)
                        {
                            if (RNG.Next(0, 10) > 4)
                            {
                                FreeSpins += RNG.Next(5, 9);
                                Spinning = true;
                            }
                        }
                        if (luckyDucks == 6)
                        {
                            FreeSpins += RNG.Next(8, 16);
                            Spinning = true;
                        }

                        if (FreeSpins == 0)
                        {
                            if (LastWinAmount >= 3 * Bet && LastWinAmount < 50 * Bet)
                            {
                                ml.SoundFX["kaching"].Play();
                                //CurrentState = GameState.SmallWinAnimation;
                                //SmallWinAnimationStart = DateTime.Now;
                            }
                            else if (LastWinAmount >= 20 * Bet)
                            {
                                ml.SoundFX["bigwinsound"].Play();
                                //CurrentState = GameState.BigWinAnimation;
                            }
                        }
                    }
                }
            SkipStop:

                foreach (Reel reel in Reels)
                {
                    reel.Update();
                }
            }
            else if (CurrentState == GameState.SmallWinAnimation)
            {
                CurrentState = GameState.InGame;
            }
        }
    }
}

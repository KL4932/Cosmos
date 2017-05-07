using Cosmos.System.Graphics;
using System.Collections.Generic;
using System;
using Canvas = Cosmos.System.Graphics.Canvas;
using Graph = Cosmos.System.Graphics;
using VGADriver = Cosmos.HAL.Drivers.VGADriver;

namespace Cosmos.System
{
    public class VGAScreen : Canvas
    {
        private VGADriver VGADriver;
        public enum TextSize { Size40x25, Size40x50, Size80x25, Size80x50, Size90x30, Size90x60 };

        public enum ScreenSize
        {
            Size640x480,
            Size720x480,
            Size320x200
        };

        public enum ColorDepth
        {
            BitDepth2, BitDepth4, BitDepth8, BitDepth16
        }

        private static VGADriver mScreen = new VGADriver();

        public static void SetGraphicsMode(ScreenSize screenSize, ColorDepth colorDepth)
        {
            VGADriver.ScreenSize ScrSize = VGADriver.ScreenSize.Size320x200;
            VGADriver.ColorDepth ClrDepth = VGADriver.ColorDepth.BitDepth8;

            switch (screenSize)
            {
                case ScreenSize.Size320x200:
                    ScrSize = VGADriver.ScreenSize.Size320x200;
                    break;
                case ScreenSize.Size640x480:
                    ScrSize = VGADriver.ScreenSize.Size640x480;
                    break;
                case ScreenSize.Size720x480:
                    ScrSize = VGADriver.ScreenSize.Size720x480;
                    break;
                default:
                    throw new Exception("This situation is not implemented!");
            }

            switch (colorDepth)
            {
                case ColorDepth.BitDepth2:
                    ClrDepth = VGADriver.ColorDepth.BitDepth2;
                    break;
                case ColorDepth.BitDepth4:
                    ClrDepth = VGADriver.ColorDepth.BitDepth4;
                    break;
                case ColorDepth.BitDepth8:
                    ClrDepth = VGADriver.ColorDepth.BitDepth8;
                    break;
                case ColorDepth.BitDepth16:
                    ClrDepth = VGADriver.ColorDepth.BitDepth16;
                    break;
                default:
                    throw new Exception("This situation is not implemented!");
            }

            mScreen.SetGraphicsMode(ScrSize, ClrDepth);
        }

        public static void SetPixel(uint X, uint Y, uint Color)
        {
            mScreen.SetPixel(X, Y, Color);
        }

        public static void Clear(int Color)
        {
            mScreen.Clear(Color);
        }

        public static void TestMode320x200x8()
        {
            mScreen.TestMode320x200x8();
        }

        public static void SetPalette(int Index, byte[] Palette)
        {
            mScreen.SetPalette(Index, Palette);
        }

        public static void SetPaletteEntry(int Index, byte R, byte G, byte B)
        {
            mScreen.SetPaletteEntry(Index, R, G, B);
        }

        public static uint GetPixel(uint X, uint Y)
        {
            return mScreen.GetPixel(X, Y);
        }

        public static void SetTextMode(TextSize Size)
        {
            switch (Size)
            {
                case TextSize.Size40x25:
                    mScreen.SetTextMode(VGADriver.TextSize.Size40x25);
                    break;
                case TextSize.Size40x50:
                    mScreen.SetTextMode(VGADriver.TextSize.Size40x50);
                    break;
                case TextSize.Size80x25:
                    mScreen.SetTextMode(VGADriver.TextSize.Size80x25);
                    break;
                case TextSize.Size80x50:
                    mScreen.SetTextMode(VGADriver.TextSize.Size80x50);
                    break;
                case TextSize.Size90x30:
                    mScreen.SetTextMode(VGADriver.TextSize.Size90x30);
                    break;
                case TextSize.Size90x60:
                    mScreen.SetTextMode(VGADriver.TextSize.Size90x60);
                    break;
                default:
                    throw new Exception("This situation is not implemented!");
            }
        }
        public VGAScreen() : base()
        {
            Global.mDebugger.SendInternal($"Creating new VGAScreen() with default mode {defaultGraphicMode}");

            VGADriver = new VGADriver(VGADriver.ScreenSize.Size320x200, VGADriver.ColorDepth.BitDepth8);
        }
        public VGAScreen(Mode mode) : base(mode)
        {
            var vgatest = "320x200x16";

            Global.mDebugger.SendInternal($"Creating new VGAScreen() with mode {vgatest}");

            ThrowIfModeIsNotValid(mode);

            //Testing a VGA on Canvas :: Cosmos(KL4932)
            VGADriver = new VGADriver(VGADriver.ScreenSize.Size320x200, VGADriver.ColorDepth.BitDepth4);
        }

        //public VGAScreen()
        //{
        //}

        public override Mode Mode
        {
            get
            {
                return mode;
            }

            set
            {
                mode = value;
                SetMode(mode);
            }
        }

        #region Drawing

        public override void Clear(Color color)
        {
            Global.mDebugger.SendInternal($"Clearing the Screen with Color {color}");
            //if (color == null)
            //   throw new ArgumentNullException(nameof(color));

            /*
             * TODO this version of Clear() works only when mode.ColorDepth == ColorDepth.ColorDepth32
             * in the other cases you should before convert color and then call the opportune ClearVRAM() overload
             * (the one that takes ushort for ColorDepth.ColorDepth16 and the one that takes byte for ColorDepth.ColorDepth8)
             * For ColorDepth.ColorDepth24 you should mask the Alpha byte.
             */
            //VGADriver.ClearVRAM((uint)color.ToArgb());
            
        }

        /*
         * As DrawPoint() is the basic block of DrawLine() and DrawRect() and in theory of all the future other methods that will
         * be implemented is better to not check the validity of the arguments here or it will repeat the check for any point
         * to be drawn slowing down all.
         */ 
        public override void DrawPoint(Pen pen, int x, int y)
        {
            Color color = pen.Color;
            uint pitch;
            uint stride;
            uint offset;
            uint ColorDepthInBytes = (uint)mode.ColorDepth / 8;

            /*
             * For now we can Draw only if the ColorDepth is 32 bit, we will throw otherwise.
             *
             * How to support other ColorDepth? The offset calculation should be the same (and so could be done out of the switch)
             * adding support ColorDepth.ColorDepth24 is easier as you need can use the version of SetVRAM() that take a byte
             * and call it 3 time for the B, G and R component (the Color class has properties to do this!), the problem is
             * for ColorDepth.ColorDepth16 and ColorDepth.ColorDepth8 than need a conversion from color (an ARGB32 color) to the RGB16 and RGB8
             * how to do this conversion faster maybe using pre-computed tables? What happens if the color cannot be converted? We will throw?
             */
            switch (mode.ColorDepth)
            {
                case Graph.ColorDepth.ColorDepth32:
                    Global.mDebugger.SendInternal("Computing offset...");
                    pitch = (uint)mode.Columns * ColorDepthInBytes;
                    stride = ColorDepthInBytes;
                    //offset = ((uint)x * pitch) + ((uint)y * stride);
                    offset = ((uint)x * stride) + ((uint)y * pitch);

                    Global.mDebugger.SendInternal($"Drawing Point of color {color} at offset {offset}");

                    //.SetVRAM(offset, (uint)color.ToArgb());

                    Global.mDebugger.SendInternal("Point drawn");
                    break;

                default:
                    String errorMsg = "DrawPoint() with ColorDepth " + (int)Mode.ColorDepth + " not yet supported";
                    throw new NotImplementedException(errorMsg);

            }
        }

        public override void DrawPoint(Pen pen, float x, float y)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Display
        /// <summary>
        /// Implementation of VGA to Canvas, we hope we can make that without any problems B)                                                                                            
        /// </summary>
        public override List<Mode> getAviableModes()
        {
            return new List<Mode>
                {
                  new Mode(320, 200, Graph.ColorDepth.ColorDepth4),
                  new Mode(320, 200, Graph.ColorDepth.ColorDepth8),
                  new Mode(640, 480, Graph.ColorDepth.ColorDepth4),
                  new Mode(720, 480, Graph.ColorDepth.ColorDepth4)
            };
        }

        protected override Mode getDefaultGraphicMode() => new Mode(320, 200, Graph.ColorDepth.ColorDepth8);

        /// <summary>
        /// Use this to setup the screen, this will disable the console.
        /// </summary>
        /// <param name="Mode">The desired Mode resolution</param>
        private void SetMode(Mode mode)
        {
            ThrowIfModeIsNotValid(mode);

            ScreenSize aSize;
            ColorDepth aDepth;
            aSize = new ScreenSize();
            aDepth = new ColorDepth();

            //set the screen
            SetGraphicsMode(aSize, aDepth);
        }
        #endregion

        public static int PixelHeight = mScreen.PixelWidth;

        public static int PixelWidth = mScreen.PixelWidth;

        public static int Colors = mScreen.Colors;
    }
}

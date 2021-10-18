/* Lifted from pk3DS
 * https://raw.githubusercontent.com/kwsch/pk3DS/master/pk3DS/3DS/BCLIM.cs
 * Author: Kaphotics
 * License: See pk3DS
 */

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace Jtex
{
    public static class Jupiter
    {
        internal static Image MakeBmp(string path, bool autoSave = false, bool crop = true)
        {
            Jtex tex = Analyze(path);

            Bitmap img = GetImg(tex);

            if (img == null)
                return null;

            Rectangle cropRect = new Rectangle(0, 0, (int) tex.Width, (int) tex.Height);
            Bitmap cropBmp = new Bitmap(cropRect.Width, cropRect.Height);
            using (Graphics g = Graphics.FromImage(cropBmp))
            {
                g.DrawImage(img,
                            new Rectangle(0, 0, cropBmp.Width, cropBmp.Height),
                            cropRect,
                            GraphicsUnit.Pixel);
            }
            if (!autoSave)
                return !crop ? img : cropBmp;

            using (MemoryStream ms = new MemoryStream())
            {
                //error will throw from here
                cropBmp.Save(ms, ImageFormat.Png);
                byte[] data = ms.ToArray();
                File.WriteAllBytes(Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path) + ".png"), data);
            }
            return !crop ? img : cropBmp;
        }
        // Bitmap Data Writing
        private static Bitmap GetImg(int width, int height, byte[] bytes, int f)
        {
            Bitmap img = new Bitmap(width, height);
            int area = img.Width * img.Height;
            // Tiles Per Width
            int p = Gcm(img.Width, 8) / 8;
            if (p == 0) p = 1;
            using (Stream bitmapStream = new MemoryStream(bytes))
            using (BinaryReader br = new BinaryReader(bitmapStream))
                for (uint i = 0; i < area; i++) // For every pixel
                {
                    uint x;
                    uint y;
                    D2Xy(i % 64, out x, out y);
                    uint tile = i / 64;

                    // Shift Tile Coordinate into Tilemap
                    x += (uint) (tile % p) * 8;
                    y += (uint) (tile / p) * 8;

                    // Get Color
                    Color c;
                    switch (f)
                    {
                        case 0x0:  // L8        // 8bit/1 byte
                        case 0x1:  // A8
                        case 0x2:  // LA4
                            c = DecodeColor(br.ReadByte(), f);
                            break;
                        case 0x3:  // LA8       // 16bit/2 byte
                        case 0x4:  // HILO8
                        case 0x5:  // RGB565
                        case 0x8:  // RGBA4444
                        case 0x7:  // RGBA5551
                        case 10:
                            c = DecodeColor(br.ReadUInt16(), f);
                            break;
                        case 0x6:  // RGB8:     // 24bit
                            byte[] data = br.ReadBytes(3); Array.Resize(ref data, 4);
                            c = DecodeColor(BitConverter.ToUInt32(data, 0), f);
                            break;
                        case 0x9:  // RGBA8888
                            c = DecodeColor(br.ReadUInt32(), f);
                            break;
                        case 0xC:  // L4
                        case 0xD:  // A4        // 4bit - Do 2 pixels at a time.
                            uint val = br.ReadByte();
                            img.SetPixel((int) x, (int) y, DecodeColor(val & 0xF, f)); // lowest bits for the low pixel
                            i++; x++;
                            c = DecodeColor(val >> 4, f);   // highest bits for the high pixel
                            break;
                        default: throw new Exception("Invalid FileFormat.");
                    }
                    img.SetPixel((int) x, (int) y, c);
                }
            return img;
        }

        private static Bitmap GetImg(Jtex tex)
        {
            // New Image
            int w = Nlpo2(Gcm((int) tex.Width, 8));
            int h = Nlpo2(Gcm((int) tex.Height, 8));
            int f = 0;
            if (tex.Format == 2)
                f = 9;
            if (tex.Format == 4)
                f = 10;
            if (tex.Format == 5)
                f = 7;
            int area = w * h;
            if (area > tex.Data.Length / 4)
            {
                w = Gcm((int) tex.Width, 8);
                h = Gcm((int) tex.Height, 8);
            }
            // Build Image
            return GetImg(w, h, tex.Data, f);
        }

        private static readonly int[] Convert5To8 = { 0x00,0x08,0x10,0x18,0x20,0x29,0x31,0x39,
                                              0x41,0x4A,0x52,0x5A,0x62,0x6A,0x73,0x7B,
                                              0x83,0x8B,0x94,0x9C,0xA4,0xAC,0xB4,0xBD,
                                              0xC5,0xCD,0xD5,0xDE,0xE6,0xEE,0xF6,0xFF };

        private static Color DecodeColor(uint val, int format)
        {
            int alpha = 0xFF, red, green, blue;
            switch (format)
            {
                case 0: // L8
                    return Color.FromArgb(alpha, (byte) val, (byte) val, (byte) val);
                case 1: // A8
                    return Color.FromArgb((byte) val, alpha, alpha, alpha);
                case 2: // LA4
                    red = (byte) (val >> 4);
                    alpha = (byte) (val & 0x0F);
                    return Color.FromArgb(alpha, red, red, red);
                case 3: // LA8
                    red = (byte) ((val >> 8 & 0xFF));
                    alpha = (byte) (val & 0xFF);
                    return Color.FromArgb(alpha, red, red, red);
                case 4: // HILO8
                    red = (byte) (val >> 8);
                    green = (byte) (val & 0xFF);
                    return Color.FromArgb(alpha, red, green, 0xFF);
                case 5: // RGB565
                    red = Convert5To8[(val >> 11) & 0x1F];
                    green = (byte) (((val >> 5) & 0x3F) * 4);
                    blue = Convert5To8[val & 0x1F];
                    return Color.FromArgb(alpha, red, green, blue);
                case 6: // RGB8
                    red = (byte) ((val >> 16) & 0xFF);
                    green = (byte) ((val >> 8) & 0xFF);
                    blue = (byte) (val & 0xFF);
                    return Color.FromArgb(alpha, red, green, blue);
                case 7: // RGBA5551
                    red = Convert5To8[(val >> 11) & 0x1F];
                    green = Convert5To8[(val >> 6) & 0x1F];
                    blue = Convert5To8[(val >> 1) & 0x1F];
                    alpha = (val & 0x0001) == 1 ? 0xFF : 0x00;
                    return Color.FromArgb(alpha, red, green, blue);
                case 8: // RGBA4444
                    alpha = (byte) (0x11 * (val & 0xf));
                    red = (byte) (0x11 * ((val >> 12) & 0xf));
                    green = (byte) (0x11 * ((val >> 8) & 0xf));
                    blue = (byte) (0x11 * ((val >> 4) & 0xf));
                    return Color.FromArgb(alpha, red, green, blue);
                case 9: // RGBA8888
                    red = (byte) ((val >> 24) & 0xFF);
                    green = (byte) ((val >> 16) & 0xFF);
                    blue = (byte) ((val >> 8) & 0xFF);
                    alpha = (byte) (val & 0xFF);
                    return Color.FromArgb(alpha, red, green, blue);
                case 10: // RGBA4444
                    alpha = (byte) (0x11 * (val & 0xf));
                    red = (byte) (0x11 * ((val >> 12) & 0xf));
                    green = (byte) (0x11 * ((val >> 8) & 0xf));
                    blue = (byte) (0x11 * ((val >> 4) & 0xf));
                    return Color.FromArgb(alpha, red, green, blue);
                // case 10:
                // case 11:
                case 12: // L4
                    return Color.FromArgb(alpha, (byte) (val * 0x11), (byte) (val * 0x11), (byte) (val * 0x11));
                case 13: // A4
                    return Color.FromArgb((byte) (val * 0x11), alpha, alpha, alpha);
                default:
                    return Color.White;
            }
        }

        // Color Conversion
        // L8
        private static byte GetL8(Color c)
        {
            byte red = c.R;
            byte green = c.G;
            byte blue = c.B;
            // Luma (Y’) = 0.299 R’ + 0.587 G’ + 0.114 B’ from wikipedia
            return (byte) (((0x4CB2 * red + 0x9691 * green + 0x1D3E * blue) >> 16) & 0xFF);
        }
        // A8
        private static byte GetA8(Color c)
        {
            return c.A;
        }
        // LA4
        private static byte GetLa4(Color c)
        {
            return (byte) ((c.A / 0x11) + (c.R / 0x11) << 4);
        }
        // LA8
        private static ushort GetLa8(Color c)
        {
            return (ushort) ((c.A) + ((c.R) << 8));
        }
        // HILO8
        private static ushort GetHilo8(Color c)
        {
            return (ushort) ((c.G) + ((c.R) << 8));
        }
        // RGB565
        private static ushort GetRgb565(Color c)
        {
            int val = 0;
            // val += c.A >> 8; // unused
            val += Convert8To5(c.B) >> 3;
            val += (c.G >> 2) << 5;
            val += Convert8To5(c.R) << 10;
            return (ushort) val;
        }
        // RGBA5551
        private static ushort GetRgba5551(Color c)
        {
            int val = 0;
            val += (byte) (c.A > 0x80 ? 1 : 0);
            val += Convert8To5(c.R) << 11;
            val += Convert8To5(c.G) << 6;
            val += Convert8To5(c.B) << 1;
            ushort v = (ushort) val;

            return v;
        }
        // RGBA4444
        private static ushort GetRgba4444(Color c)
        {
            int val = 0;
            val += (c.A / 0x11);
            val += ((c.B / 0x11) << 4);
            val += ((c.G / 0x11) << 8);
            val += ((c.R / 0x11) << 12);
            return (ushort) val;
        }
        // RGBA8888
        private static uint GetRgba8888(Color c)
        {
            uint val = 0;
            val += c.A;
            val += (uint) (c.B << 8);
            val += (uint) (c.G << 16);
            val += (uint) (c.R << 24);
            return val;
        }

        // Unit Conversion
        private static byte Convert8To5(int colorVal)
        {
            byte[] Convert8to5 = { 0x00,0x08,0x10,0x18,0x20,0x29,0x31,0x39,
                                   0x41,0x4A,0x52,0x5A,0x62,0x6A,0x73,0x7B,
                                   0x83,0x8B,0x94,0x9C,0xA4,0xAC,0xB4,0xBD,
                                   0xC5,0xCD,0xD5,0xDE,0xE6,0xEE,0xF6,0xFF };
            byte i = 0;
            while (colorVal > Convert8to5[i]) i++;
            return i;
        }
        private static uint Dm2X(uint code)
        {
            return C11(code >> 0);
        }
        private static uint Dm2Y(uint code)
        {
            return C11(code >> 1);
        }
        private static uint C11(uint x)
        {
            x &= 0x55555555;                 // x = -f-e -d-c -b-a -9-8 -7-6 -5-4 -3-2 -1-0
            x = (x ^ (x >> 1)) & 0x33333333; // x = --fe --dc --ba --98 --76 --54 --32 --10
            x = (x ^ (x >> 2)) & 0x0f0f0f0f; // x = ---- fedc ---- ba98 ---- 7654 ---- 3210
            x = (x ^ (x >> 4)) & 0x00ff00ff; // x = ---- ---- fedc ba98 ---- ---- 7654 3210
            x = (x ^ (x >> 8)) & 0x0000ffff; // x = ---- ---- ---- ---- fedc ba98 7654 3210
            return x;
        }

        /// <summary>
        /// Greatest common multiple (to round up)
        /// </summary>
        /// <param name="n">Number to round-up.</param>
        /// <param name="m">Multiple to round-up to.</param>
        /// <returns>Rounded up number.</returns>
        private static int Gcm(int n, int m)
        {
            return ((n + m - 1) / m) * m;
        }
        /// <summary>
        /// Next Largest Power of 2
        /// </summary>
        /// <param name="x">Input to round up to next 2^n</param>
        /// <returns>2^n > x && x > 2^(n-1) </returns>
        private static int Nlpo2(int x)
        {
            x--; // comment out to always take the next biggest power of two, even if x is already a power of two
            x |= (x >> 1);
            x |= (x >> 2);
            x |= (x >> 4);
            x |= (x >> 8);
            x |= (x >> 16);
            return (x+1);
        }

        // Morton Translation
        /// <summary>
        /// Combines X/Y Coordinates to a Decimal Ordinate.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private static uint Xy2D(uint x, uint y)
        {
	        x &= 0x0000ffff;
	        y &= 0x0000ffff;
	        x |= (x << 8);
	        y |= (y << 8);
	        x &= 0x00ff00ff;
	        y &= 0x00ff00ff;
	        x |= (x << 4);
	        y |= (y << 4);
	        x &= 0x0f0f0f0f;
	        y &= 0x0f0f0f0f;
	        x |= (x << 2);
	        y |= (y << 2);
	        x &= 0x33333333;
	        y &= 0x33333333;
	        x |= (x << 1);
	        y |= (y << 1);
	        x &= 0x55555555;
	        y &= 0x55555555;
	        return x | (y << 1);
        }
        /// <summary>
        /// Decimal Ordinate In to X / Y Coordinate Out
        /// </summary>
        /// <param name="d">Loop integer which will be decoded to X/Y</param>
        /// <param name="x">Output X coordinate</param>
        /// <param name="y">Output Y coordinate</param>
        private static void D2Xy(uint d, out uint x, out uint y)
        {
	        x = d;
	        y = (x >> 1);
	        x &= 0x55555555;
	        y &= 0x55555555;
	        x |= (x >> 1);
	        y |= (y >> 1);
	        x &= 0x33333333;
	        y &= 0x33333333;
	        x |= (x >> 2);
	        y |= (y >> 2);
	        x &= 0x0f0f0f0f;
	        y &= 0x0f0f0f0f;
	        x |= (x >> 4);
	        y |= (y >> 4);
	        x &= 0x00ff00ff;
	        y &= 0x00ff00ff;
	        x |= (x >> 8);
	        y |= (y >> 8);
	        x &= 0x0000ffff;
	        y &= 0x0000ffff;
        }

        private static Jtex Analyze(byte[] data)
        {
            Jtex tex = new Jtex();
            if (data[0] == 0x11) // compressed
                try
                {
                    MemoryStream oldD = new MemoryStream(data);
                    MemoryStream newD = new MemoryStream();
                    Lzss.Decompress(oldD, data.Length, newD);
                    data = newD.ToArray();
                }
                catch
                {
                    return tex;
                }

            tex.Length = BitConverter.ToUInt32(data, 0x0);
            tex.Format = BitConverter.ToUInt32(data, 0x4);
            tex.Width = BitConverter.ToUInt32(data, 0x8);
            tex.Height = BitConverter.ToUInt32(data, 0xC);

            tex.Data = data.Skip((int) tex.Length).ToArray();
            return tex;
        }
        private static Jtex Analyze(string path)
        {
            return Analyze(File.ReadAllBytes(path));
        }
        private struct Jtex
        {
            public uint Length;
            public uint Format;
            public uint Width;
            public uint Height;

            public byte[] Data;
        }
    }
}

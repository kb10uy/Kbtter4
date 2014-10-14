using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using DxLibDLL;
using CoreTweet;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace Kbtter5
{
    public static class CommonObjects
    {
        #region ユーティリティ

        public static string DataDirectory = "Kbtter5Data";

        public static string GetCommonImagePath(string fname)
        {
            return Path.Combine("Kbtter5Data", "img", fname);
        }

        #endregion

        public static int ImageLoadingCircle32 = DX.LoadGraph(GetCommonImagePath("loading32.png"));
        public static int ImageLogo = DX.LoadGraph(GetCommonImagePath("Kbtter5.png"));

        public static int ImageShot = DX.LoadGraph(GetCommonImagePath("shot.png"));
        public static int ImageStar = DX.LoadGraph(GetCommonImagePath("shot2.png"));

        public static int[] ImageNumber48 { get; private set; }
        public static int[] ImageNumber24 { get; private set; }
        public static int[] ImageNumber32 { get; private set; }
        public static int[] ImageNumber12White { get; private set; }
        public static int[] ImageNumber12Red { get; private set; }
        public static int[] ImageNumber12Blue { get; private set; }

        public static int FontSystem = DX.CreateFontToHandle("Meiryo", 16, 1, DX.DX_FONTTYPE_ANTIALIASING);
        public static int FontSystemBig = DX.CreateFontToHandle("Meiryo", 48, 1, DX.DX_FONTTYPE_ANTIALIASING);
        public static int FontBullet = DX.CreateFontToHandle("Meiryo", 20, 2, DX.DX_FONTTYPE_ANTIALIASING);

        static CommonObjects()
        {
            ImageNumber12White = new int[11];
            ImageNumber12Red = new int[11];
            ImageNumber12Blue = new int[11];
            ImageNumber24 = new int[11];
            ImageNumber32 = new int[11];
            ImageNumber48 = new int[11];
            DX.LoadDivGraph(GetCommonImagePath("num12.png"), 11, 11, 1, 6, 12, out ImageNumber12White[0]);
            DX.LoadDivGraph(GetCommonImagePath("num12_2.png"), 11, 11, 1, 6, 12, out ImageNumber12Red[0]);
            DX.LoadDivGraph(GetCommonImagePath("num12_3.png"), 11, 11, 1, 6, 12, out ImageNumber12Blue[0]);
            DX.LoadDivGraph(GetCommonImagePath("num24.png"), 11, 11, 1, 12, 24, out ImageNumber24[0]);
            DX.LoadDivGraph(GetCommonImagePath("num32.png"), 11, 11, 1, 16, 32, out ImageNumber32[0]);
            DX.LoadDivGraph(GetCommonImagePath("num48.png"), 11, 11, 1, 24, 48, out ImageNumber48[0]);
        }

        public static class Colors
        {
            public static int AliceBlue = DX.GetColor(0xF0, 0xF8, 0xFF);
            public static int AntiqueWhite = DX.GetColor(0xFA, 0xEB, 0xD7);
            public static int Aqua = DX.GetColor(0x00, 0xFF, 0xFF);
            public static int Aquamarine = DX.GetColor(0x7F, 0xFF, 0xD4);
            public static int Azure = DX.GetColor(0xF0, 0xFF, 0xFF);
            public static int Beige = DX.GetColor(0xF5, 0xF5, 0xDC);
            public static int Bisque = DX.GetColor(0xFF, 0xE4, 0xC4);
            public static int Black = DX.GetColor(0x00, 0x00, 0x00);
            public static int BlanchedAlmond = DX.GetColor(0xFF, 0xEB, 0xCD);
            public static int Blue = DX.GetColor(0x00, 0x00, 0xFF);
            public static int BlueViolet = DX.GetColor(0x8A, 0x2B, 0xE2);
            public static int Brown = DX.GetColor(0xA5, 0x2A, 0x2A);
            public static int BurlyWood = DX.GetColor(0xDE, 0xB8, 0x87);
            public static int CadetBlue = DX.GetColor(0x5F, 0x9E, 0xA0);
            public static int Chartreuse = DX.GetColor(0x7F, 0xFF, 0x00);
            public static int Chocolate = DX.GetColor(0xD2, 0x69, 0x1E);
            public static int Coral = DX.GetColor(0xFF, 0x7F, 0x50);
            public static int CornflowerBlue = DX.GetColor(0x64, 0x95, 0xED);
            public static int Cornsilk = DX.GetColor(0xFF, 0xF8, 0xDC);
            public static int Crimson = DX.GetColor(0xDC, 0x14, 0x3C);
            public static int Cyan = DX.GetColor(0x00, 0xFF, 0xFF);
            public static int DarkBlue = DX.GetColor(0x00, 0x00, 0x8B);
            public static int DarkCyan = DX.GetColor(0x00, 0x8B, 0x8B);
            public static int DarkGoldenrod = DX.GetColor(0xB8, 0x86, 0x0B);
            public static int DarkGray = DX.GetColor(0xA9, 0xA9, 0xA9);
            public static int DarkGreen = DX.GetColor(0x00, 0x64, 0x00);
            public static int DarkKhaki = DX.GetColor(0xBD, 0xB7, 0x6B);
            public static int DarkMagenta = DX.GetColor(0x8B, 0x00, 0x8B);
            public static int DarkOliveGreen = DX.GetColor(0x55, 0x6B, 0x2F);
            public static int DarkOrange = DX.GetColor(0xFF, 0x8C, 0x00);
            public static int DarkOrchid = DX.GetColor(0x99, 0x32, 0xCC);
            public static int DarkRed = DX.GetColor(0x8B, 0x00, 0x00);
            public static int DarkSalmon = DX.GetColor(0xE9, 0x96, 0x7A);
            public static int DarkSeaGreen = DX.GetColor(0x8F, 0xBC, 0x8F);
            public static int DarkSlateBlue = DX.GetColor(0x48, 0x3D, 0x8B);
            public static int DarkSlateGray = DX.GetColor(0x2F, 0x4F, 0x4F);
            public static int DarkTurquoise = DX.GetColor(0x00, 0xCE, 0xD1);
            public static int DarkViolet = DX.GetColor(0x94, 0x00, 0xD3);
            public static int DeepPink = DX.GetColor(0xFF, 0x14, 0x93);
            public static int DeepSkyBlue = DX.GetColor(0x00, 0xBF, 0xFF);
            public static int DimGray = DX.GetColor(0x69, 0x69, 0x69);
            public static int DodgerBlue = DX.GetColor(0x1E, 0x90, 0xFF);
            public static int Firebrick = DX.GetColor(0xB2, 0x22, 0x22);
            public static int FloralWhite = DX.GetColor(0xFF, 0xFA, 0xF0);
            public static int ForestGreen = DX.GetColor(0x22, 0x8B, 0x22);
            public static int Fuchsia = DX.GetColor(0xFF, 0x00, 0xFF);
            public static int Gainsboro = DX.GetColor(0xDC, 0xDC, 0xDC);
            public static int GhostWhite = DX.GetColor(0xF8, 0xF8, 0xFF);
            public static int Gold = DX.GetColor(0xFF, 0xD7, 0x00);
            public static int Goldenrod = DX.GetColor(0xDA, 0xA5, 0x20);
            public static int Gray = DX.GetColor(0x80, 0x80, 0x80);
            public static int Green = DX.GetColor(0x00, 0x80, 0x00);
            public static int GreenYellow = DX.GetColor(0xAD, 0xFF, 0x2F);
            public static int Honeydew = DX.GetColor(0xF0, 0xFF, 0xF0);
            public static int HotPink = DX.GetColor(0xFF, 0x69, 0xB4);
            public static int IndianRed = DX.GetColor(0xCD, 0x5C, 0x5C);
            public static int Indigo = DX.GetColor(0x4B, 0x00, 0x82);
            public static int Ivory = DX.GetColor(0xFF, 0xFF, 0xF0);
            public static int Khaki = DX.GetColor(0xF0, 0xE6, 0x8C);
            public static int Lavender = DX.GetColor(0xE6, 0xE6, 0xFA);
            public static int LavenderBlush = DX.GetColor(0xFF, 0xF0, 0xF5);
            public static int LawnGreen = DX.GetColor(0x7C, 0xFC, 0x00);
            public static int LemonChiffon = DX.GetColor(0xFF, 0xFA, 0xCD);
            public static int LightBlue = DX.GetColor(0xAD, 0xD8, 0xE6);
            public static int LightCoral = DX.GetColor(0xF0, 0x80, 0x80);
            public static int LightCyan = DX.GetColor(0xE0, 0xFF, 0xFF);
            public static int LightGoldenrodYellow = DX.GetColor(0xFA, 0xFA, 0xD2);
            public static int LightGray = DX.GetColor(0xD3, 0xD3, 0xD3);
            public static int LightGreen = DX.GetColor(0x90, 0xEE, 0x90);
            public static int LightPink = DX.GetColor(0xFF, 0xB6, 0xC1);
            public static int LightSalmon = DX.GetColor(0xFF, 0xA0, 0x7A);
            public static int LightSeaGreen = DX.GetColor(0x20, 0xB2, 0xAA);
            public static int LightSkyBlue = DX.GetColor(0x87, 0xCE, 0xFA);
            public static int LightSlateGray = DX.GetColor(0x77, 0x88, 0x99);
            public static int LightSteelBlue = DX.GetColor(0xB0, 0xC4, 0xDE);
            public static int LightYellow = DX.GetColor(0xFF, 0xFF, 0xE0);
            public static int Lime = DX.GetColor(0x00, 0xFF, 0x00);
            public static int LimeGreen = DX.GetColor(0x32, 0xCD, 0x32);
            public static int Linen = DX.GetColor(0xFA, 0xF0, 0xE6);
            public static int Magenta = DX.GetColor(0xFF, 0x00, 0xFF);
            public static int Maroon = DX.GetColor(0x80, 0x00, 0x00);
            public static int MediumAquamarine = DX.GetColor(0x66, 0xCD, 0xAA);
            public static int MediumBlue = DX.GetColor(0x00, 0x00, 0xCD);
            public static int MediumOrchid = DX.GetColor(0xBA, 0x55, 0xD3);
            public static int MediumPurple = DX.GetColor(0x93, 0x70, 0xDB);
            public static int MediumSeaGreen = DX.GetColor(0x3C, 0xB3, 0x71);
            public static int MediumSlateBlue = DX.GetColor(0x7B, 0x68, 0xEE);
            public static int MediumSpringGreen = DX.GetColor(0x00, 0xFA, 0x9A);
            public static int MediumTurquoise = DX.GetColor(0x48, 0xD1, 0xCC);
            public static int MediumVioletRed = DX.GetColor(0xC7, 0x15, 0x85);
            public static int MidnightBlue = DX.GetColor(0x19, 0x19, 0x70);
            public static int MintCream = DX.GetColor(0xF5, 0xFF, 0xFA);
            public static int MistyRose = DX.GetColor(0xFF, 0xE4, 0xE1);
            public static int Moccasin = DX.GetColor(0xFF, 0xE4, 0xB5);
            public static int NavajoWhite = DX.GetColor(0xFF, 0xDE, 0xAD);
            public static int Navy = DX.GetColor(0x00, 0x00, 0x80);
            public static int OldLace = DX.GetColor(0xFD, 0xF5, 0xE6);
            public static int Olive = DX.GetColor(0x80, 0x80, 0x00);
            public static int OliveDrab = DX.GetColor(0x6B, 0x8E, 0x23);
            public static int Orange = DX.GetColor(0xFF, 0xA5, 0x00);
            public static int OrangeRed = DX.GetColor(0xFF, 0x45, 0x00);
            public static int Orchid = DX.GetColor(0xDA, 0x70, 0xD6);
            public static int PaleGoldenrod = DX.GetColor(0xEE, 0xE8, 0xAA);
            public static int PaleGreen = DX.GetColor(0x98, 0xFB, 0x98);
            public static int PaleTurquoise = DX.GetColor(0xAF, 0xEE, 0xEE);
            public static int PaleVioletRed = DX.GetColor(0xDB, 0x70, 0x93);
            public static int PapayaWhip = DX.GetColor(0xFF, 0xEF, 0xD5);
            public static int PeachPuff = DX.GetColor(0xFF, 0xDA, 0xB9);
            public static int Peru = DX.GetColor(0xCD, 0x85, 0x3F);
            public static int Pink = DX.GetColor(0xFF, 0xC0, 0xCB);
            public static int Plum = DX.GetColor(0xDD, 0xA0, 0xDD);
            public static int PowderBlue = DX.GetColor(0xB0, 0xE0, 0xE6);
            public static int Purple = DX.GetColor(0x80, 0x00, 0x80);
            public static int Red = DX.GetColor(0xFF, 0x00, 0x00);
            public static int RosyBrown = DX.GetColor(0xBC, 0x8F, 0x8F);
            public static int RoyalBlue = DX.GetColor(0x41, 0x69, 0xE1);
            public static int SaddleBrown = DX.GetColor(0x8B, 0x45, 0x13);
            public static int Salmon = DX.GetColor(0xFA, 0x80, 0x72);
            public static int SandyBrown = DX.GetColor(0xF4, 0xA4, 0x60);
            public static int SeaGreen = DX.GetColor(0x2E, 0x8B, 0x57);
            public static int SeaShell = DX.GetColor(0xFF, 0xF5, 0xEE);
            public static int Sienna = DX.GetColor(0xA0, 0x52, 0x2D);
            public static int Silver = DX.GetColor(0xC0, 0xC0, 0xC0);
            public static int SkyBlue = DX.GetColor(0x87, 0xCE, 0xEB);
            public static int SlateBlue = DX.GetColor(0x6A, 0x5A, 0xCD);
            public static int SlateGray = DX.GetColor(0x70, 0x80, 0x90);
            public static int Snow = DX.GetColor(0xFF, 0xFA, 0xFA);
            public static int SpringGreen = DX.GetColor(0x00, 0xFF, 0x7F);
            public static int SteelBlue = DX.GetColor(0x46, 0x82, 0xB4);
            public static int Tan = DX.GetColor(0xD2, 0xB4, 0x8C);
            public static int Teal = DX.GetColor(0x00, 0x80, 0x80);
            public static int Thistle = DX.GetColor(0xD8, 0xBF, 0xD8);
            public static int Tomato = DX.GetColor(0xFF, 0x63, 0x47);
            public static int Turquoise = DX.GetColor(0x40, 0xE0, 0xD0);
            public static int Violet = DX.GetColor(0xEE, 0x82, 0xEE);
            public static int Wheat = DX.GetColor(0xF5, 0xDE, 0xB3);
            public static int White = DX.GetColor(0xFF, 0xFF, 0xFF);
            public static int WhiteSmoke = DX.GetColor(0xF5, 0xF5, 0xF5);
            public static int Yellow = DX.GetColor(0xFF, 0xFF, 0x00);
            public static int YellowGreen = DX.GetColor(0x9A, 0xCD, 0x32);
        }

    }

    public static class UserImageManager
    {
        public static int ImageSize = 32;
        private static Dictionary<long, int> ImageHandles = new Dictionary<long, int>();

        static UserImageManager()
        {
            LoadCachedUser();
        }

        private static void ClearCache()
        {
        }

        private static void LoadCachedUser()
        {
            DX.SetUseASyncLoadFlag(DX.TRUE);
            var path = Path.Combine(CommonObjects.DataDirectory, "icon_cache");
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            var di = new DirectoryInfo(path);
            foreach (var i in di.GetFiles())
            {
                ImageHandles[Convert.ToInt64(Path.GetFileNameWithoutExtension(i.Name))] = DX.LoadGraph(i.FullName);
            }
        }

        public static int GetUserImage(User user)
        {
            if (user == null || user.Id == null) return 0;
            if (!ImageHandles.ContainsKey((long)user.Id))
            {
                var target = Path.Combine(
                            CommonObjects.DataDirectory,
                            "icon_cache",
                            string.Format("{0}.{1}", user.Id, "png"));
                if (!File.Exists(target))
                    using (var wc = new WebClient())
                    using (var st = wc.OpenRead(user.ProfileImageUrlHttps))
                    {
                        var bm = new Bitmap(st);
                        var sav = new Bitmap(bm, 32, 32);
                        try
                        {
                            sav.Save(target);
                        }
                        catch
                        {

                        }

                    }
                ImageHandles[(long)user.Id] = DX.LoadGraph(target);
            }
            return ImageHandles[(long)user.Id];
        }

        private static async Task<int> GetUserImageTaskAsync(User user)
        {
            if (user == null || user.Id == null) return 0;
            if (!ImageHandles.ContainsKey((long)user.Id))
            {
                using (var wc = new WebClient())
                {
                    var target = Path.Combine(
                            CommonObjects.DataDirectory,
                            "icon_cache",
                            string.Format(
                                "{0}.{1}",
                                user.Id,
                                Path.GetFileNameWithoutExtension(user.ProfileImageUrlHttps.ToString().Split('/').Last())
                                )
                            );
                    await wc.DownloadFileTaskAsync(user.ProfileImageUrlHttps, target);
                    ImageHandles[(long)user.Id] = DX.LoadGraph(target);
                }

            }
            return ImageHandles[(long)user.Id];
        }
    }
}

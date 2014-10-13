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
                using (var wc = new WebClient())
                {
                    var target = Path.Combine(
                            CommonObjects.DataDirectory,
                            "icon_cache",
                            string.Format(
                                "{0}.{1}",
                                user.Id,
                                "png"
                                )
                            );
                    using (var st = wc.OpenRead(user.ProfileImageUrlHttps))
                    {
                        var bm = new Bitmap(st);
                        var sav = new Bitmap(bm, 32, 32);
                        sav.Save(target);
                    }

                    ImageHandles[(long)user.Id] = DX.LoadGraph(target);
                }

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

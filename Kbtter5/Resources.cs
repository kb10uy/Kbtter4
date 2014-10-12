using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using DxLibDLL;
using CoreTweet;

namespace Kbtter5
{
    public static class CommonObjects
    {
        public static int ImageLoadingCircle32 = LoadCommonImage("loading32.png");
        public static int ImageLogo = LoadCommonImage("Kbtter5.png");

        public static int FontSystem = DX.CreateFontToHandle("メイリオ", 16, 1, DX.DX_FONTTYPE_ANTIALIASING);
        public static int FontBullet = DX.CreateFontToHandle("メイリオ", 20, 2, DX.DX_FONTTYPE_ANTIALIASING);

        #region ユーティリティ

        public static string DataDirectory = "Kbtter5Data";

        public static int LoadCommonImage(string fname)
        {
            return DX.LoadGraph(Path.Combine("Kbtter5Data", "img", fname));
        }

        #endregion
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
                    wc.DownloadFile(user.ProfileImageUrlHttps, target);
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

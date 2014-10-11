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
        public static int Logo = LoadCommonImage("Kbtter5.png");

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
            var di = new DirectoryInfo(Path.Combine(CommonObjects.DataDirectory, "icon_cache"));
            foreach (var i in di.GetFiles())
            {
                ImageHandles[Convert.ToInt64(Path.GetFileNameWithoutExtension(i.Name))] = DX.LoadGraph(i.FullName);
            }
        }

        private static int GetUserImage(User user)
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

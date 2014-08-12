using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;


namespace Kbtter4.Updater
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Kbtter4 アップデーター");
            try
            {
                var u=File.ReadAllLines(".\\update.txt");
                Console.WriteLine("--------------------------------");
                Console.WriteLine("{0} 更新情報",u[1]);
                foreach (var i in u.Skip(3))
                {
                    Console.WriteLine(i);
                }
                Console.WriteLine("--------------------------------");
                Console.WriteLine("Enterキーでファイルの更新を開始します");
                Console.ReadLine();

                Directory.SetCurrentDirectory(".\\");
                var cp = Path.GetFullPath(".\\..\\");
                CopyDirectory(".\\", cp);

                Console.WriteLine();
                Console.WriteLine("完了しました!");
                Console.WriteLine("Enterキーで終了しKbtter4を再起動します");
                Console.ReadLine();
                Process.Start("..\\Kbtter4.exe");
            }
            catch
            {

            }
        }

        static void CopyDirectory(string ep, string cp)
        {
            foreach (var i in Directory.GetFiles(ep))
            {
                Console.WriteLine(i);
                File.Copy(i, cp + "/" + Path.GetFileName(i),true);
            }
            foreach (var i in Directory.GetDirectories(ep))
            {
                CopyDirectory(i, cp);
            }
        }
    }
}

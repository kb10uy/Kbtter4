using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreTweet;
using Kbtter4.Ayaya;
using Kbtter4.Tenko;

namespace Kbtter4.Models
{
    public sealed partial class Kbtter
    {
        private string CommandUpdate(IDictionary<string, object> args)
        {
            if (AuthenticatedUser == null) return "ログインしてください。";
            if (args["text"] as string == "") return "テキストを入力してください。";
            Token.Statuses.UpdateAsync(status => args["text"]);
            return "投稿しました。";
        }

        private string CommandLouise(IDictionary<string, object> args)
        {
            return "ルイズ！ルイズ！ルイズ！ルイズぅぅうううわぁああああああああああああああああああああああん！！！\n" +
                    "あぁああああ…ああ…あっあっー！あぁああああああ！！！ルイズルイズルイズぅううぁわぁああああ！！！\n" +
                    "あぁクンカクンカ！クンカクンカ！スーハースーハー！スーハースーハー！いい匂いだなぁ…くんくん\n" +
                    "んはぁっ！ルイズ・フランソワーズたんの桃色ブロンドの髪をクンカクンカしたいお！クンカクンカ！あぁあ！！\n" +
                    "間違えた！モフモフしたいお！モフモフ！モフモフ！髪髪モフモフ！カリカリモフモフ…きゅんきゅんきゅい！！\n" +
                    "小説12巻のルイズたんかわいかったよぅ！！あぁぁああ…あああ…あっあぁああああ！！ふぁぁあああんんっ！！\n" +
                    "アニメ2期放送されて良かったねルイズたん！あぁあああああ！かわいい！ルイズたん！かわいい！あっああぁああ！\n" +
                    "コミック2巻も発売されて嬉し…いやぁああああああ！！！にゃああああああああん！！ぎゃああああああああ！！\n" +
                    "ぐあああああああああああ！！！コミックなんて現実じゃない！！！！あ…小説もアニメもよく考えたら…\n" +
                    "ル イ ズ ち ゃ ん は 現実 じ ゃ な い？にゃあああああああああああああん！！うぁああああああああああ！！\n" +
                    "そんなぁああああああ！！いやぁぁぁあああああああああ！！はぁああああああん！！ハルケギニアぁああああ！！\n" +
                    "この！ちきしょー！やめてやる！！現実なんかやめ…て…え！？見…てる？表紙絵のルイズちゃんが僕を見てる？\n" +
                    "表紙絵のルイズちゃんが僕を見てるぞ！ルイズちゃんが僕を見てるぞ！挿絵のルイズちゃんが僕を見てるぞ！！\n" +
                    "アニメのルイズちゃんが僕に話しかけてるぞ！！！よかった…世の中まだまだ捨てたモンじゃないんだねっ！\n" +
                    "いやっほぉおおおおおおお！！！僕にはルイズちゃんがいる！！やったよケティ！！ひとりでできるもん！！！\n" +
                    "あ、コミックのルイズちゃああああああああああああああん！！いやぁあああああああああああああああ！！！！\n" +
                    "あっあんああっああんあアン様ぁあ！！シ、シエスター！！アンリエッタぁああああああ！！！タバサｧぁあああ！！\n" +
                    "ううっうぅうう！！俺の想いよルイズへ届け！！ハルゲニアのルイズへ届け";
        }

        public string CommandEternalForceBlizzard(IDictionary<string, object> args)
        {
            if (AuthenticatedUser == null) return "ログインしてください。";
            int c = 100;
            var un = args["user"] as string;
            if (args.ContainsKey("count")) c = (int)args["count"];
            Task.Run(() =>
            {
                var tl = Token.Statuses.UserTimeline(screen_name => un, count => c);
                foreach (var p in tl)
                {
                    try
                    {
                        Token.Favorites.Create(id => p.Id);
                    }
                    catch { }
                }

            });

            return un + "さんの最新ツイート" + c.ToString() + "件をエターナルフォースブリザードしました。";
        }

        private async Task<string> CommandShindanMakerDirectly(IDictionary<string, object> args)
        {
            if (AuthenticatedUser == null) return "ログインしてください。";
            string name;
            if (args.ContainsKey("name"))
            {
                name = args["name"] as string;
            }
            else
            {
                name = AuthenticatedUser.Name;
            }
            var id = (int)args["id"];
            var ret = await Kbtter4ShindanMakerCooperator.DiagnoseAsync(id, name);
            if (ret == null) return "取得できませんでした。";
            if (args.ContainsKey("tweet") && (bool)args["tweet"])
            {
                try
                {
                    await Token.Statuses.UpdateAsync(status => ret + " shindanmaker.com/" + id.ToString());
                }
                catch (TwitterException e)
                {
                    return ret + "\n\nツイートは出来ませんでした : " + e.Message;
                }
                ret += "\nツイートできました。";
            }
            return ret;
        }

        private string CommandExecuteNotepad(IDictionary<string, object> args)
        {
            try
            {
                Process.Start("notepad");
                return "起動しました。@paralleltreeへメモ帳愛を捧げましょう。";
            }
            catch
            {
                return "notepad.exeを起動てきませんでした。";
            }
        }

        private string CommandExecuteMspaint(IDictionary<string, object> args)
        {
            try
            {
                Process.Start("mspaint");
                return "起動しました。";
            }
            catch
            {
                return "mspaint.exeを起動てきませんでした。";
            }
        }
    }
}

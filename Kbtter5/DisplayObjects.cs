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
    public class DisplayObject
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double ActualX { get { return X + ParentManager.OffsetX; } }
        public double ActualY { get { return Y + ParentManager.OffsetY; } }
        public double HomeX { get; set; }
        public double HomeY { get; set; }
        public double ScaleX { get; set; }
        public double ScaleY { get; set; }
        public double Angle { get; set; }
        public double Alpha { get; set; }
        public double ActualAlpha { get { return Alpha * ParentManager.Alpha; } }
        public ObjectKind MyKind { get; set; }
        public ObjectKind TargetKind { get; set; }
        public ObjectKind DamageKind { get; set; }
        public double CollisionRadius { get; set; }
        public double GrazeRadius { get; set; }
        public ObjectManager ParentManager { get; set; }
        public int Layer { get; set; }
        public virtual bool IsDead { get; set; }

        public IEnumerator<bool> TickCoroutine { get; protected set; }
        public virtual IEnumerator<bool> Tick()
        {
            while (true) yield return true;
        }

        public IEnumerator<bool> DrawCoroutine { get; protected set; }
        public virtual IEnumerator<bool> Draw()
        {
            while (true) yield return true;
        }

        public DisplayObject()
        {
            ScaleX = 1;
            ScaleY = 1;
            Alpha = 1;
            TickCoroutine = Tick();
            DrawCoroutine = Draw();
            MyKind = ObjectKind.None;
            TargetKind = ObjectKind.None;
            DamageKind = ObjectKind.None;
        }
    }

    public class ExpandableDisplayObject : DisplayObject
    {
        public ObjectManager Manager { get; protected set; }
        public IEnumerator<bool> ExecuteCoroutine { get; protected set; }

        public ExpandableDisplayObject()
        {
            Manager = new ObjectManager(4);
            ExecuteCoroutine = Execute();
        }

        public virtual IEnumerator<bool> Execute()
        {
            while (true) yield return true;
        }

        public override IEnumerator<bool> Tick()
        {
            while (true)
            {
                IsDead = !(ExecuteCoroutine.MoveNext() && ExecuteCoroutine.Current);
                Manager.TickAll();
                yield return true;
            }
        }

        public override IEnumerator<bool> Draw()
        {
            while (true)
            {
                Manager.OffsetX = ActualX;
                Manager.OffsetY = ActualY;
                Manager.Alpha = ActualAlpha;
                Manager.DrawAll();
                yield return true;
            }
        }
    }

    public class Sprite : DisplayObject
    {
        public int Image { get; set; }
        protected bool IsImageLoaded { get; set; }

        public Sprite()
        {

        }

        public override IEnumerator<bool> Draw()
        {
            while (true)
            {
                if (IsImageLoaded)
                {
                    DX.SetDrawBlendMode(DX.DX_BLENDMODE_ALPHA, (int)(ActualAlpha * 255));
                    DX.DrawRotaGraph3F((float)ActualX, (float)ActualY, (float)HomeX, (float)HomeY, ScaleX, ScaleY, Angle, Image, DX.TRUE);
                }
                else
                {
                    IsImageLoaded = DX.CheckHandleASyncLoad(Image) == DX.FALSE;
                }
                yield return true;
            }
        }
    }

    public class CoroutineSprite : Sprite
    {
        public IEnumerator<bool> Operation { get; protected set; }

        public CoroutineSprite(CoroutineFunction<CoroutineSprite> op)
        {
            Operation = op(this);
        }

        public override IEnumerator<bool> Tick()
        {
            while (!(IsDead = !(Operation.MoveNext() && Operation.Current))) yield return true;
        }
    }

    //今までのデリゲートをひとまとめ
    public delegate IEnumerator<bool> CoroutineFunction<TValue>(TValue value);
    public delegate IEnumerator<bool> CoroutineFunction<TValue1, TValue2>(TValue1 value1, TValue2 value2);

    public class UserSprite : Sprite
    {
        protected static int EnemyBulletLayer = (int)GameLayer.EnemyBullet;
        protected static int EnemyLayer = (int)GameLayer.Enemy;
        protected static int PlayerLayer = (int)GameLayer.Player;
        protected static int PlayerBulletLayer = (int)GameLayer.PlayerBullet;
        protected static int EffectLayer = (int)GameLayer.Effect;
        public User SourceUser { get; protected set; }


        public UserSprite()
        {
            Image = CommonObjects.ImageLoadingCircle32;
            HomeX = 16;
            HomeY = 16;
            CollisionRadius = 6;
            GrazeRadius = 8;
        }
    }

    public class NumberSprite : DisplayObject
    {
        public int Digits { get; set; }
        public double DigitX { get; protected set; }
        public double DigitY { get; protected set; }
        public int Value { get; set; }
        public bool FillWithZero { get; set; }
        public IReadOnlyList<int> NumberImage { get; protected set; }

        public NumberSprite(IReadOnlyList<int> img, int x, int y, int digits)
        {
            Digits = digits;
            FillWithZero = false;
            NumberImage = img;
            Value = 0;
            DigitX = x;
            DigitY = y;
        }

        public override IEnumerator<bool> Draw()
        {
            while (true)
            {
                var v = Value;
                var reald = (int)(Math.Log10(Value) + 1);
                int[] ls = new int[Digits];
                DX.SetDrawBlendMode(DX.DX_BLENDMODE_ALPHA, (int)(ActualAlpha * 255));
                for (int i = Digits - 1; i >= 0; i--)
                {
                    DX.DrawGraphF((float)(ActualX + DigitX * i - HomeX), (float)(ActualY - HomeY), NumberImage[(Digits - 1 - i < reald) ? v % 10 : FillWithZero ? 0 : 10], DX.TRUE);
                    v /= 10;
                }
                yield return true;
            }
        }
    }

    public class ScoreSprite : NumberSprite
    {
        public ScoreSprite(IReadOnlyList<int> img, int x, int y, int pt)
            : base(img, x, y, (int)(Math.Log10(pt) + 1))
        {
            HomeX = x * Digits / 2;
            HomeY = y / 2;
            Value = pt;
        }

        public override IEnumerator<bool> Tick()
        {
            for (int i = 0; i < 20; i++)
            {
                Y--;
                Alpha -= 1.0 / 20.0;
                yield return true;
            }
        }
    }

    public class StringSprite : DisplayObject
    {
        public string Value { get; set; }
        public int FontHandle { get; set; }
        public int Color { get; set; }

        public StringSprite(int font, int col)
        {
            FontHandle = font;
            Color = col;
        }

        public override IEnumerator<bool> Draw()
        {
            while (true)
            {
                DX.SetDrawBlendMode(DX.DX_BLENDMODE_ALPHA, (int)(ActualAlpha * 255));
                DX.DrawStringFToHandle((float)(ActualX - HomeX), (float)(ActualY - HomeY), Value, Color, FontHandle);
                yield return true;
            }
        }
    }

    public class AdditionalCoroutineSprite : Sprite
    {
        public IEnumerator<bool> SpecialOperation { get; protected set; }

        public override IEnumerator<bool> Tick()
        {
            while (true)
            {
                ProcessOperations();
                yield return true;
            }
        }

        public virtual void ProcessOperations()
        {
            SpecialOperation = (SpecialOperation != null && SpecialOperation.MoveNext() && SpecialOperation.Current) ? SpecialOperation : null;
        }

        public void ApplySpecialOperation(CoroutineFunction<AdditionalCoroutineSprite> pat)
        {
            SpecialOperation = pat(this);
        }
    }

    public class MultiAdditionalCoroutineSprite : AdditionalCoroutineSprite
    {
        public IReadOnlyList<IEnumerator<bool>> AvailableSubOperations { get { return operations; } }

        private bool taken;
        private List<IEnumerator<bool>> operations = new List<IEnumerator<bool>>(), bufop = new List<IEnumerator<bool>>();

        public override IEnumerator<bool> Tick()
        {
            while (true)
            {
                ProcessOperations();
                yield return true;
            }
        }

        public override void ProcessOperations()
        {
            SpecialOperation = (SpecialOperation != null && SpecialOperation.MoveNext() && SpecialOperation.Current) ? SpecialOperation : null;
            taken = true;
            operations.RemoveAll(p => !(p.MoveNext() && p.Current));
            taken = false;
            operations.AddRange(bufop);
            bufop.Clear();
        }

        public void AddSubOperation(CoroutineFunction<MultiAdditionalCoroutineSprite> pat)
        {
            if (taken)
            {
                operations.Add(pat(this));
            }
            else
            {
                bufop.Add(pat(this));
            }
        }

        public void AddSubOperation(CoroutineFunction<AdditionalCoroutineSprite> pat)
        {
            if (taken)
            {
                operations.Add(pat(this));
            }
            else
            {
                bufop.Add(pat(this));
            }
        }
    }

    public class KeyInputObject : DisplayObject, IDisposable
    {
        public int KeyInputHandle { get; protected set; }
        public string InputString { get; protected set; }
        public int Font { get; set; }
        public bool IsCanceled { get; protected set; }
        public bool HasCompleted { get; protected set; }

        public int NormalStringColor { get; set; }
        public int NormalCursorColor { get; set; }
        public int IMEBackgroundColor { get; set; }
        public int IMECursorColor { get; set; }
        public int IMEUnderlineColor { get; set; }
        public int IMESuggestionStringColor { get; set; }
        public int IMEInputModeColor { get; set; }
        public int SelectBackgroundColor { get; set; }
        public int SelectStringColor { get; set; }
        public int IMEWindowBackgroundColor { get; set; }

        public KeyInputObject(int font, int length, bool cancel, bool sc, bool num)
        {
            Font = font;
            KeyInputHandle = DX.MakeKeyInput(length, cancel ? DX.TRUE : DX.FALSE, sc ? DX.TRUE : DX.FALSE, num ? DX.TRUE : DX.FALSE);
            DX.SetActiveKeyInput(KeyInputHandle);
            NormalStringColor = CommonObjects.Colors.Black;
            NormalCursorColor = CommonObjects.Colors.Green;
            IMEBackgroundColor = CommonObjects.Colors.Yellow;
            IMECursorColor = CommonObjects.Colors.Red;
            IMEUnderlineColor = CommonObjects.Colors.Black;
            IMESuggestionStringColor = CommonObjects.Colors.White;
            IMEInputModeColor = CommonObjects.Colors.Black;
            SelectBackgroundColor = CommonObjects.Colors.Azure;
            SelectStringColor = CommonObjects.Colors.White;
            IMEWindowBackgroundColor = CommonObjects.Colors.LightGray;
        }

        public override IEnumerator<bool> Tick()
        {
            while (true)
            {
                var st = DX.CheckKeyInput(KeyInputHandle);
                var sb = new StringBuilder();
                DX.GetKeyInputString(sb, KeyInputHandle);
                InputString = sb.ToString();
                switch (st)
                {
                    case 0:
                        break;
                    case 1:
                        IsDead = true;
                        HasCompleted = true;
                        break;
                    case 2:
                        IsDead = true;
                        IsCanceled = true;
                        break;
                }
                yield return true;
            }
        }

        public override IEnumerator<bool> Draw()
        {
            while (true)
            {
                DX.SetKeyInputStringColor(
                    (ulong)NormalStringColor,
                    (ulong)NormalCursorColor,
                    (ulong)IMEBackgroundColor,
                    (ulong)IMECursorColor,
                    (ulong)IMEUnderlineColor,
                    (ulong)IMESuggestionStringColor,
                    (ulong)IMEInputModeColor,
                    0, 0, 0, 0,
                    (ulong)IMEWindowBackgroundColor,
                    (ulong)SelectBackgroundColor,
                    (ulong)SelectStringColor,
                    0);
                DX.SetKeyInputStringFont(Font);
                DX.SetDrawBlendMode(DX.DX_BLENDMODE_ALPHA, (int)(Alpha * 255));
                DX.DrawKeyInputString((int)(ActualX - HomeX), (int)(ActualY - HomeY), KeyInputHandle);
                yield return true;
            }
        }

        public void Dispose()
        {
            IsDead = true;
            DX.DeleteKeyInput(KeyInputHandle);
        }
    }

    public class GraphicalObject : DisplayObject
    {
        public int Color { get; set; }
        public bool AllowFill { get; set; }
    }

    public class CircleObject : GraphicalObject
    {
        public double Radius { get; set; }

        public CircleObject()
        {
            Radius = 0;
        }

        public override IEnumerator<bool> Draw()
        {
            while (true)
            {
                DX.SetDrawBlendMode(DX.DX_BLENDMODE_ALPHA, (int)(ActualAlpha * 255));
                DX.DrawOval((int)(ActualX - HomeX), (int)(ActualY - HomeY), (int)Radius, (int)Radius, Color, AllowFill ? DX.TRUE : DX.FALSE);
                yield return true;
            }
        }
    }

    public class RectangleObject : GraphicalObject
    {
        public double Width { get; set; }
        public double Height { get; set; }

        public RectangleObject()
        {

        }

        public override IEnumerator<bool> Draw()
        {
            while (true)
            {
                DX.SetDrawBlendMode(DX.DX_BLENDMODE_ALPHA, (int)(ActualAlpha * 255));
                DX.DrawBox((int)(ActualX - HomeX), (int)(ActualY - HomeY), (int)(ActualX - HomeX + Width), (int)(ActualY - HomeY + Height), Color, AllowFill ? DX.TRUE : DX.FALSE);
                yield return true;
            }
        }
    }

    public class LineObject : GraphicalObject
    {
        public double Length { get; set; }

        public LineObject()
        {
            Length = 0;
        }

        public override IEnumerator<bool> Draw()
        {
            while (true)
            {
                var dx = Math.Cos(Angle) * Length;
                var dy = Math.Sin(Angle) * Length;
                DX.SetDrawBlendMode(DX.DX_BLENDMODE_ALPHA, (int)(ActualAlpha * 255));
                DX.DrawLine((int)(ActualX - HomeX), (int)(ActualY - HomeY), (int)(ActualX - HomeX + dx), (int)(ActualY - HomeY + dy), Color);
                yield return true;
            }
        }
    }

    public struct Point
    {
        public double X;
        public double Y;
    }

    [Flags]
    public enum ObjectKind
    {
        None = 1,
        Player = 2,
        Enemy = 4,
        PlayerBullet = 8,
        EnemyBullet = 16,
    }

    public enum GameLayer
    {
        Background = 0,
        EnemyBullet,
        Enemy,
        PlayerBullet,
        Player,
        Effect,
        Information,
    }
}
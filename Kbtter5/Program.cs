using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Kbtter4.Models;
using CoreTweet;
using CoreTweet.Streaming;
using Newtonsoft.Json;
using DxLibDLL;

namespace Kbtter5
{
    class Program
    {
        static void Main(string[] args)
        {
            DX.ChangeWindowMode(DX.TRUE);
            if (DX.DxLib_Init() == -1) return;
            DX.SetAlwaysRunFlag(DX.TRUE);
            DX.SetDrawScreen(DX.DX_SCREEN_BACK);
            DX.SetWindowText("Kbtter5 Polyvinyl Chroride");
            DX.SetUseASyncLoadFlag(DX.TRUE);

            Kbtter5.Instance.Run();

            DX.DxLib_End();
        }
    }

    public sealed class Kbtter5
    {
        private Kbtter Kbtter = Kbtter.Instance;

        private static Kbtter5 ins = new Kbtter5();
        public static Kbtter5 Instance
        {
            get { return ins; }
        }

        private Kbtter5()
        {

        }

        private Scene curscene;
        public Scene CurrentScene
        {
            get { return curscene; }
            set
            {
                if (value != null)
                {
                    curscene = value;
                }
                else
                {
                    curscene = new Scene();
                }
            }
        }

        public void Run()
        {
            CurrentScene = new SceneTitle();
            while (DX.ProcessMessage() != -1)
            {
                CurrentScene.TickCoroutine.MoveNext();
                DX.ClearDrawScreen();
                CurrentScene.DrawCoroutine.MoveNext();
                DX.ScreenFlip();
            }
        }
    }

    public class Scene
    {
        public ObjectManager Manager { get; protected set; }
        public IEnumerator<bool> DrawCoroutine { get; private set; }
        public IEnumerator<bool> TickCoroutine { get; private set; }

        public Scene()
        {
            Manager = new ObjectManager(16);
            TickCoroutine = Tick();
            DrawCoroutine = Draw();
        }

        public virtual IEnumerator<bool> Draw()
        {
            while (true) yield return true;
        }

        public virtual IEnumerator<bool> Tick()
        {
            while (true) yield return true;
        }
    }

    public class ObjectManager : IEnumerable<DisplayObject>
    {
        private List<List<DisplayObject>> layers;
        private List<List<DisplayObject>> bufferedlayers;
        private bool taken = true;

        public IReadOnlyList<IReadOnlyList<DisplayObject>> Layers
        {
            get { return layers; }
        }

        public ObjectManager(int layerCount)
        {
            layers = new List<List<DisplayObject>>(layerCount);
            for (int i = 0; i < layerCount; i++) layers.Add(new List<DisplayObject>(1024));
            bufferedlayers = new List<List<DisplayObject>>(layerCount);
            for (int i = 0; i < layerCount; i++) bufferedlayers.Add(new List<DisplayObject>(256));
        }

        public void Add(DisplayObject item, int layer)
        {
            item.ParentManager = this;
            item.Layer = layer;
            if (taken) bufferedlayers[layer].Add(item);
            else layers[layer].Add(item);
        }

        public void AddRangeTo(IEnumerable<DisplayObject> collection, int layer)
        {
            foreach (var i in collection)
            {
                i.ParentManager = this;
                i.Layer = layer;
            }
            if (taken) bufferedlayers[layer].AddRange(collection);
            else layers[layer].AddRange(collection);
        }

        private void Remove(DisplayObject item)
        {
            for (int i = 0; i < layers.Count; i++)
                layers[i].Remove(item);
        }

        private void RemoveAll(Predicate<DisplayObject> pred)
        {
            for (int i = 0; i < layers.Count; i++)
                layers[i].RemoveAll(pred);
        }

        private void RemoveAllInLayer(int layer, Predicate<DisplayObject> match)
        {
            layers[layer].RemoveAll(match);
        }

        private void ClearLayer(int layer)
        {
            layers[layer].Clear();
        }

        public void TickAll()
        {
            for (int i = 0; i < layers.Count; i++)
            {
                taken = true;
                foreach (var item in layers[i].AsParallel()) item.IsDead = !(item.TickCoroutine.MoveNext() && item.TickCoroutine.Current && !item.IsDead);
                taken = false;
                layers[i].AddRange(bufferedlayers[i]);
                bufferedlayers[i].Clear();
            }
            RemoveAll(p => p.IsDead);
        }

        public void DrawAll()
        {
            for (int i = 0; i < layers.Count; i++)
            {
                taken = true;
                foreach (var item in layers[i]) item.DrawCoroutine.MoveNext();
                taken = false;
            }
        }

        public int Count { get { return layers.Sum(p => p.Count); } }

        public IEnumerator<DisplayObject> GetEnumerator()
        {
            for (int i = 0; i < layers.Count; i++)
                foreach (var item in layers[i].AsParallel())
                    yield return item;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            for (int i = 0; i < layers.Count; i++)
                foreach (var item in layers[i].AsParallel())
                    yield return item;
        }
    }
}

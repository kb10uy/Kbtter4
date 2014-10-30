using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kbtter5
{
    public class Scene
    {
        public ObjectManager Manager { get; protected set; }
        public IEnumerator<bool> DrawCoroutine { get; private set; }
        public IEnumerator<bool> TickCoroutine { get; private set; }
        public ChildSceneManager Children { get; private set; }

        public Scene()
        {
            Manager = new ObjectManager(16);
            Children = new ChildSceneManager(this);
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

        public virtual void SendChildMessage(string mes)
        {

        }
    }

    public class ChildScene : Scene
    {
        public IEnumerator<bool> ExecuteCoroutine { get; private set; }
        public bool IsDead { get; protected set; }
        public Scene Parent { get; set; }

        public ChildScene()
        {
            ExecuteCoroutine = Execute();
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
                Manager.DrawAll();
                yield return true;
            }
        }

        public virtual IEnumerator<bool> Execute()
        {
            while (true) yield return true;
        }
    }

    public class ChildSceneManager
    {
        private List<ChildScene> scenes;
        public IReadOnlyList<ChildScene> Scenes { get { return scenes; } }
        public Scene Parent { get; private set; }

        public ChildSceneManager(Scene par)
        {
            Parent = par;
            scenes = new List<ChildScene>();
        }

        public void AddChildScene(ChildScene cs)
        {
            cs.Parent = Parent;
            scenes.Add(cs);
        }

        public void TickAll()
        {
            foreach (var i in scenes) i.TickCoroutine.MoveNext();
            scenes.RemoveAll(i => i.IsDead);
        }

        public void DrawAll()
        {
            foreach (var i in scenes) i.DrawCoroutine.MoveNext();
        }
    }

    public class ObjectManager : IEnumerable<DisplayObject>
    {
        private List<List<DisplayObject>> layers;
        private List<List<DisplayObject>> bufferedlayers;
        private bool taken = true;

        public double OffsetX { get; set; }
        public double OffsetY { get; set; }

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
            collection = collection.Select(p =>
            {
                p.ParentManager = this;
                p.Layer = layer;
                return p;
            });
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

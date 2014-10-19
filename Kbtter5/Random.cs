using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kbtter5
{
    public sealed class Xorshift128Random : Random
    {
        public uint StateX { get; private set; }
        public uint StateY { get; private set; }
        public uint StateZ { get; private set; }
        public uint StateW { get; private set; }

        public Xorshift128Random()
            : this((int)DateTime.Now.Ticks)
        {

        }

        public Xorshift128Random(int seed)
        {
            var sr = new Random(seed);
            var sba = new byte[13];
            sr.NextBytes(sba);
            StateX = BitConverter.ToUInt32(sba, 0);
            StateY = BitConverter.ToUInt32(sba, 3);
            StateZ = BitConverter.ToUInt32(sba, 6);
            StateW = BitConverter.ToUInt32(sba, 9);
        }

        public Xorshift128Random(uint x, uint y, uint z, uint w)
        {
            StateX = x;
            StateY = y;
            StateZ = z;
            StateW = w;
        }



        public uint NextUInt32()
        {
            var t = StateX ^ (StateX << 11);
            StateX = StateY;
            StateY = StateZ;
            StateZ = StateW;
            return StateW = (StateW ^ (StateW >> 19)) ^ (t ^ (t >> 8));
        }

        public override int Next()
        {
            return (int)NextUInt32();
        }

        public override int Next(int maxValue)
        {
            return (int)(NextUInt32() % maxValue);
        }

        public override int Next(int minValue, int maxValue)
        {
            return Next(maxValue - minValue) + minValue;
        }

        public override double NextDouble()
        {
            return NextUInt32() / 4294967296.0;
        }

        public override void NextBytes(byte[] buffer)
        {
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = (byte)Next(byte.MaxValue + 1);
            }
        }

        protected override double Sample()
        {
            return NextDouble();
        }
    }
}

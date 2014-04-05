using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WiiLib
{
    public class Acceleration
    {
        public Acceleration(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
    }
}

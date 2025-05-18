using Silk.NET.Maths;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace repulo_project
{
    internal class Ring
    {
        public Vector3D<float> Position;
        public float Radius;
        public GlObject Torus;
        public Matrix4X4<float> RotationMatrix = Matrix4X4<float>.Identity;
        public Ring(Vector3D<float> position, float radius, GlObject torus)
        {
            Position = position;
            Radius = radius;
            Torus = torus;
        }
    }
}

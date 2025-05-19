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
        public Vector3D<float> BasePosition;
        public float AnimationPhase; // optional offset per ring
        public float AnimationSpeed = 2f;
        public float AnimationDistance = 20f;
        public Matrix4X4<float> RotationMatrix = Matrix4X4<float>.Identity;
        public Vector4D<float> Color;
        public bool Passed = false;
        public Ring(Vector3D<float> position, float radius, GlObject torus)
        {
            Position = position;
            Radius = radius;
            BasePosition = position;
            Torus = torus;
            Color = new Vector4D<float>(0f, 1f, 0f, 1f);
        }
    }
}

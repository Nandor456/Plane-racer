using Silk.NET.Maths;

namespace repulo_project
{
    internal class CameraDescriptor
    {
        private float DistanceToPlane = 80f;

        public Vector3D<float> TargetPosition = Vector3D<float>.Zero;
        public float PlaneYaw = 0f;
        public bool CameraInFront = false;

        public Vector3D<float> Target => TargetPosition;

        public Vector3D<float> Position
        {
            get
            {
                var direction = Vector3D.Transform(new Vector3D<float>(0, 0, 1), Quaternion<float>.CreateFromYawPitchRoll(PlaneYaw, 0, 0));
                var offset = (CameraInFront ? -direction : direction) * DistanceToPlane + new Vector3D<float>(0, 30, 0);
                return TargetPosition + offset;
            }
        }

        public Vector3D<float> UpVector => Vector3D<float>.UnitY;
    }
}
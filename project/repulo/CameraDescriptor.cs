using Silk.NET.Maths;
using Silk.NET.Vulkan;

namespace repulo
{
    internal class CameraDescriptor
    {
        private double DistanceToOrigin = 60 * (2 * DistanceScaleFactor);

        private double AngleToZYPlane = 0;

        private double AngleToZXPlane = 0.55 - AngleChangeStepSize;

        private const double DistanceScaleFactor = 1.1;

        private const double AngleChangeStepSize = Math.PI / 180 * 5;

        private Vector3D<float> planePosition = Vector3D<float>.Zero;

        private Quaternion<float> planeRotation = Quaternion<float>.Identity;

        enum CameraMode { Orbit, Chase }
        CameraMode CurrentMode = CameraMode.Orbit;


        /// <summary>
        /// Gets the position of the camera.
        /// </summary>
        private float _distanceBehind = 20f;
        private float _heightAbove = 5f;

        public float DistanceBehind
        {
            get => _distanceBehind;
            set => _distanceBehind = value;
        }

        public float HeightAbove
        {
            get => _heightAbove;
            set => _heightAbove = value;
        }
        public void ToggleCameraMode()
        {
            CurrentMode = CurrentMode == CameraMode.Orbit ? CameraMode.Chase : CameraMode.Orbit;
        }
        public Vector3D<float> Position
        {
            get
            {
                if (CurrentMode == CameraMode.Chase)
                {
                    Vector3D<float> forward = Vector3D.Normalize(new Vector3D<float>(0, 0, -1));
                    Vector3D<float> rotatedForward = Vector3D.Transform(forward, planeRotation);
                    Vector3D<float> offset = -rotatedForward * _distanceBehind + new Vector3D<float>(0, _heightAbove, 0);
                    return planePosition + offset;
                }
                else // Orbit
                {
                    float x = (float)(DistanceToOrigin * Math.Cos(AngleToZYPlane) * Math.Sin(AngleToZXPlane));
                    float y = (float)(DistanceToOrigin * Math.Sin(AngleToZYPlane));
                    float z = (float)(DistanceToOrigin * Math.Cos(AngleToZYPlane) * Math.Cos(AngleToZXPlane));
                    return planePosition + new Vector3D<float>(x, y, z);
                }
            }
        }




        public void UpdatePlanePosition(Vector3D<float> newPos)
        {
            planePosition = newPos;
        }

        public void UpdatePlaneTransform(Vector3D<float> position, Quaternion<float> rotation)
        {
            planePosition = position;
            planeRotation = rotation;
        }

        public Vector3D<float> Target => planePosition;

        /// <summary>
        /// Gets the up vector of the camera.
        /// </summary>
        /// 
        public Vector3D<float> UpVector
        {
            get
            {
                // Use plane's rotation to determine up vector
                Vector3D<float> up = Vector3D.Normalize(new Vector3D<float>(0, 1, 0));
                return Vector3D.Transform(up, planeRotation);
            }
        }

        /// <summary>
        /// Gets the target point of the camera view.
        /// </summary>

        public void IncreaseZXAngle()
        {
            AngleToZXPlane += AngleChangeStepSize;
        }

        public void DecreaseZXAngle()
        {
            AngleToZXPlane -= AngleChangeStepSize;
        }

        public void IncreaseZYAngle()
        {
            AngleToZYPlane += AngleChangeStepSize;

        }

        public void DecreaseZYAngle()
        {
            AngleToZYPlane -= AngleChangeStepSize;
        }

        public void IncreaseDistance()
        {
            DistanceToOrigin = DistanceToOrigin * DistanceScaleFactor;
        }

        public void DecreaseDistance()
        {
            DistanceToOrigin = DistanceToOrigin / DistanceScaleFactor;
        }

        private static Vector3D<float> GetPointFromAngles(double distanceToOrigin, double angleToMinZYPlane, double angleToMinZXPlane)
        {
            var x = distanceToOrigin * Math.Cos(angleToMinZXPlane) * Math.Sin(angleToMinZYPlane);
            var z = distanceToOrigin * Math.Cos(angleToMinZXPlane) * Math.Cos(angleToMinZYPlane);
            var y = distanceToOrigin * Math.Sin(angleToMinZXPlane);

            return new Vector3D<float>((float)x, (float)y, (float)z);
        }
    }
}

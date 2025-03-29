using Silk.NET.Maths;
using System;

namespace Szeminarium
{
    internal class CameraDescriptor
    {
        public Vector3D<float> Position { get; private set; } = new Vector3D<float>(0, 0, 4);
        public Vector3D<float> Forward { get; private set; } = new Vector3D<float>(0, 0, -1);
        public Vector3D<float> Up { get; private set; } = new Vector3D<float>(0, 1, 0);

        private const float MoveSpeed = 0.1f;
        private const float RotationSpeed = 0.05f;

        /// <summary>
        /// Gets the up vector of the camera.
        /// </summary>
        public Vector3D<float> UpVector => Up;

        /// <summary>
        /// Gets the target point of the camera view.
        /// </summary>
        public Vector3D<float> Target => Position + Forward;

        public void MoveForward()
        {
            Position += Forward * MoveSpeed;
        }

        public void MoveBackward()
        {
            Position -= Forward * MoveSpeed;
        }

        public void MoveLeft()
        {
            var right = Vector3D.Normalize(Vector3D.Cross(Forward, Up));
            Position -= right * MoveSpeed;
        }

        public void MoveRight()
        {
            var right = Vector3D.Normalize(Vector3D.Cross(Forward, Up));
            Position += right * MoveSpeed;
        }

        public void MoveUp()
        {
            Position += Up * MoveSpeed;
        }

        public void MoveDown()
        {
            Position -= Up * MoveSpeed;
        }

        public void RotateLeft()
        {
            var rotation = Matrix4X4.CreateFromAxisAngle(Up, RotationSpeed);
            Forward = Vector3D.Transform(Forward, rotation);
        }

        public void RotateRight()
        {
            var rotation = Matrix4X4.CreateFromAxisAngle(Up, -RotationSpeed);
            Forward = Vector3D.Transform(Forward, rotation);
        }

        public void RotateUp()
        {
            var right = Vector3D.Normalize(Vector3D.Cross(Forward, Up));
            var rotation = Matrix4X4.CreateFromAxisAngle(right, RotationSpeed);
            Forward = Vector3D.Transform(Forward, rotation);
            Up = Vector3D.Transform(Up, rotation);
        }

        public void RotateDown()
        {
            var right = Vector3D.Normalize(Vector3D.Cross(Forward, Up));
            var rotation = Matrix4X4.CreateFromAxisAngle(right, -RotationSpeed);
            Forward = Vector3D.Transform(Forward, rotation);
            Up = Vector3D.Transform(Up, rotation);
        }
    }
}
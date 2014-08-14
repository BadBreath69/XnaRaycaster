using System;
using Microsoft.Xna.Framework;

namespace XnaRaycaster
{
    public class RayCasterCamera
    {
        public Vector2 Position;
        public Vector2 Direction;
        public Vector2 VectorPlane;

        public float VectorPlaneLength = 0.66f;
       
        public RayCasterCamera()
        {
            Position = new Vector2(21.5f, 13.5f);
            Direction = new Vector2(1, 0);
            Direction.Normalize();

            VectorPlane = new Vector2(0, -1) * VectorPlaneLength;
        }

        public void Rotate(float rads)
        {
            var rotMatrix = Matrix.CreateRotationZ(rads);

            var newDirection = Vector2.Transform(Direction, rotMatrix);
            Direction = newDirection;

            var newVectorPlane = Vector2.Transform(VectorPlane, rotMatrix);
            VectorPlane = newVectorPlane;
        }

        public Action<GameTime> Movement = null;

        public void Update(GameTime gameTime)
        {
            if (Movement != null) Movement.Invoke(gameTime);
        }

        public void MoveBackwards()
        {
            if (Movement == null)
            {
                var targetDirection = Direction * -1;
                var targetPosition = Position + targetDirection;
                
                Move(targetPosition, targetDirection);
            }
        }

        public void MoveForward()
        {
            if (Movement == null)
            {
                var targetPosition = (Position + Direction);
                Move(targetPosition, Direction);
            }
        }

        private void Move(Vector2 targetPosition, Vector2 targetDirection)
        {
            var vectorLength = (Position - targetPosition).Length();
           
            // Create the action
            Movement = (gameTime) =>
            {
                // Update the current position by a factor of the direction
                var updatePosition = (targetDirection / 350.0f) * gameTime.ElapsedGameTime.Milliseconds;
                Position += updatePosition;
                vectorLength -= updatePosition.Length();

                // Finished
                if (vectorLength <= 0)
                {
                    // Stop overshooting
                    Position = targetPosition;
                    Movement = null;
                }
            };
        }

        private void Rotate(Double target, Int32 direction)
        {
           
            var length = Math.PI/2;

            Movement = (gameTime) =>
                {
                    float rotation = 0.004f * direction * gameTime.ElapsedGameTime.Milliseconds;
                    rotation = (float) Math.Min(length, rotation);
                    length -= Math.Abs(rotation);

                    // Convert rotation to direction
                    Matrix rotMatrix = Matrix.CreateRotationZ(rotation);

                    Vector2 newDirection = Vector2.Transform(Direction, rotMatrix);
                    Direction = newDirection;

                    Vector2 newVectorPlane = Vector2.Transform(VectorPlane, rotMatrix);
                    VectorPlane = newVectorPlane;

                    if (length <= 0)
                    {
                        // set the direction vector and update 
                        Direction.X = (float) Math.Cos(target);
                        Direction.Y = (float) Math.Sin(target);

                        // Set the vector plane as the normal to the direction vector
                        VectorPlane.X = Direction.Y;
                        VectorPlane.Y = -1 * Direction.X;
                        VectorPlane.Normalize();
                        VectorPlane *= VectorPlaneLength;

                        Movement = null;
                    }
                };
        }

        public void RotateLeft()
        {
            if (Movement == null)
            {
                // Current Rotation
                var theta = Math.Atan2(Direction.Y, Direction.X);

                // Target Rotation
                var target = theta + (Math.PI / 2);

                Rotate(target,1);
            }
        }

        public void RotateRight()
        {
            if (Movement == null)
            {
                // Current Rotation
                var theta = Math.Atan2(Direction.Y, Direction.X);
                
                // Target Rotation
                var target = theta - (Math.PI / 2);

                Rotate(target,-1);
            }
        }

    }
}

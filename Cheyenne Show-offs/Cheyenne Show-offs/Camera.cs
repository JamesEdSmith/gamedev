using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Cheyenne_Show_offs
{
    public class Camera
    {
        public const float CAMERA_SCALE_SPEED = 5f;
        public const float CAMERA_PAN_SPEED = 5f;

        public Matrix matrix;
        private Game1 game;
        private float scale;
        public float desiredScale;
        private Vector2 position;
        public Vector2 desiredPosition;
        private PlayerIndex player;

        public Camera(PlayerIndex player, Game1 game)
        {
            position = Vector2.Zero;
            desiredPosition = Vector2.Zero;
            scale = desiredScale = 1f;
            matrix = Matrix.Identity;
            this.player = player;
            this.game = game;
        }

        public void update(GameTime time)
        {
            float diff = desiredScale - scale;
            scale += diff * CAMERA_SCALE_SPEED * (float)time.ElapsedGameTime.TotalSeconds;
            Vector2 posDiff = desiredPosition - position;
            position += posDiff * CAMERA_PAN_SPEED * (float)time.ElapsedGameTime.TotalSeconds;
        }

        public Matrix getMatrix()
        {
            matrix = Matrix.Identity;
            /*if(player == PlayerIndex.One)
                matrix *= Matrix.CreateTranslation(-(position.X - Game1.SCREENWIDTH/2)/scale, -(position.Y - Game1.SCREENHEIGHT / 2)/scale, 0f) * Matrix.CreateScale(scale);
            else if (player == PlayerIndex.Two)
                matrix *= Matrix.CreateTranslation(0, 0 , 0f) * Matrix.CreateScale(scale);
            else*/
            matrix *= Matrix.CreateTranslation(-position.X, -position.Y, 0f) * Matrix.CreateScale(scale);
            
            game.setCameras(this);
            return matrix;
        }

        public float getScale()
        {
            return scale;
        }

        public float getX()
        {
            return position.X;
        }

        public float getY()
        {
            return position.Y;
        }
    }
}

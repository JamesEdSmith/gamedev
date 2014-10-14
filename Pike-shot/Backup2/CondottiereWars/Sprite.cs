using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;


namespace PikeAndShot
{
    public class Sprite
    {
        private Texture2D _sourceBitmap;
        public Texture2D _flashTexture;
        private Rectangle _boundingRect;
        private Rectangle _flippedRect;
        private Rectangle _currRect;
        private Vector2 _size;
        private int _cols;
        private int _rows;
        private bool _loop;
        private int _maxFrames;
        private int _currFrame;
        private bool _playing;
        private float _animationSpeed;
        private float _animationTime;
        public bool flashable;
        private int flashStartThreshold;
        private Color flashColor;

        public Sprite(Texture2D bitmap, Rectangle boundingRect, int frameWidth, int frameHeight, bool loop)
        {
            flashable = false;
            _sourceBitmap = bitmap;
            _boundingRect = boundingRect;
            _flippedRect = new Rectangle(frameWidth - boundingRect.X - boundingRect.Width, boundingRect.Y, boundingRect.Width, boundingRect.Height);
            _size = new Vector2(frameWidth, frameHeight);
            _cols = _sourceBitmap.Width / frameWidth; 
            _rows = _sourceBitmap.Height / frameHeight;
            _currRect = new Rectangle(0, 0, frameWidth, frameHeight);
            _currFrame = 0;
            _maxFrames = _cols * _rows;
            _loop = loop;
            _playing = false;
            _animationSpeed = _animationTime = 1000;
        }

        public Sprite(Texture2D bitmap, Rectangle boundingRect, int frameWidth, int frameHeight, bool loop, bool flashable, int flashStartThreshold, Color color):
            this(bitmap, boundingRect, frameWidth, frameHeight, loop)
        {
            this.flashable = true;
            this.flashStartThreshold = flashStartThreshold;
            this.flashColor = color;

            //create flash texture
            Color[] pixelData = new Color[bitmap.Width * bitmap.Height];
            bitmap.GetData<Color>(pixelData);

            for (int i = 0; i < pixelData.Length; i++)
            {
                if (pixelData[i].A != 0)
                    pixelData[i] = Color.White;
            }
            _flashTexture = new Texture2D(bitmap.GraphicsDevice, bitmap.Width, bitmap.Height);
            _flashTexture.SetData<Color>(pixelData);
        }

        public Sprite(Texture2D bitmap, Rectangle boundingRect, int frameWidth, int frameHeight, bool loop, bool flashable) :
            this(bitmap, boundingRect, frameWidth, frameHeight, loop, flashable, 25, Color.White)
        {
        }

        public Sprite(Texture2D bitmap, Rectangle boundingRect, int frameWidth, int frameHeight, bool loop, bool flashable, int flashStartThreashold) :
            this(bitmap, boundingRect, frameWidth, frameHeight, loop, flashable, flashStartThreashold, Color.White)
        {
        }

        public Sprite(Texture2D bitmap, Rectangle boundingRect, int frameWidth, int frameHeight): 
            this(bitmap, boundingRect, frameWidth, frameHeight, false)
        {
        }

        public void setLoop (bool loop)
        {
            _loop = loop;
        }

        public void play()
        {
            _playing = true;
        }

        public void playRandomStart()
        {
            _animationTime = (float)PikeAndShotGame.random.NextDouble() * _animationSpeed;
            _playing = true;
        }

        public void stop()
        {
            _playing = false;
        }

        public void setAnimationSpeed(float speed)
        {
            _animationSpeed = speed;
        }

        public void reset()
        {
            _currFrame = 0;
            _currRect.X = 0;
            _currRect.Y = 0;
            _animationTime = _animationSpeed;
        }

        public int getCurrFrame()
        {
            return _currFrame;
        }

        public void nextFrame()
        {
            _currFrame++;

            if (_currFrame >= _maxFrames)
            {
                if (_loop)
                    _currFrame = 0;
                else
                    _currFrame--;
            }
            adjustBoundingRect();
        }

        private void adjustBoundingRect()
        {
            // Update the current bitmap rect
            _currRect.X = (_currFrame % _cols) * (int)_size.X;
            _currRect.Y = (_currFrame / _cols) * (int)_size.Y;
        }

        public void prevFrame()
        {
            _currFrame--;

            if (_currFrame < 0)
            {
                if (_loop)
                    _currFrame = _maxFrames - 1;
                else
                    _currFrame++;
            }
            adjustBoundingRect();
        }

        public Rectangle getBoundingRect()
        {
            return _boundingRect;
        }

        public Vector2 getSize()
        {
            return _size;
        }
        public int getMaxFrames()
        {
            return _maxFrames;
        }

        public void setFrame(int number)
        {
            if (number < 0)
                number = 0;
            _currFrame = number;
            adjustBoundingRect();
        }

        public void draw(SpriteBatch spritebatch, Vector2 _position, int side)
        {
            if (side == BattleScreen.SIDE_PLAYER)
            {
                spritebatch.Draw(_sourceBitmap, _position - new Vector2(_boundingRect.X, _boundingRect.Y), _currRect, Color.White);
            }
            else
            {
                spritebatch.Draw(_sourceBitmap, _position - new Vector2(_flippedRect.X, _flippedRect.Y), _currRect, Color.White, 0, Vector2.Zero, Vector2.One, SpriteEffects.FlipHorizontally, 0);
            }
        }

        public void draw(SpriteBatch spritebatch, Vector2 _position, int side, float flashAmount)
        {
            draw(spritebatch, _position, side);
            Color color = Color.Black;
            color.A = (byte)(255f * flashAmount);

            if(color.A < flashStartThreshold)
            {
                Color white = flashColor;
                white.A -= (byte)(255f * (float)color.A / (float)flashStartThreshold);
                color = white;
            }
            else
                color.A /= 2;

            if (side == BattleScreen.SIDE_PLAYER)
            {
                spritebatch.Draw(_flashTexture, _position - new Vector2(_boundingRect.X, _boundingRect.Y), _currRect, color);
            }
            else
            {
                spritebatch.Draw(_flashTexture, _position - new Vector2(_flippedRect.X, _flippedRect.Y), _currRect, color, 0, Vector2.Zero, Vector2.One, SpriteEffects.FlipHorizontally, 0);
            }
        }


        public bool getPlaying()
        {
            return _playing;
        }

        public void update(TimeSpan timeSpan)
        {
            if (_playing)
            {
                _animationTime -= (float)timeSpan.TotalMilliseconds;
                if (_animationTime <= 0)
                {
                    this.nextFrame();
                    _animationTime += _animationSpeed;
                }
            }
        }
    }

}

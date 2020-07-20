using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;


namespace MoleHillMountain
{
    public class Sprite
    {
        public const int EFFECT_FADEIN = 1;
        public const int EFFECT_FLASH_YELLOW = 2;
        public const int EFFECT_FLASH_RED = 3;

        //for horz direction
        public const int DIRECTION_LEFT = 0;
        public const int DIRECTION_RIGHT = 1;

        //for vert direction
        public const int DIRECTION_NONE = 0;
        public const int DIRECTION_UP = 1;
        public const int DIRECTION_DOWN = 2;

        private Rectangle _boundingRect;
        private Vector2 _boundingVector;
        private Texture2D _sourceBitmap;
        public Texture2D _flashTexture;
        public Texture2D _blackTexture;
        private Rectangle _flippedRect;
        private Vector2 _center;
        public Rectangle _currRect;
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
        public Color flashColor;
        public float dampening;

        private int effect = 0;
        private float effectTime = 0;
        private float effectTimer = 0;

        public Sprite(Texture2D bitmap, Rectangle boundingRect, int frameWidth, int frameHeight, bool loop)
        {
            flashable = false;
            _sourceBitmap = bitmap;
            _boundingRect = boundingRect;
            _boundingVector = new Vector2(boundingRect.X, boundingRect.Y);
            _flippedRect = new Rectangle(frameWidth - boundingRect.X - boundingRect.Width, boundingRect.Y, boundingRect.Width, boundingRect.Height);
            _size = new Vector2(frameWidth, frameHeight);
            _center = _size / 2;
            _cols = _sourceBitmap.Width / frameWidth;
            _rows = _sourceBitmap.Height / frameHeight;
            _currRect = new Rectangle(0, 0, frameWidth, frameHeight);
            _currFrame = 0;
            _maxFrames = _cols * _rows;
            _loop = loop;
            _playing = false;
            _animationSpeed = _animationTime = 1000;

            /*
            Color[] pixelData = new Color[bitmap.Width * bitmap.Height];
            bitmap.GetData<Color>(pixelData);

            for (int i = 0; i < pixelData.Length; i++)
            {
                if (pixelData[i].A != 0)
                    pixelData[i] = Color.Black;
            }
            _blackTexture = new Texture2D(bitmap.GraphicsDevice, bitmap.Width, bitmap.Height);
            _blackTexture.SetData<Color>(pixelData);*/
        }

        public Sprite(Texture2D bitmap, Rectangle boundingRect, int frameWidth, int frameHeight, bool loop, bool flashable, int flashStartThreshold, Color color, float dampening, BattleScreen screen) :
            this(bitmap, boundingRect, frameWidth, frameHeight, loop)
        {
            this.flashable = true;
            this.flashStartThreshold = flashStartThreshold;
            this.flashColor = color;
            this.dampening = dampening;

            //create flash texture
            _flashTexture = screen.getFlashTexture(bitmap);
        }

        public Sprite(Texture2D bitmap, Rectangle boundingRect, int frameWidth, int frameHeight, bool loop, bool flashable, BattleScreen screen) :
            this(bitmap, boundingRect, frameWidth, frameHeight, loop, flashable, 25, Color.White, 2, screen)
        {
        }

        public Sprite(Texture2D bitmap, Rectangle boundingRect, int frameWidth, int frameHeight, bool loop, bool flashable, int flashStartThreashold, BattleScreen screen) :
            this(bitmap, boundingRect, frameWidth, frameHeight, loop, flashable, flashStartThreashold, Color.White, 2, screen)
        {
        }

        public Sprite(Texture2D bitmap, Rectangle boundingRect, int frameWidth, int frameHeight) :
            this(bitmap, boundingRect, frameWidth, frameHeight, false)
        {
        }

        public void setLoop(bool loop)
        {
            _loop = loop;
        }

        public Texture2D getSourceBitmap()
        {
            return _sourceBitmap;
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

        public void setMaxFrames(int i)
        {
            _maxFrames = i;
        }

        public void setFrame(int number)
        {
            if (number < 0)
                number = 0;
            _currFrame = number;
            adjustBoundingRect();
        }

        // [dsl] For zoom stuff. The mouse cursor needs to inverse the scale
        public void drawWithScale(SpriteBatch spritebatch, Vector2 _position, int side, float scale = 1.0f)
        {
            if (side == BattleScreen.SIDE_PLAYER)
            {
                spritebatch.Draw(_sourceBitmap, _position - new Vector2(_boundingRect.X, _boundingRect.Y), _currRect, Color.White, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
            }
            else
            {
                spritebatch.Draw(_sourceBitmap, _position - new Vector2(_flippedRect.X, _flippedRect.Y), _currRect, Color.White, 0, Vector2.Zero, scale, SpriteEffects.FlipHorizontally, 0);
            }
        }

        public void setEffect(int effect, float time)
        {
            this.effect = effect;
            this.effectTime = time;
            this.effectTimer = time;
        }

        const float RGB_OFFSET = 1f;

        public void draw(SpriteBatch spriteBatch, Vector2 position, int side)
        {
            draw(spriteBatch, position, side, PikeAndShotGame.DUMMY_TIMESPAN);
        }

        public void draw(SpriteBatch spriteBatch, Vector2 position, float rotation, Color color)
        {
            spriteBatch.Draw(_sourceBitmap, position - _boundingVector, _currRect, color, rotation, _center, 1, SpriteEffects.None, 0);
        }

        public void draw(SpriteBatch spriteBatch, Vector2 position, float rotation)
        {
            spriteBatch.Draw(_sourceBitmap, position - _boundingVector, _currRect, Color.White, rotation, _center, 1, SpriteEffects.None, 0);
        }

        public void draw(SpriteBatch spritebatch, Vector2 _position, int horzDirection, int verticalDirection)
        {
            draw(spritebatch, _position, horzDirection, verticalDirection, Color.White);
        }
        public void draw(SpriteBatch spritebatch, Vector2 _position, int horzDirection, int verticalDirection, Color color)
        {

            if (horzDirection == DIRECTION_LEFT)
            {
                if (verticalDirection == DIRECTION_NONE)
                {
                    spritebatch.Draw(_sourceBitmap, _position - _boundingVector, _currRect, color, 0, _center, 1, SpriteEffects.None, 0);
                }
                else if (verticalDirection == DIRECTION_UP)
                {
                    spritebatch.Draw(_sourceBitmap, _position - _boundingVector, _currRect, color, MathHelper.Pi * 0.5f, _center, 1, SpriteEffects.None, 0);
                }
                else
                {
                    spritebatch.Draw(_sourceBitmap, _position - _boundingVector, _currRect, color, MathHelper.Pi * 0.5f, _center, 1, SpriteEffects.FlipHorizontally, 0);
                }
            }
            else
            {
                if (verticalDirection == DIRECTION_NONE)
                {
                    spritebatch.Draw(_sourceBitmap, _position - _boundingVector, _currRect, color, 0, _center, 1, SpriteEffects.FlipHorizontally, 0);
                }
                else if (verticalDirection == DIRECTION_UP)
                {
                    spritebatch.Draw(_sourceBitmap, _position - _boundingVector, _currRect, color, -MathHelper.Pi * 0.5f, _center, 1, SpriteEffects.FlipHorizontally, 0);
                }
                else
                {
                    spritebatch.Draw(_sourceBitmap, _position - _boundingVector, _currRect, color, -MathHelper.Pi * 0.5f, _center, 1, SpriteEffects.None, 0);
                }
            }
        }

        public void draw(SpriteBatch spritebatch, Vector2 _position, int side, TimeSpan timeSpan)
        {

            if (effect == EFFECT_FADEIN)
            {
                Color color = Color.White;
                effectTimer -= (float)timeSpan.TotalMilliseconds;

                float a = (float)((effectTime - effectTimer) / effectTime);
                color *= a;

                if (effectTimer <= 0)
                {
                    effect = 0;
                }

                if (side == BattleScreen.SIDE_PLAYER)
                {
                    spritebatch.Draw(_sourceBitmap, _position - new Vector2(_boundingRect.X, _boundingRect.Y), _currRect, color, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
                }
                else
                {
                    spritebatch.Draw(_sourceBitmap, _position - new Vector2(_flippedRect.X, _flippedRect.Y), _currRect, color, 0, Vector2.Zero, 1, SpriteEffects.FlipHorizontally, 0);
                }
            }
            else if (effect == EFFECT_FLASH_YELLOW)
            {
                Color color = new Color(Color.Yellow.R, Color.Yellow.G, 255);
                effectTimer -= (float)timeSpan.TotalMilliseconds;

                float a = Math.Abs(((effectTime / 2) - effectTimer) / (effectTime / 2));
                if (a < 0.5)
                    a = 0;
                else
                {
                    a -= 0.5f;
                    a = a / 0.5f;
                }

                color *= a;

                if (effectTimer <= 0)
                {
                    effectTimer = effectTime;
                }

                if (side == BattleScreen.SIDE_PLAYER)
                {
                    spritebatch.Draw(_sourceBitmap, _position - new Vector2(_boundingRect.X, _boundingRect.Y), _currRect, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
                }
                else
                {
                    spritebatch.Draw(_sourceBitmap, _position - new Vector2(_flippedRect.X, _flippedRect.Y), _currRect, Color.White, 0, Vector2.Zero, 1, SpriteEffects.FlipHorizontally, 0);
                }

                if (side == BattleScreen.SIDE_PLAYER)
                {
                    spritebatch.Draw(_flashTexture, _position - new Vector2(1, 1) - new Vector2(_boundingRect.X, _boundingRect.Y), _currRect, color, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
                    spritebatch.Draw(_flashTexture, _position - new Vector2(-1, -1) - new Vector2(_boundingRect.X, _boundingRect.Y), _currRect, color, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
                    spritebatch.Draw(_flashTexture, _position - new Vector2(1, -1) - new Vector2(_boundingRect.X, _boundingRect.Y), _currRect, color, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
                    spritebatch.Draw(_flashTexture, _position - new Vector2(-1, 1) - new Vector2(_boundingRect.X, _boundingRect.Y), _currRect, color, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
                }
                else
                {
                    spritebatch.Draw(_flashTexture, _position - new Vector2(_flippedRect.X, _flippedRect.Y), _currRect, color, 0, Vector2.Zero, 1, SpriteEffects.FlipHorizontally, 0);
                }
            }
            else if (effect == EFFECT_FLASH_RED)
            {
                Color color = new Color(Color.Red.R, 0, 0);
                effectTimer -= (float)timeSpan.TotalMilliseconds;

                float a = Math.Abs(((effectTime / 2) - effectTimer) / (effectTime / 2));
                if (a < 0.5)
                    a = 0;
                else
                {
                    a -= 0.5f;
                    a = a / 0.5f;
                }

                color *= a;

                if (effectTimer <= 0)
                {
                    effectTimer = effectTime;
                }

                if (side == BattleScreen.SIDE_PLAYER)
                {
                    spritebatch.Draw(_sourceBitmap, _position - new Vector2(_boundingRect.X, _boundingRect.Y), _currRect, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
                }
                else
                {
                    spritebatch.Draw(_sourceBitmap, _position - new Vector2(_flippedRect.X, _flippedRect.Y), _currRect, Color.White, 0, Vector2.Zero, 1, SpriteEffects.FlipHorizontally, 0);
                }

                if (side == BattleScreen.SIDE_PLAYER)
                {
                    spritebatch.Draw(_flashTexture, _position - new Vector2(_boundingRect.X, _boundingRect.Y), _currRect, color, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
                }
                else
                {
                    //                    spritebatch.Draw(_flashTexture, _position - new Vector2(_flippedRect.X, _flippedRect.Y), _currRect, color, 0, Vector2.Zero, 1, SpriteEffects.FlipHorizontally, 0);
                    spritebatch.Draw(_flashTexture, _position - new Vector2(1, 1) - new Vector2(_flippedRect.X, _flippedRect.Y), _currRect, color, 0, Vector2.Zero, 1, SpriteEffects.FlipHorizontally, 0);
                    spritebatch.Draw(_flashTexture, _position - new Vector2(-1, 1) - new Vector2(_flippedRect.X, _flippedRect.Y), _currRect, color, 0, Vector2.Zero, 1, SpriteEffects.FlipHorizontally, 0);
                    spritebatch.Draw(_flashTexture, _position - new Vector2(1, -1) - new Vector2(_flippedRect.X, _flippedRect.Y), _currRect, color, 0, Vector2.Zero, 1, SpriteEffects.FlipHorizontally, 0);
                    spritebatch.Draw(_flashTexture, _position - new Vector2(-1, -1) - new Vector2(_flippedRect.X, _flippedRect.Y), _currRect, color, 0, Vector2.Zero, 1, SpriteEffects.FlipHorizontally, 0);
                }
            }
            else
            {
                if (side == BattleScreen.SIDE_PLAYER)
                {
                    /*spritebatch.Draw(_blackTexture, _position - new Vector2(1, -1) - new Vector2(_boundingRect.X, _boundingRect.Y), _currRect, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
                    spritebatch.Draw(_blackTexture, _position - new Vector2(-1, 1) - new Vector2(_boundingRect.X, _boundingRect.Y), _currRect, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
                    spritebatch.Draw(_blackTexture, _position - new Vector2(-1, -1) - new Vector2(_boundingRect.X, _boundingRect.Y), _currRect, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
                    spritebatch.Draw(_blackTexture, _position - new Vector2(1, 1) - new Vector2(_boundingRect.X, _boundingRect.Y), _currRect, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0);*/
                    spritebatch.Draw(_sourceBitmap, _position - new Vector2(_boundingRect.X, _boundingRect.Y), _currRect, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
                }
                else
                {
                    /*spritebatch.Draw(_blackTexture, _position - new Vector2(-1, 1) - new Vector2(_flippedRect.X, _flippedRect.Y), _currRect, Color.White, 0, Vector2.Zero, 1, SpriteEffects.FlipHorizontally, 0);
                    spritebatch.Draw(_blackTexture, _position - new Vector2(-1, -1) - new Vector2(_flippedRect.X, _flippedRect.Y), _currRect, Color.White, 0, Vector2.Zero, 1, SpriteEffects.FlipHorizontally, 0);
                    spritebatch.Draw(_blackTexture, _position - new Vector2(1, 1) - new Vector2(_flippedRect.X, _flippedRect.Y), _currRect, Color.White, 0, Vector2.Zero, 1, SpriteEffects.FlipHorizontally, 0);
                    spritebatch.Draw(_blackTexture, _position - new Vector2(1, -1) - new Vector2(_flippedRect.X, _flippedRect.Y), _currRect, Color.White, 0, Vector2.Zero, 1, SpriteEffects.FlipHorizontally, 0);*/
                    spritebatch.Draw(_sourceBitmap, _position - new Vector2(_flippedRect.X, _flippedRect.Y), _currRect, Color.White, 0, Vector2.Zero, 1, SpriteEffects.FlipHorizontally, 0);
                }
            }

        }

        public void draw(SpriteBatch spritebatch, Vector2 _position, int side, float flashAmount)
        {
            draw(spritebatch, _position, side, PikeAndShotGame.DUMMY_TIMESPAN);
            Color color = Color.Black;
            color.A = (byte)(255f * flashAmount);

            if (color.A < flashStartThreshold)
            {
                Color white = flashColor;
                white.A -= (byte)(255f * (float)color.A / (float)flashStartThreshold);
                color = white;
            }
            else
            {
                float a = (float)color.A / dampening;
                if (a > 255)
                    a = 255;
                color.A = (byte)a;
            }

            if (side == BattleScreen.SIDE_PLAYER)
            {
                spritebatch.Draw(_flashTexture, _position - new Vector2(_boundingRect.X, _boundingRect.Y), _currRect, color);
            }
            else
            {
                spritebatch.Draw(_flashTexture, _position - new Vector2(_flippedRect.X, _flippedRect.Y), _currRect, color, 0, Vector2.Zero, Vector2.One, SpriteEffects.FlipHorizontally, 0);
            }
        }

        public void draw(SpriteBatch spritebatch, Vector2 _position, int side, float curTime, float flickerTime)
        {
            Color color = Color.White;

            float a = (float)((curTime % flickerTime) / flickerTime);
            color *= a;

            if (side == BattleScreen.SIDE_PLAYER)
            {
                //spritebatch.Draw(_sourceBitmap, _position - new Vector2(_boundingRect.X, _boundingRect.Y) - new Vector2(RGB_OFFSET, 0), _currRect, Color.Red, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
                //spritebatch.Draw(_sourceBitmap, _position - new Vector2(_boundingRect.X, _boundingRect.Y) + new Vector2(RGB_OFFSET, 0), _currRect, Color.Blue, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
                spritebatch.Draw(_sourceBitmap, _position - new Vector2(_boundingRect.X, _boundingRect.Y), _currRect, color, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
            }
            else
            {
                //spritebatch.Draw(_sourceBitmap, _position - new Vector2(_flippedRect.X, _flippedRect.Y) - new Vector2(RGB_OFFSET, 0), _currRect, Color.Red, 0, Vector2.Zero, 1, SpriteEffects.FlipHorizontally, 0);
                //spritebatch.Draw(_sourceBitmap, _position - new Vector2(_flippedRect.X, _flippedRect.Y) + new Vector2(RGB_OFFSET, 0), _currRect, Color.Blue, 0, Vector2.Zero, 1, SpriteEffects.FlipHorizontally, 0);
                spritebatch.Draw(_sourceBitmap, _position - new Vector2(_flippedRect.X, _flippedRect.Y), _currRect, color, 0, Vector2.Zero, 1, SpriteEffects.FlipHorizontally, 0);
            }
        }

        public void draw(SpriteBatch spritebatch, Vector2 _position, int side, float curTime, float flickerTime, Color color)
        {
            //TODO: fix this later so that effects can also flicker
            draw(spritebatch, _position, side, PikeAndShotGame.DUMMY_TIMESPAN);
            float a = (float)((curTime % flickerTime) / flickerTime);
            color *= a;

            if (side == BattleScreen.SIDE_PLAYER)
            {
                spritebatch.Draw(_flashTexture, _position - new Vector2(_boundingRect.X, _boundingRect.Y), _currRect, color, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
            }
            else
            {
                spritebatch.Draw(_flashTexture, _position - new Vector2(_flippedRect.X, _flippedRect.Y), _currRect, color, 0, Vector2.Zero, 1, SpriteEffects.FlipHorizontally, 0);
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
                    if (_animationTime > 3000f)
                        Console.WriteLine("fuck");
                }
            }
        }

        internal bool hasEffect()
        {
            if (effect != 0)
                return true;
            else
                return false;
        }

        internal void createFlashTexture(BattleScreen screen)
        {
            _flashTexture = screen.getFlashTexture(_sourceBitmap);
        }
    }

}

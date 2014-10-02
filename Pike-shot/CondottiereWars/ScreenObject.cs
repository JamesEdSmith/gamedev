using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections;

namespace PikeAndShot
{
    public class ScreenObject
    {
        public const int STATE_DEAD = 99;
        public const int STATE_DYING = 98;
        public const int STATE_ROUTE = 97;
        public const int STATE_ROUTED = 96;

        protected BattleScreen _screen;
        public Vector2 _position;
        public float _drawingY;
        protected int _state;
        protected int _side;
        protected float _stateTimer;
        protected float _stateTime;
        public Vector2 _guardPositionOffset;

        public ScreenObject(BattleScreen screen, int side)
        {
            _screen = screen;
            _side = side;
            _screen.addScreenObject(this);
            _guardPositionOffset = Vector2.Zero;
        }

        public Vector2 getPosition()
        {
            return _position;
        }

        public BattleScreen getScreen()
        {
            return _screen;
        }

        public virtual int getWidth()
        {
            return 0;
        }

        public virtual int getHeight()
        {
            return 0;
        }

        public virtual Vector2 getCenter()
        {
            return Vector2.Zero;
        }

        public virtual void collide(ScreenObject collider)
        {

        }

        public float getStateTimer()
        {
            return _stateTimer;
        }

        public virtual void setSide(int side)
        {
            _side = side;
        }

        public int getSide()
        {
            return _side;
        }

        public int getState()
        {
            return _state;
        }

        public virtual void setState(int state)
        {
            _state = state;
        }
    }

    public class ScreenAnimation
    {
        public Vector2 _position;
        public float _drawingY;
        protected int _side;
        protected Sprite _sprite;
        protected float _duration;
        protected Boolean _done;
        protected float _time;
        protected int _maxFrames;
        protected BattleScreen _screen;

        public ScreenAnimation(BattleScreen screen, int side, Vector2 position, Sprite sprite, float duration )
        {
            _position = position;
            _side = side;
            _sprite = sprite;
            _duration = duration;
            _time = duration;
            _done = false;
            _maxFrames = _sprite.getMaxFrames();

            screen.addAnimation(this);
            _screen = screen;
        }

        public void restart()
        {
            _done = false;
            _time = _duration;
        }

        public virtual void update(TimeSpan timeSpan)
        {
            _time -= (float)timeSpan.TotalMilliseconds;

            if (_time <= 0)
            {
                _done = true;
            }

            for (int i = 1; i <= _maxFrames; i++)
            {
                if (_time <= i * _duration / _maxFrames)
                {
                    _sprite.setFrame(_maxFrames - i);
                    i = _maxFrames;
                }
            }            
        }

        public virtual void draw(SpriteBatch spritebatch)
        {
            _sprite.draw(spritebatch, _position - _screen.getMapOffset(), _side);
            //spritebatch.Draw(BattleScreen.getDotTexture(), _position, Color.White);
            //spritebatch.Draw(BattleScreen.getDotTexture(), _position + new Vector2(2,0), Color.White);
            //spritebatch.Draw(BattleScreen.getDotTexture(), _position + new Vector2(0,2), Color.White);
        }

        public bool isDone()
        {
            return _done;
        }
    }

    public class ArquebusierSmoke : ScreenAnimation
    {
        public ArquebusierSmoke(BattleScreen screen, int side, Vector2 position)
            : base(screen, side, position, new Sprite(PikeAndShotGame.ARQUEBUSIER_SMOKE, new Rectangle(22, 14, 12, 4), 58, 25), 1000f)
        {

        }

        public override void update(TimeSpan timeSpan)
        {
            _time -= (float)timeSpan.TotalMilliseconds;

            if (_time <= 0)
            {
                _done = true;
            }

            // custom timing for the smoke where the first few frames are really quick
            if (_time <= 6f * _duration / 20f)
                _sprite.setFrame(4);
            else if (_time <= 12f * _duration / 20f)
                _sprite.setFrame(3);
            else if (_time <= 18f * _duration / 20f)
                _sprite.setFrame(2);
            else if (_time <= 19f * _duration / 20f)
                _sprite.setFrame(1);
            else
                _sprite.setFrame(0);
        }
    }

    public class Coin : ScreenAnimation
    {
        static float COIN_TIME = 400f;
        private bool _drop;
        private const float GRAVITY = 9.8f;
        private float velocity;
        private bool doneFlashing;

        public Coin(BattleScreen screen, Vector2 position)
            : base(screen, BattleScreen.SIDE_PLAYER, position, new Sprite(PikeAndShotGame.COIN, new Rectangle(0, 0, 24, 14), 24, 14, false, true, 128), COIN_TIME)
        {
            int rando = PikeAndShotGame.random.Next(3);
            if (rando == 1)
                _position.X += 2f;
            else if (rando == 2)
                _position.X -= 2f;

            _drop = false;
            velocity = -1.5f;
            doneFlashing = false;
        }

        public override void update(TimeSpan timeSpan)
        {
            if (!doneFlashing)
            {
                _time -= (float)timeSpan.TotalMilliseconds;

                if (_time <= 0)
                {
                    _time = 0;
                    doneFlashing = true;
                }
            }

            int maxFrames = _sprite.getMaxFrames();
            float deathFrameTime = COIN_TIME / (float)maxFrames;
            int frameNumber = maxFrames - (int)(_time / deathFrameTime) - 1;

            _sprite.setFrame(frameNumber);

            if (_drop)
            {
                velocity += GRAVITY * (float)timeSpan.TotalSeconds;
                _position.Y += velocity;
            }
        }

        public override void draw(SpriteBatch spritebatch)
        {
            if (_sprite.flashable && !doneFlashing)
                _sprite.draw(spritebatch, _position, _side, _time / _duration);
            else
                _sprite.draw(spritebatch, _position, _side);
            //spritebatch.Draw(BattleScreen.getDotTexture(), _position, Color.White);
            //spritebatch.Draw(BattleScreen.getDotTexture(), _position + new Vector2(2,0), Color.White);
            //spritebatch.Draw(BattleScreen.getDotTexture(), _position + new Vector2(0,2), Color.White);
        }

        public void setDone()
        {
            _done = true;
        }

        internal void drop()
        {
            _drop = true;
        }
    }
}

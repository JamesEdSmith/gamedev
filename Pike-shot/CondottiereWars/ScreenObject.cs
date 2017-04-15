using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections;
using Microsoft.Xna.Framework.Audio;

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

        public virtual void collide(ScreenObject collider, TimeSpan timeSpan)
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
        public const float GRAVITY = 9.8f;
        public static Vector2 GRAVITY_VECTOR = new Vector2(0f, GRAVITY);

        public Vector2 _position;
        public float _drawingY;
        protected int _side;
        protected Sprite _sprite;
        protected float _duration;
        protected Boolean _done;
        protected float _time;
        protected int _maxFrames;
        protected BattleScreen _screen;
        private List<ScreenAnimationListener> listeners; 

        public ScreenAnimation(BattleScreen screen, int side, Vector2 position, Sprite sprite, float duration )
        {
            _position = position;
            _side = side;
            _sprite = sprite;
            _duration = duration;
            _time = duration;
            _done = false;
            _maxFrames = _sprite.getMaxFrames();
            _drawingY = _position.Y + _sprite.getBoundingRect().Height;

            screen.addAnimation(this);
            _screen = screen;
            listeners = new List<ScreenAnimationListener>(1);
        }

        public void addListener(ScreenAnimationListener listener)
        {
            listeners.Add(listener);
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
                setDone();
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

        public virtual void setDone()
        {
            _done = true;
            foreach (ScreenAnimationListener listener in listeners)
            {
                listener.onAnimationTrigger(this);
            }
        }


        public virtual void draw(SpriteBatch spritebatch)
        {
            Vector2 _drawingPosition = _position - _screen.getMapOffset();
            //_sprite.draw(spritebatch, _position - _screen.getMapOffset(), _side);

            _screen.addDrawjob(new DrawJob(_sprite, _drawingPosition, _side, _drawingY));
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
            : base(screen, side, position, new Sprite(PikeAndShotGame.ARQUEBUSIER_SMOKE, new Rectangle(16, 16, 16, 28), 128, 64), 1000f)
        {

        }

        public override void update(TimeSpan timeSpan)
        {
            _time -= (float)timeSpan.TotalMilliseconds;

            if (_time <= 0)
            {
                setDone();
            }

            int maxFrames = _sprite.getMaxFrames();
            float frameTime = _duration / (float)maxFrames;
            int frameNumber = maxFrames - (int)(_time / frameTime) - 1;

            _sprite.setFrame(frameNumber);

        }
    }

    public class ThrownGun : ScreenAnimation
    {
        Vector2 velocity;
        Vector2 initialPosition;
        bool dirt;
        int spriteDivision;

        public ThrownGun(BattleScreen screen, int side, Vector2 position, Sprite sprite, int spriteDivision)
            : base(screen, side, position, sprite, 1000f)
        {
            velocity = new Vector2(0.08f, -0.10f);
            dirt = false;
            _position.Y += 4f;
            initialPosition = new Vector2(position.X, position.Y);
            this.spriteDivision = spriteDivision;
        }

        public override void update(TimeSpan timeSpan)
        {
            if (!dirt)
            {
                velocity += GRAVITY_VECTOR * 0.02f * (float)timeSpan.TotalSeconds;
                _position += velocity * (float)timeSpan.TotalMilliseconds;
            }

            _time -= (float)timeSpan.TotalMilliseconds;

            if (_time < 0)
            {
                if (!dirt)
                {
                    dirt = true;
                    _time = _duration = 800f;
                }
                else
                {
                    _time = 0f;
                    setDone();
                }
            }

            if (!dirt)
            {
                int maxFrames = spriteDivision;
                float frameTime = _duration / (float)maxFrames;
                int frameNumber = maxFrames - (int)(_time / frameTime) - 1;

                _sprite.setFrame(frameNumber);
            }
            else
            {
                int maxFrames = _sprite.getMaxFrames() - spriteDivision;
                float frameTime = _duration / (float)maxFrames;
                int frameNumber = maxFrames - (int)(_time / frameTime) - 1;

                _sprite.setFrame(frameNumber + spriteDivision);
            }
        }
    }

    public class ThrownFalchion : ScreenAnimation
    {
        Vector2 velocity;
        Vector2 initialPosition;
        bool dirt;
        public ThrownFalchion(BattleScreen screen, int side, Vector2 position)
            :base(screen,side, position, new Sprite(PikeAndShotGame.FALCHION_THROWN, new Rectangle(16,28,4,4), 36, 58), 500f)
        {
            velocity = new Vector2(-0.08f, -0.10f);
            dirt = false;
            _position.Y += 4f;
            initialPosition = new Vector2(position.X, position.Y);
        }

        public override void update(TimeSpan timeSpan)
        {
            if (!dirt)
            {
                velocity += GRAVITY_VECTOR * 0.02f * (float)timeSpan.TotalSeconds;
                _position += velocity * (float)timeSpan.TotalMilliseconds;
            }

            if (_position.Y > initialPosition.Y + 20f && !dirt)
            {
                _sprite = new Sprite(PikeAndShotGame.FALCHION_DIRT, new Rectangle(10, 14, 2, 2), 18, 26);
                dirt = true;
                _time = _duration*4f;
            }

            _time -= (float)timeSpan.TotalMilliseconds;
            if (_time < 0)
            {
                if (!dirt)
                    _time = _duration;
                else
                {
                    _time = 0f;
                    setDone();
                }
            }

            int maxFrames = _sprite.getMaxFrames();
            float frameTime = _duration / (float)maxFrames;
            int frameNumber = maxFrames - (int)(_time / frameTime) - 1;

            _sprite.setFrame(frameNumber);
        }
    }

    public class Coin : ScreenAnimation
    {
        static float COIN_TIME = 400f;
        private bool _drop;
        private float velocity;
        private bool doneFlashing;
        public Vector2 finalPosition;
        private float dropTarget = 0;
        protected SoundEffectInstance coinSound;
        bool playCoinSound;

        public Coin(BattleScreen screen, Vector2 position, Vector2 finalPosition)
            : base(screen, BattleScreen.SIDE_PLAYER, position, new Sprite(PikeAndShotGame.COIN, new Rectangle(0, 0, 24, 14), 24, 14, false, true, 128), COIN_TIME)
        {
            int rando = PikeAndShotGame.random.Next(3);
            if (rando == 1)
                _position.X += 2f;
            else if (rando == 2)
                _position.X -= 2f;

            _drop = false;
            velocity = 0f;//-1.5f;
            doneFlashing = false;
            this.finalPosition = finalPosition;
            coinSound = PikeAndShotGame.COIN_SOUND.CreateInstance();
            coinSound.Volume = 0.25f;
            playCoinSound = true;
        }

        public override void update(TimeSpan timeSpan)
        {
            if (!_drop)
            {
                if (_position.Y != finalPosition.Y)
                {
                    velocity += GRAVITY * (float)timeSpan.TotalSeconds;
                    _position.Y += velocity;

                    if (_position.Y >= finalPosition.Y)
                    {
                        _position.Y = finalPosition.Y;
                        if (playCoinSound)
                        {
                            coinSound.Play();
                            playCoinSound = false;
                        }
                    }
                }
                else if (!doneFlashing)
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
            }
            else
            {
                velocity += GRAVITY * (float)timeSpan.TotalSeconds;
                _position.Y += velocity;

                if (_position.Y >= dropTarget)
                    setDone();
            }
        }

        public override void draw(SpriteBatch spritebatch)
        {
            if (_sprite.flashable && !doneFlashing)
                _sprite.draw(spritebatch, _position, _side, _time / _duration);
            else
                _sprite.draw(spritebatch, _position, _side, PikeAndShotGame.DUMMY_TIMESPAN);
            //spritebatch.Draw(BattleScreen.getDotTexture(), _position, Color.White);
            //spritebatch.Draw(BattleScreen.getDotTexture(), _position + new Vector2(2,0), Color.White);
            //spritebatch.Draw(BattleScreen.getDotTexture(), _position + new Vector2(0,2), Color.White);
        }

        internal void drop(float target)
        {
            _drop = true;
            dropTarget = target;
            velocity = -0f;
        }
    }

    public class Loot : ScreenAnimation
    {
        static float FLASH_TIME = 3000f;
        SoundEffectInstance lootSound;

        public Loot(BattleScreen screen, Vector2 position)
            : base(screen, BattleScreen.SIDE_PLAYER, position, new Sprite(PikeAndShotGame.LOOT, new Rectangle(0, 0, 26, 26), 26, 22, false, true, 128, new Color(Color.Yellow.R, Color.Yellow.G, 100), 2), FLASH_TIME)
        {
            int rando = PikeAndShotGame.random.Next(_sprite.getMaxFrames());
            _sprite.setFrame(rando);
            _done = false;
            lootSound = PikeAndShotGame.LOOT_SOUND.CreateInstance();
            lootSound.Volume = 0.25f;
        }

        public override void update(TimeSpan timeSpan)
        {
            if (!_done)
            {
                _time -= (float)timeSpan.TotalMilliseconds;

                if (_time <= 0)
                {
                    _time = 0;
                    setDone();
                    lootSound.Play();
                }
            }
        }

        public override void draw(SpriteBatch spritebatch)
        {
            Vector2 mapOffset = _screen.getMapOffset();
            Vector2 _drawingPosition = _position - _screen.getMapOffset();
            if (_sprite.flashable && !_done)
                _screen.addDrawjob(new DrawJob(_sprite, _position - _screen.getMapOffset(), _side, _drawingY, _time / _duration));
            else
                _screen.addDrawjob(new DrawJob(_sprite, _position - _screen.getMapOffset(), _side, _drawingY));
            //spritebatch.Draw(BattleScreen.getDotTexture(), _position, Color.White);
            //spritebatch.Draw(BattleScreen.getDotTexture(), _position + new Vector2(2,0), Color.White);
            //spritebatch.Draw(BattleScreen.getDotTexture(), _position + new Vector2(0,2), Color.White);
        }
    }

    public class LootSpill : ScreenAnimation
    {
        private const float ANIMATION_TIME = 250f;
        private float startingDelay;
        private Vector2 velocity;        
        bool started;
        
        public LootSpill(BattleScreen screen, Vector2 position, float duration, Vector2 destination)
            : base(screen, BattleScreen.SIDE_PLAYER, position, new Sprite(PikeAndShotGame.COIN_SPINNA, new Rectangle(0, 0, 6, 6), 6, 6, true), duration)
        {
            reset(position);
            _done = true;
        }

        public void reset(Vector2 position)
        {
            this._time = ANIMATION_TIME;
            _position = new Vector2(position.X, position.Y);
            startingDelay = (float)(PikeAndShotGame.random.NextDouble() * 100f);

            velocity = new Vector2((float)(PikeAndShotGame.random.NextDouble() * PikeAndShotGame.getRandPlusMinus()),
                (float)(PikeAndShotGame.random.NextDouble() * -2f));

            started = false;
            _done = false;
            _screen.addAnimation(this);
        }

        public override void update(TimeSpan timeSpan)
        {
            if (!_done && started)
            {
                //use this if you want it to finish before it gets to the bottom of the screen
                /*_duration -= (float)timeSpan.TotalMilliseconds;
                if (_duration <= 0)
                {
                    _duration = 0;
                    _sprite.stop();
                    setDone();
                }*/

                velocity += GRAVITY_VECTOR * (float)timeSpan.TotalMilliseconds * 0.001f;
                _position += velocity;

                if (_position.Y > PikeAndShotGame.SCREENHEIGHT)
                {
                    _sprite.stop();
                    setDone();
                }

                _time -= (float)timeSpan.TotalMilliseconds;
                if (_time < 0)
                {
                    _time = ANIMATION_TIME + _time;
                }

                int maxFrames = _sprite.getMaxFrames();
                float frameTime = ANIMATION_TIME / (float)maxFrames;
                int frameNumber = maxFrames - (int)(_time / frameTime) - 1;

                _sprite.setFrame(frameNumber);

            }

            startingDelay -= (float)timeSpan.TotalMilliseconds;

            if (startingDelay <= 0)
                started = true;
        }


        public override void draw(SpriteBatch spritebatch)
        {
            if (started)
                _sprite.draw(spritebatch, _position, _side);
        }

    }

    public class LootTwinkle : ScreenAnimation, ScreenAnimationListener
    {
        private const float CONTROL_POINT_GAP = 0.25f;
        private const float ANIMATION_TIME = 250f;

        private bool started;
        private float duration;
        private Vector2 destination;
        private Vector2 origin;
        private Vector2 control1, control2;
        

        public LootTwinkle(BattleScreen screen, Vector2 position, float duration, Vector2 destination)
            : base(screen, BattleScreen.SIDE_PLAYER, position, new Sprite(PikeAndShotGame.COIN_SPINNA, new Rectangle(0, 0, 6, 6), 6, 6, true), duration)
        {
            this.duration = duration;
            started = false;
            this.destination = destination;
            origin = new Vector2(_position.X, _position.Y);
            this._time = ANIMATION_TIME;
        }

        public void onAnimationTrigger(ScreenAnimation screenAnimaton)
        {
            started = true;
            origin -= _screen.getMapOffset();
            control1 = new Vector2(origin.X, destination.Y);
            control2 = new Vector2((origin.X - destination.X) / 2f, destination.Y);
        }

        public void start()
        {
            started = true;
            control1 = new Vector2(origin.X, destination.Y);
            control2 = new Vector2((origin.X - destination.X) / 2f, destination.Y);
        }

        public override void update(TimeSpan timeSpan)
        {
            if (!_done && started)
            {    
                _duration -= (float)timeSpan.TotalMilliseconds;
                if (_duration <= 0)
                {
                    _duration = 0;
                    _sprite.stop();
                    setDone();
                }
                _position = bezier(origin, control1, control2, destination, (duration - _duration) / duration);

                _time -= (float)timeSpan.TotalMilliseconds;
                if (_time < 0)
                {
                    _time = ANIMATION_TIME + _time;
                }

                int maxFrames = _sprite.getMaxFrames();
                float frameTime = ANIMATION_TIME/ (float)maxFrames;
                int frameNumber = maxFrames - (int)(_time / frameTime) - 1;

                _sprite.setFrame(frameNumber);
            }
        }

        // for bezier curve
        Vector2 bezier(Vector2 P0, Vector2 P1, Vector2 P2, Vector2 P3, float t)
        {
            float invT = 1 - t;
            return invT * invT * invT * P0 + 3 * invT * invT * t * P1 + 3 * invT * t * t * P2 + t * t * t * P3;
        }

        public override void draw(SpriteBatch spritebatch)
        {
            if (started)
                _sprite.draw(spritebatch, _position, _side);
        }

        public override void setDone()
        {
            base.setDone();
        }

    }


    public interface ScreenAnimationListener
    {
        void onAnimationTrigger(ScreenAnimation screenAnimaton);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections;

namespace PikeAndShot
{
    public class Terrain : ScreenObject
    {
        public const int STATE_SHOWN = 1;
        public const int STATE_ANIMATING = 2;

        private Sprite _sprite;
        private float _restTime;
        private float _animationTime;

        public Terrain(BattleScreen screen, Texture2D sprite, int side, float x, float y, float restTime, float animationTime)
            : base(screen, side)
        {
            _position = new Vector2(x, y);
            _state = STATE_SHOWN;
            _sprite = new Sprite(sprite, new Rectangle(0, 0, 0, 0), 40, 40, true);
            _restTime = restTime;
            _animationTime = animationTime;
            _stateTimer = _restTime;
            _drawingY = _position.Y + 40f;
        }

        public Terrain(BattleScreen screen, Texture2D sprite, int side, float x, float y)
            : this (screen, sprite, side, x, y, 8000f, 1500f)
        {
        }

        public bool isDead()
        {
            return _state == STATE_DEAD;
        }

        public void update(TimeSpan timeSpan)
        {
            switch (_state)
            {
                case STATE_SHOWN:
                    _stateTimer -= (float)timeSpan.TotalMilliseconds;
                    if(_stateTimer <= 0)
                    {
                        _stateTimer = _animationTime;
                        _state = STATE_ANIMATING;
                    }
                    break;
                case STATE_ANIMATING:
                    int maxFrames = _sprite.getMaxFrames() * 2;
                    float frameTime = _animationTime / (float)maxFrames;
                    int frameNumber = maxFrames - (int)(_stateTimer / frameTime) - 1;

                    if (frameNumber >= _sprite.getMaxFrames())
                        frameNumber = _sprite.getMaxFrames() - (frameNumber - _sprite.getMaxFrames()) - 1;
                    _sprite.setFrame(frameNumber);
                    _stateTimer -= (float)timeSpan.TotalMilliseconds;

                    if (_stateTimer <= 0)
                    {
                        _stateTimer = _restTime;
                        _state = STATE_SHOWN;
                    }
                    break;
            }
        }

        public void draw(SpriteBatch spritebatch)
        {
            _screen.addDrawjob(new DrawJob(_sprite, _position - _screen.getMapOffset(), _side, _drawingY));
        }

        internal bool isAnimated()
        {
            return _sprite.getMaxFrames() > 1;
        }
    }

}

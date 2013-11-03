using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PikeAndShot
{
    public class Shot : ScreenObject
    {
        public const int WIDTH = 2;
        public const int HEIGHT = 2;
        public const int GRAVITY = 18;

        public const int STATE_FLYING = 1;
        public const int STATE_GROUND = 2;
        
        protected Vector2 _origin;
        protected int _damage;
        protected float _speed;
        protected float _animationTime;
        protected float _groundTime;
        protected float _height;
        protected Sprite _sprite;
        protected Sprite _ground;

        public Shot(Vector2 position, BattleScreen screen, int side, float height) : base(screen, side)
        {
            _position = position;
            _state = STATE_FLYING;
            _height = height;
            _origin = position;
            _groundTime = 500f;

            _drawingY = _position.Y + height;
            _guardPositionOffset = new Vector2(0f, -15f);
        }

        public virtual void draw(SpriteBatch spritebatch)
        {
            spritebatch.DrawString(PikeAndShotGame.getSpriteFont(), "-", _position - _screen.getMapOffset(), Color.White);
        }

        public virtual void update(TimeSpan timeSpan)
        {
            if(_state == STATE_FLYING)
            {
                if (_side == BattleScreen.SIDE_PLAYER)
                {
                    _position.X += (float)timeSpan.TotalMilliseconds * _speed;
                }
                else
                {
                    _position.X -= (float)timeSpan.TotalMilliseconds * _speed;
                }

                _position.Y += GRAVITY * (float)timeSpan.TotalSeconds;


                // check to see if out of play
                if ((_position.X + WIDTH < 0 + _screen.getMapOffset().X || _position.X > PikeAndShotGame.SCREENWIDTH + _screen.getMapOffset().X) || (_position.Y + HEIGHT < 0 + _screen.getMapOffset().Y || _position.Y > PikeAndShotGame.SCREENHEIGHT + _screen.getMapOffset().Y))
                {
                    _state = STATE_DEAD;
                }

                // check to see if hit ground
                if (_position.Y - _origin.Y > _height)
                {
                    _state = STATE_GROUND;
                    _stateTimer = _groundTime;
                }

                if (_stateTimer <= 0)
                {
                    _stateTimer = _animationTime;
                }
            }
            else if (_state == STATE_GROUND)
            {
                if (_stateTimer <= 0)
                {
                    _state = STATE_DEAD;
                }
            }

            _stateTimer -= (float)timeSpan.TotalMilliseconds;
        }

        public override int getWidth()
        {
            return _sprite.getBoundingRect().Width;
        }

        public override int getHeight()
        {
            return _sprite.getBoundingRect().Height;
        }

        public bool isDead()
        {
            return _state == STATE_DEAD;
        }

        public override void collide(ScreenObject collider)
        {
            /*
            if (collider is Soldier && collider.getSide() != _side && collider.getState() != STATE_DEAD && collider.getState() != STATE_DYING && _state != STATE_DEAD)
            {
                _state = STATE_DEAD;
            }
            else if (collider is Soldier && this == ((Soldier)collider).getKiller())
            {
                _state = STATE_DEAD;
            }
            else if (collider is Pavise && collider.getSide() != _side && collider.getStateTimer() == 500f)
                _state = STATE_DEAD;
              */
        }

        internal void hit()
        {
            _state = STATE_GROUND;
            _stateTimer = _groundTime;
        }

        public override Vector2 getCenter()
        {
            return new Vector2(_position.X + 2f, _position.Y + 1f);
        }
    }

    public class ArquebusierShot : Shot
    {
        private ScreenAnimation _smoke;

        public ArquebusierShot(Vector2 position, BattleScreen screen, int side, float height)
            : base(position, screen, side, height)
        {
            _speed = 0.5f;
            _damage = 1;
            _animationTime = 150f;

            _sprite = new Sprite(PikeAndShotGame.ARQUEBUSIER_SHOT3, new Rectangle(16, 4, 2, 2), 18, 8);
            _ground = new Sprite(PikeAndShotGame.ARQUEBUSIER_GROUND, new Rectangle(16, 12, 2, 2), 30, 14);

            // create separate smoke animation that is not dependant on the shot itself
            _smoke = new ArquebusierSmoke(screen, side, position);
        }

        public override void update(TimeSpan timeSpan)
        {
            base.update(timeSpan);
        }

        public override void draw(SpriteBatch spritebatch)
        {
            if (_state == STATE_FLYING)
            {
                if (_stateTimer <= _animationTime / 4f)
                    _sprite.setFrame(3);
                else if (_stateTimer <= 2f * _animationTime / 4f)
                    _sprite.setFrame(2);
                else if (_stateTimer <= 3f * _animationTime / 4f)
                    _sprite.setFrame(1);
                else
                    _sprite.setFrame(0);

                _screen.addDrawjob(new DrawJob(_sprite, _position - _screen.getMapOffset(), _side, _drawingY));
            }
            else if (_state == STATE_GROUND)
            {
                if (_stateTimer <= _groundTime / 9f)
                    _ground.setFrame(8);
                else if (_stateTimer <= 2f * _groundTime / 9f)
                    _ground.setFrame(7);
                else if (_stateTimer <= 3f * _groundTime / 9f)
                    _ground.setFrame(6);
                else if (_stateTimer <= 4f * _groundTime / 9f)
                    _ground.setFrame(5);
                else if (_stateTimer <= 5f * _groundTime / 9f)
                    _ground.setFrame(4);
                else if (_stateTimer <= 6f * _groundTime / 9f)
                    _ground.setFrame(3);
                else if (_stateTimer <= 7f * _groundTime / 9f)
                    _ground.setFrame(2);
                else if (_stateTimer <= 8f * _groundTime / 9f)
                    _ground.setFrame(1);
                else
                    _ground.setFrame(0);

                _screen.addDrawjob(new DrawJob(_ground, _position - _screen.getMapOffset(), _side, _drawingY));
            }

        }

    }

    public class CrossbowShot : Shot
    {        
        public CrossbowShot(Vector2 position, BattleScreen screen, int side, float height)
            : base(position, screen, side, height)
        {
            _speed = 0.4f;
            _damage = 1;
            _animationTime = 200f;
            //_sprite = new Sprite(PikeAndShotGame.CROSSBOWMAN_BOLT, new Rectangle(2, 4, 12, 2), 16, 10);
            _sprite = new Sprite(PikeAndShotGame.CROSSBOWMAN_BOLT2, new Rectangle(10, 4, 12, 2), 24, 10);
            _ground = new Sprite(PikeAndShotGame.CROSSBOWMAN_GROUND, new Rectangle(2, 10, 12, 2), 16, 12);
        }

        public override void update(TimeSpan timeSpan)
        {
            base.update(timeSpan);
        }

        public override void draw(SpriteBatch spritebatch)
        {
            if(_state == STATE_FLYING)
            {
                /*
                if (_stateTimer <= _animationTime / 7f)
                    _sprite.setFrame(6);
                else if (_stateTimer <= 2f * _animationTime / 7f)
                    _sprite.setFrame(5);
                else if (_stateTimer <= 3f * _animationTime / 7f)
                    _sprite.setFrame(4);
                else if (_stateTimer <= 4f * _animationTime / 7f)
                    _sprite.setFrame(3);
                else if (_stateTimer <= 5f * _animationTime / 7f)
                    _sprite.setFrame(2);
                else if (_stateTimer <= 6f * _animationTime / 7f)
                    _sprite.setFrame(1);
                else
                    _sprite.setFrame(0);
                */

                if (_stateTimer <= _animationTime / 11f)
                    _sprite.setFrame(10);
                else if (_stateTimer <= 2f * _animationTime / 11f)
                    _sprite.setFrame(9);
                else if (_stateTimer <= 3f * _animationTime / 11f)
                    _sprite.setFrame(8);
                else if (_stateTimer <= 4f * _animationTime / 11f)
                    _sprite.setFrame(7);
                else if (_stateTimer <= 5f * _animationTime / 11f)
                    _sprite.setFrame(6);
                else if (_stateTimer <= 6f * _animationTime / 11f)
                    _sprite.setFrame(5);
                else if (_stateTimer <= 7f * _animationTime / 11f)
                    _sprite.setFrame(4);
                else if (_stateTimer <= 8f * _animationTime / 11f)
                    _sprite.setFrame(3);
                else if (_stateTimer <= 9f * _animationTime / 11f)
                    _sprite.setFrame(2);
                else if (_stateTimer <= 10f * _animationTime / 11f)
                    _sprite.setFrame(1);
                else
                    _sprite.setFrame(0);

                _screen.addDrawjob(new DrawJob(_sprite, _position - _screen.getMapOffset(), _side, _drawingY));
            }
            else if (_state == STATE_GROUND)
            {
                if (_stateTimer <= _groundTime / 9f)
                    _ground.setFrame(8);
                else if (_stateTimer <= 2f * _groundTime / 9f)
                    _ground.setFrame(7);
                else if (_stateTimer <= 3f * _groundTime / 9f)
                    _ground.setFrame(6);
                else if (_stateTimer <= 4f * _groundTime / 9f)
                    _ground.setFrame(5);
                else if (_stateTimer <= 5f * _groundTime / 9f)
                    _ground.setFrame(4);
                else if (_stateTimer <= 6f * _groundTime / 9f)
                    _ground.setFrame(3);
                else if (_stateTimer <= 7f * _groundTime / 9f)
                    _ground.setFrame(2);
                else if (_stateTimer <= 8f * _groundTime / 9f)
                    _ground.setFrame(1);
                else
                    _ground.setFrame(0);

                _screen.addDrawjob(new DrawJob(_ground, _position - _screen.getMapOffset(), _side, _drawingY));
            }
        }

    }

    public class Pavise : Shot
    {
        public const int STATE_GETTINGHIT = 100;

        public Pavise(Vector2 position, BattleScreen screen, int side, float height)
            : base(position, screen, side, height)
        {
            _animationTime = 200f;
            _speed = 0f;
            _sprite = new Sprite(PikeAndShotGame.PLACED_PAVISE, new Rectangle(8, 4, 12, 20), 24, 24);
            _ground = new Sprite(PikeAndShotGame.PAVISE_FALL, new Rectangle(26, 6, 12, 20), 50, 28);
        }

        public override void draw(SpriteBatch spritebatch)
        {
            if(_state != STATE_GROUND)
                _screen.addDrawjob(new DrawJob(_sprite, _position - _screen.getMapOffset(), _side, _drawingY));
            else
                _screen.addDrawjob(new DrawJob(_ground, _position - _screen.getMapOffset(), _side, _drawingY));
            
        }

        public override void update(TimeSpan timeSpan)
        {
            // check to see if out of play
            if ((_position.X + WIDTH < - 100f + _screen.getMapOffset().X || _position.X > PikeAndShotGame.SCREENWIDTH + 100f + _screen.getMapOffset().X) || (_position.Y + HEIGHT < 0 || _position.Y > PikeAndShotGame.SCREENHEIGHT))
            {
                _state = STATE_DEAD;
            }
            
            if (_state == STATE_GROUND)
            {
                _stateTimer -= (float)timeSpan.TotalMilliseconds;

                if (_stateTimer <= 0f)
                {
                    _stateTimer = 0f;
                    // when the pavise lies flat, it's drawingY is a little different
                    _drawingY -= 8f;
                }
                int maxFrames = _ground.getMaxFrames();
                float deathFrameTime = _groundTime / (float)maxFrames;
                int frameNumber = maxFrames - (int)(_stateTimer / deathFrameTime) - 1;

                _ground.setFrame(frameNumber);
            }
            else if (_state == STATE_GETTINGHIT)
            {
                _stateTimer -= (float)timeSpan.TotalMilliseconds;

                if (_stateTimer <= 0f)
                {
                    _stateTimer = _groundTime;
                    _state = STATE_GROUND;
                }
            }
        }

        public override void collide(ScreenObject collider)
        {
            if (_state != STATE_GROUND)
            {
                if ((collider is Shot || collider is WeaponAttack) && collider.getSide() != _side && collider.getState() != STATE_DEAD && collider.getState() != STATE_DYING && _state != STATE_GROUND && _state != STATE_GETTINGHIT)
                {
                    if (collider is PikeTip)
                    {
                        Pikeman pikeman = ((PikeTip)collider).getPikeman();
                        if (pikeman.getState() != Pikeman.STATE_RECOILING && pikeman.getState() != STATE_DYING && pikeman.getState() != STATE_DEAD && pikeman.getState() != Soldier.STATE_MELEE_LOSS && pikeman.getState() != Soldier.STATE_MELEE_WIN)
                        {
                            pikeman.recoil();
                            _state = STATE_GROUND;
                            _stateTimer = _groundTime;
                        }
                    }
                    else if (collider is LanceTip)
                    {
                        Cavalry pikeman = ((LanceTip)collider).getCavalry();
                        if (pikeman.getState() != Cavalry.STATE_RECOILING && pikeman.getState() != STATE_DYING && pikeman.getState() != STATE_DEAD && pikeman.getState() != Soldier.STATE_MELEE_LOSS && pikeman.getState() != Soldier.STATE_MELEE_WIN)
                        {
                            pikeman.recoil();
                            _state = STATE_GROUND;
                            _stateTimer = _groundTime;
                        }
                    }
                    else if (collider is Shot)
                    {
                        collider.setState(STATE_DEAD);
                        _state = STATE_GROUND;
                        _stateTimer = _groundTime;
                    }
                }
                else if ((collider is Soldier) && collider.getSide() != _side && collider.getState() != STATE_DEAD && collider.getState() != STATE_DEAD && collider.getState() != STATE_DYING && collider.getState() != Soldier.STATE_ONEATTACK && _state != STATE_GROUND && _state != STATE_GETTINGHIT)
                {
                    ((Soldier)collider).oneAttack();
                    _state = STATE_GETTINGHIT;
                    _stateTimer = ((Soldier)collider).getOneAttackTime();
                }
            }
        }
    }

    public class SkirmisherJavelin : SlingerRock
    {
        public SkirmisherJavelin(Vector2 position, BattleScreen screen, int side, float height)
            : base(position, screen, side, height)
        {
            _speed = 0.27f;
            _damage = 1;
            _animationTime = 250f;
            _sprite = new Sprite(PikeAndShotGame.SKIRMISHER_JAVELIN, new Rectangle(32, 6, 8, 4), 48, 14);
            _ground = new Sprite(PikeAndShotGame.SKIRMISHER_GROUND, new Rectangle(34, 16, 8, 4), 48, 20);
            _groundTime = 300f;
        }
    }

    public class SlingerRock : Shot
    {
        public SlingerRock(Vector2 position, BattleScreen screen, int side, float height)
            : base(position, screen, side, height)
        {
            _speed = 0.27f;
            _damage = 1;
            _animationTime = 250f;
            _sprite = new Sprite(PikeAndShotGame.SLINGER_ROCK, new Rectangle(14, 4, 4, 4), 22, 12);
            _ground = new Sprite(PikeAndShotGame.SLINGER_GROUND, new Rectangle(12, 4, 4, 4), 28, 12);
            _groundTime = 300f;
        }

        public override void update(TimeSpan timeSpan)
        {
            base.update(timeSpan);
        }

        public override void draw(SpriteBatch spritebatch)
        {
            if (_state == STATE_FLYING)
            {
                int maxFrames = _sprite.getMaxFrames();
                float frameTime = _animationTime / (float)maxFrames;
                int frameNumber = maxFrames - (int)(_stateTimer / frameTime) - 1;

                _sprite.setFrame(frameNumber);

                _screen.addDrawjob(new DrawJob(_sprite, _position - _screen.getMapOffset(), _side, _drawingY));

            }
            else if (_state == STATE_GROUND)
            {
                int maxFrames = _ground.getMaxFrames();
                float frameTime = _groundTime / (float)maxFrames;
                int frameNumber = maxFrames - (int)(_stateTimer / frameTime) - 1;

                _ground.setFrame(frameNumber);
                _screen.addDrawjob(new DrawJob(_ground, _position - _screen.getMapOffset(), _side, _drawingY));
            }
        }
    }

    public class WeaponAttack : ScreenObject
    {
        protected int _width;
        protected int _height;
        protected Vector2 _offset;
        protected Soldier _pikeman;

        public WeaponAttack(BattleScreen screen, Soldier pikeman)
            : base(screen, pikeman.getSide())
        {
            _pikeman = pikeman;
            _screen.removeScreenObject(this);
        }

        protected void initiatePosition()
        {
            update(_pikeman.getPosition());
        }   

        public void update(Vector2 position)
        {
            if (_side == BattleScreen.SIDE_PLAYER)
                _position = position + new Vector2(_offset.X, _offset.Y);
            else
                _position = position + new Vector2(-_offset.X - _width + _pikeman.getWidth(), _offset.Y);
        }

        public override int getWidth()
        {
            return _width;
        }

        public override int getHeight()
        {
            return _height;
        }

        public override Vector2 getCenter()
        {
            return new Vector2(_position.X + ((float)_width * 0.5f), _position.Y + ((float)_height * 0.5f));
        }

        public int getSoldierState()
        {
            return _pikeman.getState();
        }

    }

    public class ShieldBlock : WeaponAttack
    {

        public ShieldBlock(BattleScreen screen, Soldier pikeman)
            : base(screen, pikeman)
        {
            _width = 14;
            _height = 35;
            _offset = new Vector2(pikeman.getWidth(), 0);
            initiatePosition();
        }

        public override void collide(ScreenObject collider)
        {
            if (collider is PikeTip)
            {
                Pikeman pikeman = ((PikeTip)collider).getPikeman();
                if (pikeman.getState() != Pikeman.STATE_RECOILING &&
                    pikeman.getState() != STATE_DYING &&
                    pikeman.getState() != STATE_DEAD &&
                    pikeman.getState() != Soldier.STATE_MELEE_LOSS &&
                    pikeman.getState() != Soldier.STATE_MELEE_WIN &&
                    pikeman.getState() != Pikeman.STATE_RAISING &&
                    pikeman.guardTarget == this._pikeman)
                {
                    pikeman.recoil();
                    ((Targeteer)_pikeman).setReactionDest(collider.getCenter().X + (collider.getSide() == BattleScreen.SIDE_ENEMY ? 1 : -1) * collider.getWidth() * 0.5f + Soldier.WIDTH * 0.35f);
                    ((Targeteer)_pikeman).resetDefendTimer();
                }
            }
        }
    }

    public class PikeTip : WeaponAttack
    {
        public PikeTip(BattleScreen screen, Soldier pikeman)
            : base(screen, pikeman)
        {
            _width = 8;
            _height = 4;
            _offset = new Vector2(76, 18);
            initiatePosition();
        }

        public Pikeman getPikeman()
        {
            return (Pikeman)_pikeman;
        }
    }

    public class LanceTip : WeaponAttack
    {
        public LanceTip(BattleScreen screen, Soldier pikeman)
            : base(screen, pikeman)
        {
            _width = 8;
            _height = 2;
            _offset = new Vector2(78, 6);
            initiatePosition();
        }

        public Cavalry getCavalry()
        {
            return (Cavalry)_pikeman;
        }
    }

    public class WeaponSwing : WeaponAttack
    {
        public WeaponSwing(BattleScreen screen, Soldier pikeman)
            : base(screen, pikeman)
        {
            _width = 31;
            _height = 35;
            _offset = new Vector2(pikeman.getWidth(), 0);
            initiatePosition();
        }

        public override void collide(ScreenObject collider)
        {
            if (collider is PikeTip && _side != collider.getSide())
            {
                Pikeman pikeman = ((PikeTip)collider).getPikeman();
                if (pikeman.getState() != Pikeman.STATE_RECOILING && pikeman.getState() != STATE_DYING && pikeman.getState() != STATE_DEAD && pikeman.getState() != Soldier.STATE_MELEE_LOSS && pikeman.getState() != Soldier.STATE_MELEE_WIN)
                    pikeman.recoil();
            }
        }
    }
}

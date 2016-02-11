using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;

namespace PikeAndShot
{
    public interface DoppelSoldier
    {
        void chargeLogic(TimeSpan timeSpan);
        void charge();
        void chargeEnd();
    }

    public class Soldier : ScreenObject
    {
        // soldier types
        public const int TYPE_PIKE = 0;
        public const int TYPE_SHOT = 1;
        public const int TYPE_SUPPORT = 2;
        public const int TYPE_CAVALRY = 3;
        public const int TYPE_SWINGER = 4;
        public const int TYPE_MELEE = 5;

        // soldier classes
        public const int CLASS_MERC_PIKEMAN = 0;
        public const int CLASS_MERC_ARQUEBUSIER = 1;
        public const int CLASS_MERC_CROSSBOWMAN = 2;
        public const int CLASS_MERC_CROSSBOWMAN_PAVISE = 3;
        public const int CLASS_MERC_DOPPLE = 4;
        public const int CLASS_MERC_SOLDIER = 5;
        public const int CLASS_MERC_CAVALRY = 6;

        // goblin classes
        public const int CLASS_GOBLIN_SLINGER = 10;
        public const int CLASS_GOBLIN_BERZERKER = 11;
        public const int CLASS_GOBLIN_BRIGAND = 12;
        public const int CLASS_GOBLIN_HAULER = 13;
        public const int CLASS_GOBLIN_WOLF = 14;

        // leader classes
        public const int CLASS_LEADER_PUCELLE = -1;

        // soldier status
        public const int STATE_READY = 0;
        public const int STATE_ATTACKING = 1;
        public const int STATE_MELEE_WIN = 2;
        public const int STATE_MELEE_LOSS = 3;
        public const int STATE_RELOADING = 4;
        public const int STATE_CHARGING = 5;
        public const int STATE_ONEATTACK = 6;
        public const int STATE_RETREAT = 7;
        public const int STATE_CHECKING_EXIT = 8;
        public const int STATE_CHARGED = 9;

        public const int WIDTH = 28;
        public const int HEIGHT = 30;

        public const int MELEE_REPETITIONS = 3;

        public Vector2 _destination;
        public Vector2 _randDestOffset;
        public Vector2 _drawingPosition;
        public Vector2 _dest;
        public Vector2 _meleeDestination;

        protected int _lastAction;
        protected Vector2 _delta;
        public float _speed;         //per second
        protected float _footSpeed;
        protected Vector2 _travel;
        protected int _type;
        protected int _class;
        protected Vector2 _jostleOffset;
        public int _stateToHave;
        protected float _attackTime;
        protected float _reloadTime;
        protected float _deathTime;
        protected float _meleeTime;
        protected float _longMeleeTime;
        protected float _routeTime;
        protected float _routedTime;
        protected float _oneAttackTime;
        protected float _chargeTime;
        protected float _plusMinus;
        protected bool _stateChanged;   //keeps track of if the state has changed already this update, so we don't do checks too much
        protected bool _shotMade;
        protected bool _reacting;
        protected int _longMelee;
        protected ScreenObject _killer;

        public int preAttackState;
        
        protected Sprite _feet;
        protected Sprite _body;

        protected Sprite _idle;
        protected Sprite _death;
        protected Sprite _melee1;
        protected Sprite _defend1;
        protected Sprite _route;
        protected Sprite _routed;
        protected Sprite _retreat;
        protected Sprite _charge;

        public bool initCharge;
        public bool inPlayerFormation;

        public ScreenObject guardTarget;
        public float guardTargetRange;
        public float guardTargetDist;
        public Formation chargeTarget;

        public Soldier _engager;
        public Formation myFormation;

        public bool DEBUGFOUNDPIKE;
        public float breakRange = 0;

        protected SoundEffectInstance bodyFallSound;
        protected SoundEffectInstance hitSound;
        protected SoundEffectInstance chargeSound;
        protected bool playedFallSound;
        protected bool playedMeleeSound;

        public Soldier(BattleScreen screen, int side, float x, float y): base(screen, side)
        {
            _position = new Vector2(x, y);
            _destination = new Vector2(x, y);
            _dest = new Vector2(0,0);
            _meleeDestination = new Vector2(0, 0);
            _drawingPosition = Vector2.Zero;
            _randDestOffset = new Vector2(PikeAndShotGame.getRandPlusMinus(3), 0);
            _lastAction = PatternAction.ACTION_IDLE;
            _delta = Vector2.Zero;
            _travel = Vector2.Zero;
            _state = STATE_READY;
            _stateTimer = 0;
            _stateChanged = false;
            _shotMade = false;
            _stateToHave = -1;
            _meleeTime = 1500f;
            _longMeleeTime = 3000f;
            _oneAttackTime = _meleeTime/3f;
            _deathTime = 2000f;
            _routeTime = 500f;
            _routedTime = 100f;
            _plusMinus = 0f;
            _jostleOffset = Vector2.Zero;
            _guardPositionOffset = Vector2.Zero;
            _reacting = false;
            _longMelee = 0;
            _drawingY = _position.Y + 36f;
            _speed = 0.15f;
            _footSpeed = 15;
            _type = Soldier.TYPE_MELEE;
            _class = Soldier.CLASS_MERC_SOLDIER;
            _attackTime = 600f;
            _reloadTime = 1000f;
            guardTarget = null;
            guardTargetRange = 0f;
            guardTargetDist = 0f;
            _engager = null;
            _chargeTime = 800f;//400f;

            _feet = new Sprite(PikeAndShotGame.SOLDIER_FEET, new Rectangle(4, 2, 16, 12), 26, 16, true);
            _idle = new Sprite(PikeAndShotGame.SOLDIER_IDLE, new Rectangle(10, 2, 16, 28), 46, 42);
            _death = new Sprite(PikeAndShotGame.SOLDIER_DEATH, new Rectangle(40, 2, 16, 28), 72, 40);
            _melee1 = new Sprite(PikeAndShotGame.SOLDIER_MELEE1, new Rectangle(24, 30, 16, 28), 64, 68);
            _defend1 = new Sprite(PikeAndShotGame.SOLDIER_DEFEND1, new Rectangle(20, 2, 16, 28), 52, 40);
            // just giving all soldiers the slinger routes for now
            _route = new Sprite(PikeAndShotGame.SOLDIER_ROUTE, new Rectangle(26, 16, 16, 28), 70, 52);
            _routed = new Sprite(PikeAndShotGame.SOLDIER_ROUTED, new Rectangle(16, 16, 16, 28), 50, 52, true);
            _retreat = new Sprite(PikeAndShotGame.SLINGER_RETREAT, new Rectangle(6, 2, 16, 28), 46, 40, true);
            _charge = new Sprite(PikeAndShotGame.SOLDIER_CHARGE, new Rectangle(20, 20, 16, 28), 60, 56);

            _body = _idle;

            _feet.setAnimationSpeed(_footSpeed / 0.11f);
            _retreat.setAnimationSpeed(_footSpeed / 0.11f);
            inPlayerFormation = false;

            // DEBUG VARS
            DEBUGFOUNDPIKE = false;
            bodyFallSound = PikeAndShotGame.BODY_FALL.CreateInstance();
            bodyFallSound.Volume = 0.3f;
            hitSound = PikeAndShotGame.OWW_ALLY.CreateInstance();
            chargeSound = PikeAndShotGame.CHARGE_ROAR.CreateInstance();
            chargeSound.Volume = 0.25f;
            playedFallSound = false;
        }

        public static void getNewSoldier(int soldierClass, BattleScreen screen, Formation _newEnemyFormation, float x, float y)
        {
            switch (soldierClass)
            {
                case Soldier.CLASS_MERC_PIKEMAN:
                    _newEnemyFormation.addSoldier(new Pikeman(screen, x, y, BattleScreen.SIDE_ENEMY));
                    break;
                case Soldier.CLASS_MERC_ARQUEBUSIER:
                    _newEnemyFormation.addSoldier(new Arquebusier(screen, x, y, BattleScreen.SIDE_ENEMY));
                    break;
                case Soldier.CLASS_MERC_CROSSBOWMAN:
                    _newEnemyFormation.addSoldier(new Crossbowman(screen, x, y, BattleScreen.SIDE_ENEMY));
                    break;
                case Soldier.CLASS_MERC_SOLDIER:
                    _newEnemyFormation.addSoldier(new Targeteer(screen, x, y, BattleScreen.SIDE_ENEMY));
                    break;
                case Soldier.CLASS_GOBLIN_SLINGER:
                    _newEnemyFormation.addSoldier(new Slinger(screen, x, y, BattleScreen.SIDE_ENEMY));
                    break;
                case Soldier.CLASS_MERC_DOPPLE:
                    _newEnemyFormation.addSoldier(new Dopple(screen, x, y, BattleScreen.SIDE_ENEMY));
                    break;
                case Soldier.CLASS_GOBLIN_BERZERKER:
                    _newEnemyFormation.addSoldier(new Berzerker(screen, x, y, BattleScreen.SIDE_ENEMY));
                    break;
                case Soldier.CLASS_GOBLIN_BRIGAND:
                    _newEnemyFormation.addSoldier(new Brigand(screen, x, y, BattleScreen.SIDE_ENEMY));
                    break;
                case Soldier.CLASS_MERC_CROSSBOWMAN_PAVISE:
                    _newEnemyFormation.addSoldier(new CrossbowmanPavise(screen, x, y, BattleScreen.SIDE_ENEMY));
                    break;
                case Soldier.CLASS_MERC_CAVALRY:
                    _newEnemyFormation.addSoldier(new Cavalry(screen, x, y, BattleScreen.SIDE_ENEMY));
                    break;
            }
        }

        public void changeRandOffset()
        {
            _randDestOffset.X = PikeAndShotGame.getRandPlusMinus(4);
            _randDestOffset.Y = PikeAndShotGame.getRandPlusMinus(4);
        }

        public void setAction(int newAction)
        {
            _lastAction = newAction;
        }

        public override Vector2 getCenter()
        {
            return new Vector2(_position.X + 8f, _position.Y + 14f);
        }

        public int getLastAction()
        {
            return _lastAction;
        }

        public float getSpeed()
        {
            return _speed;
        }

        public bool isDead()
        {
            return _state == STATE_DEAD;
        }

        public bool shotMade()
        {
            return _shotMade;
        }

        public int getType()
        {
            return _type;
        }

        public int getClass()
        {
            return _class;
        }

        public void selectedDraw(SpriteBatch spritebatch)
        {
            _drawingPosition = _position + _randDestOffset - _screen.getMapOffset();
            _charge.setFrame(3);
            _feet.setFrame(1);
            _screen.addDrawjob(new DrawJob(_feet, _drawingPosition + new Vector2(0, _idle.getBoundingRect().Height - 4), _state != STATE_RETREAT && _state != STATE_ROUTED ? _side : _side * -1, _drawingY));
            _screen.addDrawjob(new DrawJob(_charge, _drawingPosition + _jostleOffset, _state != STATE_ROUTED ? _side : _side * -1, _drawingY));
        }

        public virtual void draw(SpriteBatch spritebatch)
        {
            _drawingPosition = _position + _randDestOffset - _screen.getMapOffset();

            if (_state != STATE_DYING && _state != STATE_DEAD)
            {
                if (this is Targeteer)
                {
                    if( _state != Targeteer.STATE_SHIELDBREAK)
                        addDrawjob(new DrawJob(_feet, _drawingPosition + new Vector2(0, _idle.getBoundingRect().Height - 4), _state != STATE_RETREAT && _state != STATE_ROUTED ? _side : _side * -1, _drawingY));
                }
                else if (this is DismountedCavalry )
                {
                    if(_state != DismountedCavalry.STATE_FALLING)
                        addDrawjob(new DrawJob(_feet, _drawingPosition + new Vector2(0, _idle.getBoundingRect().Height - 4), _state != STATE_RETREAT && _state != STATE_ROUTED ? _side : _side * -1, _drawingY));
                }
                else if (this is Dopple)
                {
                    if (!(_state == STATE_CHARGING || _state == STATE_ATTACKING || _state == STATE_RELOADING) && _destination.X - _position.X < -Soldier.WIDTH)
                        addDrawjob(new DrawJob(_feet, _drawingPosition + new Vector2(0, _idle.getBoundingRect().Height - 4), _side * -1, _drawingY));
                    else
                        addDrawjob(new DrawJob(_feet, _drawingPosition + new Vector2(0, _idle.getBoundingRect().Height - 4), _state != STATE_RETREAT && _state != STATE_ROUTED ? _side : _side * -1, _drawingY));
                }
                else
                    addDrawjob(new DrawJob(_feet, _drawingPosition + new Vector2(0, _idle.getBoundingRect().Height - 4), _state != STATE_RETREAT && _state != STATE_ROUTED ? _side : _side * -1, _drawingY));
            }

            if (this is Dopple)
            {
                if (!(_state == STATE_CHARGING || _state == STATE_ATTACKING || _state == STATE_RELOADING) && _destination.X - _position.X < -Soldier.WIDTH)
                    addDrawjob(new DrawJob(_body, _drawingPosition + _jostleOffset, _side * -1, _drawingY));
                else
                    addDrawjob(new DrawJob(_body, _drawingPosition + _jostleOffset, _state != STATE_ROUTED ? _side : _side * -1, _drawingY));
            }
            else
                addDrawjob(new DrawJob(_body, _drawingPosition + _jostleOffset, _state != STATE_ROUTED ? _side : _side * -1, _drawingY));

            //spritebatch.Draw(PikeAndShotGame.getDotTexture(), _drawingPosition, Color.White);
        }

        protected void addDrawjob(DrawJob drawJob)
        {
            _screen.addDrawjob(drawJob);
            Sprite sprite = drawJob.sprite;
            if (sprite.flashable)
            {
                DrawJob flashJob = new DrawJob(sprite, drawJob.position, drawJob.side, drawJob.drawingY + 1, _stateTimer/_stateTime);
                _screen.addDrawjob(flashJob);
            }
        }

        public virtual void update(TimeSpan timeSpan)
        {
            _stateChanged = false;
            bool guarding = true;//this is Pikeman || this is Dopple || _type == TYPE_SHOT;

            if (guarding)
            {
                guarding = guardTarget != null && _state != STATE_DYING && _state != STATE_DEAD;
                if (guardTarget != null && guardTarget._position.X - _screen.getMapOffset().X < 0 - WIDTH)
                {
                    guardTarget = null;
                    _speed = 0.15f;
                    guarding = false;
                }
                    
            }

            if (_stateToHave != -1)
            {
                _state = _stateToHave;
                _stateToHave = -1;
            }

            if (_reacting && _state != STATE_DEAD && _state != STATE_DYING
                && _state != STATE_MELEE_LOSS && _state != STATE_MELEE_WIN
                && (_state != Cavalry.STATE_SLOWDOWN || !(this is Cavalry)) && (_state != Cavalry.STATE_TURNING || !(this is Cavalry)))
            {
                checkReactions(timeSpan);
                updateState(timeSpan);
                updateAnimation(timeSpan);
                return;
            }
            // check for REACTIONS
            else if ((_state == STATE_READY || _state == STATE_CHARGING || _state == STATE_CHARGED) /*&& !guarding*/)
            {
                if (checkReactions(timeSpan))
                {
                    updateState(timeSpan);
                    updateAnimation(timeSpan);
                    return;
                }
            }

            // Gotta keep enemy charging soldiers charging at teh formation
            if (_state == STATE_CHARGING && _side == BattleScreen.SIDE_ENEMY)
            {
                if (_screen.getPlayerFormation().getPosition().X > _position.X)
                    _destination.X = -1000f;
                else
                    _destination = new Vector2(_screen.getPlayerFormation().getCenter().X, _screen.getPlayerFormation().getCenter().Y);
            }
            if (guarding)
            {
                if (Math.Abs(guardTarget.getCenter().X - getCenter().X) > guardTargetRange)
                {
                    _delta = new Vector2(_destination.X - _position.X, guardTarget.getPosition().Y + guardTarget._guardPositionOffset.Y - _position.Y);
                    _dest = new Vector2(_destination.X, guardTarget.getPosition().Y + guardTarget._guardPositionOffset.Y);
                }
                else
                {
                    _delta = new Vector2(guardTarget.getCenter().X + guardTarget._guardPositionOffset.X - guardTargetDist - _position.X, guardTarget.getPosition().Y + guardTarget._guardPositionOffset.Y - _position.Y);
                    _dest = new Vector2(guardTarget.getCenter().X + guardTarget._guardPositionOffset.X - guardTargetDist, guardTarget.getPosition().Y + guardTarget._guardPositionOffset.Y);
                }
            }
            else if (_state != STATE_MELEE_LOSS && _state != STATE_MELEE_WIN && _state != STATE_ONEATTACK 
                && (_state != Cavalry.STATE_SLOWDOWN || !(this is Cavalry)) && (_state != Cavalry.STATE_TURNING || !(this is Cavalry))
                && (_state != Targeteer.STATE_SHIELDBREAK || !(this is Targeteer)) && (_state != Targeteer.STATE_DEFEND || !(this is Targeteer)) && (_state != DismountedCavalry.STATE_FALLING || !(this is DismountedCavalry))
                && (_state != Soldier.STATE_ATTACKING || !(this is Arquebusier || this is Dopple || this is CrossbowmanPavise))
                && (_state != Soldier.STATE_RELOADING || !(this is Dopple || this is CrossbowmanPavise))
                && ((_state != Soldier.STATE_CHARGING || !initCharge )|| !(this is CrossbowmanPavise))
                && ((_state != CrossbowmanPavise.STATE_PLACING) || !(this is CrossbowmanPavise))
                && ((_state != CrossbowmanPavise.STATE_RETRIEVING) || !(this is CrossbowmanPavise)))
            {                
                _delta = _destination - _position;
                _dest = _destination;
            }
            else
            {
                _delta = _meleeDestination - _position;
                _dest = _meleeDestination;

            }

            if (_side == BattleScreen.SIDE_PLAYER && myFormation == _screen.getPlayerFormation())
            {
                _travel.X = (float)timeSpan.TotalMilliseconds * _speed;
                _travel.Y = (float)timeSpan.TotalMilliseconds * _speed;
            }
            else
            {
                //float absDeltaX = Math.Abs(_delta.X);
                //float absDeltaY = Math.Abs(_delta.Y);
                double angle = Math.Atan2(_delta.Y, _delta.X);
                //_travel.X = (absDeltaX / (absDeltaX + absDeltaY)) * (float)timeSpan.TotalMilliseconds * _speed;
                //_travel.Y = (absDeltaY / (absDeltaX + absDeltaY)) * (float)timeSpan.TotalMilliseconds * _speed;
                double cos = Math.Cos(angle);
                double sin = Math.Sin(angle);
                _travel.X = (float)cos * (float)timeSpan.TotalMilliseconds * _speed;
                _travel.Y = (float)sin * (float)timeSpan.TotalMilliseconds * _speed;
                
                //fix the sign for the trig quadrant
                if (_delta.X < 0)
                    _travel.X *= -1;
                if (_delta.Y < 0)
                    _travel.Y *= -1;

            }

            // check to see if walking
            if (_delta.Length() != 0)
            {
                if (!_feet.getPlaying())
                {
                    _feet.playRandomStart();
                    _feet.nextFrame();
                }
                if (!_retreat.getPlaying())
                {
                    _retreat.play();
                    _retreat.nextFrame();
                }
            }
            else
            {
                _feet.stop();
                _feet.reset();
                _retreat.stop();
                _retreat.reset();
            }

            _feet.update(timeSpan);
            _retreat.update(timeSpan);

            if (_feet.getCurrFrame() % 2 > 0)
                _jostleOffset.Y = 1f;
            else
                _jostleOffset.Y = 0f;

            // as long as we are not at destination, keep trying to get there, but don't overshoot
            if (_delta.X > 0)
            {
                if (_delta.X - _travel.X >= 0)
                    _position.X += _travel.X;
                else
                    _position.X = _dest.X;
            }
            else if (_delta.X < 0)
            {
                if (_delta.X + _travel.X <= 0)
                    _position.X -= _travel.X;
                else
                    _position.X = _dest.X;
            }

            if (_delta.Y > 0)
            {
                if (_delta.Y - _travel.Y >= 0)
                    _position.Y += _travel.Y;
                else
                    _position.Y = _dest.Y;
            }
            else if (_delta.Y < 0)
            {
                if (_delta.Y + _travel.Y <= 0)
                    _position.Y -= _travel.Y;
                else
                    _position.Y = _dest.Y;
            }

            updateState(timeSpan);
            updateAnimation(timeSpan);
            _drawingY = _position.Y + 36f;
        }

        protected virtual bool checkReactions(TimeSpan timeSpan)
        {
            if ((_screen.findPikeTip(this, 4f) || _state == STATE_ROUTE || _state == STATE_ROUTED) && _state != STATE_ATTACKING && _state != STATE_RELOADING)
            {
                if (_state != STATE_ROUTE && _state != STATE_ROUTED)
                {
                    _reacting = true;
                    _state = STATE_ROUTE;
                    _stateTimer = _routeTime;
                    //_destination = _position;
                    _screen.addLooseSoldier(this);
                }

                setReactionDest(PikeAndShotGame.SCREENWIDTH + _screen.getMapOffset().X + Soldier.WIDTH * 2f);
                _delta = _meleeDestination - _position;
                _dest = _meleeDestination;
                float absDeltaX = Math.Abs(_delta.X);
                float absDeltaY = Math.Abs(_delta.Y);
                _travel.X = (absDeltaX / (absDeltaX + absDeltaY)) * (float)timeSpan.TotalMilliseconds * _speed;
                _travel.Y = (absDeltaY / (absDeltaX + absDeltaY)) * (float)timeSpan.TotalMilliseconds * _speed;
                // check to see if walking
                if (_delta.Length() != 0)
                {
                    if (!_feet.getPlaying())
                    {
                        _feet.play();
                        _feet.nextFrame();
                    }
                    if (!_retreat.getPlaying())
                    {
                        _retreat.play();
                        _retreat.nextFrame();
                    }
                }
                else
                {
                    _feet.stop();
                    _feet.reset();
                    _retreat.stop();
                    _retreat.reset();
                }

                _feet.update(timeSpan);
                _retreat.update(timeSpan);

                if (_feet.getCurrFrame() % 2 > 0)
                    _jostleOffset.Y = 1f;
                else
                    _jostleOffset.Y = 0f;

                // as long as we are not at destination, keep trying to get there, but don't overshoot
                if (_delta.X > 0)
                {
                    if (_delta.X - _travel.X >= 0)
                        _position.X += _travel.X;
                    else
                        _position.X = _dest.X;
                }
                else if (_delta.X < 0)
                {
                    if (_delta.X + _travel.X <= 0)
                        _position.X -= _travel.X;
                    else
                        _position.X = _dest.X;
                }

                if (_delta.Y > 0)
                {
                    if (_delta.Y - _travel.Y >= 0)
                        _position.Y += _travel.Y;
                    else
                        _position.Y = _dest.Y;
                }
                else if (_delta.Y < 0)
                {
                    if (_delta.Y + _travel.Y <= 0)
                        _position.Y -= _travel.Y;
                    else
                        _position.Y = _dest.Y;
                }

                return true;
            }

            return false;
        }

        public void setReactionDest(float p)
        {
            //if ((p - _meleeDestination.X) * _side < 0)
            //{
            if (this is Dopple)
            {
                _meleeDestination.X = p;
                _meleeDestination.Y = _position.Y;
            }
            else
            {
                _meleeDestination.X = p;
                _meleeDestination.Y = _position.Y;
            }
            //}
        }

        protected virtual void updateState(TimeSpan timeSpan)
        {
            if (!_stateChanged)
            {
                if (_state == STATE_ATTACKING)
                {
                    _stateTimer -= (float)timeSpan.TotalMilliseconds;

                    if (_stateTimer <= 0)
                    {
                        _stateTimer = 0f;
                        attackDone();
                        _stateChanged = true;
                    }
                }
                else if (_state == STATE_CHARGING)
                {
                    if (this is DoppelSoldier)
                        ((DoppelSoldier)this).chargeLogic(timeSpan);
                    else
                    {
                        _stateTimer -= (float)timeSpan.TotalMilliseconds;
                        if (initCharge)
                        {
                            setSpeed(0.11f + (0.03f * (1f - (_stateTimer / (_chargeTime/2f)))));
                        }

                        if (_stateTimer <= 0)
                        {
                            if (!initCharge)
                            {
                                initCharge = true;
                                _stateTimer = (_chargeTime / 2f);
                                _destination.X += PikeAndShotGame.SCREENWIDTH * this._side;
                                _screen.addLooseSoldier(this);
                                //_speed = 0.14f;
                            }
                            else
                                _stateTimer = 0f;
                        }
                        if (_destination == _position && initCharge)
                        {
                            if (this is Targeteer)
                            {
                                ((Targeteer)this).cover();
                                _reacting = false;
                            }
                            //change state down here so that if we cover(), the state won't stay as STATE_COVER
                            _state = STATE_CHARGED;
                        }
                    }
                }
                else if (_state == STATE_DYING)
                {
                    _stateTimer -= (float)timeSpan.TotalMilliseconds;
                    if (_stateTimer <= 0)
                    {
                        _stateTimer = 0f;
                        _state = STATE_DEAD;
                        _stateChanged = true;
                    }
                }
                else if (_state == STATE_MELEE_WIN)
                {
                    if (_engager.getState() != STATE_DYING)
                    {
                        _stateTimer -= (float)timeSpan.TotalMilliseconds;
                        if (_stateTimer <= 0)
                        {
                            if(_longMelee <= 0)
                                winMelee();
                            else
                            {
                                _stateTimer = _meleeTime;
                                _longMelee--;
                            }
                        }
                    }
                    else
                        winMelee();
                }
                else if (_state == STATE_MELEE_LOSS)
                {
                    if (_engager.getState() != STATE_DYING)
                    {
                        _stateTimer -= (float)timeSpan.TotalMilliseconds;
                        if (_stateTimer <= 0)
                        {
                            if (_longMelee <= 0)
                            {
                                _stateTimer = 0f;
                                _state = preAttackState;
                                if(!(this is Leader))
                                    hit();
                                _stateChanged = true;
                            }
                            else
                            {
                                _stateTimer = _meleeTime;
                                _longMelee--;
                            }
                        }
                    }
                    else
                        winMelee();
                }
                else if (_state == STATE_ROUTE)
                {
                    _stateTimer -= (float)timeSpan.TotalMilliseconds;
                    if (_stateTimer <= 0)
                    {
                        _stateTimer = _routedTime;
                        _state = STATE_ROUTED;
                        _stateChanged = true;
                    }
                }
                else if (_state == STATE_ROUTED)
                {
                    _stateTimer -= (float)timeSpan.TotalMilliseconds;
                    if (_stateTimer <= 0)
                    {
                        _stateTimer = _routedTime;
                        _routed.nextFrame();
                    }
                }
                else if (_state == STATE_ONEATTACK)
                {
                    _stateTimer -= (float)timeSpan.TotalMilliseconds;
                    if (_stateTimer <= 0)
                    {
                        _stateTimer = 0f;
                        _state = preAttackState;
                        _stateChanged = true;
                        paviseToHit.knockOver();
                    }
                }
            }
        }

        protected virtual void winMelee()
        {
            _stateTimer = 0f;
            _state = preAttackState;
            if (myFormation.retreated)
                route();
            _stateChanged = true;
        }

        protected virtual void updateAnimation(TimeSpan timeSpan)
        {
            if (_state == STATE_DYING)
            {
                int maxFrames = _death.getMaxFrames() * 3;
                float deathFrameTime = _deathTime / (float)maxFrames;
                int frameNumber = maxFrames - (int)(_stateTimer / deathFrameTime) -1;

                if(frameNumber == _death.getMaxFrames()-1 && !playedFallSound)
                {
                    bodyFallSound.Play();
                    playedFallSound = true;
                }

                if (frameNumber < _death.getMaxFrames())
                    _death.setFrame(frameNumber);
                else
                    _death.setFrame(_death.getMaxFrames() - 1);

                if (this is Cavalry)
                    _feet = _death;
                else
                    _body = _death;
            }
            else if (_state == STATE_CHARGING && !(this is Dopple))
            {
                int maxFrames = initCharge ? _defend1.getMaxFrames() : _charge.getMaxFrames() * 2;
                float frameTime = _chargeTime / (float)maxFrames;
                int frameNumber = maxFrames - (int)(_stateTimer / frameTime);

                frameNumber -= 1;
                if (initCharge)
                {
                    if (!(this is Targeteer) || (this is Targeteer && ((Targeteer)this)._hasShield))
                    {
                        _defend1.setFrame(frameNumber);
                        _body = _defend1;
                    }
                    else if (this is Targeteer)
                    {
                        ((Targeteer)this)._defend2.setFrame(frameNumber);
                        _body = ((Targeteer)this)._defend2;
                    }
                    else
                    {
                        _body = _idle;
                    }
                }
                else
                {
                    if (frameNumber >= _charge.getMaxFrames())
                    {
                        frameNumber -= _charge.getMaxFrames();
                        frameNumber = _charge.getMaxFrames() - frameNumber - 1;
                    }
                    if (!(this is Targeteer) || (this is Targeteer && ((Targeteer)this)._hasShield))
                    {
                        _charge.setFrame(frameNumber);
                        _body = _charge;
                    }
                    else
                    {
                        ((Targeteer)this)._chargeNoShield.setFrame(frameNumber);
                        _body = ((Targeteer)this)._chargeNoShield;
                    }
                }
            }
            else if (_state == STATE_DEAD)
            {
                if (this is Cavalry)
                    _feet = _death;
                else
                    _body = _death;
            }
            else if (_state == STATE_MELEE_WIN)
            {
                int maxFrames = _melee1.getMaxFrames() + _defend1.getMaxFrames();
                float frameTime = _meleeTime * 2 / 3 / (float)maxFrames;
                float time = _stateTimer;

                // I want this animation to look like 2 defend and attack sequences
                // the animation is split into two separate defend and attack frames
                // therefore if we are in the greater half, just reduce this by half so we can figure out where we are 
                // based on the first half since this is just repeated
                if (_stateTimer > _meleeTime * 2 / 3)
                {
                    time -= (_meleeTime * 2 / 3);
                }

                int frameNumber = maxFrames - (int)(time / frameTime) - 1;

                // if we are at a frame higher than there are defend frames, we must be attacking
                if (frameNumber >= _defend1.getMaxFrames())
                {
                    frameNumber -= _defend1.getMaxFrames();
                    _melee1.setFrame(frameNumber);
                    _body = _melee1;
                    if (!playedMeleeSound && frameNumber == _melee1.getMaxFrames() - 2 && _screen is LevelScreen)
                    {
                        ((LevelScreen)_screen).makeMeleeSound();
                        playedMeleeSound = true;
                    }
                    else if (frameNumber == _melee1.getMaxFrames() - 1)
                        playedMeleeSound = false;
                }
                else
                {
                    _defend1.setFrame(frameNumber);
                    _body = _defend1;
                }
            }
            else if (_state == STATE_MELEE_LOSS)
            {
                int maxFrames = _melee1.getMaxFrames() + _defend1.getMaxFrames();
                float frameTime = _meleeTime * 2 / 3 / (float)maxFrames;
                float time = _stateTimer;

                // I want this animation to look like 2 defend and attack sequences
                // the animation is split into two separate defend and attack frames
                // therefore if we are in the greater half, just reduce this by half so we can figure out where we are 
                // based on the first half since this is just repeated
                if (_stateTimer > _meleeTime * 2 / 3)
                {
                    time -= (_meleeTime * 2 / 3);
                }

                int frameNumber = maxFrames - (int)(time / frameTime) - 1;

                // if we are at a frame higher than there are defend frames, we must be attacking
                if (frameNumber >= _melee1.getMaxFrames())
                {
                    frameNumber -= _melee1.getMaxFrames();
                    _defend1.setFrame(frameNumber);
                    _body = _defend1;
                }
                else
                {
                    _melee1.setFrame(frameNumber);
                    _body = _melee1;
                    if (!playedMeleeSound && frameNumber == _melee1.getMaxFrames() - 2 && _screen is LevelScreen)
                    {
                        ((LevelScreen)_screen).makeMeleeSound();
                        playedMeleeSound = true;
                    }
                    else if (frameNumber == _melee1.getMaxFrames() - 1)
                        playedMeleeSound = false;
                }
            }
            else if (_state == STATE_ROUTE)
            {
                int maxFrames = _route.getMaxFrames();
                float frameTime = _routeTime / (float)maxFrames;
                int frameNumber = maxFrames - (int)(_stateTimer / frameTime) - 1;

                _route.setFrame(frameNumber);

                _body = _route;
            }
            else if (_state == STATE_ROUTED)
            {
                _body = _routed;
            }
            else if (_state == STATE_ONEATTACK)
            {
                int maxFrames = _melee1.getMaxFrames();
                float deathFrameTime = _oneAttackTime / (float)maxFrames;
                int frameNumber = maxFrames - (int)(_stateTimer / deathFrameTime) - 1;

                _melee1.setFrame(frameNumber);

                _body = _melee1;
            }
            else if (_state == STATE_READY)
            {
                if (inPlayerFormation || _side != BattleScreen.SIDE_PLAYER)
                    _body = _idle;
                else
                {
                    _defend1.setFrame(0);
                    _body = _defend1;
                }
            }
            else if (_state == STATE_RETREAT)
            {
                _retreat.update(timeSpan);
                _body = _retreat;
            }
        }

        protected virtual void attackDone()
        {
        }

        protected virtual void shotDone()
        {

        }

        public virtual bool attack()
        {
            if (_state == STATE_READY)
            {
                _state = STATE_CHARGING;
                _stateTimer = _chargeTime;
                if(_position.X < _screen.getMapOffset().X + PikeAndShotGame.SCREENWIDTH)
                chargeSound.Play();
            }
            return false;
        }

        public virtual bool attackHigh()
        {
            return attack();
        }

        public virtual bool attackLow()
        {
            return attack();
        }

        public override int getWidth()
        {
            return _idle.getBoundingRect().Width;
        }

        public override int getHeight()
        {
            return _idle.getBoundingRect().Height;
        }

        public override void collide(ScreenObject collider, TimeSpan timeSpan)
        {
            if (collider == _killer || _state == STATE_DEAD || _state == STATE_DYING)
                return;

            if (collider is Shot && collider.getSide() != _side && !(collider is Pavise) && collider.getState() != STATE_DEAD && collider.getState() != Shot.STATE_GROUND)
            {
                if (this is Targeteer)
                {
                    if (collider is ArquebusierShot || (collider is CrossbowShot && collider.getSide() == BattleScreen.SIDE_PLAYER))
                        ((Targeteer)this).shieldBreak();
                    else
                        ((Targeteer)this).shieldBlock();
                }
                else if (this is CrossbowmanPavise)
                {
                    if (_side == BattleScreen.SIDE_PLAYER)
                        ((CrossbowmanPavise)this).shieldBlock();
                    else
                        hit();
                }
                else if (!(this is Leader) && (this._side == BattleScreen.SIDE_ENEMY || _screen.getPlayerFormation().getSoldiers().Contains(this)))
                    hit();

                ((Shot)collider).hit();

                _killer = collider;
            }
            else if ((collider is PikeTip && collider.getSide() != _side) && (_state != STATE_DEAD && _state != STATE_DYING && _state != STATE_ROUTE && _state != STATE_ROUTED))
            {
                if (((PikeTip)collider).getSoldierState() == Pikeman.STATE_LOWERED)
                {
                    if (!(this is Targeteer))
                    {
                        hit();
                        _killer = collider;
                        ((PikeTip)collider).getPikeman().recoil();
                    }
                    else if(this is Targeteer)
                    {
                        if (((Targeteer)this)._hasShield)
                        {
                            ((Targeteer)this).shield();
                            this.setReactionDest(collider.getCenter().X + 50f);//((collider.getSide() == BattleScreen.SIDE_ENEMY? 1 : -1 ) * collider.getWidth() * 0.5f + Soldier.WIDTH * 0.35f
                        }
                        else
                        {
                            hit();
                            _killer = collider;
                            ((PikeTip)collider).getPikeman().recoil();
                        }
                    }
                }
            }
            else if ((collider is WeaponSwing && collider.getSide() != _side) && (_state != STATE_DEAD && _state != STATE_DYING && _state != STATE_ROUTE && _state != STATE_ROUTED))
            {
                if (this is Targeteer)
                    ((Targeteer)this).shieldBreak();
                else
                {
                    hit();
                    _killer = collider;
                }

            }
            else if ((collider is LanceTip && collider.getSide() != _side) && (_state != STATE_DEAD && _state != STATE_DYING && _state != STATE_MELEE_WIN && _state != STATE_MELEE_LOSS && _state != STATE_ROUTE && _state != STATE_ROUTED))
            {
                if (this is Targeteer)
                    ((Targeteer)this).shieldBreak();
                else
                    hit();
                ((LanceTip)collider).getCavalry().recoil();
                _killer = collider;
            }
            else if (collider is Soldier)
            {
                bool colliderInFormation = false;
                bool thisInFormation = false;
                if (_screen.getPlayerFormation() != null && _screen.getPlayerFormation().getSoldiers() != null)
                {
                    colliderInFormation = _screen.getPlayerFormation().getSoldiers().Contains(collider);
                    thisInFormation = _screen.getPlayerFormation().getSoldiers().Contains(this);
                }
                if (_state != STATE_DEAD && _state != STATE_DYING && _state != STATE_MELEE_WIN && _state != STATE_MELEE_LOSS && (!(this is Targeteer) || _state != Targeteer.STATE_SHIELDBREAK) && (!(this is DismountedCavalry) || _state != DismountedCavalry.STATE_FALLING))
                {
                    if (this is CrossbowmanPavise && collider is CrossbowmanPavise
                        && (this._state == STATE_CHARGING || this._state == STATE_RELOADING || this._state == STATE_ATTACKING|| this._state == CrossbowmanPavise.STATE_PLACING || this._state == CrossbowmanPavise.STATE_RETRIEVING)
                        && (collider.getState() == STATE_CHARGING || collider.getState() == STATE_RELOADING || collider.getState() == STATE_ATTACKING || collider.getState() == CrossbowmanPavise.STATE_PLACING || collider.getState() == CrossbowmanPavise.STATE_RETRIEVING))
                    {
                        Vector2 changeVector = new Vector2(0f, (float)timeSpan.TotalMilliseconds * _speed);
                        ((CrossbowmanPavise)this).postRetrieveState = _state;
                        ((CrossbowmanPavise)this).paviseRetrieve();
                        if (this._position.Y < collider._position.Y)
                        {

                            this._position -= changeVector;
                            ((CrossbowmanPavise)this)._meleeDestination -= changeVector;
                            ((CrossbowmanPavise)this).chargePosition -= changeVector;
                        }
                        else
                        {
                            this._position += changeVector;
                            ((CrossbowmanPavise)this)._meleeDestination += changeVector;
                            ((CrossbowmanPavise)this).chargePosition += changeVector;
                        }
                    }
                    //push from collisions with other charging enemy soldiers
                    else if (_screen.getLooseSoldiers().Contains(this) && _screen.getLooseSoldiers().Contains(collider)
                        && _side == BattleScreen.SIDE_ENEMY && collider.getSide() == BattleScreen.SIDE_ENEMY
                        && (collider.getState() != STATE_DYING && collider.getState() != STATE_DEAD))
                    {
                        if (this._position.Y < collider._position.Y)
                        {
                            this._position -= new Vector2(0f, (float)timeSpan.TotalMilliseconds * _speed);
                        }
                        else
                        {
                            this._position += new Vector2(0f, (float)timeSpan.TotalMilliseconds * _speed);
                        }
                    }
                    else
                    {
                        if (collider.getSide() == BattleScreen.SIDE_NEUTRAL)
                        {
                            collisionPush(collider);
                        }
                        //fighting
                        else if (_side != collider.getSide() && (thisInFormation || colliderInFormation) && !(this is Leader || collider is Leader) && collider.getState() != STATE_DEAD && collider.getState() != STATE_DYING && collider.getState() != STATE_MELEE_WIN && collider.getState() != STATE_MELEE_LOSS && (!(collider is Targeteer) || collider.getState() != Targeteer.STATE_SHIELDBREAK) && (!(collider is DismountedCavalry) || collider.getState() != DismountedCavalry.STATE_FALLING))
                        {
                            bool rescueFight = (_side == BattleScreen.SIDE_PLAYER && !thisInFormation) ||
                                               (collider.getSide() == BattleScreen.SIDE_PLAYER && !colliderInFormation);
                            if (this is Dopple)
                            {
                                if (_state != STATE_ATTACKING)
                                    attack();
                                ((Soldier)collider).engage(false, _position, this, rescueFight);
                            }
                            else if (collider is Dopple)
                            {
                                if (collider.getState() != STATE_ATTACKING)
                                    ((Dopple)collider).attack();
                                engage(false, collider.getPosition(), (Soldier)collider, rescueFight);
                            }
                            else if (_side == BattleScreen.SIDE_ENEMY)
                            {
                                engage(true, collider.getPosition(), (Soldier)collider, rescueFight);
                                ((Soldier)collider).engage(false, _position, this, rescueFight);
                            }
                            else
                            {
                                engage(false, collider.getPosition(), (Soldier)collider, rescueFight);
                                ((Soldier)collider).engage(true, _position, this, rescueFight);
                            }
                        }
                        //rescue
                        else if (_side == BattleScreen.SIDE_PLAYER && collider.getSide() == BattleScreen.SIDE_PLAYER && (thisInFormation != colliderInFormation))
                        {
                            if (thisInFormation &&
                                collider.getState() != STATE_CHARGING && collider.getState() != STATE_DYING
                                && collider.getState() != STATE_DEAD
                                && collider.getState() != STATE_MELEE_LOSS && collider.getState() != STATE_MELEE_WIN
                                && collider.getState() != STATE_ATTACKING && collider.getState() != STATE_RELOADING)
                            {
                                ((Soldier)collider)._state = STATE_READY;
                                ((Soldier)collider)._reacting = false;
                                if (((Soldier)collider).myFormation != null && ((Soldier)collider).myFormation != _screen.getPlayerFormation())
                                    ((Soldier)collider).myFormation.removeSoldier(((Soldier)collider));
                                _screen.getPlayerFormation().addSoldier((Soldier)collider);
                                _screen.removeLooseSoldier((Soldier)collider);
                            }
                            else if (colliderInFormation &&
                                this.getState() != STATE_CHARGING && this.getState() != STATE_DYING
                                && this.getState() != STATE_DEAD
                                && this.getState() != STATE_MELEE_LOSS && this.getState() != STATE_MELEE_WIN
                                && this.getState() != STATE_ATTACKING && this.getState() != STATE_RELOADING)
                            {
                                _reacting = false;
                                _state = STATE_READY;
                                if (myFormation != null)
                                    myFormation.removeSoldier(this);
                                _screen.getPlayerFormation().addSoldier(this);
                                _screen.removeLooseSoldier(this);
                            }
                        }
                    }
                }
            }   
        }

        private void collisionPush(ScreenObject collider)
        {
            float coX = collider.getPosition().X;
            float coY = collider.getPosition().Y;
            float coW = (float)collider.getWidth();
            float coH = (float)collider.getHeight();

            if (coX < _position.X)
            {
                if(_dest.X > coX + coW)
                    _position.X = coX + coW;
            }
            else
            {
                if (_dest.X < coX)
                    _position.X = coX - (float)getWidth();
            }

            if (coY < _position.Y)
            {
                if (_dest.Y > coY + coH)
                    _position.Y = coY + coH;
            }
            else
            {
                if (_dest.Y < coY)
                    _position.Y = coY - (float)getHeight();
            }
        }

        protected virtual void hit()
        {
            if ((_state == STATE_MELEE_LOSS || _state == STATE_MELEE_WIN) && (_engager.getState() == STATE_MELEE_LOSS || _engager.getState() == STATE_MELEE_WIN))
                _engager.setState(_engager.preAttackState);

            _state = STATE_DYING;
            _stateTimer = _deathTime;
            _destination = _position;
            _screen.addLooseSoldier(this);
            hitSound.Play();
            playedFallSound = false;

            //I want guys that are running in as replacements to count as a loss
            if (((myFormation == _screen.getPlayerFormation() && this._type != TYPE_SWINGER) || (this._type != TYPE_SWINGER && _side == BattleScreen.SIDE_PLAYER)) && _screen is LevelScreen)
            {
                ((LevelScreen)_screen).loseCoin(getType());
            }
        }

        public virtual void route()
        {
            if (_state != STATE_DEAD && _state != STATE_DYING)
            {
                if ((_state == STATE_MELEE_LOSS || _state == STATE_MELEE_WIN) && (_engager.getState() == STATE_MELEE_LOSS || _engager.getState() == STATE_MELEE_WIN))
                    _engager.setState(_engager.preAttackState);
                _state = STATE_ROUTE;
                _stateTimer = _routeTime;

                _destination.Y = PikeAndShotGame.random.Next(PikeAndShotGame.SCREENHEIGHT);
                _destination.X = -1000f;
                _speed *= 0.3f;
            }
        }

        protected virtual void engage(bool win, Vector2 position, Soldier engager, bool rescueFight)
        {
            _stateChanged = true;
            _engager = engager;

            preAttackState = _state;
            if (this is Targeteer && _state == Targeteer.STATE_DEFEND)
            {
                preAttackState = STATE_READY;
            }

            if (rescueFight)
            {
                _longMelee = MELEE_REPETITIONS;
                if (_side == BattleScreen.SIDE_PLAYER)
                    _state = STATE_MELEE_LOSS;
                else
                    _state = STATE_MELEE_WIN;
            }
            else
            {
                _longMelee = 0;
                if (win)
                    _state = STATE_MELEE_WIN;
                else
                    _state = STATE_MELEE_LOSS;
            }

            _stateTimer = _meleeTime;

            _meleeDestination = (_position + position) / 2;

            if (_side == BattleScreen.SIDE_PLAYER)
                _meleeDestination.X -= 0.60f * Soldier.WIDTH;
            else
                _meleeDestination.X += 0.60f * Soldier.WIDTH;
        }

        internal void alterDestination(bool changeX, float amount)
        {
            if (_state != STATE_DYING && _state != STATE_DEAD)
            {
                if (changeX)
                    _destination.X += amount;
                else
                    _destination.Y += amount;
            }
        }

        Pavise paviseToHit;

        internal void oneAttack(Pavise pavise)
        {
            paviseToHit = pavise;
            preAttackState = _state;
            _state = STATE_ONEATTACK;
            _stateTimer = _oneAttackTime;
            _meleeDestination = _position;
        }

        public virtual float getOneAttackTime()
        {
            return _oneAttackTime * 7f / 8f;
        }

        public virtual void react(float p)
        {
            
        }

        internal ScreenObject getKiller()
        {
            return _killer;
        }

        internal virtual void setSpeed(float p)
        {
            _speed = p;
            _feet.setAnimationSpeed(_footSpeed / (_speed - 0.04f));
        }

        internal void cancelAttack()
        {
            _state = STATE_READY;
        }

        internal Vector2 getDestination()
        {
            return _destination;
        }

        internal bool isReady()
        {
            if (_state != STATE_MELEE_LOSS && _state != STATE_MELEE_WIN && _state != STATE_DYING && _state != STATE_DEAD)
                return true;
            else
                return false;
        }
    }

    public class Pikeman : Soldier
    {
        public const int STATE_RAISING = 100;
        public const int STATE_LOWERED = 101;
        public const int STATE_RECOILING = 102;
        public const int STATE_LOWER45 = 103;

        private Sprite _pikemanLowerLow;
        private Sprite _pikemanLowerHigh;
        private Sprite _pikemanRecoil;

        // lowered Sprite is a little more dynamic so we can assign different stlyes to it
        private Sprite _loweredSprite;

        private PikeTip _pikeTip;

        public Pikeman(BattleScreen screen, float x, float y, int side): base(screen, side, x, y)
        {
            _type = Soldier.TYPE_PIKE;
            _class = Soldier.CLASS_MERC_PIKEMAN;
            _attackTime = 450f;
            _reloadTime = 600f;
            guardTargetRange = 74;
            guardTargetDist = 100;
            breakRange = Soldier.WIDTH * 10f;

            if (PikeAndShotGame.random.Next(51) > 25)
            {
                _idle = new Sprite(PikeAndShotGame.PIKEMAN1_IDLE, new Rectangle(6, 68, 16, 28), 26, 106);
                _death = new Sprite(PikeAndShotGame.PIKEMAN1_DEATH, new Rectangle(40, 2, 16, 28), 72, 40);
                _pikemanLowerLow = new Sprite(PikeAndShotGame.PIKEMAN1_LOWER_LOW, new Rectangle(32, 67, 16, 28), 124, 106);
                _pikemanLowerHigh = new Sprite(PikeAndShotGame.PIKEMAN1_LOWER_HIGH, new Rectangle(32, 67, 16, 28), 124, 106);
                _pikemanRecoil = new Sprite(PikeAndShotGame.PIKEMAN1_RECOIL, new Rectangle(28, 4, 16, 28), 108, 36);
                _melee1 = new Sprite(PikeAndShotGame.PIKEMAN1_MELEE, new Rectangle(24, 30, 16, 28), 64, 68);
                _defend1 = new Sprite(PikeAndShotGame.PIKEMAN1_DEFEND, new Rectangle(20, 2, 16, 28), 52, 40);
            }
            else
            {
                _idle = new Sprite(PikeAndShotGame.PIKEMAN2_IDLE, new Rectangle(6, 68, 16, 28), 26, 106);
                _death = new Sprite(PikeAndShotGame.PIKEMAN2_DEATH, new Rectangle(40, 2, 16, 28), 72, 40);
                _pikemanLowerLow = new Sprite(PikeAndShotGame.PIKEMAN2_LOWER_LOW, new Rectangle(32, 67, 16, 28), 124, 106);
                _pikemanLowerHigh = new Sprite(PikeAndShotGame.PIKEMAN2_LOWER_HIGH, new Rectangle(32, 67, 16, 28), 124, 106);
                _pikemanRecoil = new Sprite(PikeAndShotGame.PIKEMAN2_RECOIL, new Rectangle(28, 4, 16, 28), 108, 36);
                _melee1 = new Sprite(PikeAndShotGame.PIKEMAN2_MELEE, new Rectangle(24, 30, 16, 28), 64, 68);
                _defend1 = new Sprite(PikeAndShotGame.PIKEMAN2_DEFEND, new Rectangle(20, 2, 16, 28), 52, 40);
            }
            _feet = new Sprite(PikeAndShotGame.PIKEMAN_FEET, new Rectangle(4, 2, 16, 12), 26, 16, true);
            _route = new Sprite(PikeAndShotGame.PIKEMAN_ROUTE, new Rectangle(6, 78, 16, 28), 50, 116);
            _routed = new Sprite(PikeAndShotGame.PIKEMAN_ROUTED, new Rectangle(6, 78, 16, 28), 48, 116, true);

            _feet.setAnimationSpeed(_footSpeed / 0.11f);
            _loweredSprite = _pikemanLowerLow;

            _pikeTip = new PikeTip(_screen, this);
        }

        public override void setSide(int side)
        {
            _side = side;
            _pikeTip.setSide(side);
        }

        public override void draw(SpriteBatch spritebatch)
        {
            base.draw(spritebatch);
            //spritebatch.Draw(PikeAndShotGame.getDotTexture(), _pikeTip.getPosition() - _screen.getMapOffset(), Color.White);
            //spritebatch.Draw(PikeAndShotGame.getDotTexture(), _pikeTip.getPosition() - _screen.getMapOffset() + new Vector2(_pikeTip.getWidth(), _pikeTip.getHeight()), Color.White);
        }

        public override void setState(int state)
        {
            base.setState(state);
            if (_screen.getPlayerFormation().getState() == Formation.STATE_PIKE)
            {
                _stateTimer = _attackTime / 2f;
                _state = STATE_ATTACKING;
            }
        }

        public override bool attack()
        {
            if (_state == STATE_READY)
            {
                _stateTimer = _attackTime + PikeAndShotGame.getRandPlusMinus(50);
            }
            else if (_state == STATE_RAISING)
            {
                _stateTimer = _attackTime - _stateTimer;
            }

            if(_state != STATE_LOWERED)
                _state = STATE_ATTACKING;

            return true;
        }

        protected override void engage(bool win, Vector2 position, Soldier engager, bool rescueFight)
        {
            base.engage(win, position, engager, rescueFight);
            _screen.removeScreenObject(_pikeTip);
        }

        protected override void hit()
        {
            base.hit();
            guardTarget = null;
            _speed = 0.15f;
            _screen.removeScreenObject(_pikeTip);
        }

        public override bool attackHigh()
        {
            if (_state == STATE_ATTACKING || _state == STATE_LOWERED && _loweredSprite == _pikemanLowerLow)
                _pikemanLowerHigh.setFrame(_pikemanLowerLow.getCurrFrame());
            _loweredSprite = _pikemanLowerHigh;
            return attack();
        }

        public override bool attackLow()
        {
            if (_state == STATE_ATTACKING || _state == STATE_LOWERED && _loweredSprite == _pikemanLowerHigh)
                _pikemanLowerLow.setFrame(_pikemanLowerHigh.getCurrFrame());
            _loweredSprite = _pikemanLowerLow;
            return attack();
        }

        public void lower45()
        {
            if (inPlayerFormation)
            {
                if (_state == STATE_READY)
                {
                    _stateTimer = _attackTime + PikeAndShotGame.getRandPlusMinus(50);
                    _state = STATE_LOWER45;
                }
                else if (_state == STATE_RAISING)
                {
                    _stateTimer = _attackTime - _stateTimer;
                    _state = STATE_LOWER45;
                }
                else if (_state == STATE_LOWERED)
                {
                    _screen.removeScreenObject(_pikeTip);
                    _state = STATE_LOWER45;
                }
            }
        }

        protected override void attackDone()
        {
            if (_state != STATE_LOWERED)
            {
                _state = STATE_LOWERED;
                _screen.addScreenObject(_pikeTip);
                PikeAndShotGame.PIKES_LOWER.Play();
            }
        }

        public void raise()
        {
            
            if (_state == STATE_LOWERED)
            {
                _stateTimer = _attackTime + PikeAndShotGame.getRandPlusMinus(50);
                _screen.removeScreenObject(_pikeTip);
                _state = STATE_RAISING;
            }
            else if (_state == STATE_RECOILING)
            {
                _screen.removeScreenObject(_pikeTip);
                _state = STATE_RAISING;
                _stateTimer = _attackTime;
            }
            else if (_state == STATE_ATTACKING || _state == STATE_LOWER45)
            {
                _stateTimer = _attackTime - _stateTimer;
                _state = STATE_RAISING;
            }
            
            guardTarget = null;
            _speed = 0.15f;

        }

        protected override void updateState(TimeSpan timeSpan)
        {
            base.updateState(timeSpan);
            _pikeTip.update(_position);

            if (!_stateChanged)
            {
                if (_state == STATE_RAISING)
                {
                    _stateTimer -= (float)timeSpan.TotalMilliseconds;

                    if (_stateTimer <= 0)
                    {
                        _stateTimer = 0f;
                        _state = STATE_READY;
                        PikeAndShotGame.PIKES_RAISE.Play();
                    }
                }
                else if (_state == STATE_LOWER45)
                {
                    if (_stateTimer > _attackTime / 2)
                    {
                        if ((float)timeSpan.TotalMilliseconds <= Math.Abs(_stateTimer - (_attackTime / 2)))
                            _stateTimer -= (float)timeSpan.TotalMilliseconds;
                        else
                            _stateTimer = _attackTime / 2;
                    }
                    else if (_stateTimer < _attackTime / 2)
                    {
                        if((float)timeSpan.TotalMilliseconds <= Math.Abs(_stateTimer - (_attackTime / 2)))
                            _stateTimer += (float)timeSpan.TotalMilliseconds;
                        else
                            _stateTimer = _attackTime / 2;
                    }

                }
                else if (_state == STATE_RECOILING)
                {
                    _stateTimer -= (float)timeSpan.TotalMilliseconds;

                    if (_stateTimer <= 0)
                    {
                        _state = STATE_CHECKING_EXIT;
                        _screen.checkNonFatalCollision(_pikeTip, timeSpan);
                        if (_state != STATE_RECOILING)
                        {
                            _stateTimer = 0f;

                            if (_screen.getPreviousKeyboardState().IsKeyDown(Keys.Z) || _screen.getPreviousGamePadState().IsButtonDown(Buttons.A))
                                _state = STATE_LOWERED;
                            else
                            {
                                _state = STATE_RAISING;
                                _stateTimer = _attackTime;
                            }
                        }
                    }
                }
            }

            if (_state != STATE_LOWERED && _state != STATE_RECOILING)
            {
                _screen.removeScreenObject(_pikeTip);
            }
        }

        protected override void winMelee()
        {
            base.winMelee();
            myFormation.reiteratePikeCommand(this);
        }

        protected override void updateAnimation(TimeSpan timeSpan)
        {
            base.updateAnimation(timeSpan);

            if (_state == STATE_ATTACKING || _state == STATE_LOWER45)
            {
                int maxFrames = _loweredSprite.getMaxFrames();
                float frameTime = _attackTime / (float)maxFrames;
                int frameNumber = maxFrames - (int)(_stateTimer / frameTime) - 1;

                _loweredSprite.setFrame(frameNumber);

                _body = _loweredSprite;
            }
            else if (_state == STATE_RAISING)
            {
                //the reverse of the previous case

                int maxFrames = _loweredSprite.getMaxFrames();
                float frameTime = _attackTime / (float)maxFrames;
                int frameNumber = (int)(_stateTimer / frameTime);

                _loweredSprite.setFrame(frameNumber == maxFrames? maxFrames - 1: frameNumber);

                _body = _loweredSprite;

            }
            else if (_state == STATE_LOWERED)
            {
                int maxFrames = _loweredSprite.getMaxFrames();
                float frameTime = _attackTime / (float)maxFrames;
                int frameNumber = maxFrames - (int)(_stateTimer / frameTime) - 1;

                _loweredSprite.setFrame(frameNumber);

                _body = _loweredSprite;
            }
            else if (_state == STATE_RECOILING)
            {
                int maxFrames = _pikemanRecoil.getMaxFrames() + 1;
                float frameTime = _reloadTime / (float)maxFrames;
                int frameNumber = maxFrames - (int)(_stateTimer / frameTime) - 1;

                if(frameNumber < 2)
                {
                    _pikemanRecoil.setFrame(frameNumber);
                    _body = _pikemanRecoil;
                }
                else
                {
                    _loweredSprite.setFrame(_loweredSprite.getMaxFrames() - 1);
                    _body = _loweredSprite;
                }
            }
        }

        internal void recoil()
        {
            _state = STATE_RECOILING;
            _stateTimer = _reloadTime;
            if (_screen is LevelScreen)
                ((LevelScreen)_screen).makePikeSound();
            //_stateChanged = true;
        }
    }


    public class Arquebusier : Soldier
    {
        // Unique statuses

        private Sprite _arquebusierReload;
        private Sprite _arquebusierShoot;
        private SoundEffectInstance shotHitSound;

        public Arquebusier(BattleScreen screen, float x, float y, int side): base(screen, side, x, y)
        {
            _type = Soldier.TYPE_SHOT;
            _class = Soldier.CLASS_MERC_ARQUEBUSIER;
            _attackTime = 150f;
            _reloadTime = 3000f;

            _feet = new Sprite(PikeAndShotGame.ARQUEBUSIER_FEET, new Rectangle(4, 2, 16, 12), 26, 16, true, true, 25);
            _feet.flashable = false;
            _idle = new Sprite(PikeAndShotGame.ARQUEBUSIER_IDLE, new Rectangle(6, 4, 16, 28), 44, 42);
            _death = new Sprite(PikeAndShotGame.ARQUEBUSIER_DEATH, new Rectangle(40, 2, 16, 28), 72, 40);
            _melee1 = new Sprite(PikeAndShotGame.ARQUEBUSIER_MELEE, new Rectangle(8, 12, 16, 28), 48, 48);
            _route = new Sprite(PikeAndShotGame.ARQUEBUSIER_ROUTE, new Rectangle(16, 16, 16, 28), 48, 52);
            _routed = new Sprite(PikeAndShotGame.ARQUEBUSIER_ROUTED, new Rectangle(16, 16, 16, 28), 52, 52, true);
            _arquebusierReload = new Sprite(PikeAndShotGame.ARQUEBUSIER_RELOAD, new Rectangle(8, 4, 16, 28), 44, 42, false, true, 25);
            _arquebusierShoot = new Sprite(PikeAndShotGame.ARQUEBUSIER_SHOOT, new Rectangle(6, 4, 16, 28), 44, 42);

            _feet.setAnimationSpeed(_footSpeed / 0.11f);
            shotHitSound = PikeAndShotGame.SHOT_HIT.CreateInstance();
            shotHitSound.Volume = 0.25f;
        }

        public override bool attack()
        {
            Vector2 formationPosition = _position - _destination;
            if (_state == STATE_READY && (Math.Abs(formationPosition.X) < _speed*100 /*&& Math.Abs(formationPosition.Y) < _speed*100*/))
            {
                _state = STATE_ATTACKING;
                _plusMinus = PikeAndShotGame.getRandPlusMinus(50);
                _stateTimer = _attackTime + _plusMinus;
                _meleeDestination = new Vector2(_position.X, _position.Y);
                return true;
            }

            return false;
        }

        
        protected override void updateState(TimeSpan timeSpan)
        {
            base.updateState(timeSpan);
            if (!_stateChanged)
            {
                if (_state == STATE_RELOADING)
                {
                    _stateTimer -= (float)timeSpan.TotalMilliseconds;

                    if (_stateTimer <= 0)
                    {
                        _stateTimer = 0f;
                        _feet.flashable = false;
                        _state = STATE_READY;
                    }
                }
                else if (_state == STATE_ATTACKING)
                {
                    if (_arquebusierShoot.getCurrFrame() == 2 && !_shotMade)
                        shotDone();
                }
            }
        }

        protected override void updateAnimation(TimeSpan timeSpan)
        {
            base.updateAnimation(timeSpan);
            if (_state == STATE_RELOADING)
            {
                if (_stateTimer <= _reloadTime / 12f)
                    _arquebusierReload.setFrame(1);
                else if (_stateTimer <= 2f * _reloadTime / 12f)
                    _arquebusierReload.setFrame(6);
                else if (_stateTimer <= 3f * _reloadTime / 12f)
                    _arquebusierReload.setFrame(7);
                else if (_stateTimer <= 4f * _reloadTime / 12f)
                    _arquebusierReload.setFrame(6);
                else if (_stateTimer <= 5f * _reloadTime / 12f)
                    _arquebusierReload.setFrame(7);
                else if (_stateTimer <= 6f * _reloadTime / 12f)
                    _arquebusierReload.setFrame(6);
                else if (_stateTimer <= 7f * _reloadTime / 12f)
                    _arquebusierReload.setFrame(5);
                else if (_stateTimer <= 8f * _reloadTime / 12f)
                    _arquebusierReload.setFrame(4);
                else if (_stateTimer <= 9f * _reloadTime / 12f)
                    _arquebusierReload.setFrame(3);
                else if (_stateTimer <= 10f * _reloadTime / 12f)
                    _arquebusierReload.setFrame(2);
                else if (_stateTimer <= 11f * _reloadTime / 12f)
                    _arquebusierReload.setFrame(1);
                else
                    _arquebusierReload.setFrame(0);

                _body = _arquebusierReload;


            }
            else if (_state == STATE_ATTACKING)
            {
                if (_stateTimer < _attackTime / 3f)
                    _arquebusierShoot.setFrame(1);
                else if (_stateTimer < 2f * _attackTime / 3f)
                    _arquebusierShoot.setFrame(2);
                else
                    _arquebusierShoot.setFrame(1);

                _body = _arquebusierShoot;
            }
        }
        
        protected override void attackDone()
        {
            _state = STATE_RELOADING;
            _stateTimer = _reloadTime - _plusMinus;
            _stateTime = _stateTimer;
            _feet.flashable = true;
            _shotMade = false;
        }

        protected override void shotDone()
        {
            _shotMade = true;
            if (_side == BattleScreen.SIDE_PLAYER)
                _screen.addShot(new ArquebusierShot(new Vector2(this._position.X + 34 + _randDestOffset.X, this._position.Y + 10 + _randDestOffset.Y), this._screen, _side, _arquebusierShoot.getBoundingRect().Height - 10, shotHitSound));
            else
                _screen.addShot(new ArquebusierShot(new Vector2(this._position.X - 18 + _randDestOffset.X, this._position.Y + 10 + _randDestOffset.Y), this._screen, _side, _arquebusierShoot.getBoundingRect().Height - 10, shotHitSound));

            ((LevelScreen)_screen).makeShotSound();
        }
    }

    public class Crossbowman : Soldier
    {
        protected Sprite _crossbowmanReload;
        protected Sprite _crossbowmanShoot;

        public Crossbowman(BattleScreen screen, float x, float y, int side)
            : base(screen, side, x, y)
        {
            _type = Soldier.TYPE_SHOT;
            _class = Soldier.CLASS_MERC_CROSSBOWMAN;
            _attackTime = 300f;
            _reloadTime = 2000f;

            _feet = new Sprite(PikeAndShotGame.PIKEMAN_FEET, new Rectangle(4, 2, 16, 12), 26, 16, true);
            _idle = new Sprite(PikeAndShotGame.CROSSBOWMAN_IDLE, new Rectangle(6, 4, 16, 28), 44, 42);
            _death = new Sprite(PikeAndShotGame.CROSSBOWMAN_DEATH, new Rectangle(40, 2, 16, 28), 72, 40);
            _melee1 = new Sprite(PikeAndShotGame.CROSSBOWMAN_MELEE2, new Rectangle(24, 30, 16, 28), 64, 68);
            _defend1 = new Sprite(PikeAndShotGame.CROSSBOWMAN_DEFEND2, new Rectangle(20, 2, 16, 28), 52, 40);
            _route = new Sprite(PikeAndShotGame.CROSSBOWMAN_ROUTE, new Rectangle(16, 16, 16, 28), 50, 52);
            _routed = new Sprite(PikeAndShotGame.CROSSBOWMAN_ROUTED, new Rectangle(16, 16, 16, 28), 50, 52, true);
            _crossbowmanReload = new Sprite(PikeAndShotGame.CROSSBOWMAN_RELOAD, new Rectangle(8, 4, 16, 28), 44, 42);
            _crossbowmanShoot = new Sprite(PikeAndShotGame.CROSSBOWMAN_SHOOT, new Rectangle(6, 4, 16, 28), 44, 42);

            _feet.setAnimationSpeed(_footSpeed / 0.11f);
        }

        protected override void updateAnimation(TimeSpan timeSpan)
        {
            base.updateAnimation(timeSpan);

            if (_state == STATE_RELOADING)
            {
                int maxFrames = _crossbowmanReload.getMaxFrames();
                float frameTime = _reloadTime / (float)maxFrames;
                int frameNumber = maxFrames - (int)(_stateTimer / frameTime) - 1;

                _crossbowmanReload.setFrame(frameNumber);

                _body = _crossbowmanReload;
            }
            else if (_state == STATE_ATTACKING)
            {
                int maxFrames = _crossbowmanShoot.getMaxFrames();
                float frameTime = _attackTime / (float)maxFrames;
                int frameNumber = maxFrames - (int)(_stateTimer / frameTime) - 1;

                _crossbowmanShoot.setFrame(frameNumber);

                _body = _crossbowmanShoot;
            }
        }

        public override bool attack()
        {
            Vector2 formationPosition = _position - _destination;
            if (_state == STATE_READY && (Math.Abs(formationPosition.X) < _speed * 100 /*&& Math.Abs(formationPosition.Y) < _speed * 100*/))
            {
                _state = STATE_ATTACKING;
                _plusMinus = PikeAndShotGame.getRandPlusMinus(50);
                _stateTimer = _attackTime + _plusMinus;
                return true;
            }

            return false;
        }


        protected override void updateState(TimeSpan timeSpan)
        {
            base.updateState(timeSpan);
            if (!_stateChanged)
            {
                if (_state == STATE_RELOADING)
                {
                    _stateTimer -= (float)timeSpan.TotalMilliseconds;

                    if (_stateTimer <= 0)
                    {
                        _stateTimer = 0f;
                        if (_side == BattleScreen.SIDE_ENEMY)
                            _state = STATE_READY;
                        else
                            _state = STATE_CHARGING;
                    }
                }
                else if (_state == STATE_ATTACKING)
                {
                    if (_crossbowmanShoot.getCurrFrame() == 2 && !_shotMade)
                        shotDone();
                }
            }
        }

        protected override void attackDone()
        {
            _state = STATE_RELOADING;
            _stateTimer = _reloadTime - _plusMinus;
            _crossbowmanShoot.setFrame(0);
            _shotMade = false;
        }

        protected override void shotDone()
        {
            _shotMade = true;
            if (_side == BattleScreen.SIDE_PLAYER)
                _screen.addShot(new CrossbowShot(new Vector2(this._position.X + 18 + _randDestOffset.X, this._position.Y + 10 + _randDestOffset.Y), this._screen, _side, _crossbowmanShoot.getBoundingRect().Height - 10));
            else
                _screen.addShot(new CrossbowShot(new Vector2(this._position.X - 14 + _randDestOffset.X, this._position.Y + 10 + _randDestOffset.Y), this._screen, _side, _crossbowmanShoot.getBoundingRect().Height - 10));
        }

        public override float getOneAttackTime()
        {
            return (_meleeTime * 2f / 3f) * 0.6f;
        }
    }

    public class CrossbowmanPavise : Crossbowman, DoppelSoldier
    {
        public const int STATE_PLACING = 100;
        public const int STATE_RETRIEVING = 101;
        public const float COVER_TIME = 1000f;
        private Sprite _crossbowmanPavisePlace;
        public bool hasPavise;
        private bool covering;
        private bool uncovering;
        private bool blocking;
        private float _placeTime;
        public int postRetrieveState;
        public int postPlaceState;
        public float preRetrieveStateTimer;
        public Pavise myPavise;
        private ArrayList possibleTargets;
        public Vector2 chargePosition;
        public Soldier bestTarget;
        private Sprite _shieldBreak;

        public CrossbowmanPavise(BattleScreen screen, float x, float y, int side)
            : base(screen, x, y, side)
        {
            _type = TYPE_SWINGER;
            _class = Soldier.CLASS_MERC_CROSSBOWMAN_PAVISE;
            hasPavise = true;
            _idle = new Sprite(PikeAndShotGame.CROSSBOWMAN_PAVISE, new Rectangle(10, 2, 16, 28), 44, 38);
            _crossbowmanReload = new Sprite(PikeAndShotGame.CROSSBOWMAN_RELOAD2, new Rectangle(8, 4, 16, 28), 44, 42);
            _crossbowmanPavisePlace = new Sprite(PikeAndShotGame.CROSSBOWMAN_PAVISE_PLACE, new Rectangle(12, 2, 16, 28), 46, 38);
            _shieldBreak = new Sprite(PikeAndShotGame.CROSSBOWMAN_SHIELDBREAK, new Rectangle(24, 4, 16, 28), 60, 46);
            if(side == BattleScreen.SIDE_ENEMY)
                _placeTime = 800f;
            else
                _placeTime = 200f;
            _chargeTime = 200f;
            _attackTime = 150f;
            possibleTargets = new ArrayList();
            chargePosition = new Vector2(100f, -13f);
        }

        internal override void setSpeed(float p)
        {
            _speed = p;
        }

        public void charge()
        {
            if (_state == STATE_READY)
            {
                _state = STATE_CHARGING;
                initCharge = false;
                _stateTimer = _chargeTime;
                chargePosition = new Vector2(100f, -13f);
                _meleeDestination = myFormation.getCenter() + chargePosition;
                if (_meleeDestination.X > _screen.getMapOffset().X + 1000)
                    Console.WriteLine("WTF");
                postRetrieveState = STATE_CHARGING;
                postPlaceState = STATE_CHARGING;
                preRetrieveStateTimer = _attackTime;
                _stateTimer = _attackTime;
                setSpeed(0.25f);
            }
        }

        public void chargeLogic(TimeSpan timeSpan)
        {
            _stateTimer -= (float)timeSpan.TotalMilliseconds;
            if (_stateTimer <= 0 || covering || uncovering)
            {
                if (!initCharge)
                {
                    initCharge = true;
                }
                else
                {
                    if (_meleeDestination != myFormation.getCenter() + chargePosition)
                    {
                        _meleeDestination = myFormation.getCenter() + chargePosition;
                        if(!hasPavise)
                            paviseRetrieve();
                        else if (!covering)
                            cover();
                        setSpeed(0.25f);
                    }
                    else if (_position == _meleeDestination && _state == STATE_CHARGING /*&& !covering && !uncovering*/)
                    {
                        attack();
                        setSpeed(0.25f);
                    }
                }
            }
        }

        internal void shieldBlock()
        {
        }

        private void uncover()
        {
            Console.WriteLine("UNCOVER");
            covering = false;
            _stateTimer = COVER_TIME;
            uncovering = true;
        }

        private void cover()
        {
            Console.WriteLine("COVER");
            covering = true;
            if(_state == STATE_CHARGING)
                _stateTimer = COVER_TIME;
        }

        public void chargeEnd()
        {
            if (_state != STATE_DYING && _state != STATE_MELEE_LOSS && _state != STATE_MELEE_WIN)
            {
                if (!hasPavise)
                {
                    postRetrieveState = STATE_READY;
                    paviseRetrieve();
                    covering = false;
                }
                else
                {
                    _state = STATE_READY;
                    uncover();
                }

                setSpeed(0.25f);
            }
        }

        protected override void shotDone()
        {
            _shotMade = true;
            if (_side == BattleScreen.SIDE_PLAYER)
            {
                if(bestTarget != null)
                    _screen.addShot(new AimedBolt(new Vector2(this._position.X + 18 + _randDestOffset.X, this._position.Y + 10 + _randDestOffset.Y), this._screen, _side, _crossbowmanShoot.getBoundingRect().Height - 10, bestTarget.getCenter(), bestTarget));
                else
                    _screen.addShot(new AimedBolt(new Vector2(this._position.X + 18 + _randDestOffset.X, this._position.Y + 10 + _randDestOffset.Y), this._screen, _side, _crossbowmanShoot.getBoundingRect().Height - 10, this.getCenter() + new Vector2(100f, 0), bestTarget));
            }
            else
                _screen.addShot(new CrossbowShot(new Vector2(this._position.X - 14 + _randDestOffset.X, this._position.Y + 10 + _randDestOffset.Y), this._screen, _side, _crossbowmanShoot.getBoundingRect().Height - 10));
        }

        protected override void attackDone()
        {
            _state = STATE_RELOADING;
            _stateTimer = _reloadTime - _plusMinus;
            _crossbowmanShoot.setFrame(0);
            _shotMade = false;
        }

        public override bool attack()
        {
            Vector2 formationPosition = _position - _destination;
            bestTarget = null;
            covering = false;
            uncovering = false;

            if (hasPavise)
            {
                if (_state != STATE_DYING)
                {
                    _state = STATE_PLACING;
                    _stateTimer = _placeTime + PikeAndShotGame.getRandPlusMinus(50);
                    return true;
                }
            }
            else
            {
                if (_state == STATE_READY || _state == STATE_CHARGING)
                {
                    possibleTargets = ((LevelScreen)_screen).dangerOnScreen();
                    Vector2 bestDistance = new Vector2(600, 600);
                    Vector2 distance;
                    double angle;
                    foreach (Soldier s in possibleTargets)
                    {
                        distance = s.getCenter() - this.getCenter();
                        angle = Math.Atan2(distance.Y, distance.X);
                        if ((bestTarget == null || bestDistance.Length() > distance.Length()) && angle < MathHelper.PiOver4 && angle > -MathHelper.PiOver4)
                        {
                            bestTarget = s;
                            bestDistance = distance;
                        }
                    }
                    if (bestTarget != null)
                    {
                        _state = STATE_ATTACKING;
                        _plusMinus = 0;
                        _stateTimer = _attackTime + _plusMinus;
                    }
                    return true;
                }
            }

            return false;
        }

        int debug= -1;
        protected override void updateState(TimeSpan timeSpan)
        {
            base.updateState(timeSpan);
            if (_state != debug || _state == STATE_RETRIEVING)
            {
                debug = _state;
                Console.WriteLine(_stateTimer + " o " + debug + " o " + _body.getCurrFrame());
            }

            if (!_stateChanged)
            {
                if (_state == STATE_RELOADING)
                {
                    if (_meleeDestination != myFormation.getCenter() + chargePosition)
                    {
                        _meleeDestination = myFormation.getCenter() + chargePosition;
                        postPlaceState = _state;
                        preRetrieveStateTimer = _stateTimer;
                        paviseRetrieve();
                        setSpeed(0.15f);
                    }
                }
                else if (_state == STATE_PLACING)
                {
                    _stateTimer -= (float)timeSpan.TotalMilliseconds;

                    if (_stateTimer <= 0)
                    {
                        if (_side == BattleScreen.SIDE_ENEMY)
                        {
                            _stateTimer = _reloadTime;
                            _state = STATE_RELOADING;
                        }
                        else
                        {
                            _state = postPlaceState;
                            _stateTimer = preRetrieveStateTimer;
                        }
                        _stateChanged = true;
                    }
                    if (this._crossbowmanPavisePlace.getCurrFrame() == 4 && hasPavise)
                        pavisePlaced();
                }
                else if (_state == STATE_RETRIEVING)
                {
                    _stateTimer -= (float)timeSpan.TotalMilliseconds;

                    if (_stateTimer <= 0)
                    {
                        if (_side == BattleScreen.SIDE_ENEMY)
                        {
                            _stateTimer = _reloadTime;
                            _state = STATE_RELOADING;
                        }
                        else
                        {
                            _stateTimer = 0;
                            _state = postRetrieveState;
                            _stateTimer = preRetrieveStateTimer;
                            chargePosition = new Vector2(100f, chargePosition.Y);
                            chargePosition += new Vector2(8, 0);
                            _meleeDestination = myFormation.getCenter() + chargePosition;
                            if (_meleeDestination.X > _screen.getMapOffset().X + 1000)
                                Console.WriteLine("WTF");
                        }
                        _stateChanged = true;
                    }
                }
                if (_state == STATE_CHARGING && (covering || uncovering))
                {
                    _stateTimer -= (float)timeSpan.TotalMilliseconds;
                    if (_stateTimer < 0)
                    {
                        _stateTimer = 0;
                        if (uncovering)
                            uncovering = false;
                    }
                }
            }
        }

        protected override void updateAnimation(TimeSpan timeSpan)
        {
            base.updateAnimation(timeSpan);

            if (_state == STATE_PLACING)
            {
                int maxFrames = _crossbowmanPavisePlace.getMaxFrames();
                float frameTime = _placeTime / (float)maxFrames;
                int frameNumber = maxFrames - (int)(_stateTimer / frameTime) - 1;

                _crossbowmanPavisePlace.setFrame(frameNumber);

                _body = _crossbowmanPavisePlace;
            }
            else if(_state == STATE_RETRIEVING)
            {
                int maxFrames = _crossbowmanPavisePlace.getMaxFrames();
                float frameTime = _placeTime / (float)maxFrames;
                int frameNumber = maxFrames - (int)(_stateTimer / frameTime) - 1;

                _crossbowmanPavisePlace.setFrame(maxFrames - frameNumber - 1);

                _body = _crossbowmanPavisePlace;
            }
            else if (_state == STATE_CHARGING)
            {
                _body = _idle;
                if (covering)
                {
                    int maxFrames = 5;
                    float frameTime = COVER_TIME / (float)maxFrames;
                    int frameNumber = maxFrames - (int)(_stateTimer / frameTime) - 1;

                    _shieldBreak.setFrame(frameNumber);

                    _body = _shieldBreak;
                }
                else if (uncovering)
                {
                    //the reverse of the previous case

                    int maxFrames = 5;
                    float frameTime = COVER_TIME / (float)maxFrames;
                    int frameNumber = (int)(_stateTimer / frameTime);

                    _shieldBreak.setFrame(frameNumber == maxFrames ? maxFrames - 1 : frameNumber);

                    _body = _shieldBreak;
                }
            }
        }

        private void pavisePlaced()
        {
            hasPavise = false;
            _idle = new Sprite(PikeAndShotGame.CROSSBOWMAN_IDLE, new Rectangle(6, 4, 16, 28), 44, 42);
            if (_side == BattleScreen.SIDE_PLAYER)
            {
                myPavise = new Pavise(new Vector2(this._position.X + 16f + _randDestOffset.X, this._position.Y + 12f + _randDestOffset.Y), this._screen, _side, 24f);
                _screen.addShot(myPavise);
                chargePosition -= new Vector2(8, 0);
                _meleeDestination = myFormation.getCenter() + chargePosition;
                if (_meleeDestination.X > _screen.getMapOffset().X + 1000)
                    Console.WriteLine("WTF");
            }
            else
                _screen.addShot(new Pavise(new Vector2(this._position.X + 4f + _randDestOffset.X, this._position.Y + 12f + _randDestOffset.Y), this._screen, _side, 24f));
        }

        public void paviseRetrieve()
        {
            if (_state == STATE_PLACING || _state == STATE_RETRIEVING)
            {
                _state = STATE_CHARGING;
            }
            if (_screen.getShots().Contains(myPavise) && _state != STATE_DYING && _state != STATE_DEAD)
            {
                _screen.removeShot(myPavise);
                hasPavise = true;
                _idle = new Sprite(PikeAndShotGame.CROSSBOWMAN_PAVISE, new Rectangle(10, 2, 16, 28), 44, 38);
                _state = STATE_RETRIEVING;
                _stateTimer = _placeTime;
            }
        }

        internal void setChargePosition(Vector2 vector2)
        {
            if (_state != STATE_MELEE_LOSS && _state != STATE_MELEE_WIN && _state != STATE_DYING)
            {
                _stateTimer = 0;
            }
            //chargePosition = vector2;
            //_meleeDestination = new Vector2(_meleeDestination.X, myFormation.getCenter().Y + chargePosition.Y);
        }
    }

    public class Dopple : Soldier, DoppelSoldier
    {
        private ArrayList possibleTargets;
        private Formation playerFormation;
        private Sprite _doppleReload1;
        private Sprite _doppleSwing1;

        private WeaponSwing _weaponSwing;
        private SoundEffectInstance slashSound;

        public static float CHARGE_PERIOD = 650f; 

        public float patternTimer;

        public Dopple(BattleScreen screen, float x, float y, int side)
            : base(screen, side, x, y)
        {
            _type = Soldier.TYPE_SWINGER;
            _class = Soldier.CLASS_MERC_DOPPLE;
            _attackTime = 300f;
            _reloadTime = 500f;
            guardTargetDist = (float)getWidth()*3f;
            guardTargetRange = 2000f;
            breakRange = Soldier.WIDTH * 1000f;
            _chargeTime = 200f;

            _feet = new Sprite(PikeAndShotGame.PIKEMAN_FEET, new Rectangle(4, 2, 16, 12), 26, 16, true);
            _idle = new Sprite(PikeAndShotGame.DOPPLE_IDLE, new Rectangle(20, 6, 16, 28), 44, 42);
            _death = new Sprite(PikeAndShotGame.DOPPLE_DEATH, new Rectangle(40, 2, 16, 28), 72, 40);
            _melee1 = new Sprite(PikeAndShotGame.DOPPLE_SWING1, new Rectangle(24, 20, 16, 28), 76, 68);
            _defend1 = new Sprite(PikeAndShotGame.DOPPLE_RELOAD1, new Rectangle(24, 16, 16, 28), 76, 64);
            _route = new Sprite(PikeAndShotGame.DOPPLE_ROUTE, new Rectangle(30, 12, 16, 28), 76, 48);
            _routed = new Sprite(PikeAndShotGame.DOPPLE_ROUTED, new Rectangle(30, 12, 16, 28), 76, 49, true);
            _doppleReload1 = new Sprite(PikeAndShotGame.DOPPLE_RELOAD1, new Rectangle(24, 16, 16, 28), 76, 64);
            _doppleSwing1 = new Sprite(PikeAndShotGame.DOPPLE_SWING1, new Rectangle(24, 20, 16, 28), 76, 68);

            _feet.setAnimationSpeed(_footSpeed / 0.11f);
            _weaponSwing = new WeaponSwing(_screen, this);
            _screen.removeScreenObject(_weaponSwing);
            playerFormation = _screen.getPlayerFormation();
            _speed = 0.15f;
            possibleTargets = new ArrayList(5);
            slashSound = PikeAndShotGame.SLASH.CreateInstance();
        }

        public override void setSide(int side)
        {
            _side = side;
            _weaponSwing.setSide(side);
        }

        protected override void engage(bool win, Vector2 position, Soldier engager, bool rescueFight)
        {
            base.engage(win, position, engager, rescueFight);
            _screen.removeScreenObject(_weaponSwing);
        }

        internal override void setSpeed(float p)
        {
            _speed = p;
        }

        protected override void hit()
        {
            base.hit();
            _screen.removeScreenObject(_weaponSwing);
        }

        public override void draw(SpriteBatch spritebatch)
        {
            base.draw(spritebatch);
            //spritebatch.Draw(PikeAndShotGame.getDotTexture(), _weaponSwing.getPosition(), Color.White);
            //spritebatch.Draw(PikeAndShotGame.getDotTexture(), _weaponSwing.getPosition() + new Vector2(_weaponSwing.getWidth(), _weaponSwing.getHeight()), Color.White);
        }

        public override bool attack()
        {
            if (_state == STATE_READY || _state == STATE_CHARGING || _state == STATE_CHARGED)
            {
                preAttackState = _state;
                _state = STATE_ATTACKING;
                _stateTimer = _attackTime;
                _meleeDestination = _position;
                return true;
            }

            return false;
        }

        protected override void updateState(TimeSpan timeSpan)
        {
            base.updateState(timeSpan);
            _weaponSwing.update(_position);
            if (!_stateChanged)
            {
                if (_state == STATE_RELOADING)
                {
                    _stateTimer -= (float)timeSpan.TotalMilliseconds;

                    if (_stateTimer <= 0)
                    {
                        _stateTimer = 0f;
                        _state = preAttackState;
                    }

                    patternTimer += (float)timeSpan.TotalMilliseconds / CHARGE_PERIOD;
                }
                else if (_state == STATE_ATTACKING)
                {
                    if (_doppleSwing1.getCurrFrame() >= 7 && !_shotMade)
                        shotDone();

                    patternTimer += (float)timeSpan.TotalMilliseconds / CHARGE_PERIOD;
                }
                else if (_state == STATE_CHARGED)
                {
                    _destination = _screen.getPlayerFormation().getCenter();
                }
            }
        }

        protected override void updateAnimation(TimeSpan timeSpan)
        {
            base.updateAnimation(timeSpan);

            if (_state == STATE_RELOADING)
            {
                int maxFrames = _doppleReload1.getMaxFrames();
                float frameTime = _reloadTime / (float)maxFrames;
                int frameNumber = maxFrames - (int)(_stateTimer / frameTime) - 1;

                _doppleReload1.setFrame(frameNumber);

                _body = _doppleReload1;
            }
            else if (_state == STATE_ATTACKING)
            {

                int maxFrames = _doppleSwing1.getMaxFrames();
                float frameTime = _attackTime / (float)maxFrames;
                int frameNumber = maxFrames - (int)(_stateTimer / frameTime) - 1;

                _doppleSwing1.setFrame(frameNumber);

                _body = _doppleSwing1;
            }
            else if (_state == STATE_CHARGING)
            {
                int maxFrames = _doppleReload1.getMaxFrames();
                float frameTime = _chargeTime / (float)maxFrames;
                int frameNumber = maxFrames - (int)(_stateTimer / frameTime)-1;

                if (initCharge)
                    _body = _idle;
                else
                {
                    _doppleReload1.setFrame(_doppleReload1.getMaxFrames() - frameNumber - 1);
                    _body = _doppleReload1;
                }
            }
        }

        protected override void attackDone()
        {
            _state = STATE_RELOADING;
            _stateTimer = _reloadTime;
            _doppleSwing1.setFrame(0);
            _shotMade = false;
            _reacting = false;
            _screen.removeScreenObject(_weaponSwing);
        }

        protected override void shotDone()
        {
            _shotMade = true;
            _meleeDestination = _position;
            if (_meleeDestination.X > _screen.getMapOffset().X + 1000)
                Console.WriteLine("WTF");
            _screen.addScreenObject(_weaponSwing);
            slashSound.Play();
        }

        public override void react(float p)
        {
            setReactionDest(p + (getWidth()*0.5f));
        }

        protected override bool checkReactions(TimeSpan timeSpan)
        {
            if (_screen.findSoldier(this, 1.25f, 0.5f))
            {
                if (!_reacting)
                {
                    attack();
                    _reacting = true;
                }
                else
                {
                    _reacting = false;
                }

                float absDeltaX = Math.Abs(_delta.X);
                float absDeltaY = Math.Abs(_delta.Y);
                _travel.X = (absDeltaX / (absDeltaX + absDeltaY)) * (float)timeSpan.TotalMilliseconds * _speed;
                _travel.Y = (absDeltaY / (absDeltaX + absDeltaY)) * (float)timeSpan.TotalMilliseconds * _speed;

                // check to see if walking
                if (_delta.Length() != 0)
                {
                    if (!_feet.getPlaying())
                    {
                        _feet.play();
                        _feet.nextFrame();
                    }
                    if (!_retreat.getPlaying())
                    {
                        _retreat.play();
                        _retreat.nextFrame();
                    }
                }
                else
                {
                    _feet.stop();
                    _feet.reset();
                    _retreat.stop();
                    _retreat.reset();
                }

                _feet.update(timeSpan);

                if (_feet.getCurrFrame() % 2 > 0)
                    _jostleOffset.Y = 1f;
                else
                    _jostleOffset.Y = 0f;

                // as long as we are not at destination, keep trying to get there, but don't overshoot
                if (_delta.X > 0)
                {
                    if (_delta.X - _travel.X >= 0)
                        _position.X += _travel.X;
                    else
                        _position.X = _dest.X;
                }
                else if (_delta.X < 0)
                {
                    if (_delta.X + _travel.X <= 0)
                        _position.X -= _travel.X;
                    else
                        _position.X = _dest.X;
                }

                if (_delta.Y > 0)
                {
                    if (_delta.Y - _travel.Y >= 0)
                        _position.Y += _travel.Y;
                    else
                        _position.Y = _dest.Y;
                }
                else if (_delta.Y < 0)
                {
                    if (_delta.Y + _travel.Y <= 0)
                        _position.Y -= _travel.Y;
                    else
                        _position.Y = _dest.Y;
                }

                return true;
            }
        
            return false;
        }

        public void charge()
        {
            if (_state == STATE_READY)
            {
                _state = STATE_CHARGING;
                initCharge = false;
                _stateTimer = _chargeTime;
            }
        }

        public void chargeEnd()
        {
            if (_state != STATE_DYING && _state != STATE_DEAD)
            {
                _state = STATE_READY;
                guardTarget = null;
                _stateTimer = 0;
            }
        }

        public void chargeLogic(TimeSpan timeSpan)
        {
            if (!initCharge)
            {
                _stateTimer -= (float)timeSpan.TotalMilliseconds;
                if (_stateTimer <= 0)
                {
                    _stateTimer = 0;
                    initCharge = true;
                    Soldier bestTarget = getBestTarget();
                    if (bestTarget != null)
                    {
                        guardTarget = bestTarget;
                        setSpeed(0.25f);
                    }
                    else
                    {
                        _stateTimer = _chargeTime;
                        setSpeed(0.25f);
                    }
                }
            }
            else
            {
                if (guardTarget == null)
                {
                    Soldier bestTarget = getBestTarget();
                    if (bestTarget != null)
                    {
                        guardTarget = bestTarget;
                        setSpeed(0.25f);
                    }
                    else
                    {
                        _stateTimer = _chargeTime;
                        setSpeed(0.25f);
                    }
                }
            }
            
        }

        private Soldier getBestTarget()
        {
            possibleTargets = ((LevelScreen)_screen).dangerOnScreen();

            Soldier bestTarget = null;
            Vector2 bestDistance = new Vector2(112, 112);
            Vector2 distance;
            foreach (Soldier s in possibleTargets)
            {
                distance = s.getCenter() - myFormation.getCenter();
                if (bestDistance.Length() > distance.Length())
                {
                    bestTarget = s;
                    bestDistance = distance;
                }
            }

            return bestTarget;
        }
    }

    public class Leader : Soldier
    {
        protected Sprite _motion;
        float _motionTime;
        float _idleTime;

        public Leader(BattleScreen screen, float x, float y, int side)
            : base(screen, side, x, y)
        {
            _type = Soldier.TYPE_SUPPORT;
            _class = Soldier.CLASS_LEADER_PUCELLE;

            _idle = new Sprite(PikeAndShotGame.PUCELLE_IDLE, new Rectangle(6, 68, 16, 28), 54, 106);
            _motion = new Sprite(PikeAndShotGame.PUCELLE_MOTION, new Rectangle(6, 68, 16, 28), 90, 110);

            _feet = new Sprite(PikeAndShotGame.PIKEMAN_FEET, new Rectangle(4, 2, 16, 12), 26, 16, true);

            _feet.setAnimationSpeed(_footSpeed / 0.11f);
            _retreat.setAnimationSpeed(_footSpeed / _speed);
            _stateTimer = _idleTime = 3000f;
            _motionTime = 2000f;
        }

        protected override void updateAnimation(TimeSpan timeSpan)
        {
            base.updateAnimation(timeSpan);
            if (_state == STATE_READY)
            {
                int maxFrames = _idle.getMaxFrames();
                float deathFrameTime = _idleTime / (float)maxFrames;
                int frameNumber = maxFrames - (int)(_stateTimer / deathFrameTime) - 1;

                _idle.setFrame(frameNumber);
                _body = _idle;
            }
            else if (_state == STATE_ATTACKING)
            {   
                int maxFrames = _motion.getMaxFrames();
                float deathFrameTime = _motionTime / (float)maxFrames;
                int frameNumber = maxFrames - (int)(_stateTimer / deathFrameTime) - 1;

                _motion.setFrame(frameNumber);
                _body = _motion;
            }
        }
        public override bool attack()
        {
            if (_state != STATE_DEAD &&
                _state != STATE_DYING &&
                _state != STATE_MELEE_WIN &&
                _state != STATE_MELEE_LOSS &&
                _state != STATE_ATTACKING)
            {
                _state = STATE_ATTACKING;
                _stateTimer = _motionTime;
            }
            return false;
        }

        protected override void updateState(TimeSpan timeSpan)
        {
            base.updateState(timeSpan);
            if (_state == STATE_READY)
            {
                _stateTimer -= (float)timeSpan.TotalMilliseconds;
                if (_stateTimer <= 0)
                {
                    _stateTimer = _idleTime;
                }
            }
            else if (_state == STATE_ATTACKING)
            {
                _stateTimer -= (float)timeSpan.TotalMilliseconds;
                if (_stateTimer <= 0)
                {
                    _state = STATE_READY;
                    _stateTimer = _idleTime;
                }
            }
            else if (_state == STATE_DEAD)
            {
                _screen.getPlayerFormation().retreat();
            }
        }
    }

    public class Slinger : Soldier
    {
        private Sprite _slingerReload;
        private Sprite _slingerShoot;
        private SoundEffectInstance slingSound;
        private SoundEffectInstance rockHitSound;

        private bool variant;
        private bool soundMade;

        public Slinger(BattleScreen screen, float x, float y, int side)
            : base(screen, side, x, y)
        {
            _type = Soldier.TYPE_SHOT;
            _class = Soldier.CLASS_GOBLIN_SLINGER;
            _attackTime = 600f;
            _reloadTime = 1000f;
            soundMade = false;

            if (PikeAndShotGame.random.Next(51) > 25)
            {
                _idle = new Sprite(PikeAndShotGame.SLINGER_IDLE, new Rectangle(6, 2, 16, 28), 34, 40);
                _slingerReload = new Sprite(PikeAndShotGame.SLINGER_RELOAD, new Rectangle(28, 12, 16, 28), 72, 50);
                _slingerShoot = new Sprite(PikeAndShotGame.SLINGER_SHOOT, new Rectangle(28, 12, 16, 28), 72, 50);
                _death = new Sprite(PikeAndShotGame.SLINGER_DEATH, new Rectangle(40, 2, 16, 28), 72, 40);
                _melee1 = new Sprite(PikeAndShotGame.SLINGER_MELEE, new Rectangle(24, 30, 16, 28), 64, 68);
                _defend1 = new Sprite(PikeAndShotGame.SLINGER_DEFEND, new Rectangle(20, 2, 16, 28), 52, 40);
                _retreat = new Sprite(PikeAndShotGame.SLINGER_RETREAT, new Rectangle(6, 2, 16, 28), 46, 40, true);
                variant = false;
            }
            else
            {
                _idle = new Sprite(PikeAndShotGame.SKIRMISHER_IDLE, new Rectangle(10, 2, 16, 28), 46, 42);
                _slingerReload = new Sprite(PikeAndShotGame.SKIRMISHER_RELOAD, new Rectangle(28, 12, 16, 28), 72, 50);
                _slingerShoot = new Sprite(PikeAndShotGame.SKIRMISHER_SHOOT, new Rectangle(28, 12, 16, 28), 72, 50);
                _death = new Sprite(PikeAndShotGame.SKIRMISHER_DEATH, new Rectangle(40, 2, 16, 28), 72, 40);
                _melee1 = new Sprite(PikeAndShotGame.SKIRMISHER_MELEE, new Rectangle(24, 30, 16, 28), 64, 68);
                _defend1 = new Sprite(PikeAndShotGame.SKIRMISHER_DEFEND, new Rectangle(20, 2, 16, 28), 52, 40);
                _retreat = new Sprite(PikeAndShotGame.SKIRMISHER_RETREAT, new Rectangle(6, 2, 16, 28), 46, 40, true);
                variant = true;
            }
            _feet = new Sprite(PikeAndShotGame.GOBLIN_FEET, new Rectangle(4, 2, 16, 12), 26, 16, true);
            _route = new Sprite(PikeAndShotGame.SLINGER_ROUTE, new Rectangle(12, 10, 16, 28), 40, 46);
            _routed = new Sprite(PikeAndShotGame.SLINGER_ROUTED, new Rectangle(12, 10, 16, 28), 40, 46, true);

            _feet.setAnimationSpeed(_footSpeed / 0.11f);
            _retreat.setAnimationSpeed(_footSpeed / _speed);
            slingSound = PikeAndShotGame.SLING_ROCK.CreateInstance();
            rockHitSound = PikeAndShotGame.ROCK_HIT.CreateInstance();
            rockHitSound.Volume = 0.6f;
            hitSound = PikeAndShotGame.OWW_ENEMY.CreateInstance();
            hitSound.Volume = 0.25f;
        }

        public override bool attack()
        {
            Vector2 formationPosition = _position - _destination;
            if (_state == STATE_READY && (Math.Abs(formationPosition.X) < _speed * 100 && _screen.getPlayerFormation().getPosition().X < _position.X))
            {
                _state = STATE_ATTACKING;
                _stateTimer = _attackTime + PikeAndShotGame.getRandPlusMinus(50);
                return true;
            }

            return false;
        }

        public override void react(float p)
        {

        }

        protected override void hit()
        {
            base.hit();
            if (getSide() == BattleScreen.SIDE_ENEMY)
                ((LevelScreen)_screen).collectCoin(this);
        }

        protected override bool checkReactions(TimeSpan timeSpan)
        {
            return false;
        }

        public void reload()
        {
            if (_screen.getPlayerFormation().getPosition().X < _position.X)
            {
                _state = STATE_RELOADING;
                _stateTimer = _reloadTime;
            }
        }

        protected override void updateState(TimeSpan timeSpan)
        {
            base.updateState(timeSpan);
            if (!_stateChanged)
            {
                if (_state == STATE_RELOADING)
                {
                    _stateTimer -= (float)timeSpan.TotalMilliseconds;

                    if (_stateTimer <= 0)
                    {
                        _stateTimer = 0f;
                        if(_side != BattleScreen.SIDE_PLAYER)
                            _state = STATE_READY;
                        else
                        {
                            _state = STATE_ATTACKING;
                            _stateTimer = _attackTime;
                        }
                    }
                }
                else if (_state == STATE_ATTACKING)
                {
                    if (_slingerShoot.getCurrFrame() == (variant ? 10 : 9) && !soundMade)
                    {
                        soundMade = true;
                        slingSound.Play();
                    }

                    if (_slingerShoot.getCurrFrame() == (variant ? 12 : 11) && !_shotMade)
                    {
                        shotDone();
                    }
                }
            }
        }

        protected override void updateAnimation(TimeSpan timeSpan)
        {
            base.updateAnimation(timeSpan);

            if (_state == STATE_RELOADING)
            {
                int maxFrames = _slingerReload.getMaxFrames();
                float frameTime = _reloadTime / (float)maxFrames;
                int frameNumber = maxFrames - (int)(_stateTimer / frameTime) - 1;

                _slingerReload.setFrame(frameNumber);

                _body = _slingerReload;
            }
            else if (_state == STATE_ATTACKING)
            {
                int maxFrames = _slingerShoot.getMaxFrames();
                float frameTime = _attackTime / (float)maxFrames;
                int frameNumber = maxFrames - (int)(_stateTimer / frameTime) - 1;

                _slingerShoot.setFrame(frameNumber);

                _body = _slingerShoot;
            }
        }

        protected override void attackDone()
        {
            if (_side == BattleScreen.SIDE_PLAYER)
            {
                _state = STATE_RELOADING;
                _stateTimer = _reloadTime;
            }
            else
            {
                _state = STATE_READY;
                _reacting = false;
            }
            _shotMade = false;
            soundMade = false;
        }

        protected override void shotDone()
        {
            _shotMade = true;
            if (_side == BattleScreen.SIDE_PLAYER)
            {
                if(variant)
                    _screen.addShot(new SkirmisherJavelin(new Vector2(this._position.X + 32 + _randDestOffset.X, this._position.Y + 4 + _randDestOffset.Y), this._screen, _side, _slingerShoot.getBoundingRect().Height, rockHitSound));
                else
                    _screen.addShot(new SlingerRock(new Vector2(this._position.X + 28 + _randDestOffset.X, this._position.Y + _randDestOffset.Y), this._screen, _side, _slingerShoot.getBoundingRect().Height, rockHitSound));
            }
            else
            {
                if (variant)
                    _screen.addShot(new SkirmisherJavelin(new Vector2(this._position.X - 18 + _randDestOffset.X, this._position.Y + 4 + _randDestOffset.Y), this._screen, _side, _slingerShoot.getBoundingRect().Height, rockHitSound));
                else
                    _screen.addShot(new SlingerRock(new Vector2(this._position.X - 12 + _randDestOffset.X, this._position.Y + _randDestOffset.Y), this._screen, _side, _slingerShoot.getBoundingRect().Height, rockHitSound));
            }
        }
    }

    public class Berzerker : Targeteer
    {
        public Berzerker(BattleScreen screen, float x, float y, int side)
            : base(screen, x, y, side)
        {
            _type = Soldier.TYPE_MELEE;
            _class = Soldier.CLASS_GOBLIN_BERZERKER;

            _feet = new Sprite(PikeAndShotGame.BROWN_FEET, new Rectangle(4, 2, 16, 12), 26, 16, true);
            if (PikeAndShotGame.random.Next(51) > 25)
            {
                _idle = new Sprite(PikeAndShotGame.BERZERKER2_IDLE, new Rectangle(10, 2, 16, 28), 46, 42);
                _death = new Sprite(PikeAndShotGame.BERZERKER2_DEATH, new Rectangle(40, 2, 16, 28), 72, 40);
                _melee1 = new Sprite(PikeAndShotGame.BERZERKER2_MELEE1, new Rectangle(24, 30, 16, 28), 64, 68);
                _defend1 = new Sprite(PikeAndShotGame.BERZERKER2_DEFEND1, new Rectangle(20, 2, 16, 28), 52, 40);
                _route = new Sprite(PikeAndShotGame.BERZERKER2_ROUTE, new Rectangle(12, 10, 16, 28), 40, 46);
                _routed = new Sprite(PikeAndShotGame.BERZERKER2_ROUTED, new Rectangle(12, 10, 16, 28), 40, 46, true);
                _noshieldIdle = new Sprite(PikeAndShotGame.BERZERKER2_IDLENOSHIELD, new Rectangle(10, 2, 16, 28), 46, 42);
                _shieldBreak = new Sprite(PikeAndShotGame.BERZERKER2_SHIELDBREAK, new Rectangle(24, 4, 16, 28), 60, 46);
                _shieldFall = new Sprite(PikeAndShotGame.BERZERKER2_FALL, new Rectangle(76, 42, 16, 18), 110, 86);
                _melee2 = new Sprite(PikeAndShotGame.BERZERKER2_MELEE2, new Rectangle(24, 30, 16, 28), 64, 68);
                _defend2 = new Sprite(PikeAndShotGame.BERZERKER2_DEFEND2, new Rectangle(20, 2, 16, 28), 52, 40);
                _chargeNoShield = new Sprite(PikeAndShotGame.BERZERKER2_CHARGENOSHIELD, new Rectangle(20, 20, 16, 28), 60, 56);
                _charge = new Sprite(PikeAndShotGame.BERZERKER2_CHARGE, new Rectangle(20, 20, 16, 28), 60, 56);
            }
            else
            {
                _idle = new Sprite(PikeAndShotGame.BERZERKER_IDLE, new Rectangle(10, 2, 16, 28), 46, 42);
                _death = new Sprite(PikeAndShotGame.BERZERKER_DEATH, new Rectangle(40, 2, 16, 28), 72, 40);
                _melee1 = new Sprite(PikeAndShotGame.BERZERKER_MELEE1, new Rectangle(24, 30, 16, 28), 64, 68);
                _defend1 = new Sprite(PikeAndShotGame.BERZERKER_DEFEND1, new Rectangle(20, 2, 16, 28), 52, 40);
                _route = new Sprite(PikeAndShotGame.BERZERKER_ROUTE, new Rectangle(12, 10, 16, 28), 40, 46);
                _routed = new Sprite(PikeAndShotGame.BERZERKER_ROUTED, new Rectangle(12, 10, 16, 28), 40, 46, true);
                _noshieldIdle = new Sprite(PikeAndShotGame.BERZERKER_IDLENOSHIELD, new Rectangle(10, 2, 16, 28), 46, 42);
                _shieldBreak = new Sprite(PikeAndShotGame.BERZERKER_SHIELDBREAK, new Rectangle(24, 4, 16, 28), 60, 46);
                _shieldFall = new Sprite(PikeAndShotGame.BERZERKER_FALL, new Rectangle(76, 42, 16, 18), 110, 86);
                _melee2 = new Sprite(PikeAndShotGame.BERZERKER_MELEE2, new Rectangle(24, 30, 16, 28), 64, 68);
                _defend2 = new Sprite(PikeAndShotGame.BERZERKER_DEFEND2, new Rectangle(20, 2, 16, 28), 52, 40);
                _chargeNoShield = new Sprite(PikeAndShotGame.BERZERKER_CHARGENOSHIELD, new Rectangle(20, 20, 16, 28), 60, 56);
                _charge = new Sprite(PikeAndShotGame.BERZERKER_CHARGE, new Rectangle(20, 20, 16, 28), 60, 56);
            }

            _body = _idle;

            _feet.setAnimationSpeed(_footSpeed / 0.11f);
            hitSound = PikeAndShotGame.OWW_ENEMY.CreateInstance();
            hitSound.Volume = 0.25f;
        }
    }

    public class Brigand : Soldier
    {
        public Brigand(BattleScreen screen, float x, float y, int side)
            : base(screen, side, x, y)
        {
            _type = Soldier.TYPE_MELEE;
            _class = Soldier.CLASS_GOBLIN_BRIGAND;

            _feet = new Sprite(PikeAndShotGame.BROWN_FEET, new Rectangle(4, 2, 16, 12), 26, 16, true);
            if (PikeAndShotGame.random.Next(51) > 25)
            {
                _idle = new Sprite(PikeAndShotGame.BRIGAND2_IDLE, new Rectangle(10, 2, 16, 28), 46, 42);
                _death = new Sprite(PikeAndShotGame.BRIGAND2_DEATH, new Rectangle(40, 2, 16, 28), 72, 40);
                _melee1 = new Sprite(PikeAndShotGame.BRIGAND2_MELEE1, new Rectangle(24, 30, 16, 28), 64, 68);
                _defend1 = new Sprite(PikeAndShotGame.BRIGAND2_DEFEND1, new Rectangle(20, 2, 16, 28), 52, 40);
                _route = new Sprite(PikeAndShotGame.BERZERKER2_ROUTE, new Rectangle(12, 10, 16, 28), 40, 46);
                _routed = new Sprite(PikeAndShotGame.BERZERKER2_ROUTED, new Rectangle(12, 10, 16, 28), 40, 46, true);
                _charge = new Sprite(PikeAndShotGame.BRIGAND2_CHARGE, new Rectangle(20, 20, 16, 28), 60, 56);
            }
            else
            {
                _idle = new Sprite(PikeAndShotGame.BRIGAND1_IDLE, new Rectangle(10, 2, 16, 28), 46, 42);
                _death = new Sprite(PikeAndShotGame.BRIGAND1_DEATH, new Rectangle(40, 2, 16, 28), 72, 40);
                _melee1 = new Sprite(PikeAndShotGame.BRIGAND1_MELEE1, new Rectangle(24, 30, 16, 28), 64, 68);
                _defend1 = new Sprite(PikeAndShotGame.BRIGAND1_DEFEND1, new Rectangle(20, 2, 16, 28), 52, 40);
                _route = new Sprite(PikeAndShotGame.BERZERKER_ROUTE, new Rectangle(12, 10, 16, 28), 40, 46);
                _routed = new Sprite(PikeAndShotGame.BERZERKER_ROUTED, new Rectangle(12, 10, 16, 28), 40, 46, true);
                _charge = new Sprite(PikeAndShotGame.BRIGAND1_CHARGE, new Rectangle(20, 20, 16, 28), 60, 56);
            }

            _body = _idle;

            _feet.setAnimationSpeed(_footSpeed / 0.11f);
            hitSound = PikeAndShotGame.OWW_ENEMY.CreateInstance();
            hitSound.Volume = 0.25f;
        }

        protected override bool checkReactions(TimeSpan timeSpan)
        {
            return false;
        }
    }

    public class Hauler : Soldier
    {
        public const int STATE_HAULING = 100;

        public Hauler(BattleScreen screen, float x, float y, int side)
            : base(screen, side, x, y)
        {
            _type = Soldier.TYPE_MELEE;
            _class = Soldier.CLASS_GOBLIN_BRIGAND;

            _feet = new Sprite(PikeAndShotGame.BROWN_FEET, new Rectangle(4, 2, 16, 12), 26, 16, true);

            _idle = new Sprite(PikeAndShotGame.HAULER_HAUL, new Rectangle(6, 6, 16, 28), 40, 42);
            _death = new Sprite(PikeAndShotGame.BRIGAND2_DEATH, new Rectangle(40, 2, 16, 28), 72, 40);
            _melee1 = new Sprite(PikeAndShotGame.BRIGAND2_MELEE1, new Rectangle(24, 30, 16, 28), 64, 68);
            _defend1 = new Sprite(PikeAndShotGame.BRIGAND2_DEFEND1, new Rectangle(20, 2, 16, 28), 52, 40);
            _route = new Sprite(PikeAndShotGame.BERZERKER2_ROUTE, new Rectangle(12, 10, 16, 28), 40, 46);
            _routed = new Sprite(PikeAndShotGame.BERZERKER2_ROUTED, new Rectangle(12, 10, 16, 28), 40, 46, true);
            _charge = new Sprite(PikeAndShotGame.BRIGAND2_CHARGE, new Rectangle(20, 20, 16, 28), 60, 56);

            _body = _idle;

            _feet.setAnimationSpeed(_footSpeed / 0.11f);
        }

        protected override bool checkReactions(TimeSpan timeSpan)
        {
            return false;
        }

        public override bool attack()
        {
            if (_state == STATE_READY)
            {
                _state = STATE_HAULING;
                _stateTimer = _attackTime;
                return true;
            }

            return false;
        }

        protected override void updateState(TimeSpan timeSpan)
        {
            base.updateState(timeSpan);
            if (!_stateChanged)
            {
                if (_state == STATE_HAULING)
                {
                    _stateTimer -= (float)timeSpan.TotalMilliseconds;

                    if (_stateTimer <= 0)
                    {
                        _stateTimer = 0f;
                        _state = STATE_READY;
                    }
                }
            }
        }

        protected override void updateAnimation(TimeSpan timeSpan)
        {
            base.updateAnimation(timeSpan);

            if (_state == STATE_HAULING)
            {
                int maxFrames = _idle.getMaxFrames();
                float frameTime = _attackTime / (float)maxFrames;
                int frameNumber = maxFrames - (int)(_stateTimer / frameTime) - 1;

                _idle.setFrame(frameNumber);

                _body = _idle;
            }
        }
    }

    public class Wolf : Soldier
    {
        public const int STATE_ATTACK = 100;
        protected Sprite _idleFeet;
        protected Sprite _runningFeet;
        protected Sprite _attackFeet;
        float _idleTime;

        public Wolf(BattleScreen screen, float x, float y, int side)
            : base(screen, side, x, y)
        {
            _type = Soldier.TYPE_MELEE;
            _class = Soldier.CLASS_GOBLIN_WOLF;
            _idleTime = 3000f;
            _attackTime = 1000f;

            _idleFeet = new Sprite(PikeAndShotGame.WOLF_IDLE, new Rectangle(16, 18, 14, 14), 48, 38, true);
            //_idleFeet = new Sprite(PikeAndShotGame.TEST, new Rectangle(0, 0, 512, 512), 1276, 368, true);
            _attackFeet = new Sprite(PikeAndShotGame.WOLF_SPOOKED, new Rectangle(18, 16, 14, 14), 48, 38, true);
            _feet = _runningFeet = new Sprite(PikeAndShotGame.WOLF_RUN, new Rectangle(16, 10, 14, 14), 44, 26, true);

            _body = _idle;
            _footSpeed = 8f;
            _feet.setAnimationSpeed(_footSpeed/0.11f);
        }

        protected override bool checkReactions(TimeSpan timeSpan)
        {
            return false;
        }

        public override bool attack()
        {
            if (_state == STATE_READY)
            {
                _state = STATE_ATTACK;
                _stateTimer = _attackTime;
                return true;
            }

            return false;
        }

        public override void draw(SpriteBatch spritebatch)
        {
            _drawingPosition = _position + _randDestOffset - _screen.getMapOffset();

            if (_state != STATE_DYING && _state != STATE_DEAD)
            {
                addDrawjob(new DrawJob(_feet, _drawingPosition + new Vector2(0, _idle.getBoundingRect().Height - 4), _state != STATE_RETREAT && _state != STATE_ROUTED ? _side : _side * -1, _drawingY));
            }

            //addDrawjob(new DrawJob(_body, _drawingPosition + _jostleOffset, _state != STATE_ROUTED ? _side : _side * -1, _drawingY));

            //spritebatch.Draw(PikeAndShotGame.getDotTexture(), _drawingPosition, Color.White);
        }

        protected override void updateState(TimeSpan timeSpan)
        {
            base.updateState(timeSpan);
            if (_delta.Length() != 0)
                _feet = _runningFeet;
            else if (_state == STATE_ATTACK)
                _feet = _attackFeet;
            else
                _feet = _idleFeet;

            if (!_stateChanged)
            {
                if (_state == STATE_ATTACK)
                {
                    _stateTimer -= (float)timeSpan.TotalMilliseconds;

                    if (_stateTimer <= 0)
                    {
                        _stateTimer = _idleTime;
                        _state = STATE_READY;
                    }
                }
                else if (_state == STATE_READY)
                {
                    _stateTimer -= (float)timeSpan.TotalMilliseconds;

                    if (_stateTimer <= 0)
                    {
                        _stateTimer = _idleTime;
                    }
                }
            }
        }

        protected override void updateAnimation(TimeSpan timeSpan)
        {
            base.updateAnimation(timeSpan);

            if (_state == STATE_ATTACK)
            {
                int maxFrames = _attackFeet.getMaxFrames();
                float frameTime = _attackTime / (float)maxFrames;
                int frameNumber = maxFrames - (int)(_stateTimer / frameTime) - 1;

                _attackFeet.setFrame(frameNumber);
            }
            else if (_state == STATE_READY && _delta.Length() == 0)
            {
                if (_stateTimer <= _attackTime)
                {
                    int maxFrames = _idleFeet.getMaxFrames();
                    float frameTime = _attackTime / (float)maxFrames;
                    int frameNumber = maxFrames - (int)(_stateTimer / frameTime) - 1;

                    _idleFeet.setFrame(frameNumber);
                }
                else
                    _idleFeet.setFrame(0);
            }
        }
    }

    public class DismountedCavalry : Targeteer
    {
        protected Sprite _fall;
        public const int STATE_FALLING = 200;
        protected float _fallTime;

        public DismountedCavalry(BattleScreen screen, float x, float y, int side)
            : base(screen, x, y, side)
        {
            _type = Soldier.TYPE_MELEE;
            _class = Soldier.CLASS_GOBLIN_BERZERKER;

            _feet = new Sprite(PikeAndShotGame.PIKEMAN_FEET, new Rectangle(4, 2, 16, 12), 26, 16, true);
            _idle = new Sprite(PikeAndShotGame.DISMOUNTED_CAVALRY_IDLE, new Rectangle(10, 2, 16, 28), 46, 42);
            _death = new Sprite(PikeAndShotGame.DISMOUNTED_CAVALRY_DEATH, new Rectangle(40, 2, 16, 28), 72, 40);
            _melee1 = new Sprite(PikeAndShotGame.SOLDIER_MELEE1, new Rectangle(24, 30, 16, 28), 64, 68);
            _defend1 = new Sprite(PikeAndShotGame.SOLDIER_DEFEND1, new Rectangle(20, 2, 16, 28), 52, 40);
            _route = new Sprite(PikeAndShotGame.BERZERKER_ROUTE, new Rectangle(12, 10, 16, 28), 40, 46);
            _routed = new Sprite(PikeAndShotGame.BERZERKER_ROUTED, new Rectangle(12, 10, 16, 28), 40, 46, true);
            _fall = new Sprite(PikeAndShotGame.CAVALRY_FALL, new Rectangle(76, 42, 16, 18), 110, 86);
            _noshieldIdle = new Sprite(PikeAndShotGame.DISMOUNTED_CAVALRY_IDLENOSHIELD, new Rectangle(10, 2, 16, 28), 46, 42);
            _shieldBreak = new Sprite(PikeAndShotGame.DISMOUNTED_CAVALRY_SHIELDBREAK, new Rectangle(24, 4, 16, 28), 60, 46);
            _shieldFall = new Sprite(PikeAndShotGame.DISMOUNTED_CAVALRY_FALL, new Rectangle(76, 42, 16, 18), 110, 86);
            _melee1 = new Sprite(PikeAndShotGame.DISMOUNTED_CAVALRY_MELEE1, new Rectangle(24, 30, 16, 28), 64, 68);
            _defend1 = new Sprite(PikeAndShotGame.DISMOUNTED_CAVALRY_DEFEND1, new Rectangle(20, 2, 16, 28), 52, 40);

            _body = _idle;

            _feet.setAnimationSpeed(_footSpeed / 0.11f);
            _fallTime = 1500f;
        }

        protected override void updateState(TimeSpan timeSpan)
        {
            base.updateState(timeSpan);
            if (!_stateChanged)
            {
                if (_state == STATE_FALLING)
                {
                    _stateTimer -= (float)timeSpan.TotalMilliseconds;

                    if (_stateTimer <= 0)
                    {
                        _stateTimer = 0f;
                        _state = STATE_READY;
                    }
                }
            }
        }

        protected override void updateAnimation(TimeSpan timeSpan)
        {
            base.updateAnimation(timeSpan);

            if (_state == STATE_FALLING)
            {
                int maxFrames = _fall.getMaxFrames();
                float frameTime = _fallTime / (float)maxFrames;
                int frameNumber = maxFrames - (int)(_stateTimer / frameTime) - 1;

                _fall.setFrame(frameNumber);

                _body = _fall;
            }
        }

        public void fall()
        {
            _state = STATE_FALLING;
            _meleeDestination = _position;
            _stateTimer = _fallTime;
        }
    }

    public class Targeteer : Soldier
    {
        public const int STATE_DEFEND = 100;
        public const int STATE_SHIELDBREAK = 101;
        public const int STATE_COVER = 102;
        public const int STATE_UNCOVER = 103;
        public const int STATE_SHIELDRECOIL = 104;

        protected float _defendTimer;
        protected float _shieldBreakTime;
        protected float _coverTime;
        protected float _shieldRecoilTime;
        protected ShieldBlock _shieldBlock;
        protected Sprite _shieldBreak;
        protected Sprite _noshieldIdle;
        protected Sprite _shieldFall;
        protected Sprite _melee2;
        public Sprite _defend2;
        public Sprite _chargeNoShield;
        public bool _hasShield;
        protected bool _dropShield;

        protected SoundEffectInstance shieldBreakSound;

        public Targeteer(BattleScreen screen, float x, float y, int side)
            : base(screen, side, x, y)
        {
            _type = Soldier.TYPE_MELEE;
            _class = Soldier.CLASS_MERC_SOLDIER;
            _defendTimer = 0f;
            _shieldBreakTime = 1500f;
            _coverTime = 375f;
            _shieldRecoilTime = 150f;

            _shieldBlock = new ShieldBlock(screen, this);
            _shieldBreak = new Sprite(PikeAndShotGame.SOLDIER_SHIELDBREAK, new Rectangle(24, 4, 16, 28), 60, 46);
            _noshieldIdle = new Sprite(PikeAndShotGame.SOLDIER_IDLENOSHIELD, new Rectangle(10, 2, 16, 28), 46, 42);
            _shieldFall = new Sprite(PikeAndShotGame.SOLDIER_FALL, new Rectangle(76, 42, 16, 18), 110, 86);
            _melee2 = new Sprite(PikeAndShotGame.SOLDIER_MELEE2, new Rectangle(24, 30, 16, 28), 64, 68);
            _defend2 = new Sprite(PikeAndShotGame.SOLDIER_DEFEND2, new Rectangle(20, 2, 16, 28), 52, 40);
            _chargeNoShield = new Sprite(PikeAndShotGame.SOLDIER_CHARGENOSHIELD, new Rectangle(20, 20, 16, 28), 60, 56);
            _hasShield = true;
            _dropShield = false;

            shieldBreakSound = PikeAndShotGame.SHIELD_BREAK.CreateInstance();
            shieldBreakSound.Volume = 0.5f;
        }

        public override bool attack()
        {
            if (_hasShield)
            {
                coverDone();
            }
            return base.attack();
        }

        public override void draw(SpriteBatch spritebatch)
        {
            base.draw(spritebatch);
            //spritebatch.Draw(PikeAndShotGame.getDotTexture(), _shieldBlock.getPosition() - _screen.getMapOffset(), Color.White);
            //spritebatch.Draw(PikeAndShotGame.getDotTexture(), _shieldBlock.getPosition() - _screen.getMapOffset() + new Vector2(_shieldBlock.getWidth(), _shieldBlock.getHeight()), Color.White);
        }

        protected override bool checkReactions(TimeSpan timeSpan)
        {
            if (_hasShield)
            {
                if (_screen.findPikeTip(this, 0.30f))
                {
                    if (!_reacting)
                    {
                        shield();
                    }
                    _defendTimer = _meleeTime * 2f / 3f;
                    _delta = _meleeDestination - _position;
                    _dest = _meleeDestination;
                    //_travel = (float)timeSpan.TotalMilliseconds * _speed;
                    float absDeltaX = Math.Abs(_delta.X);
                    float absDeltaY = Math.Abs(_delta.Y);
                    _travel.X = (absDeltaX / (absDeltaX + absDeltaY)) * (float)timeSpan.TotalMilliseconds * _speed;
                    _travel.Y = (absDeltaY / (absDeltaX + absDeltaY)) * (float)timeSpan.TotalMilliseconds * _speed;

                    // check to see if walking
                    if (_delta.Length() != 0)
                    {
                        if (!_feet.getPlaying())
                        {
                            _feet.play();
                            _feet.nextFrame();
                        }
                        if (!_retreat.getPlaying())
                        {
                            _retreat.play();
                            _retreat.nextFrame();
                        }
                    }
                    else
                    {
                        _feet.stop();
                        _feet.reset();
                        _retreat.stop();
                        _retreat.reset();
                    }

                    _feet.update(timeSpan);
                    _retreat.update(timeSpan);

                    if (_feet.getCurrFrame() % 2 > 0)
                        _jostleOffset.Y = 1f;
                    else
                        _jostleOffset.Y = 0f;

                    // as long as we are not at destination, keep trying to get there, but don't overshoot
                    if (_delta.X > 0)
                    {
                        if (_delta.X - _travel.X >= 0)
                            _position.X += _travel.X;
                        else
                            _position.X = _dest.X;
                    }
                    else if (_delta.X < 0)
                    {
                        if (_delta.X + _travel.X <= 0)
                            _position.X -= _travel.X;
                        else
                            _position.X = _dest.X;
                    }

                    if (_delta.Y > 0)
                    {
                        if (_delta.Y - _travel.Y >= 0)
                            _position.Y += _travel.Y;
                        else
                            _position.Y = _dest.Y;
                    }
                    else if (_delta.Y < 0)
                    {
                        if (_delta.Y + _travel.Y <= 0)
                            _position.Y -= _travel.Y;
                        else
                            _position.Y = _dest.Y;
                    }

                    return true;
                }
                else if (_screen.findShot(this, 30f) && _state != STATE_CHARGING)
                {
                    if (!_reacting)
                    {
                        cover();
                    }
                    _delta = _destination - _position;
                    _dest = _destination;
                    //_travel = (float)timeSpan.TotalMilliseconds * _speed;
                    float absDeltaX = Math.Abs(_delta.X);
                    float absDeltaY = Math.Abs(_delta.Y);
                    _travel.X = (absDeltaX / (absDeltaX + absDeltaY)) * (float)timeSpan.TotalMilliseconds * _speed;
                    _travel.Y = (absDeltaY / (absDeltaX + absDeltaY)) * (float)timeSpan.TotalMilliseconds * _speed;

                    // check to see if walking
                    if (_delta.Length() != 0)
                    {
                        if (!_feet.getPlaying())
                        {
                            _feet.play();
                            _feet.nextFrame();
                        }
                        if (!_retreat.getPlaying())
                        {
                            _retreat.play();
                            _retreat.nextFrame();
                        }
                    }
                    else
                    {
                        _feet.stop();
                        _feet.reset();
                        _retreat.stop();
                        _retreat.reset();
                    }

                    _feet.update(timeSpan);
                    _retreat.update(timeSpan);

                    if (_feet.getCurrFrame() % 2 > 0)
                        _jostleOffset.Y = 1f;
                    else
                        _jostleOffset.Y = 0f;

                    // as long as we are not at destination, keep trying to get there, but don't overshoot
                    if (_delta.X > 0)
                    {
                        if (_delta.X - _travel.X >= 0)
                            _position.X += _travel.X;
                        else
                            _position.X = _dest.X;
                    }
                    else if (_delta.X < 0)
                    {
                        if (_delta.X + _travel.X <= 0)
                            _position.X -= _travel.X;
                        else
                            _position.X = _dest.X;
                    }

                    if (_delta.Y > 0)
                    {
                        if (_delta.Y - _travel.Y >= 0)
                            _position.Y += _travel.Y;
                        else
                            _position.Y = _dest.Y;
                    }
                    else if (_delta.Y < 0)
                    {
                        if (_delta.Y + _travel.Y <= 0)
                            _position.Y -= _travel.Y;
                        else
                            _position.Y = _dest.Y;
                    }

                    return true;
                }
                else
                {
                    coverDone();
                }
            }
            else
            {
                //return base.checkReactions(timeSpan);
                return false;
            }

            return false;
        }

        public void shield()
        {
            if (!_hasShield)
                return;
            else if (_state != STATE_DEFEND)
            {
                if ((_state == STATE_MELEE_LOSS || _state == STATE_MELEE_WIN) && (_engager.getState() == STATE_MELEE_LOSS || _engager.getState() == STATE_MELEE_WIN))
                    _engager.setState(_engager.preAttackState);

                _defendTimer = _meleeTime * 2f / 3f;
                preAttackState = _state != STATE_MELEE_WIN && _state != STATE_MELEE_LOSS && _state != STATE_DEFEND ? _state : STATE_READY;
                _reacting = true;
                _state = STATE_DEFEND;
                _stateTimer = _meleeTime * 2f / 3f;
                _feet.setFrame(0);
                _screen.addScreenObject(_shieldBlock);
            }
        }

        public void cover()
        {
            if (_hasShield && _state != STATE_COVER)
            {
                preAttackState = _state != STATE_MELEE_WIN && _state != STATE_MELEE_LOSS && _state != STATE_DEFEND ? _state : STATE_READY;
                _reacting = true;
                _state = STATE_COVER;
                _stateTimer = _coverTime - _stateTimer;
            }
        }

        public void coverDone()
        {
            if (_hasShield && _state == STATE_COVER)
            {
                _reacting = false;
                _state = STATE_UNCOVER;
                _stateTimer = _coverTime - _stateTimer;
            }
        }

        public void shieldBreak()
        {
            if (_hasShield)
            {
                if ((_state == STATE_MELEE_LOSS || _state == STATE_MELEE_WIN) && (_engager.getState() == STATE_MELEE_LOSS || _engager.getState() == STATE_MELEE_WIN))
                    _engager.setState(_engager.preAttackState);

                _state = STATE_SHIELDBREAK;
                _stateTimer = _shieldBreakTime;
                new ScreenAnimation(_screen, _side, new Vector2(_position.X, _position.Y), new Sprite(PikeAndShotGame.SOLDIER_BROKENSHIELD1, new Rectangle(24, 4, 16, 28), 60, 46), (_shieldBreakTime / 8f) * 11f);
                _idle = _noshieldIdle;
                _hasShield = false;
                _reacting = false;
                _meleeDestination = _position;
                //_chargeTime = 2000f;
                shieldBreakSound.Play();
            }
            else
                hit();
        }

        protected override void engage(bool win, Vector2 position, Soldier engager, bool rescueFight)
        {
            base.engage(win, position, engager, rescueFight);
            _screen.removeScreenObject(_shieldBlock);
        }

        protected override void hit()
        {
            base.hit();
            _screen.removeScreenObject(_shieldBlock);
            if (getSide() == BattleScreen.SIDE_ENEMY)
                ((LevelScreen)_screen).collectCoin(this);
        }

        protected override void updateAnimation(TimeSpan timeSpan)
        {
            base.updateAnimation(timeSpan);

            if (_state == STATE_DEFEND)
            {
                bool isFirstHalf;
                int maxFrames = _defend1.getMaxFrames();
                float animationTime = _meleeTime * 2f / 6f;
                float frameTime = animationTime / maxFrames;
                float time;
                if (_stateTimer > animationTime)
                {
                    time = _stateTimer - animationTime;
                    isFirstHalf = true;
                }
                else
                {
                    time = _stateTimer;
                    isFirstHalf = false;
                }

                int frameNumber = maxFrames - (int)(time / frameTime) - 1;

                // we want to run this animation in reverse once we get halfway
                if (isFirstHalf)
                {
                    if (frameNumber <= (maxFrames / 2))
                    {
                        frameNumber = maxFrames / 2 + frameNumber - 1;
                        _defend1.setFrame(frameNumber);
                        _body = _defend1;
                    }
                    else
                    {
                        frameNumber -= maxFrames / 2;
                        _melee1.setFrame(frameNumber);
                        _body = _melee1;
                    }
                }
                else
                {
                    if (frameNumber <= (maxFrames / 2))
                    {
                        frameNumber = (maxFrames / 2) - frameNumber;
                        _melee1.setFrame(frameNumber);
                        _body = _melee1;   
                    }
                    else
                    {
                        frameNumber = maxFrames - (frameNumber / 2);
                        _defend1.setFrame(frameNumber);
                        _body = _defend1;
                    }
                }
            }
            else if (_state == STATE_SHIELDBREAK)
            {
                int maxFrames = /*9;*/_shieldFall.getMaxFrames();
                float frameTime = _shieldBreakTime / (float)maxFrames;
                int frameNumber = maxFrames - (int)(_stateTimer / frameTime) - 1;

                if (frameNumber == 3 && !playedFallSound)
                {
                    bodyFallSound.Play();
                    playedFallSound = true;                    
                }

                _shieldFall.setFrame(frameNumber);

                _body = _shieldFall;
            }
            else if (_state == STATE_COVER ||(_state == STATE_CHARGED && _hasShield))
            {
                int maxFrames = 5;
                float frameTime = _coverTime / (float)maxFrames;
                int frameNumber = maxFrames - (int)(_stateTimer / frameTime) - 1;

                _shieldBreak.setFrame(frameNumber);

                _body = _shieldBreak;
            }
            else if (_state == STATE_UNCOVER)
            {
                //the reverse of the previous case

                int maxFrames = 5;
                float frameTime = _coverTime / (float)maxFrames;
                int frameNumber = (int)(_stateTimer / frameTime);

                _shieldBreak.setFrame(frameNumber == maxFrames ? maxFrames - 1 : frameNumber);

                _body = _shieldBreak;
            }
            else if (_state == STATE_MELEE_WIN && !_hasShield)
            {
                int maxFrames = _melee2.getMaxFrames() + _defend2.getMaxFrames();
                float frameTime = _meleeTime * 2 / 3 / (float)maxFrames;
                float time = _stateTimer;

                // I want this animation to look like 2 defend and attack sequences
                // the animation is split into two separate defend and attack frames
                // therefore if we are in the greater half, just reduce this by half so we can figure out where we are 
                // based on the first half since this is just repeated
                if (_stateTimer > _meleeTime * 2 / 3)
                {
                    time -= (_meleeTime * 2 / 3);
                }

                int frameNumber = maxFrames - (int)(time / frameTime) - 1;

                // if we are at a frame higher than there are defend frames, we must be attacking
                if (frameNumber >= _defend2.getMaxFrames())
                {
                    frameNumber -= _defend2.getMaxFrames();
                    _melee2.setFrame(frameNumber);
                    _body = _melee2;
                }
                else
                {
                    _defend2.setFrame(frameNumber);
                    _body = _defend2;
                }
            }
            else if (_state == STATE_MELEE_LOSS && !_hasShield)
            {
                int maxFrames = _melee2.getMaxFrames() + _defend2.getMaxFrames();
                float frameTime = _meleeTime * 2 / 3 / (float)maxFrames;
                float time = _stateTimer;

                // I want this animation to look like 2 defend and attack sequences
                // the animation is split into two separate defend and attack frames
                // therefore if we are in the greater half, just reduce this by half so we can figure out where we are 
                // based on the first half since this is just repeated
                if (_stateTimer > _meleeTime * 2 / 3)
                {
                    time -= (_meleeTime * 2 / 3);
                }

                int frameNumber = maxFrames - (int)(time / frameTime) - 1;

                // if we are at a frame higher than there are defend frames, we must be attacking
                if (frameNumber >= _melee2.getMaxFrames())
                {
                    frameNumber -= _melee2.getMaxFrames();
                    _defend2.setFrame(frameNumber);
                    _body = _defend2;
                }
                else
                {
                    _melee2.setFrame(frameNumber);
                    _body = _melee2;
                }
            }
            else if (_state == STATE_ONEATTACK && !_hasShield)
            {
                int maxFrames = _melee2.getMaxFrames();
                float deathFrameTime = _oneAttackTime / (float)maxFrames;
                int frameNumber = maxFrames - (int)(_stateTimer / deathFrameTime) - 1;

                _melee2.setFrame(frameNumber);

                _body = _melee2;
            }
            else if (_state == STATE_SHIELDRECOIL)
            {
               _shieldBreak.setFrame(5);            
                _body = _shieldBreak;
            }
            
        }

        protected override void updateState(TimeSpan timeSpan)
        {
            base.updateState(timeSpan);
            _shieldBlock.update(_position);
            if (!_stateChanged)
            {
                if (_state == STATE_DEFEND)
                {
                    _stateTimer -= (float)timeSpan.TotalMilliseconds;
                    _defendTimer -= (float)timeSpan.TotalMilliseconds;

                    if (_stateTimer <= 0)
                    {
                        _stateTimer = _meleeTime * 2f / 3f;
                    }
                    if (_defendTimer <= 0)
                    {
                        shieldDone();
                    }
                }
                else if (_state == STATE_SHIELDBREAK)
                {
                    _stateTimer -= (float)timeSpan.TotalMilliseconds;
                    if (_stateTimer <= 0)
                    {
                        _stateTimer = 0;
                        _state = STATE_READY;
                    }
                }
                else if (_state == STATE_COVER || _state == STATE_CHARGED)
                {
                    _stateTimer -= (float)timeSpan.TotalMilliseconds;
                    if (_stateTimer <= 0)
                    {
                        _stateTimer = 0;
                    }
                }
                else if (_state == STATE_SHIELDRECOIL)
                {
                    _stateTimer -= (float)timeSpan.TotalMilliseconds;
                    if (_stateTimer <= 0)
                    {
                        _stateTimer = 0;
                        _state = STATE_COVER;
                    }
                }
                else if (_state == STATE_UNCOVER)
                {
                    _stateTimer -= (float)timeSpan.TotalMilliseconds;
                    if (_stateTimer <= 0)
                    {
                        _stateTimer = 0;
                        _stateToHave = preAttackState;
                    }
                }
            }
        }

        private void shieldDone()
        {
            if (!_screen.findPikeTip(this, 0.30f))
            {
                _stateToHave = preAttackState;
                _reacting = false;
                _stateChanged = true;
                _stateTimer = 0f;
                _defendTimer = 0f;
                _screen.removeScreenObject(_shieldBlock);
                DEBUGFOUNDPIKE = true;
            }
            else
            {
                //shield();
                _defendTimer = _meleeTime * 2f / 3f;
            }
        }

        public void resetDefendTimer()
        {
            _defendTimer = _meleeTime * 2f / 3f;
        }

        internal void shieldBlock()
        {
            _state = STATE_SHIELDRECOIL;
            _stateTimer = _shieldRecoilTime;
        }
    }

    public class Cavalry : Soldier
    {
        protected Sprite _stand;
        protected Sprite _run;
        protected Sprite _halt;
        protected Sprite _turn;
        protected Sprite _leftFoot;
        protected Sprite _rightFoot;
        protected Sprite _leftIdle;
        protected Sprite _rightIdle;
        protected Sprite _leftBody;
        protected Sprite _rightBody;
        protected Sprite _leftLower;
        protected Sprite _rightLower;
        protected Sprite _leftRecoil;
        protected Sprite _rightRecoil;

        protected float _haltTime;
        protected float _turnTime;
        protected float _stoppingDistance;
        protected float _turningDistance;
        protected bool _turned;
        protected bool _threwRider;
        protected Vector2 _turningOffset;

        protected LanceTip _lanceTip;

        public const int STATE_SLOWDOWN = 100;
        public const int STATE_TURNING = 101;
        public const int STATE_LOWERED = 102;
        public const int STATE_RAISING = 103;
        public const int STATE_RECOILING = 104;

        public Cavalry(BattleScreen screen, float x, float y, int side)
            : base(screen, side, x, y)
        {
            _type = Soldier.TYPE_CAVALRY;
            _class = Soldier.CLASS_MERC_CAVALRY;

            _stand = new Sprite(PikeAndShotGame.CAVALRY_HORSE_IDLE, new Rectangle(24, 24, 38, 24), 88, 60);
            _run = new Sprite(PikeAndShotGame.CAVALRY_HORSE_RUN, new Rectangle(24, 24, 38, 24), 88, 60, true);
            _halt = new Sprite(PikeAndShotGame.CAVALRY_HORSE_HALT, new Rectangle(24, 24, 38, 24), 88, 60);
            _turn = new Sprite(PikeAndShotGame.CAVALRY_HORSE_TURN, new Rectangle(80, 16, 38, 24), 152, 52);
            _death = new Sprite(PikeAndShotGame.CAVALRY_HORSE_DEATH, new Rectangle(46, 23, 38, 24), 100, 64);

            _leftFoot = new Sprite(PikeAndShotGame.CAVALRY_LEFTFOOT, new Rectangle(4, 2, 16, 12), 26, 16, true);
            _rightFoot = new Sprite(PikeAndShotGame.CAVALRY_RIGHTFOOT, new Rectangle(4, 2, 16, 12), 26, 16, true);
            _leftIdle = new Sprite(PikeAndShotGame.CAVALRY_LEFTIDLE, new Rectangle(10, 40, 16, 28), 36, 76);
            _rightIdle = new Sprite(PikeAndShotGame.CAVALRY_RIGHTIDLE, new Rectangle(10, 40, 16, 28), 36, 76);
            _leftLower = new Sprite(PikeAndShotGame.CAVALRY_LEFTLOWER, new Rectangle(10, 40, 16, 28), 90, 78);
            _rightLower = new Sprite(PikeAndShotGame.CAVALRY_RIGHTLOWER, new Rectangle(10, 40, 16, 28), 36, 78);
            _leftRecoil = new Sprite(PikeAndShotGame.CAVALRY_LEFTRECOIL, new Rectangle(12, 2, 16, 28), 86, 52);
            _rightRecoil = new Sprite(PikeAndShotGame.CAVALRY_RIGHTRECOIL, new Rectangle(12, 2, 16, 28), 86, 52);

            _leftBody = _leftIdle;
            _rightBody = _rightIdle;
            _feet = _stand;
            _speed = 0.22f;
            _turned = false;
            _attackTime = 450f;
            _reloadTime = 600f;
            _guardPositionOffset = new Vector2(0f, -14);

            _run.setAnimationSpeed(15f / _speed);
            _leftFoot.setAnimationSpeed(15f / _speed);
            _rightFoot.setAnimationSpeed(15f / _speed);
            _haltTime = 1000f;
            _turnTime = 750f;
            _stoppingDistance = 100f;
            _turningDistance = 40f;
            _turningOffset = Vector2.Zero;

            _lanceTip = new LanceTip(screen, this);
            _threwRider = false;
        }

        public override void update(TimeSpan timeSpan)
        {
            if (_state != STATE_MELEE_LOSS && _state != STATE_MELEE_WIN && _state != STATE_ONEATTACK)
            {
                _delta = _destination - _position;
                _dest = _destination;
            }
            else
            {
                _delta = _meleeDestination - _position;
                _dest = _meleeDestination;
            }

            //_travel = (float)timeSpan.TotalMilliseconds * _speed;
            float absDeltaX = Math.Abs(_delta.X);
            float absDeltaY = Math.Abs(_delta.Y);
            _travel.X = (absDeltaX / (absDeltaX + absDeltaY)) * (float)timeSpan.TotalMilliseconds * _speed;
            _travel.Y = (absDeltaY / (absDeltaX + absDeltaY)) * (float)timeSpan.TotalMilliseconds * _speed;

            // check to see if walking
            if (_delta.Length() != 0)
            {
                _feet = _run;
                if (!_feet.getPlaying())
                {
                    _feet.play();
                    _feet.nextFrame();
                    //_leftFoot.play();
                    //_rightFoot.play();
                    //_leftFoot.nextFrame();
                    //_rightFoot.nextFrame();
                }
                if (!_retreat.getPlaying())
                {
                    _retreat.play();
                    _retreat.nextFrame();
                }
            }
            else
            {
                _feet.stop();
                _feet.reset();
                _feet = _stand;
                /*
                _leftFoot.stop();
                _leftFoot.reset();
                _rightFoot.stop();
                _rightFoot.reset();
                _retreat.stop();
                _retreat.reset();
                */
            }
            base.update(timeSpan);
        }

        public override void draw(SpriteBatch spritebatch)
        {
            _drawingPosition = _position + _randDestOffset - _screen.getMapOffset();
            bool facing = _side == BattleScreen.SIDE_PLAYER ? _turned : !_turned;

            int frame = _feet.getCurrFrame();
            int jostle = frame == 3 || frame == 2 ? -1 : 0;

            if (_threwRider)
            {
                if (_state == STATE_DYING || _state == STATE_DEAD)
                    _screen.addDrawjob(new DrawJob(_feet, _drawingPosition, _side, _drawingY));
                else
                    _screen.addDrawjob(new DrawJob(_feet, _drawingPosition, !_turned ? _side : _side * -1, _drawingY));
            }
            else
            {
                if (_state == STATE_TURNING && _turn.getCurrFrame() > 4)
                {
                    if (_turn.getCurrFrame() == 5)
                    {
                        _screen.addDrawjob(new DrawJob(_rightFoot, _drawingPosition + _jostleOffset + new Vector2(!facing ? 14f : 8f, 0 + jostle) + new Vector2(!facing ? _turningOffset.X : -1 * _turningOffset.X, _turningOffset.Y), !_turned ? _side : _side * -1, _drawingY));
                        _screen.addDrawjob(new DrawJob(_rightBody, _drawingPosition + _jostleOffset + new Vector2(!facing ? 14f : 8f, -26f + jostle) + new Vector2(!facing ? _turningOffset.X : -1 * _turningOffset.X, _turningOffset.Y), !_turned ? _side : _side * -1, _drawingY));
                        _screen.addDrawjob(new DrawJob(_leftFoot, _drawingPosition + _jostleOffset + new Vector2(!facing ? 14f : 8f, 0 + jostle) + new Vector2(!facing ? _turningOffset.X : -1 * _turningOffset.X, _turningOffset.Y), !_turned ? _side : _side * -1, _drawingY));
                        _screen.addDrawjob(new DrawJob(_leftBody, _drawingPosition + _jostleOffset + new Vector2(!facing ? 14f : 8f, -26f + jostle) + new Vector2(!facing ? _turningOffset.X : -1 * _turningOffset.X, _turningOffset.Y), !_turned ? _side : _side * -1, _drawingY));
                    }
                    else
                    {
                        _screen.addDrawjob(new DrawJob(_rightFoot, _drawingPosition + _jostleOffset + new Vector2(!facing ? 14f : 8f, 0 + jostle) + new Vector2(!facing ? _turningOffset.X : -1 * _turningOffset.X, _turningOffset.Y), !_turned ? _side : _side * -1, _drawingY));
                        _screen.addDrawjob(new DrawJob(_rightBody, _drawingPosition + _jostleOffset + new Vector2(!facing ? 14f : 8f, -26f + jostle) + new Vector2(!facing ? _turningOffset.X : -1 * _turningOffset.X, _turningOffset.Y), !_turned ? _side : _side * -1, _drawingY));
                    }
                }
                else
                {
                    _screen.addDrawjob(new DrawJob(_leftFoot, _drawingPosition + _jostleOffset + new Vector2(!facing ? 14f : 8f, 0 + jostle) + new Vector2(!facing ? _turningOffset.X : -1 * _turningOffset.X, _turningOffset.Y), !_turned ? _side : _side * -1, _drawingY));
                    _screen.addDrawjob(new DrawJob(_leftBody, _drawingPosition + _jostleOffset + new Vector2(!facing ? 14f : 8f, -26f + jostle) + new Vector2(!facing ? _turningOffset.X : -1 * _turningOffset.X, _turningOffset.Y), !_turned ? _side : _side * -1, _drawingY));
                }

                if (_state == STATE_DYING || _state == STATE_DEAD)
                    _screen.addDrawjob(new DrawJob(_feet, _drawingPosition, _side, _drawingY));
                else
                    _screen.addDrawjob(new DrawJob(_feet, _drawingPosition, !_turned ? _side : _side * -1, _drawingY));

                if (_state == STATE_TURNING && _turn.getCurrFrame() > 4)
                {
                    if (_turn.getCurrFrame() > 5)
                    {
                        _screen.addDrawjob(new DrawJob(_leftFoot, _drawingPosition + _jostleOffset + new Vector2(!facing ? 14f : 8f, 0 + jostle) + new Vector2(!facing ? _turningOffset.X : -1 * _turningOffset.X, _turningOffset.Y), !_turned ? _side : _side * -1, _drawingY));
                        _screen.addDrawjob(new DrawJob(_leftBody, _drawingPosition + _jostleOffset + new Vector2(!facing ? 14f : 8f, -26f + jostle) + new Vector2(!facing ? _turningOffset.X : -1 * _turningOffset.X, _turningOffset.Y), !_turned ? _side : _side * -1, _drawingY));
                    }
                }
                else
                {
                    _screen.addDrawjob(new DrawJob(_rightFoot, _drawingPosition + _jostleOffset + new Vector2(!facing ? 14f : 8f, 0 + jostle) + new Vector2(!facing ? _turningOffset.X : -1 * _turningOffset.X, _turningOffset.Y), !_turned ? _side : _side * -1, _drawingY));
                    _screen.addDrawjob(new DrawJob(_rightBody, _drawingPosition + _jostleOffset + new Vector2(!facing ? 14f : 8f, -26f + jostle) + new Vector2(!facing ? _turningOffset.X : -1 * _turningOffset.X, _turningOffset.Y), !_turned ? _side : _side * -1, _drawingY));
                }
            }
        }

        protected override void updateState(TimeSpan timeSpan)
        {
            base.updateState(timeSpan);
            _lanceTip.update(_position);

            if (!_stateChanged)
            {
                if (_state == STATE_RAISING)
                {
                    _stateTimer -= (float)timeSpan.TotalMilliseconds;

                    if (_stateTimer <= 0)
                    {
                        _stateTimer = 0f;
                        _state = STATE_READY;
                    }
                }
                else if (_state == STATE_RECOILING)
                {
                    _stateTimer -= (float)timeSpan.TotalMilliseconds;

                    if (_stateTimer <= 0)
                    {
                        _state = STATE_CHECKING_EXIT;
                        _screen.checkNonFatalCollision(_lanceTip, timeSpan);
                        if (_state != STATE_RECOILING)
                        {
                            _stateTimer = 0f;

                            if (_screen.getPreviousKeyboardState().IsKeyDown(Keys.Z) || _screen.getPreviousGamePadState().IsButtonDown(Buttons.A))
                                _state = STATE_LOWERED;
                            else
                            {
                                _state = STATE_RAISING;
                                _stateTimer = _attackTime;
                            }
                        }
                    }
                }
                else if (_state == STATE_SLOWDOWN)
                {
                    _stateTimer -= (float)timeSpan.TotalMilliseconds;

                    if (_stateTimer <= 0)
                    {
                        haltDone();
                    }
                }
                else if (_state == STATE_TURNING)
                {
                    _stateTimer -= (float)timeSpan.TotalMilliseconds;

                    if (_stateTimer <= 0)
                    {
                        turnDone();
                    }
                }
            }

            if (_state != STATE_LOWERED && _state != STATE_RECOILING)
            {
                _screen.removeScreenObject(_lanceTip);
            }
        }

        protected override void updateAnimation(TimeSpan timeSpan)
        {
            base.updateAnimation(timeSpan);

            if (_state == STATE_READY)
            {
                _leftBody = _leftIdle;
                _rightBody = _rightIdle;
            }
            if (_state == STATE_SLOWDOWN)
            {
                int maxFrames = _halt.getMaxFrames();
                float frameTime = _haltTime / (float)maxFrames;
                int frameNumber = maxFrames - (int)(_stateTimer / frameTime) - 1;

                _halt.setFrame(frameNumber);

                _feet = _halt;
            }
            else if (_state == STATE_TURNING)
            {
                int maxFrames = _turn.getMaxFrames();
                float frameTime = _turnTime / (float)maxFrames;
                int frameNumber = maxFrames - (int)(_stateTimer / frameTime) - 1;

                switch (frameNumber)
                {
                    case 1:
                       _turningOffset.X= 2f;
                        break;
                    case 2:
                       _turningOffset.X= 0f;
                        break;
                    case 4:
                       _turningOffset.X= -4f;
                        break;
                    case 5:
                       _turningOffset.X= -6f;
                        break;
                    case 6:
                       _turningOffset.X= -8f;
                        break;
                    case 7:
                       _turningOffset.X= -10f;
                        break;
                    case 8:
                       _turningOffset.X= -14f;
                        break;
                    case 9:
                       _turningOffset.X= -24f;
                        break;
                    case 10:
                       _turningOffset.X= -28f;
                        break;
                    case 11:
                       _turningOffset.X= -42f;
                        break;
                }

                _turn.setFrame(frameNumber);

                _feet = _turn;
            }
            else if (_state == STATE_DYING)
            {
                int maxFrames = _death.getMaxFrames() * 3;
                float deathFrameTime = _deathTime / (float)maxFrames;
                int frameNumber = maxFrames - (int)(_stateTimer / deathFrameTime) - 1;

                if (frameNumber >= _death.getMaxFrames())
                {
                    frameNumber = _death.getMaxFrames() - 1;
                    if (!_threwRider)
                    {
                        _threwRider = true;
                        DismountedCavalry dc = new DismountedCavalry(_screen, _position.X + (_turningOffset.X * _side), _position.Y, _side);
                        dc.fall();
                        _screen.addLooseSoldierNext(dc);
                    }
                }

                _death.setFrame(frameNumber);
                _feet = _death;

                switch (frameNumber)
                {
                    case 0:
                        _turningOffset.X = -2f;
                        _turningOffset.Y = -4f;
                        break;
                    case 1:
                        _turningOffset.X = -8f;
                        _turningOffset.Y = -4f;
                        break;
                    case 2:
                        _turningOffset.X = -12f;
                        _turningOffset.Y = -4f;
                        break;
                    case 3:
                        _turningOffset.X = -16f;
                        _turningOffset.Y = -4f;
                        break;
                    case 4:
                        _turningOffset.X = -22f;
                        _turningOffset.Y = -2f;
                        break;
                    case 5:
                        _turningOffset.X = -28f;
                        _turningOffset.Y = 6f;
                        break;
                    case 6:
                        _turningOffset.X = -32f;
                        _turningOffset.Y = 10f;
                        break;
                    case 7:
                        _turningOffset.X = -38f;
                        _turningOffset.Y = 12f;
                        break;
                    case 8:
                        _turningOffset.X = -42f;
                        _turningOffset.Y = 14f;
                        break;
                    case 9:
                        _turningOffset.X = -48f;
                        _turningOffset.Y = 16f;
                        break;
                    case 10:
                        _turningOffset.X = -50f;
                        _turningOffset.Y = 20f;
                        break;
                    case 11:
                        _turningOffset.X = -50f;
                        _turningOffset.Y = 22f;
                        break;
                }
            }
            else if (_state == STATE_ATTACKING )
            {
                int maxFrames = _leftLower.getMaxFrames();
                float frameTime = _attackTime / (float)maxFrames;
                int frameNumber = maxFrames - (int)(_stateTimer / frameTime) - 1;

                _leftLower.setFrame(frameNumber);
                _rightLower.setFrame(frameNumber);

                _leftBody = _leftLower;
                _rightBody = _rightLower;
            }
            else if (_state == STATE_RAISING)
            {
                //the reverse of the previous case

                int maxFrames = _leftLower.getMaxFrames();
                float frameTime = _attackTime / (float)maxFrames;
                int frameNumber = (int)(_stateTimer / frameTime);

                _leftLower.setFrame(frameNumber == maxFrames ? maxFrames - 1 : frameNumber);
                _rightLower.setFrame(_leftLower.getCurrFrame());

                _leftBody = _leftLower;
                _rightBody = _rightLower;

            }
            else if (_state == STATE_LOWERED)
            {
                _leftBody = _leftLower;
                _rightBody = _rightLower;
            }
            else if (_state == STATE_RECOILING)
            {
                
                int maxFrames = _leftRecoil.getMaxFrames();
                float frameTime = _reloadTime / (float)maxFrames;
                int frameNumber = maxFrames - (int)(_stateTimer / frameTime) - 1;

                _leftRecoil.setFrame(frameNumber);
                _rightRecoil.setFrame(frameNumber);

                _leftBody = _leftRecoil;
                _rightBody = _rightRecoil;
                
            }
        }

        public void halt()
        {
            _state = STATE_SLOWDOWN;
            _stateTimer = _haltTime;
            _meleeDestination = new Vector2(_position.X + (_turned?_side*-1 :_side) * _stoppingDistance, _position.Y);
            _speed = 0.05f;
        }

        public void turn()
        {
            _state = STATE_TURNING;
            _stateTimer = _turnTime;
            _meleeDestination = _position;
        }

        public void turnDone()
        {
            bool facing = _side == BattleScreen.SIDE_PLAYER ? _turned : !_turned;
            _state = STATE_READY;
            _stateTimer = 0f;
            _position = new Vector2(_position.X + (!facing ? -1 : 1) * _turningDistance, _position.Y);
            _destination = _position;
            _speed = 0.22f;
            _feet = _stand;
            _turned = !_turned;
            _turningOffset = Vector2.Zero;
        }

        public void haltDone()
        {
            _state = STATE_READY;
            _stateTimer = 0f;
            _destination = _position;
            _speed = 0.22f;
            _feet = _stand;
        }

        public override bool attack()
        {
            if (_state == STATE_READY)
            {
                _stateTimer = _attackTime + PikeAndShotGame.getRandPlusMinus(50);
            }
            else if (_state == STATE_RAISING)
            {
                _stateTimer = _attackTime - _stateTimer;
            }

            if (_state != STATE_LOWERED)
                _state = STATE_ATTACKING;
           
            return true;
        }

        public override void setSide(int side)
        {
            _side = side;
            _lanceTip.setSide(side);
        }

        protected override void engage(bool win, Vector2 position, Soldier engager, bool rescueFight)
        {
            base.engage(win, position, engager, rescueFight);
            _screen.removeScreenObject(_lanceTip);
        }

        protected override void hit()
        {
            base.hit();
            _screen.removeScreenObject(_lanceTip);
        }

        protected override void attackDone()
        {
            if (_state != STATE_LOWERED)
            {
                _state = STATE_LOWERED;
                _screen.addScreenObject(_lanceTip);
            }
        }

        public void raise()
        {
            if (_state == STATE_LOWERED)
            {
                _stateTimer = _attackTime + PikeAndShotGame.getRandPlusMinus(50);
                _screen.removeScreenObject(_lanceTip);
                _state = STATE_RAISING;
            }
            else if (_state == STATE_RECOILING)
            {
                _screen.removeScreenObject(_lanceTip);
                _state = STATE_RAISING;
                _stateTimer = _attackTime;
            }
            else if (_state == STATE_ATTACKING)
            {
                _stateTimer = _attackTime - _stateTimer;
                _state = STATE_RAISING;
            }

        }

        internal void recoil()
        {
            _state = STATE_RECOILING;
            _stateTimer = _reloadTime;
            //_stateChanged = true;
        }
    }
}

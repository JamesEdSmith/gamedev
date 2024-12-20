﻿using System;
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
        public const int CLASS_GOBLIN_COLMILLOS = 15;
        public const int CLASS_GOBLIN_CANNON = 16;

        // leader classes
        public const int CLASS_LEADER_PUCELLE = -1;

        //npcs
        public const int CLASS_NPC_FLEER = 20;

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
        public const int STATE_SPAWN = 10;

        public const int WIDTH = 28;
        public const int HEIGHT = 30;

        public const int MELEE_REPETITIONS = 3;

        public Vector2 _destination;
        public Vector2 randDestOffset;
        public Vector2 _drawingPosition;
        public Vector2 _dest;
        public Vector2 _meleeDestination;

        protected int _lastAction;
        protected Vector2 _delta;
        public float _speed;         //per second
        public float _footSpeed;
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
        protected float _spawnTime;
        protected float _plusMinus;
        protected bool _stateChanged;   //keeps track of if the state has changed already this update, so we don't do checks too much
        protected bool _shotMade;
        public bool _reacting;
        protected int _longMelee;
        protected ScreenObject _killer;
        private float dropTimer = 0;
        private const int dropTime = 500;

        public int preAttackState;
        
        public Sprite _feet;
        protected Sprite _body;

        protected Sprite _idle;
        protected Sprite _death;
        protected Sprite _melee1;
        protected Sprite _defend1;
        protected Sprite _route;
        protected Sprite _routed;
        protected Sprite _retreat;
        protected Sprite _charge;
        protected Sprite _spawn;
        protected Sprite _wading;

        public bool initCharge;
        public bool inPlayerFormation;

        public ScreenObject guardTarget;
        public float guardTargetRange;
        public float guardTargetDist;
        public Formation chargeTarget;

        public Soldier _engager;
        public Formation myFormation;
        public bool givesRescueReward;

        public bool DEBUGFOUNDPIKE;
        public float breakRange = 0;

        protected SoundEffectInstance bodyFallSound;
        protected SoundEffectInstance hitSound;
        protected SoundEffectInstance chargeSound;
        protected bool playedFallSound;
        protected bool playedMeleeSound;

        public ArrayList terrainColliders;

        public Soldier(BattleScreen screen, int side, float x, float y): base(screen, side)
        {
            _position = new Vector2(x, y);
            _destination = new Vector2(x, y);
            _dest = new Vector2(0,0);
            _meleeDestination = new Vector2(0, 0);
            _drawingPosition = Vector2.Zero;
            randDestOffset = new Vector2(PikeAndShotGame.getRandPlusMinus(3), 0);
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
            _routedTime = 500f;
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
            _spawnTime = 3000f;
            guardTarget = null;
            guardTargetRange = 0f;
            guardTargetDist = 0f;
            _engager = null;
            _chargeTime = 800f;//400f;
            givesRescueReward = false;
            terrainColliders = new ArrayList(4);

            _wading = new Sprite(PikeAndShotGame.WADING, new Rectangle(10, 6, 16, 12), 36, 26, true);
            _wading.setAnimationSpeed(150f);
            _feet = new Sprite(PikeAndShotGame.SOLDIER_FEET, new Rectangle(4, 2, 16, 12), 26, 16, true);
            _idle = new Sprite(PikeAndShotGame.SOLDIER_IDLE, new Rectangle(10, 2, 16, 28), 46, 42);
            _death = new Sprite(PikeAndShotGame.SOLDIER_DEATH, new Rectangle(40, 2, 16, 28), 72, 40);
            _melee1 = new Sprite(PikeAndShotGame.SOLDIER_MELEE1, new Rectangle(14, 10, 16, 28), 62, 46);
            _defend1 = new Sprite(PikeAndShotGame.SOLDIER_DEFEND1, new Rectangle(16, 10, 16, 28), 54, 46);
            
            _route = new Sprite(PikeAndShotGame.SOLDIER_ROUTE, new Rectangle(26, 16, 16, 28), 70, 52);
            _routed = new Sprite(PikeAndShotGame.SOLDIER_ROUTED, new Rectangle(18, 16, 16, 28), 50, 52, true);
            _retreat = new Sprite(PikeAndShotGame.SLINGER_RETREAT, new Rectangle(6, 2, 16, 28), 46, 40, true);
            _charge = new Sprite(PikeAndShotGame.SOLDIER_CHARGE, new Rectangle(20, 20, 16, 28), 60, 56);
            _spawn = new Sprite(PikeAndShotGame.BRIGAND1_SPAWN, new Rectangle(68, 24, 16, 28), 120, 68);
            //_spawn = new Sprite(PikeAndShotGame.BRIGAND2_SPAWN, new Rectangle(26, 20, 16, 28), 72, 72);

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
                case Soldier.CLASS_GOBLIN_WOLF:
                    _newEnemyFormation.addSoldier(new Wolf(screen, x, y, BattleScreen.SIDE_ENEMY));
                    break;
                case Soldier.CLASS_NPC_FLEER:
                    _newEnemyFormation.addSoldier(new NPCFleer(screen, x, y, BattleScreen.SIDE_ENEMY));
                    break;
                case Soldier.CLASS_GOBLIN_HAULER:
                    _newEnemyFormation.addSoldier(new Hauler(screen, x, y, BattleScreen.SIDE_ENEMY));
                    break;
                case Soldier.CLASS_GOBLIN_CANNON:
                    _newEnemyFormation.addSoldier(new Slinger(screen, x, y, BattleScreen.SIDE_ENEMY, true));
                    break;

            }
        }

        public void changeRandOffset()
        {
            randDestOffset.X = PikeAndShotGame.getRandPlusMinus(4);
            randDestOffset.Y = PikeAndShotGame.getRandPlusMinus(4);
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
            _drawingPosition = _position + randDestOffset - _screen.getMapOffset();
            _charge.setFrame(3);
            _feet.setFrame(1);
            _screen.addDrawjob(new DrawJob(_feet, _drawingPosition + new Vector2(0, _idle.getBoundingRect().Height - 4), _state != STATE_RETREAT && _state != STATE_ROUTED ? _side : _side * -1, _drawingY));
            _screen.addDrawjob(new DrawJob(_charge, _drawingPosition + _jostleOffset, _state != STATE_ROUTED ? _side : _side * -1, _drawingY));
        }

        public virtual void draw(SpriteBatch spritebatch)
        {
            _drawingPosition = _position + randDestOffset - _screen.getMapOffset();
            if (this is Slinger && ((Slinger)this).cannon)
            {

            }
            else if (_state != STATE_DYING && _state != STATE_DEAD && _state != STATE_SPAWN && ((_state != STATE_MELEE_WIN && _state != STATE_MELEE_LOSS) || !(this is Arquebusier)))
            {
                if (this is Targeteer)
                {
                    if (_state != Targeteer.STATE_SHIELDBREAK)
                    {
                        addDrawjob(new DrawJob(_feet, _drawingPosition + new Vector2(0, _idle.getBoundingRect().Height - 4), _state != STATE_RETREAT && _state != STATE_ROUTED ? _side : _side * -1, _drawingY));
                        if(_wading.getPlaying())
                            addDrawjob(new DrawJob(_wading, _drawingPosition + new Vector2(0, _idle.getBoundingRect().Height - 4), _state != STATE_RETREAT && _state != STATE_ROUTED ? _side : _side * -1, _drawingY));
                    }
                }
                else if (this is Pikeman)
                {
                    if (_state != Pikeman.STATE_TUG)
                    {
                        addDrawjob(new DrawJob(_feet, _drawingPosition + new Vector2(0, _idle.getBoundingRect().Height - 4), _state != STATE_RETREAT && _state != STATE_ROUTED ? _side : _side * -1, _drawingY));
                        if (_wading.getPlaying())
                            addDrawjob(new DrawJob(_wading, _drawingPosition + new Vector2(0, _idle.getBoundingRect().Height - 4), _state != STATE_RETREAT && _state != STATE_ROUTED ? _side : _side * -1, _drawingY));
                    }
                }
                else if (this is DismountedCavalry )
                {
                    if (_state != DismountedCavalry.STATE_FALLING)
                    {
                        addDrawjob(new DrawJob(_feet, _drawingPosition + new Vector2(0, _idle.getBoundingRect().Height - 4), _state != STATE_RETREAT && _state != STATE_ROUTED ? _side : _side * -1, _drawingY));
                        if (_wading.getPlaying())
                            addDrawjob(new DrawJob(_wading, _drawingPosition + new Vector2(0, _idle.getBoundingRect().Height - 4), _state != STATE_RETREAT && _state != STATE_ROUTED ? _side : _side * -1, _drawingY));
                    }
                }
                else if (this is Dopple)
                {
                    if (!(_state == STATE_CHARGING || _state == STATE_ATTACKING || _state == STATE_RELOADING) && _destination.X - _position.X < -Soldier.WIDTH)
                    {
                        addDrawjob(new DrawJob(_feet, _drawingPosition + new Vector2(0, _idle.getBoundingRect().Height - 4), _side * -1, _drawingY));
                        if (_wading.getPlaying())
                            addDrawjob(new DrawJob(_wading, _drawingPosition + new Vector2(0, _idle.getBoundingRect().Height - 4), _side * -1, _drawingY));
                    }
                    else
                    {
                        addDrawjob(new DrawJob(_feet, _drawingPosition + new Vector2(0, _idle.getBoundingRect().Height - 4), _state != STATE_RETREAT && _state != STATE_ROUTED ? _side : _side * -1, _drawingY));
                        if (_wading.getPlaying())
                            addDrawjob(new DrawJob(_wading, _drawingPosition + new Vector2(0, _idle.getBoundingRect().Height - 4), _state != STATE_RETREAT && _state != STATE_ROUTED ? _side : _side * -1, _drawingY));
                    }
                }
                else
                {
                    addDrawjob(new DrawJob(_feet, _drawingPosition + new Vector2(0, _idle.getBoundingRect().Height - 4), _state != STATE_RETREAT && _state != STATE_ROUTED ? _side : _side * -1, _drawingY));
                    if (_wading.getPlaying())
                        addDrawjob(new DrawJob(_wading, _drawingPosition + new Vector2(0, _idle.getBoundingRect().Height - 4), _state != STATE_RETREAT && _state != STATE_ROUTED ? _side : _side * -1, _drawingY));
                }
            }

            if (this is Dopple)
            {
                if (!(_state == STATE_CHARGING || _state == STATE_ATTACKING || _state == STATE_RELOADING) && _destination.X - _position.X < -Soldier.WIDTH)
                    addDrawjob(new DrawJob(_body, _drawingPosition + _jostleOffset, _side * -1, _drawingY));
                else
                    addDrawjob(new DrawJob(_body, _drawingPosition + _jostleOffset, _state != STATE_ROUTED ? _side : _side * -1, _drawingY));
            }
            else if (this is Slinger && ((Slinger)this).cannon)
            {
                addDrawjob(new DrawJob(_body, _drawingPosition + _jostleOffset, _state != STATE_ROUTED && _state != STATE_SPAWN ? _side * -1 : _side * -1, _drawingY));
            }
            else
            {
                addDrawjob(new DrawJob(_body, _drawingPosition + _jostleOffset, _state != STATE_ROUTED && _state != STATE_SPAWN ? _side : _side * -1, _drawingY));
                if (_wading.getPlaying())
                {
                    if (_state != STATE_DYING || _death.getCurrFrame() != _death.getMaxFrames() - 1)
                        addDrawjob(new DrawJob(_wading, _drawingPosition + new Vector2(0, _idle.getBoundingRect().Height - 4), _state != STATE_RETREAT && _state != STATE_ROUTED ? _side : _side * -1, _drawingY));
                    else
                        addDrawjob(new DrawJob(_wading, _drawingPosition + new Vector2(getWidth(), _idle.getBoundingRect().Height - 4), _state != STATE_RETREAT && _state != STATE_ROUTED ? _side : _side * -1, _drawingY));

                }
            }

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

        protected void addDrawjob(DrawJob drawJob, bool flicker)
        {
            if (!flicker)
                addDrawjob(drawJob);
            else
            {
                Sprite sprite = drawJob.sprite;
                DrawJob flickerJob = new DrawJob(sprite, drawJob.position, drawJob.side, drawJob.drawingY + 1, true, 500f);
                _screen.addDrawjob(flickerJob);
            }
                
        }

        Vector2 getDestCenter(Vector2 dest)
        {
            return new Vector2(dest.X + getWidth() / 2, dest.Y + getHeight() / 2);
        }

        Vector2 getCollidedDest(Vector2 dest)
        {
            float shimmy = 10f;
            int i = 0;
            Vector2[] dests = new Vector2[terrainColliders.Count]; 

            foreach (Terrain terrain in terrainColliders)
            {
                Vector2 destCenter = this.getDestCenter(dest);
                /*if (terrain is CollisionCircle)
                {

                    shimmy = 50f;
                    Vector2 delta = terrain.getCenter() - this.getCenter();

                    double angle = Math.Atan2(delta.Y, delta.X);
                    double cos = Math.Cos(angle);
                    double sin = Math.Sin(angle);

                    dest.X = (float)cos * (((CollisionCircle)terrain).radius + shimmy);
                    dest.Y = (float)sin * (((CollisionCircle)terrain).radius + shimmy);

                    dest = terrain.getCenter() - dest;
                }
                else
                {*/
                if (destCenter.X < terrain.collisionCenter.X)
                {
                    if (destCenter.Y < terrain.collisionCenter.Y && terrain.collisionCenter.Y - destCenter.Y > terrain.collisionCenter.X - destCenter.X)
                    {
                        dest.X = terrain.collisionBox.X - getWidth() - shimmy;
                        dest.Y = terrain.collisionBox.Y - getHeight();
                    }
                    else if (destCenter.Y > terrain.collisionCenter.Y && destCenter.Y - terrain.collisionCenter.Y > terrain.collisionCenter.X - destCenter.X)
                    {
                        dest.X = terrain.collisionBox.X - getWidth() - shimmy;
                        dest.Y = terrain.collisionBox.Y + terrain.collisionBox.Height;
                    }
                    else
                    {
                        if (destCenter.Y < terrain.collisionCenter.Y)
                        {
                            dest.Y = terrain.collisionBox.Y - getHeight() - shimmy;
                            dest.X = terrain.collisionBox.X - getWidth() - shimmy;
                        }
                        else
                        {
                            dest.Y = terrain.collisionBox.Y + terrain.collisionBox.Height + shimmy;
                            dest.X = terrain.collisionBox.X - getWidth() - shimmy;
                        }
                    }
                }
                else
                {
                    if (destCenter.Y < terrain.collisionCenter.Y && terrain.collisionCenter.Y - destCenter.Y > destCenter.X - terrain.collisionCenter.X)
                    {
                        dest.X = terrain.collisionBox.X + terrain.collisionBox.Width + shimmy;
                        dest.Y = terrain.collisionBox.Y - getHeight() - shimmy;
                    }
                    else if (destCenter.Y > terrain.collisionCenter.Y && destCenter.Y - terrain.collisionCenter.Y > destCenter.X - terrain.collisionCenter.X)
                    {
                        dest.X = terrain.collisionBox.X + terrain.collisionBox.Width + shimmy;
                        dest.Y = terrain.collisionBox.Y + terrain.collisionBox.Height + shimmy;
                    }
                    else
                    {
                        if (destCenter.Y < terrain.collisionCenter.Y)
                        {
                            dest.Y = terrain.collisionBox.Y - getHeight() - shimmy;
                            dest.X = terrain.collisionBox.X + terrain.collisionBox.Width + shimmy;
                        }
                        else
                        {
                            dest.Y = terrain.collisionBox.Y + terrain.collisionBox.Height + shimmy;
                            dest.X = terrain.collisionBox.X + terrain.collisionBox.Width + shimmy;
                        }
                    }
                }
                //}
                dests[i] = dest;
                i++;
            }
            
            dest = Vector2.Zero;
            
            foreach (Vector2 vect in dests)
            {
                dest += vect;
            }

            dest /= dests.Length;

            terrainColliders.Clear();

            return dest;
        }

        public virtual void update(TimeSpan timeSpan)
        {
            _stateChanged = false;
            bool guarding = true;//this is Pikeman || this is Dopple || _type == TYPE_SHOT;

            if (guarding)
            {
                guarding = guardTarget != null && _state != STATE_DYING && _state != STATE_DEAD && (this is Pikeman && _state != Pikeman.STATE_TUG);
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

            if (_reacting && _state != STATE_DEAD && _state != STATE_DYING && _state != Targeteer.STATE_SHIELDBREAK
                && _state != STATE_MELEE_LOSS && _state != STATE_MELEE_WIN
                && (_state != Cavalry.STATE_SLOWDOWN || !(this is Cavalry)) && (_state != Cavalry.STATE_TURNING || !(this is Cavalry)))
            {
                checkReactions(timeSpan);
                updateState(timeSpan);
                updateAnimation(timeSpan);
                return;
            }
            // check for REACTIONS
            else if ((_state == STATE_READY || _state == STATE_CHARGING || _state == STATE_CHARGED) || (this is Colmillos && _state == Colmillos.STATE_COVER) /*&& !guarding*/)
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
                && ((_state != CrossbowmanPavise.STATE_RETRIEVING) || !(this is CrossbowmanPavise))
                && ((_state != Wolf.STATE_KILL && _state != Wolf.STATE_HOWLING && _state != Wolf.STATE_TUG) || !(this is Wolf))
                && ((_state != Pikeman.STATE_TUG) || !(this is Pikeman)))
            {
                if (terrainColliders.Count > 0)
                {
                    Vector2 tempDest = getCollidedDest(_destination);
                    _delta = tempDest - _position;
                    _dest = tempDest;
                }
                else
                {
                    _delta = _destination - _position;
                    _dest = _destination;
                }
            }
            else
            {
                if (terrainColliders.Count > 0)
                {
                    Vector2 tempDest = getCollidedDest(_meleeDestination);
                    _delta = tempDest - _position;
                    _dest = tempDest;
                }
                _delta = _meleeDestination - _position;
                _dest = _meleeDestination;
            }

            double angle = Math.Atan2(_delta.Y, _delta.X);
            double cos = Math.Cos(angle);
            double sin = Math.Sin(angle);
            _travel.X = (float)cos * (float)timeSpan.TotalMilliseconds * _speed;
            _travel.Y = (float)sin * (float)timeSpan.TotalMilliseconds * _speed;
                
            //fix the sign for the trig quadrant
            if (_delta.X < 0)
                _travel.X *= -1;
            if (_delta.Y < 0)
                _travel.Y *= -1;


            bool inWater = _screen.checkWaterSituation(_position.X, getCenter().Y) == BattleScreen.TerrainSituationResult.WATER;
            if (inWater)
            {
                if (!_wading.getPlaying())
                {
                    _wading.playRandomStart();
                }
            }
            else
            {
                _wading.stop();
            }

            // check to see if walking
            if (_delta.Length() != 0)
            {
                if (inWater)
                {
                    //water drops for when walking in water
                    dropTimer -= (float)timeSpan.TotalMilliseconds;
                    if (dropTimer < 0)
                    {
                        dropTimer = dropTime;
                        float speed;
                        //float dropDelta = _travel.X / (float)timeSpan.TotalMilliseconds * (_delta.X > 0 ? 1 : -1) * (_delta.X == 0 ? 0 : 1) * speed * (myFormation == _screen.getPlayerFormation() ? 2.5f : 7);
                        if (myFormation == _screen.getPlayerFormation())
                        {
                            speed = myFormation.getSpeed();
                        }
                        else
                        {
                            speed = _speed;
                        }
                        Vector2 dropDelta = new Vector2((float)cos * speed, (float)sin * speed);

                        new Drop(_screen, _side, _position + new Vector2(WIDTH / 2f, HEIGHT), dropDelta, (float)angle);
                        new Drop(_screen, _side, _position + new Vector2(WIDTH / 2f, HEIGHT), dropDelta, (float)angle);
                        new Drop(_screen, _side, _position + new Vector2(WIDTH / 2f, HEIGHT), dropDelta, (float)angle);
                        new Drop(_screen, _side, _position + new Vector2(WIDTH / 2f, HEIGHT), dropDelta, (float)angle);
                        new Drop(_screen, _side, _position + new Vector2(WIDTH / 2f, HEIGHT), dropDelta, (float)angle);
                        new Drop(_screen, _side, _position + new Vector2(WIDTH / 2f, HEIGHT), dropDelta, (float)angle);
                    }
                }

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
                dropTimer = 0;
                _feet.stop();
                _feet.reset();
                _retreat.stop();
                _retreat.reset();
            }

            _wading.update(timeSpan);
            _feet.update(timeSpan);
            _retreat.update(timeSpan);

            if (_feet.getCurrFrame() % 2 > 0)
                _jostleOffset.Y = 1f;
            else
                _jostleOffset.Y = 0f;

            // as long as we are not at destination, keep trying to get there, but don't overshoot
            //if (!(this is Wolf && _state == Wolf.STATE_TURNING))
            //{
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
            //}

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
                    _state = STATE_ROUTED;
                    _stateTimer = _routedTime;
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
                        if (initCharge && !(this is Wolf))
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
                    if (!splashed && _death.getCurrFrame() == 3 && _screen.checkWaterSituation(_position.X, _position.Y) == BattleScreen.TerrainSituationResult.WATER)
                    {
                        new ScreenAnimation(_screen, -1 * _side, _side == BattleScreen.SIDE_PLAYER ? new Vector2(_position.X - 40f, _position.Y + getHeight() + 1f) : new Vector2(_position.X + 40f, _position.Y + getHeight() + 1f), new Sprite(PikeAndShotGame.SPLASHING, new Rectangle(14, 8, 4, 4), 48, 24), 1250f);
                        splashed = true;
                    }
                    if (_stateTimer <= 0)
                    {
                        if (this is Colmillos)
                        {
                            _stateTimer = ((Colmillos)this)._riseTime;
                            _state = Colmillos.STATE_RISE;
                            _stateChanged = true;
                        }
                        else
                        {
                            _stateTimer = 0f;
                            _state = STATE_DEAD;
                            _stateChanged = true;
                        }
                    }
                }
                else if (_state == STATE_MELEE_WIN)
                {
                    if (_engager == null || _engager.getState() != STATE_DYING)
                    {
                        _stateTimer -= (float)timeSpan.TotalMilliseconds;
                        if (_stateTimer <= 0)
                        {
                            if (_longMelee <= 0)
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
                else if (_state == STATE_SPAWN)
                {
                    _stateTimer -= (float)timeSpan.TotalMilliseconds;
                    if (_stateTimer <= 0)
                    {
                        _stateTimer = 0;
                        _state = STATE_READY;
                        _stateChanged = true;
                    }
                }
            }
        }

        protected virtual void winMelee()
        {
            _stateTimer = 0f;
            _state = preAttackState;
            if (myFormation != null && myFormation.retreated)
                route();
            _stateChanged = true;
        }

        int lastDyingFrame = 0;
        protected virtual void updateAnimation(TimeSpan timeSpan)
        {
            if (_state == STATE_DYING)
            {
                int maxFrames = _death.getMaxFrames() * 3;
                float deathFrameTime = _deathTime / (float)maxFrames;
                int frameNumber = maxFrames - (int)(_stateTimer / deathFrameTime) -1;

                if(frameNumber == _death.getMaxFrames()-1 && !playedFallSound && !(this is Wolf))
                {
                    bodyFallSound.Play();
                    playedFallSound = true;
                }

                if (frameNumber < _death.getMaxFrames())
                    _death.setFrame(frameNumber);
                else
                    _death.setFrame(_death.getMaxFrames() - 1);

                if(_death.getCurrFrame() == _death.getMaxFrames() - 1 && lastDyingFrame == _death.getMaxFrames() - 2 && this is Colmillos)
                    new ScreenAnimation(_screen, _side, new Vector2(_position.X - (_side == BattleScreen.SIDE_PLAYER ? 22f : -22f), _position.Y + 22f), new Sprite(PikeAndShotGame.COLMILLOS_HELMET, new Rectangle(42, 8, 16, 16), 60, 24), Colmillos.helmetTime);


                lastDyingFrame = _death.getCurrFrame();

                if (this is Cavalry || this is Wolf)
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
                int maxFrames = _routed.getMaxFrames();
                float frameTime = _routedTime / (float)maxFrames;
                int frameNumber = maxFrames - (int)(_stateTimer / frameTime) - 1;

                _routed.setFrame(frameNumber);

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
                _body = _idle;
            }
            else if (_state == STATE_RETREAT)
            {
                _retreat.update(timeSpan);
                _body = _retreat;
            }
            else if (_state == STATE_SPAWN)
            {
                int maxFrames = _spawn.getMaxFrames();
                float frameTime = _spawnTime / (float)maxFrames;
                int frameNumber = maxFrames - (int)(_stateTimer / frameTime) - 1;

                _spawn.setFrame(frameNumber);

                _body = _spawn;
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

        public void spawn()
        {
            if (_state == STATE_READY)
            {
                _state = STATE_SPAWN;
                _stateTimer = _spawnTime;
            }
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
            float shimmy = (float)timeSpan.TotalMilliseconds * _speed;

            if (collider == _killer || _state == STATE_DEAD || _state == STATE_DYING)
                return;
            if (collider is Terrain)
            {
                terrainColliders.Add((Terrain)collider);
            }
            else if (collider is Shot && collider.getSide() != _side && !(collider is Pavise) && collider.getState() != STATE_DEAD && collider.getState() != Shot.STATE_GROUND)
            {
                if (this is Targeteer)
                {
                    if (collider is ArquebusierShot || (collider is CrossbowShot && collider.getSide() == BattleScreen.SIDE_PLAYER))
                        ((Targeteer)this).shieldBreak();
                    else
                        ((Targeteer)this).shieldBlock();

                    ((Shot)collider).hit();
                    _killer = collider;
                }
                else if (this is CrossbowmanPavise)
                {
                    if (_side == BattleScreen.SIDE_PLAYER)
                        ((CrossbowmanPavise)this).shieldBlock();
                    else
                        hit();

                    ((Shot)collider).hit();
                    _killer = collider;
                }
                else if (!(this is Leader) && (this._side == BattleScreen.SIDE_ENEMY || _screen.getPlayerFormation() == this.myFormation))
                {
                    hit();
                    ((Shot)collider).hit();
                    _killer = collider;
                }
                
            }
            else if ((collider is PikeTip && collider.getSide() != _side) && (_state != STATE_DEAD && _state != STATE_DYING && _state != STATE_ROUTE && _state != STATE_ROUTED))
            {
                if (((PikeTip)collider).getSoldierState() == Pikeman.STATE_LOWERED)
                {
                    if (!(this is Targeteer) && !(this is Wolf) && _side != BattleScreen.SIDE_NEUTRAL)
                    {
                        hit();
                        _killer = collider;
                        ((PikeTip)collider).getPikeman().recoil();
                    }
                    else if (this is Targeteer)
                    {
                        if ( (!(this is Colmillos) && ((Targeteer)this)._hasShield) || (this is Colmillos && !((ColmillosFormation)((Colmillos)this).myFormation).attacked))
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
                    else if (this is Wolf && _state != Wolf.STATE_TUG && _state != Wolf.STATE_SPOOKED && _state != STATE_DYING && _state != STATE_DEAD && _state != Wolf.STATE_FLEE && _state != Wolf.STATE_TURNING)
                    {
                        ((PikeTip)collider).getPikeman().tug(((Wolf)this));
                        ((Wolf)this).tug(((PikeTip)collider).getPikeman());
                    }
                }
            }
            else if ((collider is WeaponSwing && collider.getSide() != _side) && (_state != STATE_DEAD && _state != STATE_DYING && _state != STATE_ROUTE && _state != STATE_ROUTED))
            {
                if (((WeaponSwing)collider).getSoldier() is Colmillos)
                {
                    if (!((WeaponSwing)collider).hit)
                    {
                        if (this is Targeteer)
                            ((Targeteer)this).shieldBreak();
                        else
                        {
                            hit();
                            _killer = collider;
                        }
                        ((WeaponSwing)collider).hit = true;
                        _screen.removeScreenObject(collider);
                    }

                }
                else if (this is Targeteer)
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
                    colliderInFormation = ((Soldier)collider).myFormation == _screen.getPlayerFormation();
                    thisInFormation = myFormation == _screen.getPlayerFormation();
                }
                if (_state != STATE_DEAD && _state != STATE_DYING && _state != STATE_MELEE_WIN && _state != STATE_MELEE_LOSS && _state != STATE_ROUTED && (!(this is Targeteer) || _state != Targeteer.STATE_SHIELDBREAK) && (!(this is DismountedCavalry) || _state != DismountedCavalry.STATE_FALLING))
                {
                    if (this is CrossbowmanPavise && collider is CrossbowmanPavise
                        && (this._state == STATE_CHARGING || this._state == STATE_RELOADING || this._state == STATE_ATTACKING || this._state == CrossbowmanPavise.STATE_PLACING || this._state == CrossbowmanPavise.STATE_RETRIEVING)
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
                    else if (myFormation == null && ((Soldier)collider).myFormation == null
                        && _side == BattleScreen.SIDE_ENEMY && collider.getSide() == BattleScreen.SIDE_ENEMY
                        && (collider.getState() != STATE_DYING && collider.getState() != STATE_DEAD && collider.getState() != STATE_MELEE_WIN && collider.getState() != STATE_MELEE_LOSS
                        && collider.getState() != Wolf.STATE_TUG))
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
                        if (!((collider.getSide() == BattleScreen.SIDE_NEUTRAL && _side == BattleScreen.SIDE_PLAYER) || (collider.getSide() == BattleScreen.SIDE_PLAYER && _side == BattleScreen.SIDE_NEUTRAL)))
                        {
                            //fighting
                            if (_side != collider.getSide() && !(this is Leader || collider is Leader) && collider.getState() != STATE_DEAD && collider.getState() != STATE_DYING && collider.getState() != STATE_MELEE_WIN && collider.getState() != STATE_MELEE_LOSS && collider.getState() != STATE_ROUTED && (!(collider is Targeteer) || collider.getState() != Targeteer.STATE_SHIELDBREAK) && (!(collider is DismountedCavalry) || collider.getState() != DismountedCavalry.STATE_FALLING) && (!(collider is Wolf) || collider.getState() != Wolf.STATE_KILL) && (!(this is Wolf) || this._state != Wolf.STATE_KILL))
                            {
                                bool rescueFight = (_side == BattleScreen.SIDE_PLAYER && !thisInFormation) ||
                                                    (collider.getSide() == BattleScreen.SIDE_PLAYER && !colliderInFormation);

                                if (this is Colmillos)
                                {
                                    if (_state != Colmillos.STATE_ATTACK)
                                        attack();
                                    _destination = _position;
                                }
                                else if (collider is Colmillos)
                                {
                                    if (collider.getState() != Colmillos.STATE_ATTACK)
                                        ((Colmillos)collider).attack();

                                    ((Colmillos)collider)._destination = ((Colmillos)collider)._position;
                                }
                                else if (this is Dopple)
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
                                else if (_side == BattleScreen.SIDE_ENEMY || this is Wolf)
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
                                    && collider.getState() != STATE_ATTACKING && collider.getState() != STATE_RELOADING
                                    && collider.getState() != STATE_ROUTED)
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
                                    && this.getState() != STATE_ATTACKING && this.getState() != STATE_RELOADING
                                    && this.getState() != STATE_ROUTED)
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

        public virtual void hit()
        {
            if (_state != STATE_DYING && _state != STATE_DEAD)
            {
                if ((_state == STATE_MELEE_LOSS || _state == STATE_MELEE_WIN) && (_engager.getState() == STATE_MELEE_LOSS || _engager.getState() == STATE_MELEE_WIN))
                {
                    if (_engager.givesRescueReward)
                    {
                        ((LevelScreen)_screen).collectCoin(_engager);
                        _engager.myFormation.retreat();
                    }
                    else
                    {
                        _engager.setState(_engager.preAttackState);
                    }
                }

                _state = STATE_DYING;
                _stateTimer = _deathTime;
                _destination = _position;
                _screen.addLooseSoldier(this);
                hitSound.Play();
                playedFallSound = false;
                splashed = false;

                //I want guys that are running in as replacements to count as a loss
                if (((myFormation == _screen.getPlayerFormation() && this._type != TYPE_SWINGER) || ((this._type == TYPE_PIKE || this._type == TYPE_SHOT) && _side == BattleScreen.SIDE_PLAYER)) && _screen is LevelScreen)
                {
                    ((LevelScreen)_screen).loseCoin(getType());
                }
            }
        }

        public virtual void route()
        {
            if (_state != STATE_DEAD && _state != STATE_DYING)
            {
                if ((_state == STATE_MELEE_LOSS || _state == STATE_MELEE_WIN) && (_engager.getState() == STATE_MELEE_LOSS || _engager.getState() == STATE_MELEE_WIN))
                    _engager.setState(_engager.preAttackState);
                _state = STATE_ROUTED;
                _stateTimer = _routedTime;

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
                {
                    _state = STATE_MELEE_WIN;
                }
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

        public virtual void alterDestination(bool changeX, float amount)
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
        private bool splashed;

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
            if (_state != STATE_MELEE_LOSS && _state != STATE_MELEE_WIN && _state != STATE_DYING && _state != STATE_DEAD && _state != Pikeman.STATE_TUG)
                return true;
            else
                return false;
        }
    }

    public class NPCFleer : Soldier
    {
        public const int STATE_FLEE = 900;
        public const int MAX_FLEERS = 6;

        private Sprite _flee;

        float _fleeTime;

        static List<int> variants;

        public NPCFleer(BattleScreen screen, float x, float y, int side)
            : base(screen, side, x, y)
        {
            _type = Soldier.TYPE_MELEE;
            _class = Soldier.CLASS_NPC_FLEER;

            if (variants == null)
            {
                variants = new List<int>();
            }

            if (variants.Count < 1)
            {
                for (int i = 0; i < MAX_FLEERS; i++)
                    variants.Add(i);
            }

            int r = PikeAndShotGame.random.Next(variants.Count);
            _flee = null;
            _fleeTime = 2000f;

            switch(variants[r])
            {
                case 0:
                    _idle = new Sprite(PikeAndShotGame.PEASANT1_IDLE, new Rectangle(10, 8, 16, 28), 42, 42);
                    _flee = new Sprite(PikeAndShotGame.PEASANT1_FLEE, new Rectangle(10, 8, 16, 28), 36, 42);
                    _feet = new Sprite(_screen._game.getDimmerClone(PikeAndShotGame.BLUE_FEET), new Rectangle(4, 2, 16, 12), 26, 16, true);
                    _state = STATE_FLEE;
                    _fleeTime = 2500f;
                    _stateTimer = _fleeTime;
                    break;
                case 1:
                    _idle = new Sprite(PikeAndShotGame.PEASANT2_IDLE, new Rectangle(10, 8, 16, 28), 42, 42);
                    _feet = new Sprite(_screen._game.getDimmerClone(PikeAndShotGame.BLUE_FEET), new Rectangle(4, 2, 16, 12), 26, 16, true);
                    break;
                case 2:
                    _idle = new Sprite(PikeAndShotGame.PEASANT3_IDLE, new Rectangle(10, 8, 16, 28), 42, 42);
                    _feet = new Sprite(_screen._game.getDimmerClone(PikeAndShotGame.BLUE_FEET), new Rectangle(4, 2, 16, 12), 26, 16, true);
                    break;
                case 3:
                    _idle = new Sprite(PikeAndShotGame.PEASANT4_IDLE, new Rectangle(10, 8, 16, 28), 42, 42);
                    _feet = new Sprite(_screen._game.getDimmerClone(PikeAndShotGame.BLUE_FEET), new Rectangle(4, 2, 16, 12), 26, 16, true);
                    break;
                case 4:
                    _idle = new Sprite(PikeAndShotGame.PEASANT5_IDLE, new Rectangle(10, 8, 16, 28), 42, 42);
                    _flee = new Sprite(PikeAndShotGame.PEASANT5_FLEE, new Rectangle(10, 8, 16, 28), 38, 48);
                    _feet = new Sprite(_screen._game.getDimmerClone(PikeAndShotGame.SOLDIER_FEET), new Rectangle(4, 2, 16, 12), 26, 16, true);
                    _state = STATE_FLEE;
                    _stateTimer = _fleeTime;
                    break;
                case 5:
                    _idle = new Sprite(PikeAndShotGame.PEASANT6_IDLE, new Rectangle(8, 12, 16, 28), 34, 48);
                    _feet = new Sprite(_screen._game.getDimmerClone(PikeAndShotGame.GOBLIN_FEET), new Rectangle(4, 2, 16, 12), 26, 16, true);
                    break;
            }
            variants.RemoveAt(r);
            _speed = 0.1f;
            _feet.setAnimationSpeed(_footSpeed / (_speed - 0.04f));
        }

        public override void setSide(int side)
        {
            _side = side;
        }

        public override void  update(TimeSpan timeSpan)
        {
            _speed = 0.1f;
            _feet.setAnimationSpeed(_footSpeed / (0.11f));
            if(_screen is LevelScreen)
                _destination = new Vector2 (_screen.getMapOffset().X - 200f, _position.Y);
 	        base.update(timeSpan);
        }

        protected override void  updateState(TimeSpan timeSpan)
        {
 	        base.updateState(timeSpan);
            if (!_stateChanged)
            {
                if (_state == STATE_FLEE)
                {
                    _stateTimer -= (float)timeSpan.TotalMilliseconds;

                    if (_stateTimer <= 0)
                    {
                        _stateTimer = _fleeTime;

                        _state = STATE_FLEE;
                    }
                }
            }
        }

        protected override void  updateAnimation(TimeSpan timeSpan)
        {
            base.updateAnimation(timeSpan);

            if (_state == STATE_FLEE)
            {
                int maxFrames = _flee.getMaxFrames();
                float frameTime = _fleeTime / (float)maxFrames;
                int frameNumber = maxFrames - (int)(_stateTimer / frameTime) - 1;

                _flee.setFrame(frameNumber);

                _body = _flee;
            }
        }
 
    }

    public class Pikeman : Soldier
    {
        public const int STATE_RAISING = 100;
        public const int STATE_LOWERED = 101;
        public const int STATE_RECOILING = 102;
        public const int STATE_LOWER45 = 103;
        public const int STATE_TUG = 104;

        private Sprite _pikemanLowerLow;
        private Sprite _pikemanLowerHigh;
        private Sprite _pikemanRecoil;
        private Sprite _tug;

        // lowered Sprite is a little more dynamic so we can assign different stlyes to it
        private Sprite _loweredSprite;

        private PikeTip _pikeTip;
        public float tugTime = 2000f;

        public bool variant;
        public Wolf wolf;

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
                variant = false;
                _idle = new Sprite(PikeAndShotGame.PIKEMAN1_IDLE, new Rectangle(6, 68, 16, 28), 26, 106);
                _death = new Sprite(PikeAndShotGame.PIKEMAN1_DEATH, new Rectangle(40, 2, 16, 28), 72, 40);
                _pikemanLowerLow = new Sprite(PikeAndShotGame.PIKEMAN1_LOWER_LOW, new Rectangle(32, 67, 16, 28), 124, 106);
                _pikemanLowerHigh = new Sprite(PikeAndShotGame.PIKEMAN1_LOWER_HIGH, new Rectangle(32, 67, 16, 28), 124, 106);
                _pikemanRecoil = new Sprite(PikeAndShotGame.PIKEMAN1_RECOIL, new Rectangle(28, 4, 16, 28), 108, 36);
                _melee1 = new Sprite(PikeAndShotGame.PIKEMAN1_MELEE, new Rectangle(24, 30, 16, 28), 64, 68);
                _defend1 = new Sprite(PikeAndShotGame.PIKEMAN1_DEFEND, new Rectangle(20, 2, 16, 28), 52, 40);
                _routed = new Sprite(PikeAndShotGame.PIKEMAN_ROUTED, new Rectangle(24, 20, 16, 28), 64, 64);
                _tug = new Sprite(PikeAndShotGame.PIKEMAN_TUG, new Rectangle(38, 12, 16, 28), 164, 50);
                tugTime = 2350f;
            }
            else
            {
                variant = true;
                _idle = new Sprite(PikeAndShotGame.PIKEMAN2_IDLE, new Rectangle(6, 68, 16, 28), 26, 106);
                _death = new Sprite(PikeAndShotGame.PIKEMAN2_DEATH, new Rectangle(40, 2, 16, 28), 72, 40);
                _pikemanLowerLow = new Sprite(PikeAndShotGame.PIKEMAN2_LOWER_LOW, new Rectangle(32, 67, 16, 28), 124, 106);
                _pikemanLowerHigh = new Sprite(PikeAndShotGame.PIKEMAN2_LOWER_HIGH, new Rectangle(32, 67, 16, 28), 124, 106);
                _pikemanRecoil = new Sprite(PikeAndShotGame.PIKEMAN2_RECOIL, new Rectangle(28, 4, 16, 28), 108, 36);
                _melee1 = new Sprite(PikeAndShotGame.PIKEMAN2_MELEE, new Rectangle(24, 30, 16, 28), 64, 68);
                _defend1 = new Sprite(PikeAndShotGame.PIKEMAN2_DEFEND, new Rectangle(20, 2, 16, 28), 52, 40);
                _routed = new Sprite(PikeAndShotGame.PIKEMAN2_ROUTED, new Rectangle(14, 10, 16, 28), 48, 48);
                _tug = new Sprite(PikeAndShotGame.PIKEMAN2_TUG, new Rectangle(38, 12, 16, 28), 164, 50);
                tugTime = 1200f;
            }
            _feet = new Sprite(PikeAndShotGame.PIKEMAN_FEET, new Rectangle(4, 2, 16, 12), 26, 16, true);
            _route = new Sprite(PikeAndShotGame.PIKEMAN_ROUTE, new Rectangle(6, 78, 16, 28), 50, 116);

            _feet.setAnimationSpeed(_footSpeed / 0.11f);
            _loweredSprite = _pikemanLowerLow;

            _pikeTip = new PikeTip(_screen, this);
        }

        public override void setSide(int side)
        {
            _side = side;
            _pikeTip.setSide(side);
        }

        public override void route()
        {
            base.route();
            new ScreenAnimation(_screen, _side, _position, new Sprite(PikeAndShotGame.PIKEMAN_DROP, new Rectangle(24, 88, 16, 24), 128, 128), 1000f);
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
            _stateTimer = 0;
            myFormation.reiteratePikeCommand(this);
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

            if (_state != STATE_LOWERED && _state != STATE_TUG)
                _state = STATE_ATTACKING;
            else
                preAttackState = STATE_ATTACKING;

            return true;
        }

        protected override void engage(bool win, Vector2 position, Soldier engager, bool rescueFight)
        {
            base.engage(win, position, engager, rescueFight);
            _screen.removeScreenObject(_pikeTip);
        }

        public override void hit()
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
            else
                preAttackState = STATE_RAISING;
            
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
                else if (_state == STATE_TUG)
                {
                    _stateTimer -= (float)timeSpan.TotalMilliseconds;

                    if (_stateTimer <= 0)
                    {
                        _stateTimer = tugTime;
                    }
                    if (wolf == null || wolf.getState() != Wolf.STATE_TUG)
                        untug();
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
            else if (_state == STATE_TUG)
            {
                int maxFrames = _tug.getMaxFrames();
                float frameTime = tugTime / (float)maxFrames;
                int frameNumber = maxFrames - (int)(_stateTimer / frameTime) - 1;

                _tug.setFrame(frameNumber);

                _body = _tug;
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

        internal void tug(Wolf wolf)
        {
            _state = STATE_TUG;
            _stateTimer = tugTime;
            _meleeDestination = _position;
            this.wolf = wolf;
        }

        public void untug()
        {
            if (_state == STATE_TUG)
            {
                wolf = null;
                _state = STATE_LOWERED;
                _stateTimer = 0f;
                if (myFormation != null)
                {
                    if (myFormation.retreated)
                        route();
                    else
                        myFormation.reiteratePikeCommand(this);
                }
            }
        }
    }


    public class Arquebusier : Soldier
    {
        private bool variant;

        private Sprite _arquebusierReload;
        private Sprite _arquebusierShoot;
        private SoundEffectInstance shotHitSound;

        public Arquebusier(BattleScreen screen, float x, float y, int side): base(screen, side, x, y)
        {
            _type = Soldier.TYPE_SHOT;
            _class = Soldier.CLASS_MERC_ARQUEBUSIER;
            _attackTime = 150f;
            _reloadTime = 3000f;

            if (PikeAndShotGame.random.Next(51) > 25)
            {
                variant = true;
                _feet = new Sprite(PikeAndShotGame.BROWN_FEET, new Rectangle(4, 2, 16, 12), 26, 16, true, true, 25, screen);
                _feet.flashable = false;
                _idle = new Sprite(PikeAndShotGame.ARQUEBUSIER_IDLE, new Rectangle(16, 16, 16, 28), 128, 64);
                _death = new Sprite(PikeAndShotGame.ARQUEBUSIER_DEATH, new Rectangle(40, 2, 16, 28), 72, 40);
                _melee1 = new Sprite(PikeAndShotGame.ARQUEBUSIER_MELEE, new Rectangle(24, 20, 16, 28), 72, 60);
                _defend1 = new Sprite(PikeAndShotGame.ARQUEBUSIER_DEFEND, new Rectangle(24, 20, 16, 28), 72, 60);
                _route = new Sprite(PikeAndShotGame.ARQUEBUSIER_ROUTE, new Rectangle(16, 16, 16, 28), 48, 52);
                _routed = new Sprite(PikeAndShotGame.ARQUEBUSIER_ROUTED, new Rectangle(14, 10, 16, 28), 48, 48);
                _arquebusierReload = new Sprite(PikeAndShotGame.ARQUEBUSIER_RELOAD, new Rectangle(16, 16, 16, 28), 128, 64, false, true, 25, screen);
                _arquebusierShoot = new Sprite(PikeAndShotGame.ARQUEBUSIER_SHOOT, new Rectangle(16, 16, 16, 28), 128, 64);
            }
            else
            {
                variant = false;
                _feet = new Sprite(PikeAndShotGame.ARQUEBUSIER_FEET2, new Rectangle(4, 2, 16, 12), 26, 16, true, true, 25, screen);
                _feet.flashable = false;
                _idle = new Sprite(PikeAndShotGame.ARQUEBUSIER_IDLE2, new Rectangle(14, 10, 16, 28), 96, 48);
                _death = new Sprite(PikeAndShotGame.ARQUEBUSIER_DEATH2, new Rectangle(40, 2, 16, 28), 72, 40);
                _melee1 = new Sprite(PikeAndShotGame.ARQUEBUSIER_MELEE2, new Rectangle(24, 20, 16, 28), 76, 60);
                _defend1 = new Sprite(PikeAndShotGame.ARQUEBUSIER_DEFEND2, new Rectangle(24, 20, 16, 28), 76, 60);
                _route = new Sprite(PikeAndShotGame.ARQUEBUSIER_ROUTE, new Rectangle(30, 12, 16, 28), 48, 52);
                _routed = new Sprite(PikeAndShotGame.ARQUEBUSIER2_ROUTED, new Rectangle(24, 24, 16, 28), 64, 64, true);
                _routedTime = 2000f;
                _arquebusierReload = new Sprite(PikeAndShotGame.ARQUEBUSIER_RELOAD2, new Rectangle(14, 10, 16, 28), 96, 48, false, true, 25, screen);
                _arquebusierShoot = new Sprite(PikeAndShotGame.ARQUEBUSIER_SHOOT2, new Rectangle(14, 10, 16, 28), 96, 48);
            }

            _feet.setAnimationSpeed(_footSpeed / 0.11f);
            shotHitSound = PikeAndShotGame.SHOT_HIT.CreateInstance();
            shotHitSound.Volume = 0.25f;
        }

        public override void route()
        {
            base.route();
            if(variant)
                new ThrownGun(_screen, _side, _position, new Sprite(PikeAndShotGame.ARQUEBUSIER_DROP, new Rectangle(0, 0, 40, 40), 40, 40), 1, 800f);
            else
                new ThrownGun(_screen, _side, _position, new Sprite(PikeAndShotGame.ARQUEBUSIER2_DROP, new Rectangle(0, 0, 36, 36), 36, 36), 5, 800f);
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
                    if (_arquebusierShoot.getCurrFrame() >= _arquebusierShoot.getMaxFrames()/2 && !_shotMade)
                        shotDone();
                }
            }
        }

        protected override void updateAnimation(TimeSpan timeSpan)
        {
            base.updateAnimation(timeSpan);
            if (_state == STATE_RELOADING)
            {
                int maxFrames = _arquebusierReload.getMaxFrames();
                float frameTime = _reloadTime / (float)maxFrames;
                int frameNumber = maxFrames - (int)(_stateTimer / frameTime) - 1;

                _arquebusierReload.setFrame(frameNumber);

                _body = _arquebusierReload;

            }
            else if (_state == STATE_ATTACKING)
            {
                int maxFrames = _arquebusierShoot.getMaxFrames();
                float frameTime = _attackTime / (float)maxFrames;
                int frameNumber = maxFrames - (int)(_stateTimer / frameTime) - 1;

                _arquebusierShoot.setFrame(frameNumber);

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
            {
                _screen.addShot(new ArquebusierShot(new Vector2(this._position.X + 34 + randDestOffset.X, this._position.Y + 10 + randDestOffset.Y), this._screen, _side, _arquebusierShoot.getBoundingRect().Height - 10, shotHitSound));
                new ArquebusierSmoke(_screen, _side, new Vector2(this._position.X + randDestOffset.X, this._position.Y + randDestOffset.Y));
            }
            else
            {
                _screen.addShot(new ArquebusierShot(new Vector2(this._position.X - 18 + randDestOffset.X, this._position.Y + 10 + randDestOffset.Y), this._screen, _side, _arquebusierShoot.getBoundingRect().Height - 10, shotHitSound));
                new ArquebusierSmoke(_screen, _side, new Vector2(this._position.X + randDestOffset.X, this._position.Y + randDestOffset.Y));
            }

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
                _screen.addShot(new CrossbowShot(new Vector2(this._position.X + 18 + randDestOffset.X, this._position.Y + 10 + randDestOffset.Y), this._screen, _side, _crossbowmanShoot.getBoundingRect().Height - 10));
            else
                _screen.addShot(new CrossbowShot(new Vector2(this._position.X - 14 + randDestOffset.X, this._position.Y + 10 + randDestOffset.Y), this._screen, _side, _crossbowmanShoot.getBoundingRect().Height - 10));
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
            covering = false;
            _stateTimer = COVER_TIME;
            uncovering = true;
        }

        private void cover()
        {
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
                    _screen.addShot(new AimedBolt(new Vector2(this._position.X + 18 + randDestOffset.X, this._position.Y + 10 + randDestOffset.Y), this._screen, _side, _crossbowmanShoot.getBoundingRect().Height - 10, bestTarget.getCenter(), bestTarget));
                else
                    _screen.addShot(new AimedBolt(new Vector2(this._position.X + 18 + randDestOffset.X, this._position.Y + 10 + randDestOffset.Y), this._screen, _side, _crossbowmanShoot.getBoundingRect().Height - 10, this.getCenter() + new Vector2(100f, 0), bestTarget));
            }
            else
                _screen.addShot(new CrossbowShot(new Vector2(this._position.X - 14 + randDestOffset.X, this._position.Y + 10 + randDestOffset.Y), this._screen, _side, _crossbowmanShoot.getBoundingRect().Height - 10));
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
                myPavise = new Pavise(new Vector2(this._position.X + 16f + randDestOffset.X, this._position.Y + 12f + randDestOffset.Y), this._screen, _side, 24f);
                _screen.addShot(myPavise);
                chargePosition -= new Vector2(8, 0);
                _meleeDestination = myFormation.getCenter() + chargePosition;
            }
            else
                _screen.addShot(new Pavise(new Vector2(this._position.X + 4f + randDestOffset.X, this._position.Y + 12f + randDestOffset.Y), this._screen, _side, 24f));
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

        public override void hit()
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
            _routed = new Sprite(PikeAndShotGame.PUCELLE_ROUTED, new Rectangle(4, 2, 16, 28), 26, 34);

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

        public override void route()
        {
            base.route();
            new ScreenAnimation(_screen, _side, _position, new Sprite(PikeAndShotGame.PUCELLE_DROP, new Rectangle(22, 90, 16, 24), 164, 128), 1000f);
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
        public bool cannon;
        int health;
        float flashTimer;

        public Slinger(BattleScreen screen, float x, float y, int side)
            : base(screen, side, x, y)
        {
            _type = Soldier.TYPE_SHOT;
            _class = Soldier.CLASS_GOBLIN_SLINGER;
            _attackTime = 600f;
            _reloadTime = 1000f;
            soundMade = false;
            cannon = false;

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
            _feet = new Sprite(PikeAndShotGame.SOLDIER_FEET, new Rectangle(4, 2, 16, 12), 26, 16, true);
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

        public Slinger(BattleScreen screen, float x, float y, int side, bool cannon)
            : this(screen, x, y, side)
        {
            this.cannon = true;
            _idle = new Sprite(PikeAndShotGame.CANNON_IDLE, new Rectangle(44, 16, 90, 46), 138, 76, false, true, screen);
            _idle.flashable = false;
            _slingerReload = new Sprite(PikeAndShotGame.CANNON, new Rectangle(44, 16, 90, 46), 138, 76,false, true, screen);
            _slingerReload.flashable = false;
            _slingerShoot = new Sprite(PikeAndShotGame.SLINGER_SHOOT, new Rectangle(28, 12, 16, 28), 72, 50);
            _death = new Sprite(PikeAndShotGame.SLINGER_DEATH, new Rectangle(40, 2, 16, 28), 72, 40);
            _melee1 = new Sprite(PikeAndShotGame.SLINGER_MELEE, new Rectangle(24, 30, 16, 28), 64, 68);
            _defend1 = new Sprite(PikeAndShotGame.SLINGER_DEFEND, new Rectangle(20, 2, 16, 28), 52, 40);
            _retreat = new Sprite(PikeAndShotGame.SLINGER_RETREAT, new Rectangle(6, 2, 16, 28), 46, 40, true);
            variant = false;
            _class = Soldier.CLASS_GOBLIN_CANNON;
            _attackTime = 600f;
            _reloadTime = 7000f;
            slingSound = slingSound = PikeAndShotGame.SHOT_4.CreateInstance();
            health = 3;
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

        public override void collide(ScreenObject collider, TimeSpan timeSpan)
        {
            if (cannon)
            {
                if (!(collider is PikeTip) && !(collider is Soldier))
                    base.collide(collider, timeSpan);
            }
            else
            {
                base.collide(collider, timeSpan);
            }
        }

        public override void hit()
        {
            if (health < 1)
            {
                base.hit();
            }
            else
            {
                health--;
                _slingerReload.setEffect(Sprite.EFFECT_FLASH_RED, 1500f / 8f);
                _idle.setEffect(Sprite.EFFECT_FLASH_RED, 1500f / 8f);
                flashTimer = 500f;
            }
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

            if(flashTimer > 0)
            {
                flashTimer -= (float)timeSpan.TotalMilliseconds;
                if (flashTimer <= 0)
                {
                    _slingerReload.setEffect(0, 1500f / 8f);
                    _idle.setEffect(0, 1500f / 8f);
                }
            }

            if (!_stateChanged)
            {
                if (_state == STATE_RELOADING)
                {
                    _stateTimer -= (float)timeSpan.TotalMilliseconds;

                    if (cannon)
                    {
                        if (_slingerReload.getCurrFrame() == (60) && !soundMade)
                        {
                            soundMade = true;
                            slingSound.Play();
                            shotDone();
                        }
                        if (_slingerReload.getCurrFrame() == (62))
                        {
                            soundMade = false;
                        }
                    }

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

            if (cannon)
            {
                _screen.addShot(new CannonBall(new Vector2(this._position.X + randDestOffset.X, this._position.Y + 16 + randDestOffset.Y), this._screen, _side, _slingerShoot.getBoundingRect().Height, rockHitSound));
            }
            else if (_side == BattleScreen.SIDE_PLAYER)
            {
                if(variant)
                    _screen.addShot(new SkirmisherJavelin(new Vector2(this._position.X + 32 + randDestOffset.X, this._position.Y + 4 + randDestOffset.Y), this._screen, _side, _slingerShoot.getBoundingRect().Height, rockHitSound));
                else
                    _screen.addShot(new SlingerRock(new Vector2(this._position.X + 28 + randDestOffset.X, this._position.Y + randDestOffset.Y), this._screen, _side, _slingerShoot.getBoundingRect().Height, rockHitSound));
            }
            else
            {
                if (variant)
                    _screen.addShot(new SkirmisherJavelin(new Vector2(this._position.X - 18 + randDestOffset.X, this._position.Y + 4 + randDestOffset.Y), this._screen, _side, _slingerShoot.getBoundingRect().Height, rockHitSound));
                else
                    _screen.addShot(new SlingerRock(new Vector2(this._position.X - 12 + randDestOffset.X, this._position.Y + randDestOffset.Y), this._screen, _side, _slingerShoot.getBoundingRect().Height, rockHitSound));
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

            _hasShield = true;
            _body = _idle;

            _feet.setAnimationSpeed(_footSpeed / 0.11f);
            hitSound = PikeAndShotGame.OWW_ENEMY.CreateInstance();
            hitSound.Volume = 0.25f;
        }
    }

    public class Brigand : Soldier
    {
        private bool variant;
        private int chargeVariant;

        public Brigand(BattleScreen screen, float x, float y, int side)
            : base(screen, side, x, y)
        {
            _type = Soldier.TYPE_MELEE;
            _class = Soldier.CLASS_GOBLIN_BRIGAND;

            chargeVariant = PikeAndShotGame.random.Next(2);

            _feet = new Sprite(PikeAndShotGame.BROWN_FEET, new Rectangle(4, 2, 16, 12), 26, 16, true);
            if (PikeAndShotGame.random.Next(51) > 25)
            {
                variant = true;
                _idle = new Sprite(PikeAndShotGame.BRIGAND2_IDLE, new Rectangle(10, 2, 16, 28), 46, 42);
                _death = new Sprite(PikeAndShotGame.BRIGAND2_DEATH, new Rectangle(40, 2, 16, 28), 72, 40);
                _melee1 = new Sprite(PikeAndShotGame.BRIGAND2_MELEE1, new Rectangle(24, 30, 16, 28), 64, 68);
                _defend1 = new Sprite(PikeAndShotGame.BRIGAND2_DEFEND1, new Rectangle(20, 4, 16, 28), 54, 40);
                _route = new Sprite(PikeAndShotGame.BERZERKER2_ROUTE, new Rectangle(12, 10, 16, 28), 40, 46);
                _routed = new Sprite(PikeAndShotGame.BERZERKER2_ROUTED, new Rectangle(12, 10, 16, 28), 40, 46, true);
                _charge = new Sprite(PikeAndShotGame.BRIGAND2_CHARGE, new Rectangle(20, 20, 16, 28), 60, 56);
                _spawn = new Sprite(PikeAndShotGame.BRIGAND2_SPAWN, new Rectangle(26, 20, 16, 28), 72, 72);
            }
            else
            {
                variant = false;
                _idle = new Sprite(PikeAndShotGame.BRIGAND1_IDLE, new Rectangle(10, 2, 16, 28), 46, 42);
                _death = new Sprite(PikeAndShotGame.BRIGAND1_DEATH, new Rectangle(40, 2, 16, 28), 72, 40);
                _melee1 = new Sprite(PikeAndShotGame.BRIGAND1_MELEE1, new Rectangle(24, 30, 16, 28), 64, 68);
                _defend1 = new Sprite(PikeAndShotGame.BRIGAND1_DEFEND1, new Rectangle(24, 4, 16, 28), 60, 40);
                _route = new Sprite(PikeAndShotGame.BERZERKER_ROUTE, new Rectangle(12, 10, 16, 28), 40, 46);
                _routed = new Sprite(PikeAndShotGame.BERZERKER_ROUTED, new Rectangle(12, 10, 16, 28), 40, 46, true);
                _charge = new Sprite(PikeAndShotGame.BRIGAND1_CHARGE, new Rectangle(20, 20, 16, 28), 60, 56);
                _spawn = new Sprite(PikeAndShotGame.BRIGAND1_SPAWN, new Rectangle(68, 24, 16, 28), 120, 68);
                _spawnTime = 4000f;
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

        protected override void updateAnimation(TimeSpan timeSpan)
        {
            base.updateAnimation(timeSpan);

            if (_state == STATE_CHARGING && initCharge)
            {
                int maxFrames = 4;
                float frameTime = (_chargeTime / 2f) / (float)maxFrames;
                int frameNumber = maxFrames - (int)(_stateTimer / frameTime);

                if (chargeVariant == 0)
                {
                    _melee1.setFrame(frameNumber);
                    _body = _melee1;
                }
                else
                {
                    _defend1.setFrame(frameNumber);
                    _body = _defend1;
                }
            }
        }
    }

    public class Hauler : Soldier
    {
        public const int STATE_HAULING = 100;
        public bool variant;
        private bool _holding;
        private Sprite _noHaulIdle;
        private Sprite _baggerIdle;

        public Hauler(BattleScreen screen, float x, float y, int side)
            : base(screen, side, x, y)
        {
            _type = Soldier.TYPE_MELEE;
            _class = Soldier.CLASS_GOBLIN_HAULER;

            _feet = new Sprite(PikeAndShotGame.HAULER_FEET, new Rectangle(4, 2, 16, 12), 26, 16, true);

            _holding = false;

            if (PikeAndShotGame.random.Next(51) > 25)
            {
                variant = true;

                _idle = new Sprite(PikeAndShotGame.BAGGER_HAUL, new Rectangle(22, 10, 16, 28), 64, 56);
                _death = new Sprite(PikeAndShotGame.BAGGER_DEATH, new Rectangle(40, 2, 16, 28), 72, 40);
                _melee1 = new Sprite(PikeAndShotGame.BRIGAND2_MELEE1, new Rectangle(24, 30, 16, 28), 64, 68);
                _defend1 = new Sprite(PikeAndShotGame.BRIGAND2_DEFEND1, new Rectangle(20, 2, 16, 28), 52, 40);
                _route = new Sprite(PikeAndShotGame.BERZERKER2_ROUTE, new Rectangle(12, 10, 16, 28), 40, 46);
                _routed = new Sprite(PikeAndShotGame.BERZERKER2_ROUTED, new Rectangle(12, 10, 16, 28), 40, 46, true);
                _charge = new Sprite(PikeAndShotGame.BRIGAND2_CHARGE, new Rectangle(20, 20, 16, 28), 60, 56);
                _noHaulIdle = new Sprite(PikeAndShotGame.BAGGER_IDLE, new Rectangle(10, 4, 16, 28), 36, 36);
                _baggerIdle = new Sprite(PikeAndShotGame.BAGGER_BAG, new Rectangle(10, 4, 16, 28), 36, 36);
                _attackTime = 1400f;
            }
            else
            {
                variant = false;

                _idle = new Sprite(PikeAndShotGame.HAULER_HAUL, new Rectangle(20, 16, 16, 28), 56, 56);
                _death = new Sprite(PikeAndShotGame.HAULER_DEATH, new Rectangle(40, 2, 16, 28), 72, 40);
                _melee1 = new Sprite(PikeAndShotGame.BRIGAND2_MELEE1, new Rectangle(24, 30, 16, 28), 64, 68);
                _defend1 = new Sprite(PikeAndShotGame.BRIGAND2_DEFEND1, new Rectangle(20, 2, 16, 28), 52, 40);
                _route = new Sprite(PikeAndShotGame.BERZERKER2_ROUTE, new Rectangle(12, 10, 16, 28), 40, 46);
                _routed = new Sprite(PikeAndShotGame.BERZERKER2_ROUTED, new Rectangle(12, 10, 16, 28), 40, 46, true);
                _charge = new Sprite(PikeAndShotGame.BRIGAND2_CHARGE, new Rectangle(20, 20, 16, 28), 60, 56);
                _noHaulIdle = new Sprite(PikeAndShotGame.HAULER_IDLE, new Rectangle(10, 4, 16, 28), 36, 36);
                _attackTime = 800f;
            }

            _body = _idle;

            _feet.setAnimationSpeed(_footSpeed / 0.11f);
        }

        public override void draw(SpriteBatch spritebatch)
        {
            _drawingPosition = _position + randDestOffset - _screen.getMapOffset();

            int flipValue = 1;

            if (_delta.X > 0)
                flipValue = -1;
            
            Hauler hauler = (Hauler)this;
            if ((_state != Hauler.STATE_HAULING || !hauler.variant) && _state != Hauler.STATE_DEAD && _state != Hauler.STATE_DYING)
                addDrawjob(new DrawJob(_feet, _drawingPosition + new Vector2(0, _idle.getBoundingRect().Height - 4), _state != STATE_RETREAT && _state != STATE_ROUTED ? _side * flipValue : _side * -1, _drawingY));

            addDrawjob(new DrawJob(_body, _drawingPosition + _jostleOffset, _state != STATE_ROUTED ? _side * flipValue : _side * -1, _drawingY));
                
        }

        public override void hit()
        {
            base.hit();
            if(variant)
                new ThrownGun(_screen, _side, _position, new Sprite(PikeAndShotGame.BAGGER_THROW, new Rectangle(22, 18, 24, 24), 48, 48), 7, 1500f, true);
            else
                new ThrownGun(_screen, _side, _position, new Sprite(PikeAndShotGame.HAULER_THROW, new Rectangle(22, 12, 24, 24), 48, 48), 6, 1500f, true);
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
                _holding = true;
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
            else if (_state == STATE_READY)
            {
                if (_holding)
                {
                    if (variant)
                    {
                        _body = _baggerIdle;
                    }
                    else
                    {
                        _idle.setFrame(_idle.getMaxFrames() - 1);
                        _body = _idle;
                    }
                }
                else
                {
                    _body = _noHaulIdle;
                }
            }
        }
    }

    public class Colmillos : Targeteer
    {
        public const int STATE_ATTACK = 200;
        public const int STATE_RISE = 201;
        public const int STATE_EATEN = 202;
        public const int STATE_HOWL = 203;
        public const int STATE_RUN = 204;
        public const int STATE_STAGGER = 205;

        private const int HEALTH = 2;
        
        Sprite _attack;
        Sprite _noShieldAttack;
        Sprite _noArmourAttack;
        Sprite _armourFall;
        Sprite _noArmourIdle;
        Sprite _rise;
        Sprite _howl;
        Sprite _noShieldHowl;
        Sprite _noArmourHowl;
        Sprite _stagger;

        WeaponSwing _weaponSwing;

        public float _riseTime = 1000f;
        public float _howlTime = 1000f;
        public float _runTime = 300;
        public float _eatenTime = 4500f;
        public float _staggerTime = 1250f;
        public static float helmetTime = 2000f;
        public float hurtTimer;

        int health;

        bool hasArmour;

        ColmillosWolf whiteWolf = null;

        protected SoundEffectInstance hurtSound;
        protected SoundEffectInstance yellSound;
        protected SoundEffectInstance slash;

        public Colmillos(BattleScreen screen, float x, float y, int side)
            : base(screen, x, y, side)
        {
            _type = Soldier.TYPE_MELEE;
            _class = Soldier.CLASS_GOBLIN_COLMILLOS;
            _attackTime = 1800f;
            _deathTime = 2000f;
            hurtTimer = 0f;
            health = HEALTH;

            _feet = new Sprite(PikeAndShotGame.BROWN_FEET, new Rectangle(4, 2, 16, 12), 26, 16, true, true, screen);
            _feet.flashable = false;
            _idle = new Sprite(PikeAndShotGame.COLMILLOS_IDLE, new Rectangle(20, 4, 16, 28), 54, 42, false, true, screen);
            _idle.flashable = false;
            _death = new Sprite(PikeAndShotGame.COLMILLOS_DEATH, new Rectangle(40, 2, 16, 28), 72, 40);
            _melee1 = new Sprite(PikeAndShotGame.BERZERKER2_MELEE1, new Rectangle(24, 30, 16, 28), 64, 68);
            _defend1 = new Sprite(PikeAndShotGame.BERZERKER2_DEFEND1, new Rectangle(20, 2, 16, 28), 52, 40);
            _route = new Sprite(PikeAndShotGame.BERZERKER2_ROUTE, new Rectangle(12, 10, 16, 28), 40, 46);
            _routed = new Sprite(PikeAndShotGame.BERZERKER2_ROUTED, new Rectangle(12, 10, 16, 28), 40, 46, true);
            _noshieldIdle = new Sprite(PikeAndShotGame.COLMILLOS_IDLENOSHIELD, new Rectangle(20, 4, 16, 28), 54, 42, false, true, screen);
            _noshieldIdle.flashable = false;
            _attack = new Sprite(PikeAndShotGame.COLMILLOS_ATTACK, new Rectangle(12, 14, 16, 28), 78, 52, false, true, screen);
            _attack.flashable = false;
            _noShieldAttack = new Sprite(PikeAndShotGame.COLMILLOS_ATTACK2, new Rectangle(22, 12, 16, 28), 98, 50, false, true, screen);
            _noShieldAttack.flashable = false;
            _noArmourAttack = new Sprite(PikeAndShotGame.COLMILLOS_ATTACK3, new Rectangle(14, 22, 16, 28), 114, 60, false, true, screen);
            _noArmourAttack.flashable = false;
            _noArmourIdle = new Sprite(PikeAndShotGame.COLMILLOS_IDLENOARMOUR, new Rectangle(20, 4, 16, 28), 54, 42, false, true, screen);
            _noArmourIdle.flashable = false;
            _shieldBreak = new Sprite(PikeAndShotGame.COLMILLOS_SHIELDBREAK, new Rectangle(24, 4, 16, 28), 60, 46,false, true, screen);
            _shieldBreak.flashable = false;
            _shieldFall = new Sprite(PikeAndShotGame.COLMILLOS_FALL, new Rectangle(76, 42, 16, 18), 110, 86,false,true, screen);
            _shieldFall.flashable = false;
            _armourFall = new Sprite(PikeAndShotGame.COLMILLOS_FALLNOSHIELD, new Rectangle(76, 42, 16, 18), 110, 86,false,true, screen);
            _armourFall.flashable = false;
            _rise = new Sprite(PikeAndShotGame.COLMILLOS_RISE, new Rectangle(40, 2, 16, 28), 72, 40,false, true, screen);
            _rise.flashable = false;
            _howl = new Sprite(PikeAndShotGame.COLMILLOS_HOWL, new Rectangle(12, 10, 16, 28), 50, 54,false,true, screen);
            _howl.flashable = false;
            _noShieldHowl = new Sprite(PikeAndShotGame.COLMILLOS_HOWL_NOSHIELD, new Rectangle(20, 12, 16, 28), 56, 44, false, true, screen);
            _noShieldHowl.flashable = false;
            _noArmourHowl = new Sprite(PikeAndShotGame.COLMILLOS_HOWL_NOARMOUR, new Rectangle(16, 22, 16, 28), 52, 58, false, true, screen);
            _noArmourHowl.flashable = false;
            _stagger = new Sprite(PikeAndShotGame.COLMILLOS_STAGGER, new Rectangle(10, 18, 16, 28), 46, 58, false, true, screen);
            _stagger.flashable = false;
            _body = _idle;
            _feet.setAnimationSpeed(_footSpeed / 0.11f);
            hitSound = chargeSound;

            hasArmour = true;
            _speed = 0.4f;
            _weaponSwing = new WeaponSwing(_screen, this);
            _screen.removeScreenObject(_weaponSwing);
            hurtSound = PikeAndShotGame.COLMILLOS_HURT.CreateInstance();
            yellSound = PikeAndShotGame.COLMILLOS_YELL.CreateInstance();
            slash = PikeAndShotGame.SLASH.CreateInstance();
        }

        public override void setSide(int side)
        {
            base.setSide(side);
            _weaponSwing.setSide(side);
        }

        protected override void shieldDone()
        {
            if (!_screen.findPikeTip(this, 0.30f))
            {
                _stateToHave = STATE_READY;
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
                _defendTimer = _attackTime;
            }
        }

        public override bool attack()
        {
            if (_state != STATE_DYING && _state != STATE_DEAD && _state != STATE_RISE && _state != STATE_EATEN && _state != STATE_ATTACK)
            {
                _state = STATE_ATTACK;
                _stateTimer = _attackTime;
                _weaponSwing.hit = false;
                return true;
            }

            return false;
        }

        public override void shieldBreak()
        {
            hurtSound.Play();
            _screen.removeScreenObject(_weaponSwing);
            health--;
            hurtTimer = 1000f;
            if (health > 0 && myFormation != null)
            {
                ((ColmillosFormation)myFormation).setAttacked();
            }
            else
            {
                health = HEALTH;

                if (_hasShield)
                {
                    if ((_state == STATE_MELEE_LOSS || _state == STATE_MELEE_WIN) && (_engager.getState() == STATE_MELEE_LOSS || _engager.getState() == STATE_MELEE_WIN))
                        _engager.setState(_engager.preAttackState);

                    _state = STATE_SHIELDBREAK;
                    _stateTimer = _shieldBreakTime;
                    new ScreenAnimation(_screen, _side, new Vector2(_position.X, _position.Y), new Sprite(PikeAndShotGame.SOLDIER_BROKENSHIELD1, new Rectangle(24, 4, 16, 28), 60, 46), (_shieldBreakTime / 8f) * 11f);
                    _idle = _noshieldIdle;
                    _attackTime = 1500f;
                    _attack = _noShieldAttack;
                    _howl = _noShieldHowl;
                    _hasShield = false;
                    _reacting = false;
                    _meleeDestination = _position;
                    //_chargeTime = 2000f;
                    shieldBreakSound.Play();
                }
                else if (hasArmour)
                {
                    _state = STATE_SHIELDBREAK;
                    _stateTimer = _shieldBreakTime;
                    new ScreenAnimation(_screen, _side, new Vector2(_position.X + (_side == BattleScreen.SIDE_PLAYER ? -8f : 8f), _position.Y), new Sprite(PikeAndShotGame.SOLDIER_BROKENARMOUR, new Rectangle(24, 4, 16, 28), 60, 46), (_shieldBreakTime / 8f) * 8f);
                    _idle = _noArmourIdle;
                    _shieldFall = _armourFall;
                    _attackTime = 2000f;
                    _attack = _noArmourAttack;
                    _howl = _noArmourHowl;
                    hasArmour = false;
                    _reacting = false;
                    _meleeDestination = _position;
                    //_chargeTime = 2000f;
                    shieldBreakSound.Play();
                }
                else
                {
                    _state = STATE_STAGGER;
                    _stateTimer = _staggerTime;
                    ((ColmillosFormation)myFormation).setEnd();
                }
            }
        }

        public override void draw(SpriteBatch spritebatch)
        {
            _drawingPosition = _position + randDestOffset - _screen.getMapOffset();

            if (_state == STATE_ATTACK && (_attack.getCurrFrame() == 20 || _attack.getCurrFrame() == 21))
            {
                addDrawjob(new DrawJob(_body, _drawingPosition + _jostleOffset, _state != STATE_ROUTED ? _side : _side * -1, _drawingY));
            }
            else if (_state == STATE_RISE || _state == STATE_DYING)
            {
                addDrawjob(new DrawJob(_body, _drawingPosition + _jostleOffset, _state != STATE_ROUTED ? _side : _side * -1, _drawingY));
            }
            else if (_state == STATE_EATEN)
            {
                if(_stateTimer >= _eatenTime/4f)
                    addDrawjob(new DrawJob(_body, _drawingPosition + _jostleOffset, _state != STATE_ROUTED ? _side : _side * -1, _drawingY, true, 100f));
            }
            else if (hurtTimer > 0f)
            {
                if(_state == STATE_READY || _state == STATE_STAGGER || _state == STATE_RUN || _state == STATE_COVER)
                    addDrawjob(new DrawJob(_feet, _drawingPosition + new Vector2(0, _idle.getBoundingRect().Height - 4), _state != STATE_RETREAT && _state != STATE_ROUTED ? _side : _side * -1, _drawingY, true, 100f, Color.Red));
                addDrawjob(new DrawJob(_body, _drawingPosition + _jostleOffset, _state != STATE_ROUTED ? _side : _side * -1, _drawingY, true, 100f, Color.Red));
            }
            else if (((ColmillosFormation)myFormation).attacked)
            {
                addDrawjob(new DrawJob(_feet, _drawingPosition + new Vector2(0, _idle.getBoundingRect().Height - 4), _state != STATE_RETREAT && _state != STATE_ROUTED ? _side : _side * -1, _drawingY,true,100f));
                addDrawjob(new DrawJob(_body, _drawingPosition + _jostleOffset, _state != STATE_ROUTED ? _side : _side * -1, _drawingY, true, 100f));
            }
            else
                base.draw(spritebatch);
        }

        protected override void updateState(TimeSpan timeSpan)
        {
            base.updateState(timeSpan);
            _weaponSwing.update(_position);

            if (!_stateChanged)
            {
                if (_state == STATE_ATTACK)
                {
                     _stateTimer -= (float)timeSpan.TotalMilliseconds;
                    if(_reacting)
                        ((Colmillos)this).myFormation._position.X = _position.X + 100f;

                    if (_stateTimer <= 0)
                    {
                        _stateTimer = 0;
                        _state = STATE_READY;
                        if (myFormation is ColmillosFormation)
                        {
                            ((ColmillosFormation)myFormation).setAttacked();
                            _reacting = false;
                        }
                        _screen.removeScreenObject(_weaponSwing);
                    }
                }
                else if (_state == STATE_RISE)
                {
                    _stateTimer -= (float)timeSpan.TotalMilliseconds;
                    if (_stateTimer <= 0)
                    {
                        _state = STATE_EATEN;
                        _stateTimer = _eatenTime;
                        _stateChanged = true;
                    }
                }
                else if (_state == STATE_EATEN)
                {
                    _stateTimer -= (float)timeSpan.TotalMilliseconds;
                    if (_stateTimer <= _eatenTime/4f && whiteWolf == null)
                    {
                        whiteWolf = new ColmillosWolf(_screen, new Vector2(_position.X + 10f, _position.Y), _side);
                        _screen.addLooseSoldierNext(whiteWolf);
                    }
                    else if (_stateTimer <= 0)
                    {
                        _state = STATE_DEAD;
                        _stateTimer = 0f;
                        _stateChanged = true;
                    }
                }
                else if (_state == STATE_HOWL)
                {
                    _stateTimer -= (float)timeSpan.TotalMilliseconds;
                    if (_stateTimer <= 0)
                    {
                        _state = STATE_READY;
                        _stateTimer = 0f;
                        _stateChanged = true;
                    }
                }
                else if (_state == STATE_RUN)
                {
                    _stateTimer -= (float)timeSpan.TotalMilliseconds;
                    if (_stateTimer <= 0)
                    {
                        _stateTimer = 0f;
                        _stateChanged = true;
                    }
                }
                else if (_state == STATE_STAGGER)
                {
                    _stateTimer -= (float)timeSpan.TotalMilliseconds;
                    if (_stateTimer <= 0)
                    {
                        _stateTimer = 0f;
                        hit();
                        _stateChanged = true;
                        _screen.addAnimation(new ThrownFalchion(_screen,_side,_position));
                    }
                }
            }
        }
        int prevFrame = 0;
        protected override void updateAnimation(TimeSpan timeSpan)
        {
            base.updateAnimation(timeSpan);

            if (_state == STATE_ATTACK || _state == STATE_DEFEND)
            {
                int maxFrames = _attack.getMaxFrames();
                float frameTime = _attackTime / (float)maxFrames;
                int frameNumber = maxFrames - (int)(_stateTimer / frameTime) - 1;

                if (_hasShield)
                {
                    if ((prevFrame == 8 && frameNumber == 9) || (prevFrame == 17 && frameNumber == 18))
                    {
                        slash.Play();
                        if (!_screen.getScreenObjects().Contains(_weaponSwing))
                        {
                            _screen.addScreenObject(_weaponSwing);
                            _weaponSwing.hit = false;
                        }
                    }
                }
                else if (hasArmour)
                {
                    if ((prevFrame == 8 && frameNumber == 9) || (prevFrame == 12 && frameNumber == 13) || (prevFrame == 5 && frameNumber == 6))
                    {
                        slash.Play();
                        if (!_screen.getScreenObjects().Contains(_weaponSwing))
                        {
                            _screen.addScreenObject(_weaponSwing);
                            _weaponSwing.hit = false;
                        }
                    }
                }
                else
                {
                    if ((prevFrame == 6 && frameNumber == 7) || (prevFrame == 12 && frameNumber == 13) || (prevFrame == 21 && frameNumber == 22))
                    {
                        slash.Play();
                        if (!_screen.getScreenObjects().Contains(_weaponSwing))
                        {
                            _screen.addScreenObject(_weaponSwing);
                            _weaponSwing.hit = false;
                        }
                    }
                }

                _attack.setFrame(frameNumber);
                _body = _attack;
                prevFrame = frameNumber;
            }
            else if (_state == STATE_RISE)
            {
                int maxFrames = _rise.getMaxFrames();
                float frameTime = _riseTime / (float)maxFrames;
                int frameNumber = maxFrames - (int)(_stateTimer / frameTime) - 1;

                _rise.setFrame(frameNumber);
                _body = _rise;
            }
            else if (_state == STATE_EATEN)
            {
                _rise.setFrame(_rise.getMaxFrames()-1);
                _body = _rise;
            }
            else if (_state == STATE_HOWL)
            {
                int maxFrames = _howl.getMaxFrames();
                float frameTime = _howlTime / (float)maxFrames;
                int frameNumber = maxFrames - (int)(_stateTimer / frameTime) - 1;

                _howl.setFrame(frameNumber);
                _body = _howl;
            }
            else if (_state == STATE_RUN)
            {
                int maxFrames = 4;
                float frameTime = _runTime / (float)maxFrames;
                int frameNumber = maxFrames - (int)(_stateTimer / frameTime) - 1;

                _attack.setFrame(frameNumber);
                _body = _attack;
            }
            else if (_state == STATE_READY)
            {
                _body = _idle;
            }
            else if (_state == STATE_STAGGER)
            {
                int maxFrames = 8;
                float frameTime = _staggerTime / (float)maxFrames;
                int frameNumber = maxFrames - (int)(_stateTimer / frameTime) - 1;

                switch (frameNumber)
                {
                    case 0:
                        frameNumber = 1;
                        break;
                    case 1:
                        frameNumber = 2;
                        break;
                    case 2:
                        frameNumber = 3;
                        break;
                    case 3:
                        frameNumber = 4;
                        break;
                    case 4:
                        frameNumber = 3;
                        break;
                    case 5:
                        frameNumber = 2;
                        break;
                    case 6:
                        frameNumber = 1;
                        break;
                    case 7:
                        frameNumber = 0;
                        break;
                }
                _stagger.setFrame(frameNumber);
                _body = _stagger;
            }
        }

        public void howl()
        {
            if (_state != STATE_DYING && _state != STATE_DEAD && _state != STATE_RISE && _state != STATE_EATEN)
            {
                _state = STATE_HOWL;
                _stateTimer = _howlTime;
                _reacting = false;
                yellSound.Play();
            }
        }

        internal void run()
        {
            if (_state != STATE_DYING && _state != STATE_DEAD && _state != STATE_RISE && _state != STATE_EATEN)
            {
                _state = STATE_RUN;
                _stateTimer = _runTime;
            }
        }
    }

    public class Wolf : Soldier
    {
        public const int STATE_BARK = 800;
        public const int STATE_TURNING = 801;
        public const int STATE_KILL = 802;
        public const int STATE_SPOOKED = 803;
        public const int STATE_FLEE = 804;
        public const int STATE_HOWLING = 805;
        public const int STATE_TUG = 806;
        
        protected Sprite _idleFeet;
        public Sprite _runningFeet;
        protected Sprite _attackFeet;
        protected Sprite _turnFeet;
        protected Sprite _killFeet;
        protected Sprite _howlFeet;
        protected Sprite _tug;
        protected Sprite _tug1;
        protected Sprite _tug2;

        float _idleTime;
        float _turnTime;
        float _killTime;
        float _tugTime;
        protected float _howlTime;
        protected float _idleAnimTime;
        public bool _turned;
        float turnOffset;
        public ColmillosFormation bossFormation;

        public bool flee;
        public bool retreat;
        private bool playAttackSound;
        private bool playHowlSound;

        private Pikeman pikeman;

        public Wolf(BattleScreen screen, float x, float y, int side)
            : base(screen, side, x, y)
        {
            randDestOffset = Vector2.Zero;
            _type = Soldier.TYPE_MELEE;
            _class = Soldier.CLASS_GOBLIN_WOLF;
            _idleTime = 3000f;
            _attackTime = 700f;
            _turnTime = 300f;
            _idleAnimTime = 1000f;
            _deathTime = 800f;
            _howlTime = 1000f;
            _turned = false;
            _killTime = 1500f;

            flee = false;
            retreat = false;

            if (PikeAndShotGame.random.Next(2) == 0)
            {
                _idleFeet = new Sprite(PikeAndShotGame.WOLF_IDLE, new Rectangle(16, 18, 14, 14), 48, 38, true);
                _death = new Sprite(PikeAndShotGame.WOLF_SPOOKED, new Rectangle(18, 16, 14, 14), 48, 38, true);
                _turnFeet = new Sprite(PikeAndShotGame.WOLF_TURN, new Rectangle(26, 10, 14, 14), 54, 26, true);
                _attackFeet = new Sprite(PikeAndShotGame.WOLF_BITE, new Rectangle(20, 8, 14, 14), 54, 26, true);
                _melee1 = new Sprite(PikeAndShotGame.WOLF_MELEE, new Rectangle(16, 10, 14, 14), 64, 24, true);
                _defend1 = new Sprite(PikeAndShotGame.WOLF_DEFEND, new Rectangle(16, 12, 14, 14), 64, 26, true);
                _killFeet = new Sprite(PikeAndShotGame.WOLF_KILL, new Rectangle(14, 12, 14, 14), 68, 28, true);
                _howlFeet = new Sprite(PikeAndShotGame.WOLF_HOWL, new Rectangle(18, 24, 14, 14), 52, 40, true);
                _feet = _runningFeet = new Sprite(PikeAndShotGame.WOLF_RUN, new Rectangle(16, 10, 14, 14), 44, 26, true);
                _tug = _tug1 = new Sprite(PikeAndShotGame.WOLF_TUG, new Rectangle(124, 34, 14, 14), 164, 50);
                _tug2 = new Sprite(PikeAndShotGame.WOLF2_TUG, new Rectangle(124, 34, 14, 14), 164, 50);
            }
            else
            {
                _idleFeet = new Sprite(PikeAndShotGame.WOLF_IDLEg, new Rectangle(16, 18, 14, 14), 48, 38, true);
                _death = new Sprite(PikeAndShotGame.WOLF_SPOOKEDg, new Rectangle(18, 16, 14, 14), 48, 38, true);
                _turnFeet = new Sprite(PikeAndShotGame.WOLF_TURNg, new Rectangle(26, 10, 14, 14), 54, 26, true);
                _attackFeet = new Sprite(PikeAndShotGame.WOLF_BITEg, new Rectangle(20, 8, 14, 14), 54, 26, true);
                _melee1 = new Sprite(PikeAndShotGame.WOLF_MELEEg, new Rectangle(16, 10, 14, 14), 64, 24, true);
                _defend1 = new Sprite(PikeAndShotGame.WOLF_DEFENDg, new Rectangle(16, 12, 14, 14), 64, 26, true);
                _killFeet = new Sprite(PikeAndShotGame.WOLF_KILLg, new Rectangle(14, 12, 14, 14), 68, 28, true);
                _howlFeet = new Sprite(PikeAndShotGame.WOLF_HOWLg, new Rectangle(18, 24, 14, 14), 52, 40, true);
                _feet = _runningFeet = new Sprite(PikeAndShotGame.WOLF_RUNg, new Rectangle(16, 10, 14, 14), 44, 26, true);
                _tug = _tug1 = new Sprite(PikeAndShotGame.WOLF_TUGg, new Rectangle(124, 34, 14, 14), 164, 50);
                _tug2 = new Sprite(PikeAndShotGame.WOLF2_TUGg, new Rectangle(124, 34, 14, 14), 164, 50);
            }

            _howlFeet.setMaxFrames(_howlFeet.getMaxFrames() - 1);
            _body = _idle;
            _footSpeed = 6.5f;
            _speed = 0.24f;
            _feet.setAnimationSpeed(_footSpeed / (_speed - 0.13f));
            _stateTimer = PikeAndShotGame.random.Next((int)_idleTime);
        }

        public override int getWidth()
        {
            return _idleFeet.getBoundingRect().Width;
        }

        public override int getHeight()
        {
            return _idleFeet.getBoundingRect().Height;
        }

        public override void hit()
        {
            if ((_state == STATE_MELEE_LOSS || _state == STATE_MELEE_WIN) && (_engager.getState() == STATE_MELEE_LOSS || _engager.getState() == STATE_MELEE_WIN))
            {
                _engager.setState(_engager.preAttackState);
                _engager._stateTimer = 0f;
            }

            if (_state == STATE_TUG && pikeman.getState() == Pikeman.STATE_TUG)
                pikeman.untug();

            _state = STATE_SPOOKED;
            _stateTimer = _deathTime;
            _destination = _position;
            hitSound.Play();
        }

        protected override void engage(bool win, Vector2 position, Soldier engager, bool rescueFight)
        {
            if (_state == STATE_TUG && pikeman.getState() == Pikeman.STATE_TUG)
                pikeman.untug();

            base.engage(win, position, engager, rescueFight);
        }

        public override void update(TimeSpan timeSpan)
        {
            base.update(timeSpan);
            _drawingY += 2;

            if (bossFormation != null)
            {
                if (!bossFormation.getSoldiers().Contains(this))
                {
                    if (_position.X < (_screen.getMapOffset().X - 50) || _position.X > (_screen.getMapOffset().X + PikeAndShotGame.SCREENWIDTH + 200))
                    {
                        _state = STATE_DEAD;
                        setSpeed(0.24f);
                    }
                }
            }
            if (_state == STATE_TUG && pikeman.getState() != Pikeman.STATE_TUG)
            {
                pikeman = null;
                fleeStart();
            }

        }

        internal override void setSpeed(float p)
        {
            _speed = p;
            _feet.setAnimationSpeed(_footSpeed / (_speed - 0.13f));
        }

        protected override void winMelee()
        {
            base.winMelee();
            _state = STATE_KILL;
            _stateTimer = _killTime;
        }

        public void kill()
        {
            _state = STATE_KILL;
            _stateTimer = _killTime;
        }

        protected override bool checkReactions(TimeSpan timeSpan)
        {
            return false;
        }

        public virtual void bark()
        {
            if (_state == STATE_READY)
            {
                _state = STATE_BARK;
                _stateTimer = _attackTime;
                playAttackSound = true;
            }
        }

        public override void alterDestination(bool changeX, float amount)
        {
            if (_state != STATE_DYING && _state != STATE_DEAD && _state != STATE_SPOOKED && _state != STATE_FLEE)
            {
                if (changeX)
                    _destination.X += amount;
                else
                    _destination.Y += amount;
            }
        }

        public virtual void howl()
        {
            _state = STATE_HOWLING;
            _stateTimer = _howlTime;
            _meleeDestination = _position;
            playHowlSound = true;
        }

        public bool turn()
        {
            if (_state != STATE_HOWLING)
            {
                _state = STATE_TURNING;
                _stateTimer = _turnTime;
            }
            return true;
        }

        internal void retreatStart()
        {
            _reacting = false;
            if (!_turned)
            {
                retreat = true;
                turn();
            }
            else
            {
                _state = STATE_FLEE;
            }
        }

        internal void fleeStart()
        {
            _reacting = false;
            if (!_turned)
            {
                flee = true;
                turn();
            }
            else
            {
                _state = STATE_READY;
                alterDestination(true, _screen.getMapOffset().X + PikeAndShotGame.SCREENWIDTH + 999999 - _destination.X);
                _state = STATE_FLEE;
                _screen.addLooseSoldier(this);
                if (bossFormation != null && bossFormation.getSoldiers().Contains(this))
                {
                    bossFormation.reduceWolfNumber();
                    bossFormation.removeSoldier(this);
                    _screen.addLooseSoldier(this);
                }
            }
        }

        public void turnDone()
        {
            _turned = !_turned;
            
            if (flee)
            {
                flee = false;
                fleeStart();
            }
            else if (retreat)
            {
                retreat = false;
                retreatStart();
            }
            else
            {
                _state = STATE_READY;
            }

            //doing a sneaky XOR
            /*
            if (_turned != (_side == BattleScreen.SIDE_ENEMY))
            {
                _position.X -= 12f;
                myFormation._position.X -= 12f;
                alterDestination(true, -12);
            }
            else
            {
                _position.X += 12f;
                myFormation._position.X += 12f;
                alterDestination(true, 12);
            }
            */
        }

        public override void draw(SpriteBatch spritebatch)
        {
            turnOffset = 0;
            if (_state == STATE_TURNING)
            {
                if (_turned != (_side == BattleScreen.SIDE_ENEMY))
                {
                    turnOffset = -12 * ((_turnTime - _stateTimer) / _turnTime);
                }
                else
                {
                    turnOffset = 12 * ((_turnTime - _stateTimer) / _turnTime);
                }
            }
            _drawingPosition = _position + randDestOffset - _screen.getMapOffset() + new Vector2(turnOffset,0);

            if ((_state == STATE_FLEE || flee == true || retreat == true ) && !(this is ColmillosWolf))
                addDrawjob(new DrawJob(_feet, _drawingPosition, (_state != STATE_RETREAT && _state != STATE_ROUTED && !_turned && !killOrientRight) || (_state == STATE_MELEE_WIN || _state == STATE_MELEE_LOSS || (_state == STATE_KILL && !killOrientRight)) ? _side : _side * -1, _drawingY, true, 100f));
            else if (_state != STATE_DEAD)
            {
                addDrawjob(new DrawJob(_feet, _drawingPosition, (_state != STATE_RETREAT && _state != STATE_ROUTED && _state != STATE_TUG && !_turned && !killOrientRight) || (_state == STATE_MELEE_WIN || _state == STATE_MELEE_LOSS || (_state == STATE_KILL && !killOrientRight)) ? _side : _side * -1, _drawingY));
            }

            if (_wading.getPlaying())
                addDrawjob(new DrawJob(_wading, _drawingPosition + new Vector2(0, _idleFeet.getBoundingRect().Height - 4), _state != STATE_RETREAT && _state != STATE_ROUTED ? _side : _side * -1, _drawingY));

            if (_screen.getDrawDots())
            {
                spritebatch.Draw(PikeAndShotGame.getDotTexture(),
                        new Rectangle(
                            (int)((_position.X) - _screen.getMapOffset().X),
                            (int)((_position.Y) - _screen.getMapOffset().Y),
                            (int)(getWidth()),
                            2),
                        Color.White);
                spritebatch.Draw(PikeAndShotGame.getDotTexture(),
                    new Rectangle(
                        (int)(_position.X - _screen.getMapOffset().X),
                        (int)(_position.Y - _screen.getMapOffset().Y),
                        (int)(2),
                        getHeight()),
                    Color.White);
                spritebatch.Draw(PikeAndShotGame.getDotTexture(),
                    new Rectangle(
                        (int)(_position.X - _screen.getMapOffset().X),
                        (int)(_position.Y + getHeight() - _screen.getMapOffset().Y),
                        getWidth(),
                        2),
                    Color.White);
                spritebatch.Draw(PikeAndShotGame.getDotTexture(),
                    new Rectangle(
                        (int)(_position.X + getWidth() - _screen.getMapOffset().X),
                        (int)(_position.Y - _screen.getMapOffset().Y),
                        2,
                        (int)(getHeight())),
                    Color.White);
            }
            //addDrawjob(new DrawJob(_body, _drawingPosition + _jostleOffset, _state != STATE_ROUTED ? _side : _side * -1, _drawingY));
            //spritebatch.Draw(PikeAndShotGame.getDotTexture(), _drawingPosition, Color.White);
        }

        protected override void updateState(TimeSpan timeSpan)
        {
            base.updateState(timeSpan);

            if (!_stateChanged)
            {
                if (_state == STATE_BARK)
                {
                    _stateTimer -= (float)timeSpan.TotalMilliseconds;

                    if (_stateTime < (_attackTime / 2f) && playAttackSound)
                    {
                        chargeSound.Play();
                        playAttackSound = false;
                    }

                    if (_stateTimer <= 0)
                    {
                        _stateTimer = _idleTime;
                        _state = STATE_READY;
                    }
                }
                else if (_state == STATE_TURNING)
                {
                    _stateTimer -= (float)timeSpan.TotalMilliseconds;

                    if (_stateTimer <= 0)
                    {
                        _stateTimer = _idleTime;
                        turnDone();
                    }
                }
                else if (_state == STATE_KILL)
                {
                    _stateTimer -= (float)timeSpan.TotalMilliseconds;

                    if (_stateTimer <= 0)
                    {
                        _stateTimer = _idleTime;
                        fleeStart();
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
                else if (_state == STATE_SPOOKED)
                {
                    _stateTimer -= (float)timeSpan.TotalMilliseconds;

                    if (_stateTimer <= 0)
                    {
                        _stateTimer = _idleTime;
                        fleeStart();
                    }
                }
                else if (_state == STATE_FLEE)
                {
                    _stateTimer -= (float)timeSpan.TotalMilliseconds;

                    if (_stateTimer <= 0)
                    {
                        _stateTimer = _idleTime;
                    }
                }
                else if (_state == STATE_TUG)
                {
                    _stateTimer -= (float)timeSpan.TotalMilliseconds;

                    if (_stateTimer <= 0)
                    {
                        _stateTimer = _tugTime;
                    }
                }
                else if (_state == STATE_HOWLING)
                {
                    _stateTimer -= (float)timeSpan.TotalMilliseconds;

                    if (_stateTimer <= 0)
                    {
                        _stateTimer = _idleTime;
                        _state = STATE_READY;
                        if (bossFormation != null && bossFormation.getState() == ColmillosFormation.STATE_END)
                        {
                            _destination = bossFormation.colmillos.getPosition() + new Vector2(45f, PikeAndShotGame.getRandPlusMinus(PikeAndShotGame.random.Next(10)));
                            _meleeDestination = _destination;
                            if (_meleeDestination.X > _position.X)
                            {
                                killOrientRight = true;
                                _destination = bossFormation.colmillos.getPosition() - new Vector2(15f, PikeAndShotGame.getRandPlusMinus(PikeAndShotGame.random.Next(10)));
                                _meleeDestination = _destination;
                            }
                        }
                    }
                }
            }
        }

        bool killOrientRight = false;

        public void attackColmillos()
        {
            _destination = bossFormation.colmillos.getPosition() + new Vector2(50f, PikeAndShotGame.getRandPlusMinus(PikeAndShotGame.random.Next(30)));
            _meleeDestination = _destination;
            if (_meleeDestination.X > _position.X)
            {
                killOrientRight = true;
                _destination = bossFormation.colmillos.getPosition() - new Vector2(20f, PikeAndShotGame.getRandPlusMinus(PikeAndShotGame.random.Next(30)));
                _meleeDestination = _destination;
            }
        }

        protected override void updateAnimation(TimeSpan timeSpan)
        {
            base.updateAnimation(timeSpan);

            if (_state == STATE_BARK)
            {
                int maxFrames = _attackFeet.getMaxFrames();
                float frameTime = _attackTime / (float)maxFrames;
                int frameNumber = maxFrames - (int)(_stateTimer / frameTime) - 1;

                _attackFeet.setFrame(frameNumber);
            }
            else if (_state == STATE_TURNING)
            {
                int maxFrames = _turnFeet.getMaxFrames();
                float frameTime = _turnTime / (float)maxFrames;
                int frameNumber = maxFrames - (int)(_stateTimer / frameTime) - 1;

                _turnFeet.setFrame(frameNumber);
            }
            else if (_state == STATE_KILL)
            {
                int maxFrames = _killFeet.getMaxFrames();
                float frameTime = _killTime / (float)maxFrames;
                int frameNumber = maxFrames - (int)(_stateTimer / frameTime) - 1;

                _killFeet.setFrame(frameNumber);
            }
            else if (_state == STATE_TUG)
            {
                int maxFrames = _tug.getMaxFrames();
                float frameTime = _tugTime / (float)maxFrames;
                int frameNumber = maxFrames - (int)(_stateTimer / frameTime) - 1;

                _tug.setFrame(frameNumber);
            }
            else if (_state == STATE_READY && _delta.Length() == 0)
            {
                if (_stateTimer <= _idleAnimTime)
                {
                    int maxFrames = _idleFeet.getMaxFrames();
                    float frameTime = _idleAnimTime / (float)maxFrames;
                    int frameNumber = maxFrames - (int)(_stateTimer / frameTime) - 1;

                    _idleFeet.setFrame(frameNumber);
                }
                else
                    _idleFeet.setFrame(0);
            }
            else if (_state == STATE_SPOOKED)
            {
                int maxFrames = _death.getMaxFrames();
                float frameTime = _deathTime / (float)maxFrames;
                int frameNumber = maxFrames - (int)(_stateTimer / frameTime) - 1;

                _death.setFrame(frameNumber);
            }
            else if (_state == STATE_HOWLING)
            {
                int maxFrames = _howlFeet.getMaxFrames();
                float frameTime = _howlTime / (float)maxFrames;
                int frameNumber = maxFrames - (int)(_stateTimer / frameTime) - 1;

                if (frameNumber == 8 && playHowlSound)
                {
                    if(this is ColmillosWolf)
                        ((ColmillosWolf)this).yellSound.Play();
                    else 
                        chargeSound.Play();
                }

                _howlFeet.setFrame(frameNumber);
            }

            if (_state == STATE_BARK)
                _feet = _attackFeet;
            else if (_state == STATE_TUG)
                _feet = _tug;
            else if (_state == STATE_TURNING)
                _feet = _turnFeet;
            else if (_state == STATE_DYING || _state == STATE_SPOOKED)
                _feet = _death;
            else if (_state == STATE_KILL)
                _feet = _killFeet;
            else if (_state == STATE_MELEE_WIN || _state == STATE_MELEE_LOSS /*|| _state == STATE_DEFEND*/)
                _feet = _body;
            else if (_state == STATE_HOWLING)
                _feet = _howlFeet;
            else if (_delta.Length() != 0)
                _feet = _runningFeet;
            else
                _feet = _idleFeet;
        }

        internal void tug(Pikeman pikeman)
        {
            if ((_state == STATE_MELEE_LOSS || _state == STATE_MELEE_WIN) && (_engager.getState() == STATE_MELEE_LOSS || _engager.getState() == STATE_MELEE_WIN))
            {
                _engager.setState(_engager.preAttackState);
            }

            this.pikeman = pikeman;
            _state = STATE_TUG;
            _stateTimer = _tugTime = pikeman.tugTime;
            _meleeDestination = pikeman.getPosition() + pikeman.randDestOffset + new Vector2(86, 22);
            if (pikeman.variant)
                _tug = _tug2;
            else
                _tug = _tug1;
        }
    }

    public struct WolfSchedule
    {
        public int state;
        public float time;
    }

    public class ColmillosWolf : Wolf
    {
        public const float SIT_TIME = 4000f;
        public const int STATE_SIT = 700;
        public const int STATE_GETUP = 701;
        WolfSchedule[] schedule;
        int curSchedule = 0;
        float scheduleTimer = 0;

        Sprite _getUp;

        float getUpTime = 500f;

        public SoundEffectInstance yellSound;

        public ColmillosWolf(BattleScreen screen, Vector2 position, int side)
            :base(screen, position.X, position.Y, side)
        {
            _getUp = new Sprite(PikeAndShotGame.WOLF_GETUP_COL, new Rectangle(24, 14, 14, 14), 52, 34, false);
            _idleFeet = new Sprite(PikeAndShotGame.WOLF_IDLE_COL, new Rectangle(16, 18, 14, 14), 48, 38, true);
            _turnFeet = new Sprite(PikeAndShotGame.WOLF_TURN_COL, new Rectangle(26, 10, 14, 14), 54, 26, true);
            _attackFeet = new Sprite(PikeAndShotGame.WOLF_ATTACK_COL, new Rectangle(20, 8, 14, 14), 54, 26, true);
            _howlFeet = new Sprite(PikeAndShotGame.WOLF_HOWL_COL, new Rectangle(20, 24, 14, 14), 54, 40, true);
            _runningFeet = new Sprite(PikeAndShotGame.WOLF_RUN_COL, new Rectangle(16, 10, 14, 14), 44, 26, true);
            _howlFeet.setMaxFrames(_howlFeet.getMaxFrames() - 1);
            _footSpeed = 6.5f;
            _speed = 0.24f;
            _runningFeet.setAnimationSpeed(_footSpeed / 0.11f);

            schedule = new WolfSchedule[6];

            for (int i = 0 ; i < schedule.Length; i++)
                schedule[0] = new WolfSchedule();

            _howlTime = 1800f;

            schedule[0].state = STATE_SIT;
            schedule[0].time = SIT_TIME;
            schedule[1].state = STATE_GETUP;
            schedule[1].time = 2500f;
            schedule[2].state = STATE_BARK;
            schedule[2].time = _attackTime + 200f;
            schedule[3].state = STATE_BARK;
            schedule[3].time = _attackTime + 1000f;
            schedule[4].state = STATE_HOWLING;
            schedule[4].time = _howlTime;
            schedule[5].state = STATE_FLEE;
            schedule[5].time = 1000f;

            _state = STATE_SIT;

            yellSound = PikeAndShotGame.COLMILLOS_YELL.CreateInstance();
        }

        public override void update(TimeSpan timeSpan)
        {
            base.update(timeSpan);
            if (curSchedule < schedule.Length)
            {
                scheduleTimer -= (float)timeSpan.TotalMilliseconds;
                if (scheduleTimer <= 0)
                {
                    switch (schedule[curSchedule].state)
                    {
                        case STATE_SIT:
                            sit();
                            break;
                        case STATE_GETUP:
                            getUp();
                            break;
                        case STATE_BARK:
                            attack();
                            break;
                        case STATE_HOWLING:
                            howl();
                            break;
                        case STATE_FLEE:
                            fleeStart();
                            break;
                    }
                    scheduleTimer = schedule[curSchedule].time;
                    curSchedule++;
                }
            }
        }

        private void getUp()
        {
            _state = STATE_GETUP;
            _stateTimer = getUpTime;
        }

        private void sit()
        {
            _state = STATE_SIT;
            _getUp.setEffect(Sprite.EFFECT_FADEIN, SIT_TIME);
        }

        protected override void updateAnimation(TimeSpan timeSpan)
        {
            base.updateAnimation(timeSpan);

            if (_state == STATE_READY && _delta.Length() == 0)
            {
                _idleFeet.setFrame(0);
            }
            else if (_state == STATE_SIT)
            {
                _getUp.setFrame(0);
                _feet = _getUp;
            }
            else if (_state == STATE_GETUP)
            {
                int maxFrames = _getUp.getMaxFrames()-1;
                float frameTime = getUpTime / (float)maxFrames;
                int frameNumber = maxFrames - (int)(_stateTimer / frameTime) - 1;

                _getUp.setFrame(frameNumber);
                _feet = _getUp;
            }
        }

        protected override void updateState(TimeSpan timeSpan)
        {
            base.updateState(timeSpan);

            if (!_stateChanged)
            {
                if (_state == STATE_GETUP)
                {
                    _stateTimer -= (float)timeSpan.TotalMilliseconds;
                    if (_stateTimer <= 0)
                    {
                        _stateTimer = 0;
                        _state = STATE_READY;
                    }
                }
                else if (_state == STATE_SIT)
                {
                    if (_stateTimer > 0)
                    {
                        _stateTimer -= (float)timeSpan.TotalMilliseconds;
                        if (_stateTimer <= 0)
                            _stateTimer -= 0;
                    }
                }
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
        public ShieldBlock _shieldBlock;
        protected Sprite _shieldBreak;
        protected Sprite _noshieldIdle;
        protected Sprite _shieldFall;
        protected Sprite _melee2;
        public Sprite _defend2;
        public Sprite _chargeNoShield;
        public bool _hasShield;

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
            givesRescueReward = true;

            if (PikeAndShotGame.random.Next(2) == 0)
            {
                _shieldBlock = new ShieldBlock(screen, this);
                _shieldBreak = new Sprite(PikeAndShotGame.SOLDIER_SHIELDBREAK, new Rectangle(24, 4, 16, 28), 60, 46);
                _noshieldIdle = new Sprite(PikeAndShotGame.SOLDIER_IDLENOSHIELD, new Rectangle(10, 2, 16, 28), 46, 42);
                _shieldFall = new Sprite(PikeAndShotGame.SOLDIER_FALL, new Rectangle(76, 42, 16, 18), 110, 86);
                _melee2 = new Sprite(PikeAndShotGame.SOLDIER_MELEE2, new Rectangle(24, 30, 16, 28), 64, 68);
                _defend2 = new Sprite(PikeAndShotGame.SOLDIER_DEFEND2, new Rectangle(20, 2, 16, 28), 52, 40);
                _chargeNoShield = new Sprite(PikeAndShotGame.SOLDIER_CHARGENOSHIELD, new Rectangle(20, 20, 16, 28), 60, 56);
                _feet = new Sprite(PikeAndShotGame.BLUE_FEET, new Rectangle(4, 2, 16, 12), 26, 16, true);
                _hasShield = true;
            }
            else
            {
                _shieldBlock = new ShieldBlock(screen, this);
                _shieldBreak = new Sprite(PikeAndShotGame.SOLDIER_SHIELDBREAK, new Rectangle(24, 4, 16, 28), 60, 46);
                _idle = _noshieldIdle = new Sprite(PikeAndShotGame.SOLDIER2_IDLE, new Rectangle(12, 16, 16, 28), 64, 46);
                _shieldFall = new Sprite(PikeAndShotGame.SOLDIER_FALL, new Rectangle(76, 42, 16, 18), 110, 86);
                _melee2 = new Sprite(PikeAndShotGame.SOLDIER2_MELEE2, new Rectangle(16, 30, 16, 28), 78, 60);
                _defend2 = new Sprite(PikeAndShotGame.SOLDIER2_DEFEND2, new Rectangle(12, 16, 16, 28), 64, 46);
                _chargeNoShield = new Sprite(PikeAndShotGame.SOLDIER_CHARGENOSHIELD, new Rectangle(20, 20, 16, 28), 60, 56);
                _feet = new Sprite(PikeAndShotGame.BLUE_FEET, new Rectangle(4, 2, 16, 12), 26, 16, true);
                _death = new Sprite(PikeAndShotGame.SOLDIER2_DEATH, new Rectangle(40, 2, 16, 28), 72, 40);
                _routed = new Sprite(PikeAndShotGame.SOLDIER2_ROUTED, new Rectangle(4, 2, 16, 28), 30, 34);
                _hasShield = false;
                _body = _noshieldIdle;
            }

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
            if (_hasShield || this is Colmillos)
            {
                if (_screen.findPikeTip(this, 0.30f) && (!(this is Colmillos) || !((ColmillosFormation)((Colmillos)this).myFormation).attacked))
                {
                    if (!_reacting)
                    {
                        shield();
                    }
                    _defendTimer = (this is Colmillos) ? _attackTime : _meleeTime * 2f / 3f;
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
                    if(_state == STATE_COVER)
                        coverDone();
                    if (_state == Colmillos.STATE_ATTACK)
                    {
                        _meleeDestination = _screen.getPlayerFormation().getCenter();
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

                        return false;
                    }
                    else if (_state == Colmillos.STATE_RUN)
                    {
                        _reacting = false;
                    }
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
            if (!_hasShield && !(this is Colmillos) )
                return;
            else if (_state != STATE_DEFEND && _state != Colmillos.STATE_ATTACK)
            {
                if ((_state == STATE_MELEE_LOSS || _state == STATE_MELEE_WIN) && (_engager.getState() == STATE_MELEE_LOSS || _engager.getState() == STATE_MELEE_WIN))
                    _engager.setState(_engager.preAttackState);

                _defendTimer = (this is Colmillos) ? 0 : _meleeTime * 2f / 3f;
                preAttackState = _state != STATE_MELEE_WIN && _state != STATE_MELEE_LOSS && _state != STATE_DEFEND ? _state : STATE_READY;
                _reacting = true;
                _state = (this is Colmillos) ? Colmillos.STATE_ATTACK : STATE_DEFEND;
                _stateTimer = (this is Colmillos) ? _attackTime : _meleeTime * 2f / 3f;
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

        public virtual void shieldBreak()
        {
            if (_hasShield)
            {
                if ((_state == STATE_MELEE_LOSS || _state == STATE_MELEE_WIN) && (_engager.getState() == STATE_MELEE_LOSS || _engager.getState() == STATE_MELEE_WIN))
                    _engager.setState(_engager.preAttackState);

                _state = STATE_SHIELDBREAK;
                _stateTimer = _shieldBreakTime;
                new ScreenAnimation(_screen, _side, new Vector2(_drawingPosition.X, _drawingPosition.Y), new Sprite(PikeAndShotGame.SOLDIER_BROKENSHIELD1, new Rectangle(24, 4, 16, 28), 60, 46), (_shieldBreakTime / 8f) * 11f);
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

        public override void hit()
        {
            base.hit();
            _screen.removeScreenObject(_shieldBlock);
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
                        if (myFormation is ColmillosFormation)
                        {
                            ((ColmillosFormation)myFormation).setAttacked();
                            _reacting = false;
                        }
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

        protected virtual void shieldDone()
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
                _defendTimer = (this is Colmillos) ? _attackTime : _meleeTime * 2f / 3f;
            }
        }

        public void resetDefendTimer()
        {
            _defendTimer = (this is Colmillos) ? _attackTime : _meleeTime * 2f / 3f;
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
            _drawingPosition = _position + randDestOffset - _screen.getMapOffset();
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

        public override void hit()
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

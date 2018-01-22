#define DRAW_FORMATION_DOTS

using System;
using System.Collections.Generic;
using System.Collections;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PikeAndShot
{
    public class Formation
    {
        public const int STATE_MELEE = 0;
        public const int STATE_PIKE = 1;
        public const int STATE_SHOT = 2;

        public BattleScreen _screen;
        public Vector2 _position;
        protected ArrayList _soldiers;
        private ArrayList _soldiersToRemove;
        private ArrayList _soldiersToMove;
        private ArrayList _shotRows;
        private ArrayList _pikeRows;
        private ArrayList _meleeRows;
        //private ArrayList _swingerRows;
        private ArrayList _cavRows;
        public ArrayList _supportRows;
        private ArrayList _enemiesToGuard;
        private ArrayList _enemiesToShoot;
        private ArrayList _shotsToBlock;
        public int collisions;

        private Vector2 _size;
        private int _side;
        private int _width;
        protected float _speed;
        private float avgSpeed;
        protected int _state;
        public bool needTriggerUp;
        private bool _soldierDied;
        private bool _allShotsMade;
        private bool _needResetupFormation;
        private bool _addedSoldier;
        private bool DEBUGdangerClose;

        public bool selected { get; set; }
        public int numberOfPikes;
        public int numberOfShots;
        public bool retreated;
        public bool hasAppeared;

        public Formation(BattleScreen screen, float x, float y, int initialCapacity, int side)
        {
            _screen = screen;
            hasAppeared = false;
            _position = new Vector2(x, y);
            _size = Vector2.Zero;
            _soldiers = new ArrayList(initialCapacity);
            _soldiersToRemove = new ArrayList(initialCapacity);
            _soldiersToMove = new ArrayList(initialCapacity);
            _width = 5;
            _shotRows = new ArrayList(5);
            _pikeRows = new ArrayList(5);
            _meleeRows = new ArrayList(5);
 //           _swingerRows = new ArrayList(5);
            _cavRows = new ArrayList(5);
            _supportRows = new ArrayList(2);
            _enemiesToGuard = new ArrayList(20);
            _enemiesToShoot = new ArrayList(20);
            _shotsToBlock = new ArrayList(20);
            collisions = 0;
            if (_side == BattleScreen.SIDE_PLAYER)
                _speed = 0.1f;
                //_speed = 0.2f;

            _state = STATE_SHOT;
            
            _side = side;
            needTriggerUp = false;
            _needResetupFormation = false;
            _addedSoldier = false;
            DEBUGdangerClose = false;
            selected = false;
            retreated = false;
            numberOfPikes = 0;
            numberOfShots = 0;
        }

        public int getWidth()
        {
            return _width;
        }
        
        public int getSide()
        {
            return _side;
        }

        public void kill()
        {
            foreach (Soldier s in _soldiers)
            {
                if(s is Pikeman)
                    s.hit();
            }
        }

        public virtual void update(TimeSpan timeSpan)
        {
            if (this.getSoldiers().Count <= 0)
                return;

            if (!hasAppeared && _position.X < _screen.getMapOffset().X + PikeAndShotGame.SCREENWIDTH - Soldier.WIDTH * 4)
                hasAppeared = true;

            _soldierDied = false;
            _allShotsMade = true;
            bool isDangerClose = dangerClose();
            bool answer = false;

            //dangerBefore();

            foreach (Soldier pike in _soldiers)
            {
                if (pike.guardTarget != null)
                {
                    if (pike is Pikeman)
                        answer = Math.Abs(pike.guardTarget.getPosition().Y - pike._destination.Y) >= pike.getHeight();
                    else
                        answer = false;

                    if (pike.guardTarget.getState() == Soldier.STATE_DYING || pike.guardTarget.getState() == Soldier.STATE_DEAD
                    //|| pike.guardTarget.getState() == Soldier.STATE_MELEE_WIN || pike.guardTarget.getState() == Soldier.STATE_MELEE_LOSS
                    || pike.guardTarget.getState() == Soldier.STATE_ROUTE || pike.guardTarget.getState() == Soldier.STATE_ROUTED
                    || pike.guardTarget.getState() == Soldier.STATE_RETREAT
                    || Math.Abs(pike.guardTarget.getPosition().X - this._position.X) > pike.breakRange
                    || (answer))
                    {
                        pike.guardTarget = null;
                        pike.setSpeed(0.15f);
                    }
                }
            }


            // check to see if pikemen need to getready
            if (_pikeRows.Count != 0)
            {
                if (_state != STATE_PIKE)
                {
                    if (isDangerClose)
                    {
                        foreach (Soldier pikeman in (ArrayList)_pikeRows[0])
                        {
                            if (pikeman is Pikeman && !_soldiersToRemove.Contains(pikeman) && (pikeman.getState() != Soldier.STATE_MELEE_WIN && pikeman.getState() != Soldier.STATE_MELEE_LOSS))
                                ((Pikeman)pikeman).lower45();
                        }
                        DEBUGdangerClose = true;
                    }
                    else
                    {
                        foreach (Soldier pikeman in (ArrayList)_pikeRows[0])
                        {
                            if (pikeman is Pikeman && !_soldiersToRemove.Contains(pikeman) && (pikeman.getState() != Soldier.STATE_MELEE_WIN && pikeman.getState() != Soldier.STATE_MELEE_LOSS))
                                ((Pikeman)pikeman).raise();
                        }
                        DEBUGdangerClose = false;
                    }
                }
                // position pike men to block attackers when in STATE_PIKE
                else
                {
                    if (isDangerClose)
                    {
                        foreach (Soldier enemy in _enemiesToGuard)
                        {
                            assignGuard(enemy);
                        }
                    }
                    else
                    {
                        foreach (Pikeman pike in (ArrayList)_pikeRows[0])
                        {
                            pike.guardTarget = null;
                            pike.setSpeed(0.15f);
                        }
                        if (_pikeRows.Count > 1)
                        {
                            foreach (Pikeman pike in (ArrayList)_pikeRows[1])
                            {
                                pike.guardTarget = null;
                                pike.setSpeed(0.15f);
                            }
                        }
                    }
                }
            }

            if (_shotRows.Count > 0)
            {
                int forwardShotRow = getForwardShotRow();
                if (_shotRows.Count > 1 && forwardShotRow != -1)
                {
                    ArrayList firstShotRow = (ArrayList)_shotRows[forwardShotRow];

                    if (_state == STATE_SHOT && !_needResetupFormation)
                    {
                        foreach (Soldier s in firstShotRow)
                        {
                            if (s.getState() == Soldier.STATE_RELOADING)
                                _soldiersToMove.Add(s);
                        }
                        foreach (Soldier s in _soldiersToMove)
                        {
                            if(((ArrayList)_shotRows[0]).IndexOf(s) != -1)
                                replaceWithReadyShooter(s, ((ArrayList)_shotRows[0]).IndexOf(s));
                        }

                        _soldiersToMove.Clear();

                        //foreach (Soldier enemy in _enemiesToShoot)
                        //{
                        //    assignShooter(enemy);
                        //}
                    }
                    else
                    {
                        //for (int i = _width - firstShotRow.Count; i > 0; i--)
                        //{
                        //    moveUpShooter();
                        //}
                    }
                }
                foreach (Shot shot in _shotsToBlock)
                {
                    assignShielder(shot);
                }
            
            }

            foreach (Soldier s in _soldiers)
            {
                s.update(timeSpan);
                if (s.isDead() || s.getState() == Soldier.STATE_DYING || s.getState() == Soldier.STATE_ROUTE || s.getState() == Soldier.STATE_ROUTED || (s.getState() == Soldier.STATE_CHARGING && s.initCharge && !(s is DoppelSoldier)))
                {
                    _soldiersToRemove.Add(s);
                    _soldierDied = true;
                }
            }

            if (_shotRows.Count > 0 && _needResetupFormation)
            {
                foreach (Soldier shooter in (ArrayList)_shotRows[_shotRows.Count - 1])
                {
                    if (!shooter.shotMade() && shooter.getState() != Soldier.STATE_RELOADING && shooter.getState() != Soldier.STATE_READY)
                    {
                        _allShotsMade = false;
                    }
                }

                if (_allShotsMade)
                {
                    resetupFormation();
                    _needResetupFormation = false;
                }
            }

            foreach (Soldier r in _soldiersToRemove)
            {
                _soldiers.Remove(r);
                if (r.getType() == Soldier.TYPE_PIKE)
                {
                    numberOfPikes--;
                    foreach (ArrayList list in _pikeRows)
                        list.Remove(r);
                }
                else if (r.getType() == Soldier.TYPE_SHOT)
                {
                    numberOfShots--;
                    foreach (ArrayList list in _shotRows)
                        list.Remove(r);
                }
                else if (r.getType() == Soldier.TYPE_SUPPORT)
                {
                    foreach (ArrayList list in _supportRows)
                        list.Remove(r);
                }
            }
            _soldiersToRemove.Clear();

            // if a soldier died then we need to reorganize the formation
            if (_soldierDied)
            {
                reformFormation();
            }

            if (_addedSoldier && !(this is ColmillosFormation))
            {
                if (_width < 5)
                {
                    _width = 5;
                }
                
                _addedSoldier = false;
                resetupFormation();
            }
            _speed = avgSpeed - 0.04f;
        }

        public Vector2 getSize()
        {
            return _size;
        }

        private void assignDoppel(Soldier enemy)
        {
            Soldier guard = null;

            foreach (Soldier dopp in _soldiers)
            {
                if (dopp is Dopple)
                {
                    if (dopp.guardTarget == enemy)
                        return;
                }
            }
            foreach (Soldier dopp in _soldiers)
            {
                if (dopp is Dopple)
                {
                    if (dopp.guardTarget == null)
                    {
                        if (guard == null && dopp.guardTarget == null)
                            guard = dopp;
                        else
                        {
                            bool ySmaller = Math.Abs(enemy.getPosition().Y - dopp.getPosition().Y) < Math.Abs(enemy.getPosition().Y - guard.getPosition().Y);
                            if (ySmaller && dopp.guardTarget == null)
                                guard = dopp;
                        }
                    }
                }
            }
            if (guard != null)
                guard.guardTarget = enemy;
        }

        private Soldier checkGuard(Soldier guard, Soldier shot, ScreenObject enemy)
        {
            if (guard == null)
            {
                if (shot.getState() != Soldier.STATE_MELEE_WIN && shot.getState() != Soldier.STATE_MELEE_LOSS)
                    return shot;
                else
                    return null;
            }
            else
            {
                bool ySmaller = Math.Abs(enemy.getPosition().Y - shot.getDestination().Y) < Math.Abs(enemy.getPosition().Y - guard.getDestination().Y);
                if (ySmaller && shot.guardTarget == null && shot.getState() != Soldier.STATE_MELEE_WIN && shot.getState() != Soldier.STATE_MELEE_LOSS)
                    return shot;
            }

            return guard;
        }

        private void assignShooter(Soldier enemy)
        {
            if (this is EnemyFormation)
                return;

            Soldier guard = null;

            foreach (Soldier shot in _soldiers)
            {
                if (shot.getType() == Soldier.TYPE_SHOT )
                {
                    if (shot.guardTarget == enemy)
                        return;
                }
            }
            foreach (Soldier shot in (ArrayList)_shotRows[0])
            {
                if (shot.getType() == Soldier.TYPE_SHOT)
                {
                    if (shot.guardTarget == null && shot.getState() != Soldier.STATE_RELOADING)
                    {
                        guard = checkGuard(guard, shot, enemy);
                    }
                }
            }
            if (guard != null)
                guard.guardTarget = enemy;
        }

        private void assignShielder(Shot enemy)
        {
            if (this is EnemyFormation)
                return;

            Soldier guard = null;

            foreach (Soldier shot in (ArrayList)_meleeRows[0])
            {
                if (shot is Targeteer)
                {
                    if (shot.guardTarget == enemy)
                        return;
                }
            }
            foreach (Soldier shot in (ArrayList)_meleeRows[0])
            {
                if (shot is Targeteer)
                {
                    if (shot.guardTarget == null && ((Targeteer)shot)._hasShield)
                    {
                        guard = checkGuard(guard, shot, enemy);
                    }
                }
            }
            if (guard != null)
                guard.guardTarget = enemy;
        }

        private void assignGuard(Soldier enemy)
        {
            Soldier guard = null;

            foreach (Pikeman pike in (ArrayList)_pikeRows[0])
            {
                if (pike.guardTarget == enemy)
                    return;
            }

            if (_pikeRows.Count > 1)
            {
                foreach (Pikeman pike in (ArrayList)_pikeRows[1])
                {
                    if (pike.guardTarget == enemy)
                        return;
                }
            }

            foreach (Pikeman pike in (ArrayList)_pikeRows[0])
            {
                if (pike.guardTarget == null)
                {
                    if (guard == null && pike.getState() != Soldier.STATE_MELEE_WIN && pike.getState() != Soldier.STATE_MELEE_LOSS)
                        guard = pike;
                    else
                    {
                        guard = checkGuard(guard, pike, enemy);
                    }
                }
            }

            if (_pikeRows.Count > 1)
            {
                foreach (Pikeman pike in (ArrayList)_pikeRows[1])
                {
                    if (pike.guardTarget == null && pike.getState() != Soldier.STATE_MELEE_WIN && pike.getState() != Soldier.STATE_MELEE_LOSS)
                    {
                        if (guard == null)
                            guard = pike;
                        else
                        {
                            bool ySmaller = Math.Abs(enemy.getPosition().Y - pike.getDestination().Y) < Math.Abs(enemy.getPosition().Y - guard.getDestination().Y);
                            bool xBigger = (pike.getDestination().X - guard.getDestination().X) * _side >= 0;
                            bool ySubstanciallyBigger = Math.Abs(guard.getDestination().Y - pike.getDestination().Y) > (float)Soldier.HEIGHT * 3f;
                            if (ySmaller && (xBigger || ySubstanciallyBigger) && pike.guardTarget == null && pike.getState() != Soldier.STATE_MELEE_WIN && pike.getState() != Soldier.STATE_MELEE_LOSS)
                                guard = pike;
                        }
                    }
                }
            }
            if (guard != null)
            {
                guard.guardTarget = enemy;
                guard.setSpeed(0.18f);
            }
        }

        private void moveUpShooter()
        {
            int index = -1;
            bool eligableMovee = false;
            for (int i = 0; i < _shotRows.Count; i++)
            {
                if (i != getForwardShotRow())
                {
                    if (eligableMovee == false)
                    {
                        foreach (Soldier movee in (ArrayList)_shotRows[i])
                        {
                            index++;
                            if (movee.getState() == Soldier.STATE_READY)
                            {
                                eligableMovee = true;
                                break;
                            }
                        }
                        if (eligableMovee == true)
                        {
                            // found a guy to move up
                            Soldier temp = (Soldier)((ArrayList)_shotRows[i])[index];
                            ((ArrayList)_shotRows[i]).Remove(temp);
                            ((ArrayList)_shotRows[i]).Add(temp);
                            reformFormation();
                            return;
                        }
                        index = -1;
                    }
                }
            }
        }

        private void replaceWithReadyShooter(Soldier s, int ind)
        {
            int index = -1;
            bool eligableMovee = false;
            for (int i = 0; i < _shotRows.Count; i++)
            {
                if(i != getForwardShotRow())
                {
                    if (eligableMovee == false)
                    {
                        foreach (Soldier movee in (ArrayList)_shotRows[i])
                        {
                            index++;
                            if (movee.getState() == Soldier.STATE_READY)
                            {
                                eligableMovee = true;
                                break;
                            }
                        }
                        if (eligableMovee == true)
                        {
                            // found a guy to move up
                            Soldier temp = (Soldier)((ArrayList)_shotRows[i])[index];
                            Vector2 vect1 = new Vector2(temp._destination.X, temp._destination.Y);
                            Vector2 vect2 = new Vector2(((Soldier)((ArrayList)_shotRows[0])[ind])._destination.X, ((Soldier)((ArrayList)_shotRows[0])[ind])._destination.Y);
                            ((ArrayList)_shotRows[i])[index] = ((ArrayList)_shotRows[0])[ind];
                            ((ArrayList)_shotRows[0])[ind] = temp;
                            ((Soldier)((ArrayList)_shotRows[i])[index])._destination = vect1;
                            ((Soldier)((ArrayList)_shotRows[0])[ind])._destination = vect2;
                            return;
                        }
                        index = -1;
                    }
                }
            }
        }

        public void setPosition(Vector2 pos)
        {
            _position = pos;
        }

        public void setPosition(float x, float y)
        {
            _position = new Vector2(x, y);
        }

        public void reformFormation()
        {
            selected = false;
            _soldiersToRemove.Clear();
            foreach (Soldier s in _soldiers)
            {
                _soldiersToRemove.Add(s);
            }
            numberOfPikes = 0;
            numberOfShots = 0;
            _soldiers.Clear();
            _shotRows.Clear();
            _pikeRows.Clear();
            _meleeRows.Clear();
 //           _swingerRows.Clear();
            _cavRows.Clear();
            _supportRows.Clear();
            foreach (Soldier r in _soldiersToRemove)
            {
                addSoldier(r);
            }
            _soldiersToRemove.Clear();
        }

        public Vector2 getPosition()
        {
            return _position;
        }

        public int getState()
        {
            return _state;
        }

        public Vector2 getScreenPosition()
        {
            return _position - _screen.getMapOffset();
        }

        public void pikeAttack()
        {
            if (_supportRows.Count != 0)
            {
                foreach (ArrayList row in _supportRows)
                {
                    foreach (Soldier s in row)
                    {
                        if (s is Leader)
                            s.attack();
                        else if (s is Dopple)
                            ((Dopple)s).charge();
                        else if (s is CrossbowmanPavise)
                            ((DoppelSoldier)s).chargeEnd();
                    }
                }
            }
            if (_pikeRows.Count != 0)
            {

                if (_state != STATE_PIKE)
                {
                    _state = STATE_PIKE;
                    resetupFormation();
                }

                foreach (Soldier pikeman in (ArrayList)_pikeRows[0])
                {
                    if (pikeman is Pikeman && (pikeman.getState() != Soldier.STATE_MELEE_WIN && pikeman.getState() != Soldier.STATE_MELEE_LOSS))
                        pikeman.attackLow();
                    else
                        pikeman.preAttackState = Pikeman.STATE_ATTACKING;
                }

                if (_pikeRows.Count > 1)
                {
                    foreach (Soldier pikeman in (ArrayList)_pikeRows[1])
                    {
                        if (pikeman is Pikeman && (pikeman.getState() != Soldier.STATE_MELEE_WIN && pikeman.getState() != Soldier.STATE_MELEE_LOSS))
                            pikeman.attackHigh();
                        else
                            pikeman.preAttackState = Pikeman.STATE_ATTACKING;
                    }
                }

                if (_pikeRows.Count > 2)
                {
                    foreach (Soldier pikeman in (ArrayList)_pikeRows[2])
                    {
                        if (pikeman is Pikeman && (pikeman.getState() != Soldier.STATE_MELEE_WIN && pikeman.getState() != Soldier.STATE_MELEE_LOSS))
                            ((Pikeman)pikeman).lower45();
                        else
                            pikeman.preAttackState = Pikeman.STATE_LOWER45;
                    }
                }
            }

            if(_cavRows.Count != 0)
            {
                foreach (Soldier horsie in (ArrayList)_cavRows[0])
                {
                    if (horsie is Cavalry && (horsie.getState() != Soldier.STATE_MELEE_WIN && horsie.getState() != Soldier.STATE_MELEE_LOSS))
                        horsie.attack();
                }
            }
        }

        public void pikeRaise()
        {
            foreach (ArrayList row in _supportRows)
            {
                foreach (Soldier s in row)
                {
                    if (s is Dopple)
                        ((DoppelSoldier)s).chargeEnd();
                }
            }

            _state = STATE_MELEE;
            resetupFormation();
            foreach (Soldier pikeman in _soldiers)
            {
                if (pikeman is Pikeman)
                {
                    if (pikeman.getState() != Soldier.STATE_MELEE_WIN && pikeman.getState() != Soldier.STATE_MELEE_LOSS)
                        ((Pikeman)pikeman).raise();
                    else
                        ((Pikeman)pikeman).preAttackState = Pikeman.STATE_RAISING;
                }
            }

            foreach (Soldier horsie in _soldiers)
            {
                if (horsie is Cavalry && (horsie.getState() != Soldier.STATE_MELEE_WIN && horsie.getState() != Soldier.STATE_MELEE_LOSS))
                    ((Cavalry)horsie).raise();
            }
        }

        private bool dangerClose()
        {
            if (_screen is LevelScreen)
            {
                _enemiesToGuard.Clear();
                _enemiesToGuard = ((LevelScreen)_screen).dangerCloseToFormation();
                return _enemiesToGuard.Count > 0;
            }

            return false;
        }

        private void dangerBefore()
        {
            if (_screen is LevelScreen)
            {
                _enemiesToShoot.Clear();
                //_shotsToBlock.Clear();
                _enemiesToShoot = ((LevelScreen)_screen).dangerBeforeFormation();
                //_shotsToBlock = ((LevelScreen)_screen).shotBeforeFormation();
            }
        }

        public void meleeCharge()
        {
            if (_meleeRows.Count > 0)
            {
                foreach (Soldier s in (ArrayList)_meleeRows[0])
                {
                    if (s.getState() != Soldier.STATE_MELEE_LOSS && s.getState() != Soldier.STATE_MELEE_WIN)
                        s.attack();
                }
            }

            // ACTIVATE THE DOPPELS
            Vector2 distanceFromHome;
            if (_supportRows.Count > 0)
            {
                foreach (ArrayList row in _supportRows)
                {
                    foreach (Soldier d in row)
                    {   
                        if (d is DoppelSoldier && d.getState() == Soldier.STATE_READY)
                        {
                            distanceFromHome = d._destination - d._position;
                            if (distanceFromHome.Length() < 5f)
                            {
                                DoppelSoldier dopple = (DoppelSoldier)d;
                                dopple.charge();
                                break;
                            }
                        }
                    }
                }
            }
        }

        internal void cancelCharge()
        {
            if (_meleeRows.Count > 0)
            {
                foreach (Soldier s in (ArrayList)_meleeRows[0])
                {
                    if (s.getState() != Soldier.STATE_MELEE_LOSS && s.getState() != Soldier.STATE_MELEE_WIN)
                        s.cancelAttack();
                }
            }
            if(_supportRows.Count > 0)
            {
                foreach (Soldier d in (ArrayList)_supportRows[0])
                {
                    if (d is Dopple && d.getState() != Soldier.STATE_MELEE_LOSS && d.getState() != Soldier.STATE_MELEE_WIN && d.getState() != Soldier.STATE_READY && d.getState() != Soldier.STATE_DYING && d.getState() != Soldier.STATE_DEAD)
                    {
                        d.cancelAttack();
                    }
                }
            }
        }

        public void haltHorses()
        {
            foreach (ArrayList al in _cavRows)
            {
                foreach (Cavalry s in al)
                {
                    if (s.getState() != Soldier.STATE_MELEE_LOSS && s.getState() != Soldier.STATE_MELEE_WIN)
                    {
                        s.halt();
                       // s.alterDestination(true, 50f);
                    }
                }
            }

            if (_meleeRows.Count > 0)
            {
                foreach (Soldier s in (ArrayList)_meleeRows[0])
                {
                    if (s.getState() != Soldier.STATE_MELEE_LOSS && s.getState() != Soldier.STATE_MELEE_WIN && s is Wolf)
                        ((Wolf)s).howl();
                }
            }

            //_position.X += 50f;
        }

        public void turnHorses()
        {
            foreach (ArrayList al in _cavRows)
            {
                foreach (Cavalry s in al)
                {
                    if (s.getState() != Soldier.STATE_MELEE_LOSS && s.getState() != Soldier.STATE_MELEE_WIN)
                    {
                        s.turn();
                    }
                }
            }
            foreach (ArrayList al in _meleeRows)
            {
                foreach (Wolf s in al)
                {
                    if (s.getState() != Soldier.STATE_MELEE_LOSS && s.getState() != Soldier.STATE_MELEE_WIN)
                    {
                        s.turn();
                    }
                }

            }

        }

        public void reload()
        {
            if (_shotRows.Count > 0)
            {
                foreach (Soldier soldier in (ArrayList)_shotRows[0])
                {
                    if (soldier.getState() != Soldier.STATE_MELEE_WIN && soldier.getState() != Soldier.STATE_MELEE_LOSS && soldier.getState() != Soldier.STATE_RELOADING && soldier is Slinger)
                        ((Slinger)soldier).reload();
                }
            }
        }

        public void spawn()
        {
            foreach (Soldier soldier in _soldiers)
            {
                if (soldier.getState() != Soldier.STATE_MELEE_WIN && soldier.getState() != Soldier.STATE_MELEE_LOSS && soldier.getState() != Soldier.STATE_RELOADING && soldier.getState() != Soldier.STATE_DYING)
                    soldier.spawn();
            }
        }

        public void shotAttack()
        {
            bool didAttack = false;
            ArrayList tempRow;
            ArrayList prevRow;

            if (_supportRows.Count != 0 && !needTriggerUp)
            {
                foreach (ArrayList row in _supportRows)
                {
                    float chargeY = (float)(Soldier.HEIGHT*(row.Count - 1))/-2f;
                    foreach (Soldier s in row)
                    {
                        if (s is Leader)
                            s.attack();
                        else if (s is CrossbowmanPavise)
                        {
                            ((DoppelSoldier)s).charge();
                            ((CrossbowmanPavise)s).chargePosition = new Vector2(100f, chargeY);
                            chargeY += Soldier.HEIGHT;
                        }
                    }
                }
            }

            if (_shotRows.Count != 0)
            {
                prevRow = (ArrayList)_shotRows[0];

                if (_state != STATE_SHOT)
                {
                    _state = STATE_SHOT;
                    resetupFormation();
                }
                else if (firstShotIsForward() && (!needTriggerUp || _side == BattleScreen.SIDE_ENEMY))
                {
                    foreach (Soldier soldier in (ArrayList)_shotRows[0])
                    {
                        if (soldier.getState() != Soldier.STATE_MELEE_WIN && soldier.getState() != Soldier.STATE_MELEE_LOSS)
                        {
                            if (soldier.attack())
                            {
                                didAttack = true;
                            }
                        }
                    }

                    if (!didAttack)
                        return;

                    ((LevelScreen)_screen).clearShotSounds();

                    for (int i = _shotRows.Count - 1; i >= 0; i--)
                    {
                        tempRow = (ArrayList)_shotRows[i];
                        _shotRows[i] = prevRow;
                        prevRow = tempRow;
                    }

                    //the formation used to be resetup here but the call was moved to update so that the formation could check 
                    //to see that all soldiers had finished making their shots before resetupFormation() is called
                    _needResetupFormation = true;

                    needTriggerUp = true;
                }
            }

        }

        private bool firstShotIsForward()
        {
            bool forward = false;

            if (_shotRows.Count > 1)
            {
                foreach (Soldier shot in (ArrayList)_shotRows[0])
                {
                    foreach (Soldier shot2 in (ArrayList)_shotRows[_shotRows.Count - 1])
                    {
                        if (((_side * shot._position.X - _side * shot2._position.X >= Soldier.WIDTH - 0.9) && shot.isReady()) || !shot2.isReady())
                            forward = true;
                    }
                }
            }
            else
                forward = true;

            bool meleeForward = false;
            if (_pikeRows.Count > 0)
            {
                /*melee = (Soldier)((ArrayList)_pikeRows[0])[0];
                if (_side * shot._position.X - _side * melee._position.X >= 0)
                    return true;
                else
                    return false;
                 */
                foreach (Soldier shot in (ArrayList)_shotRows[0])
                {
                    foreach (Soldier melee in (ArrayList)_pikeRows[0])
                    {
                        if ((_side * shot._position.X - _side * melee._position.X >= 0 && shot.isReady()) || !melee.isReady())
                        {
                            meleeForward = true;
                        }
                    }
                }
            }
            else
                return forward;

            return forward && meleeForward;
        }

        public void draw(SpriteBatch spritebatch)
        {
            foreach (Soldier s in _soldiers)
            {
                if (selected)
                    s.selectedDraw(spritebatch);
                else
                    s.draw(spritebatch);
            }

            if (_screen.getDrawDots())
            {
                if (DEBUGdangerClose)
                {
                    spritebatch.DrawString(PikeAndShotGame.getSpriteFont(), "HEY!", new Vector2(500f, 500f), Color.White);
                }
                spritebatch.Draw(PikeAndShotGame.getDotTexture(), _position - _screen.getMapOffset(), Color.White);
                spritebatch.Draw(PikeAndShotGame.getDotTexture(), new Vector2(_position.X - _screen.getMapOffset().X + (getTotalRows() * Soldier.WIDTH), _position.Y - _screen.getMapOffset().Y + getTotalRows() * Soldier.HEIGHT), Color.White);
                spritebatch.Draw(PikeAndShotGame.BERZERKER_IDLE, this.getCenter() - _screen.getMapOffset(), Color.White);
            }
        }

        public virtual void addSoldier(Soldier soldier)
        {
            _addedSoldier = true;
            switch(soldier.getType())
            {
                case Soldier.TYPE_PIKE:
                    addSoldierToRow(soldier, _pikeRows);
                    break;
                case Soldier.TYPE_SWINGER:
                    addSoldierToRow(soldier, _supportRows);
                    break;
                case Soldier.TYPE_SHOT:
                    addSoldierToRow(soldier, _shotRows);
                    break;
                case Soldier.TYPE_CAVALRY:
                    addSoldierToRow(soldier, _cavRows);
                    break;
                case Soldier.TYPE_SUPPORT:
                    addSoldierToRow(soldier, _supportRows);
                    break;
                case Soldier.TYPE_MELEE:
                    addSoldierToRow(soldier, _meleeRows);
                    break;
                default:
                    addSoldierToRow(soldier, _supportRows);
                    break;
            }

            soldier.setSide(_side);
            if (_screen.getPlayerFormation() == this)
                soldier.inPlayerFormation = true;
            soldier.initCharge = false;
            if(soldier is Wolf)
                soldier.setSpeed(0.20f);
            else
                soldier.setSpeed(0.15f);

            // determine speed
            avgSpeed = 0;

            foreach (Soldier s in _soldiers)
            {
                avgSpeed += s.getSpeed();
            }
            avgSpeed /= (float)_soldiers.Count;
            _speed = avgSpeed - 0.04f;            
            //_speed = avgSpeed - (0.002f * (float)_soldiers.Count);            
            soldier.myFormation = this;

            if (soldier.getType() == Soldier.TYPE_PIKE)
            {
                reiteratePikeCommand((Pikeman)soldier);
                numberOfPikes++;
            }
            else if (soldier is Dopple && _state == STATE_PIKE)
            {
                ((Dopple)soldier).charge();
            }
            else if (soldier.getType() == Soldier.TYPE_SHOT)
            {
                numberOfShots++;
            }
        }

        public void reiteratePikeCommand(Pikeman soldier)
        {
            ArrayList firstRow = (ArrayList)_pikeRows[0];
            int i = 0;
            Pikeman pikeman = (Pikeman)firstRow[i];

            while ((pikeman.getState() == Pikeman.STATE_TUG || pikeman.getState() == Pikeman.STATE_MELEE_WIN || pikeman.getState() == Pikeman.STATE_MELEE_LOSS)
                && i + 1 < firstRow.Count)
            {
                i++;
                pikeman = (Pikeman)firstRow[i];
            }

            if (pikeman.getState() == Pikeman.STATE_ATTACKING || pikeman.getState() == Pikeman.STATE_LOWERED ||
                pikeman.getState() == Pikeman.STATE_RECOILING || pikeman.getState() == Pikeman.STATE_TUG)
                soldier.attack();
            else 
                soldier.raise();
        }

        private float getSlowedSoldiers()
        {
            float slowEffect = 0;
            int slowedPikes = 0;

            foreach (ArrayList al in _pikeRows)
            {
                foreach (Pikeman pMan in al)
                {
                    if (pMan.getState() == Pikeman.STATE_RECOILING)
                        slowedPikes++;
                }
            }
            slowEffect = (float)slowedPikes / (float)_soldiers.Count;

            return 0.1f * slowEffect;
        }

        private void addSoldierToRow(Soldier soldier, ArrayList rows)
        {
            int addLeader = soldier.getType() == Soldier.TYPE_SWINGER ? 2 : 0;
            int rowsOfType = (getTotalSoldiers(soldier.getType()) + addLeader) / _width;
            Soldier prevSoldier;
            ArrayList row;
            float lastRowStartHeight = 0f;

            if (soldier is Pikeman && _state == STATE_PIKE)
            {
                if (rows.Count < 2)
                    ((Pikeman)soldier).attackLow();
                else
                    ((Pikeman)soldier).attackHigh();
            }

            if (rows.Count < rowsOfType + 1)
            {
                rows.Add(new ArrayList(_width));
                ((ArrayList)rows[rows.Count - 1]).Add(soldier);
                row = (ArrayList)rows[rows.Count - 1];

                if (rowsOfType > 0)
                {
                    prevSoldier = ((Soldier)((ArrayList)rows[rows.Count - 2])[0]);
                    soldier._destination = new Vector2(prevSoldier._destination.X - (_side * Soldier.WIDTH), prevSoldier._destination.Y);
                }
                else
                    soldier._destination = new Vector2(_position.X - (_side * Soldier.WIDTH), _position.Y);
            }
            else
            {
                if (soldier.getType() == Soldier.TYPE_SWINGER && rows.Count < 2 && ((ArrayList)rows[0]).Count % 2 == 0)
                {
                    row = (ArrayList)rows[0];
                    row.Insert(0, soldier);
                }
                else
                {
                    row = findSmallestRow(rows);
                    row.Add(soldier);
                    prevSoldier = ((Soldier)row[row.Count - 2]);
                }
            }

            //see if rows should be evened out a little
            if (rows.Count > 1 && row.Count < _width / 2)
            {
                ArrayList prevRow = ((ArrayList)rows[rows.Count - 2]);
                Soldier oldSoldier = (Soldier)prevRow[prevRow.Count -1];
                row.Add(oldSoldier);
                prevSoldier = ((Soldier)((ArrayList)rows[rows.Count - 1])[0]);
                oldSoldier._destination = new Vector2(prevSoldier._destination.X, prevSoldier._destination.Y);
                prevRow.RemoveAt(prevRow.Count - 1);
                lastRowStartHeight = (float)_width / 2f - (float)(prevRow.Count) / 2f;

                for (int i = 0; i < prevRow.Count; i++)
                {
                    ((Soldier)prevRow[i])._destination.Y = _position.Y + Soldier.HEIGHT * ((float)i + lastRowStartHeight);
                }
            }
            
            lastRowStartHeight = (float)_width / 2f - (float)(row.Count) / 2f;

            for (int i = 0; i < row.Count; i++)
            {
                ((Soldier)row[i])._destination.Y = _position.Y + Soldier.HEIGHT * ((float)i + lastRowStartHeight);
            }

            _soldiers.Add(soldier);
        }

        private ArrayList findSmallestRow(ArrayList rows)
        {
            ArrayList smallestRow = (ArrayList)rows[rows.Count-1];
            ArrayList currentRow;
            int smallestCount = 100000;

            for (int i = 0; i < rows.Count; i++)
            {
                currentRow = (ArrayList)rows[i];

                if (currentRow.Count < smallestCount)
                {
                    smallestCount = currentRow.Count;
                    smallestRow = currentRow;
                }
            }
            return smallestRow;
        }

        public void resetupFormation()
        {
            int i = 0;

            // DEBUG
            if (!(this is EnemyFormation))
                i = 0;

            if (_state == STATE_PIKE)
            {
                arrangeFormation(_pikeRows, ref i);
                arrangeFormation(_meleeRows, ref i);
                arrangeFormation(_shotRows, ref i);
                arrangeFormation(_supportRows, ref i);
            }
            else if (_state == STATE_SHOT)
            {
                arrangeFormation(_shotRows, ref i);
                arrangeFormation(_meleeRows, ref i);
                arrangeFormation(_pikeRows, ref i);
                arrangeFormation(_supportRows, ref i);
            }
            else // STATE_MELEE
            {
                arrangeFormation(_meleeRows, ref i);
                arrangeFormation(_pikeRows, ref i);
                arrangeFormation(_shotRows, ref i);
                arrangeFormation(_supportRows, ref i);
            }

            arrangeFormation(_cavRows, ref i);

            _size.X = i * Soldier.WIDTH;
            _size.Y = _width * Soldier.WIDTH;
        }

        private void arrangeFormation(ArrayList rows, ref int i)
        {
            foreach (ArrayList row in rows)
            {
                for (int j = 0; j < row.Count; j++)
                {
                    ((Soldier)row[j])._destination.X = _position.X - (_side * Soldier.WIDTH * (float)i);
                    //((Soldier)row[j])._destination.Y = _position.Y + Soldier.HEIGHT * (float)j;
                }
                i++;
            }  
        }

        public int getTotalSoldiers(int type)
        {
            int totalSoldiers = 0;

            for (int i = 0; i < _soldiers.Count; i++)
            {
                if (((Soldier)_soldiers[i]).getType() == type)
                    totalSoldiers++;
            }

            return totalSoldiers;
        }

        public ArrayList getSoldiers()
        {
            return _soldiers;
        }

        public void removeSoldier(Soldier soldier)
        {
            //_soldiers.Remove(soldier);
            _soldiersToRemove.Add(soldier);
        }

        public void removeSoldier(int soldier)
        {
            _soldiersToRemove.Add(_soldiers[soldier]);
        }

        public void marchUp(double milliseconds, bool diagonal)
        {
            if (!checkFormationCollided() || !mostSoldiersBelow())
            {
                float amount;

                if (diagonal)
                    amount = _speed * (float)milliseconds * 0.708f;// * 1.0f;
                else
                    amount = _speed * (float)milliseconds;

                if (getCenter().Y - this.getWidth() * Soldier.WIDTH / 2 > _screen.getMapOffset().Y || _side == BattleScreen.SIDE_ENEMY)
                {

                    _position.Y -= amount;

                    foreach (Soldier s in _soldiers)
                    {
                        s.alterDestination(false, -amount);
                    }

                    if (_supportRows.Count != 0)
                    {
                        foreach (ArrayList row in _supportRows)
                        {
                            float chargeY = (float)(Soldier.HEIGHT * (row.Count - 1)) / -2f;
                            foreach (Soldier s in row)
                            {
                                if (s is CrossbowmanPavise)
                                {
                                    ((CrossbowmanPavise)s).setChargePosition(new Vector2(100f, chargeY));
                                    chargeY += Soldier.HEIGHT;
                                }
                            }
                        }
                    }
                }
            }
        }

        private bool mostSoldiersBelow()
        {
            Vector2 pos = Vector2.Zero;
            foreach (Soldier soldier in _soldiers)
            {
                pos += soldier.getPosition();
            }
            pos /= _soldiers.Count;

            return pos.Y < getCenter().Y;
        }

        private bool mostSoldiersRight()
        {
            Vector2 pos = Vector2.Zero;
            foreach (Soldier soldier in _soldiers)
            {
                pos += soldier.getPosition();
            }
            pos /= _soldiers.Count;

            return pos.X > getCenter().X;
        }

        public void marchDown(double milliseconds, bool diagonal)
        {
            if (!checkFormationCollided() || mostSoldiersBelow())
            {
                float amount;

                if (diagonal)
                    amount = _speed * (float)milliseconds * 0.708f; //* 1.0f;
                else
                    amount = _speed * (float)milliseconds;

                if (getCenter().Y + this.getWidth() * Soldier.WIDTH / 2 < _screen.getMapOffset().Y + PikeAndShotGame.SCREENHEIGHT || _side == BattleScreen.SIDE_ENEMY)
                {
                    _position.Y += amount;

                    foreach (Soldier s in _soldiers)
                    {
                        s.alterDestination(false, amount);
                    }

                    if (_supportRows.Count != 0)
                    {
                        foreach (ArrayList row in _supportRows)
                        {
                            float chargeY = (float)(Soldier.HEIGHT * (row.Count - 1)) / -2f;
                            foreach (Soldier s in row)
                            {
                                if (s is CrossbowmanPavise)
                                {
                                    ((CrossbowmanPavise)s).setChargePosition(new Vector2(100f, chargeY));
                                    chargeY += Soldier.HEIGHT;
                                }
                            }
                        }
                    }
                }
            }
        }

        public void marchLeft(double milliseconds, bool diagonal)
        {
            if (!checkFormationCollided() || !mostSoldiersRight())
            {
                Soldier lastSoldier;
                bool shot = true;
                bool melee = true;
                bool pike = true;
                float amount;

                if (getCenter().X - this.getWidth() * Soldier.WIDTH / 2 > _screen.getMapOffset().X || _side == BattleScreen.SIDE_ENEMY)
                {
                    if (diagonal)
                        amount = _speed * (float)milliseconds * 0.708f;
                    else
                        amount = _speed * (float)milliseconds;

                    _position.X -= amount;

                    if (_state == STATE_PIKE && _pikeRows.Count > 0)
                    {
                        if (_shotRows.Count > 0)
                        {
                            lastSoldier = getFirstNonMeleeSoldier((ArrayList)_shotRows[_shotRows.Count - 1]);
                            if (lastSoldier != null)
                            {
                                if (!((_side * lastSoldier._destination.X - _side * lastSoldier._position.X) > _speed || (_side * lastSoldier._destination.X - _side * lastSoldier._position.X) < -_speed))
                                {
                                    lastSoldier = null;
                                    shot = false;
                                }
                            }
                        }
                        else
                            lastSoldier = null;

                        if (lastSoldier == null)
                        {
                            if (_meleeRows.Count > 0)
                            {
                                lastSoldier = getFirstNonMeleeSoldier((ArrayList)_meleeRows[_meleeRows.Count - 1]);
                                if (lastSoldier != null)
                                {
                                    if (!((_side * lastSoldier._destination.X - _side * lastSoldier._position.X) > _speed || (_side * lastSoldier._destination.X - _side * lastSoldier._position.X) < -_speed))
                                    {
                                        lastSoldier = null;
                                        melee = false;
                                    }
                                }
                            }
                            else
                                lastSoldier = null;
                        }

                        if (lastSoldier != null)
                        {
                            if (_side * lastSoldier._position.X - _side * lastSoldier._destination.X > _speed)
                            {
                                if ((_side * lastSoldier._destination.X - _side * lastSoldier._position.X) > _speed || (_side * lastSoldier._destination.X - _side * lastSoldier._position.X) < -_speed)
                                {
                                    _position.X = lastSoldier._position.X + _side * (_pikeRows.Count - 1 + /*(swing ? _swingerRows.Count : 0) +*/ (shot ? _shotRows.Count : 0) + (melee ? _meleeRows.Count : 0)) * Soldier.WIDTH;
                                    resetupFormation();
                                }
                            }
                        }
                    }
                    else if (_state == STATE_MELEE && _meleeRows.Count > 0)
                    {
                        if (_shotRows.Count > 0)
                        {
                            lastSoldier = getFirstNonMeleeSoldier((ArrayList)_shotRows[_shotRows.Count - 1]);
                            if (lastSoldier != null)
                            {
                                if (!((_side * lastSoldier._destination.X - _side * lastSoldier._position.X) > _speed || (_side * lastSoldier._destination.X - _side * lastSoldier._position.X) < -_speed))
                                {
                                    lastSoldier = null;
                                    shot = false;
                                }
                            }
                        }
                        else
                            lastSoldier = null;

                        if (lastSoldier == null)
                        {
                            if (_pikeRows.Count > 0)
                            {
                                lastSoldier = getFirstNonMeleeSoldier((ArrayList)_pikeRows[_pikeRows.Count - 1]);
                                if (lastSoldier != null)
                                {
                                    if (!((_side * lastSoldier._destination.X - _side * lastSoldier._position.X) > _speed || (_side * lastSoldier._destination.X - _side * lastSoldier._position.X) < -_speed))
                                    {
                                        lastSoldier = null;
                                        pike = false;
                                    }
                                }
                            }
                            else
                                lastSoldier = null;
                        }

                        if (lastSoldier != null)
                        {
                            if (_side * lastSoldier._position.X - _side * lastSoldier._destination.X > _speed)
                            {
                                if ((_side * lastSoldier._destination.X - _side * lastSoldier._position.X) > _speed || (_side * lastSoldier._destination.X - _side * lastSoldier._position.X) < -_speed)
                                {
                                    _position.X = lastSoldier._position.X + _side * ((pike ? _pikeRows.Count : 0) + /*(swing?_swingerRows.Count:0) +*/ (shot ? _shotRows.Count : 0) + _meleeRows.Count - 1) * Soldier.WIDTH;
                                    resetupFormation();
                                }
                            }
                        }

                    }
                    else if (_state == STATE_SHOT && _shotRows.Count != 0)
                    {
                        if (_meleeRows.Count > 0)
                        {
                            lastSoldier = getFirstNonMeleeSoldier((ArrayList)_meleeRows[_meleeRows.Count - 1]);
                            if (lastSoldier != null)
                            {
                                if (!((_side * lastSoldier._destination.X - _side * lastSoldier._position.X) > _speed || (_side * lastSoldier._destination.X - _side * lastSoldier._position.X) < -_speed))
                                {
                                    lastSoldier = null;
                                    melee = false;
                                }
                            }
                            else
                            {
                                lastSoldier = null;
                            }
                        }
                        else
                            lastSoldier = null;

                        if (lastSoldier == null)
                        {
                            if (_pikeRows.Count > 0)
                            {
                                lastSoldier = getFirstNonMeleeSoldier((ArrayList)_pikeRows[_pikeRows.Count - 1]);
                                if (lastSoldier != null)
                                {
                                    if (!((_side * lastSoldier._destination.X - _side * lastSoldier._position.X) > _speed || (_side * lastSoldier._destination.X - _side * lastSoldier._position.X) < -_speed))
                                    {
                                        lastSoldier = null;
                                        pike = false;
                                    }
                                }
                                else
                                {
                                    lastSoldier = null;
                                }
                            }
                            else
                                lastSoldier = null;
                        }

                        if (lastSoldier == null)
                        {
                            if (_shotRows.Count > 1)
                            {
                                lastSoldier = getFirstNonMeleeSoldier((ArrayList)_shotRows[_shotRows.Count - 1]);
                                if (lastSoldier != null)
                                {
                                    if (!((_side * lastSoldier._destination.X - _side * lastSoldier._position.X) > _speed || (_side * lastSoldier._destination.X - _side * lastSoldier._position.X) < -_speed))
                                    {
                                        lastSoldier = null;
                                        shot = false;
                                    }
                                }
                                else
                                {
                                    lastSoldier = null;
                                }
                            }
                            else
                                lastSoldier = null;
                        }

                        if (lastSoldier != null)
                        {
                            if (_side * lastSoldier._position.X - _side * lastSoldier._destination.X > _speed)
                            {
                                if ((_side * lastSoldier._destination.X - _side * lastSoldier._position.X) > _speed || (_side * lastSoldier._destination.X - _side * lastSoldier._position.X) < -_speed)
                                {
                                    _position.X = lastSoldier._position.X + _side * ((pike ? _pikeRows.Count : 0) + /*(swing?_swingerRows.Count:0) +*/ (melee ? _meleeRows.Count : 0) + _shotRows.Count - 1) * Soldier.WIDTH;
                                    resetupFormation();
                                }
                            }
                        }
                    }

                    foreach (Soldier s in _soldiers)
                    {
                        s.alterDestination(true, -amount);
                    }

                    if (_supportRows.Count != 0)
                    {
                        foreach (ArrayList row in _supportRows)
                        {
                            float chargeY = (float)(Soldier.HEIGHT * (row.Count - 1)) / -2f;
                            foreach (Soldier s in row)
                            {
                                if (s is CrossbowmanPavise)
                                {
                                    ((CrossbowmanPavise)s).setChargePosition(new Vector2(100f, chargeY));
                                    chargeY += Soldier.HEIGHT;
                                }
                            }
                        }
                    }
                }
            }

        }

        public void marchRight(double milliseconds, bool diagonal)
        {
            if (!checkFormationCollided() || mostSoldiersRight())
            {
                Soldier firstSoldier;
                Soldier lastSoldier;
                float amount;
                float speed = _speed - (float)getSlowedSoldiers();

                if (getCenter().X + this.getWidth() * Soldier.WIDTH / 2 < _screen.getMapOffset().X + PikeAndShotGame.SCREENWIDTH || _side == BattleScreen.SIDE_ENEMY)
                {
                    if (diagonal)
                        amount = speed * (float)milliseconds * 0.708f;
                    else
                        amount = speed * (float)milliseconds;

                    _position.X += amount;

                    if (_state == STATE_SHOT && _shotRows.Count != 0)
                    {
                        firstSoldier = getFirstNonMeleeSoldier((ArrayList)_shotRows[0]);
                        if (_meleeRows.Count > 0)
                        {
                            lastSoldier = getFirstNonMeleeSoldier((ArrayList)_meleeRows[_meleeRows.Count - 1]);
                            if (lastSoldier != null)
                            {
                                if (!((_side * lastSoldier._destination.X - _side * lastSoldier._position.X) > _speed || (_side * lastSoldier._destination.X - _side * lastSoldier._position.X) < -_speed))
                                {
                                    lastSoldier = null;
                                }
                            }
                        }
                        else
                            lastSoldier = null;

                        if (lastSoldier == null)
                        {
                            if (_pikeRows.Count > 0)
                            {
                                lastSoldier = getFirstNonMeleeSoldier((ArrayList)_pikeRows[_pikeRows.Count - 1]);
                                if (lastSoldier != null)
                                {
                                    if (!((_side * lastSoldier._destination.X - _side * lastSoldier._position.X) > _speed || (_side * lastSoldier._destination.X - _side * lastSoldier._position.X) < -_speed))
                                    {
                                        lastSoldier = null;
                                    }
                                }
                            }
                            else
                                lastSoldier = null;
                        }

                        if (lastSoldier == null)
                        {
                            if (_shotRows.Count > 1)
                            {
                                lastSoldier = getFirstNonMeleeSoldier((ArrayList)_shotRows[_shotRows.Count - 1]);
                                if (lastSoldier != null)
                                {
                                    if (!((_side * lastSoldier._destination.X - _side * lastSoldier._position.X) > _speed || (_side * lastSoldier._destination.X - _side * lastSoldier._position.X) < -_speed))
                                    {
                                        lastSoldier = null;
                                    }
                                }
                            }
                            else
                                lastSoldier = null;
                        }

                        if (firstSoldier != null && lastSoldier != null)
                        {
                            if ((_side * firstSoldier._destination.X - _side * firstSoldier._position.X) > _speed)
                            {
                                _position.X = firstSoldier._position.X;
                                resetupFormation();
                            }
                        }
                    }
                    else if (_state == STATE_PIKE && _pikeRows.Count != 0)
                    {
                        firstSoldier = getFirstNonMeleeSoldier((ArrayList)_pikeRows[0]);

                        if (_shotRows.Count > 0)
                        {
                            lastSoldier = getFirstNonMeleeSoldier((ArrayList)_shotRows[_shotRows.Count - 1]);
                            if (lastSoldier != null)
                            {
                                if (!((_side * lastSoldier._destination.X - _side * lastSoldier._position.X) > _speed || (_side * lastSoldier._destination.X - _side * lastSoldier._position.X) < -_speed))
                                {
                                    lastSoldier = null;
                                }
                            }
                        }
                        else
                            lastSoldier = null;

                        if (lastSoldier == null)
                        {
                            if (_meleeRows.Count > 0)
                            {
                                lastSoldier = getFirstNonMeleeSoldier((ArrayList)_meleeRows[_meleeRows.Count - 1]);
                                if (lastSoldier != null)
                                {
                                    if (!((_side * lastSoldier._destination.X - _side * lastSoldier._position.X) > _speed || (_side * lastSoldier._destination.X - _side * lastSoldier._position.X) < -_speed))
                                    {
                                        lastSoldier = null;
                                    }
                                }
                            }
                            else
                                lastSoldier = null;
                        }

                        if (firstSoldier != null && lastSoldier != null)
                        {
                            if ((_side * firstSoldier._destination.X - _side * firstSoldier._position.X) > _speed)
                            {
                                _position.X = firstSoldier._position.X;
                                resetupFormation();
                            }
                        }
                    }
                    else if (_state == STATE_MELEE && _meleeRows.Count != 0)
                    {
                        firstSoldier = getFirstNonMeleeSoldier((ArrayList)_meleeRows[0]);
                        if (_shotRows.Count > 0)
                        {
                            lastSoldier = getFirstNonMeleeSoldier((ArrayList)_shotRows[_shotRows.Count - 1]);
                            if (lastSoldier != null)
                            {
                                if (!((_side * lastSoldier._destination.X - _side * lastSoldier._position.X) > _speed || (_side * lastSoldier._destination.X - _side * lastSoldier._position.X) < -_speed))
                                {
                                    lastSoldier = null;
                                }
                            }
                        }
                        else
                            lastSoldier = null;

                        if (lastSoldier == null)
                        {
                            if (_pikeRows.Count > 0)
                            {
                                lastSoldier = getFirstNonMeleeSoldier((ArrayList)_pikeRows[_pikeRows.Count - 1]);
                                if (lastSoldier != null)
                                {
                                    if (!((_side * lastSoldier._destination.X - _side * lastSoldier._position.X) > _speed || (_side * lastSoldier._destination.X - _side * lastSoldier._position.X) < -_speed))
                                    {
                                        lastSoldier = null;
                                    }
                                }
                            }
                            else
                                lastSoldier = null;
                        }

                        if (firstSoldier != null && lastSoldier != null)
                        {
                            if ((_side * firstSoldier._destination.X - _side * firstSoldier._position.X) > _speed)
                            {
                                _position.X = firstSoldier._position.X;
                                resetupFormation();
                            }
                        }
                    }

                    foreach (Soldier s in _soldiers)
                    {
                        s.alterDestination(true, amount);
                    }

                    if (_supportRows.Count != 0)
                    {
                        foreach (ArrayList row in _supportRows)
                        {
                            float chargeY = (float)(Soldier.HEIGHT * (row.Count - 1)) / -2f;
                            foreach (Soldier s in row)
                            {
                                if (s is CrossbowmanPavise)
                                {
                                    ((CrossbowmanPavise)s).setChargePosition(new Vector2(100f, chargeY));
                                    chargeY += Soldier.HEIGHT;
                                }
                            }
                        }
                    }
                }
            }
        }

        private bool checkFormationCollided()
        {
            return !(collisions < _soldiers.Count / 5 || (_soldiers.Count < 5 && collisions < 1));
        }

        private Soldier getFirstNonMeleeSoldier(ArrayList soldierList)
        {
            int i = 0;

            Soldier soldier = (Soldier)soldierList[i];

            while (soldier.getState() == Soldier.STATE_MELEE_WIN || soldier.getState() == Soldier.STATE_MELEE_LOSS || soldier.getState() == Pikeman.STATE_TUG || soldier.getState() == Soldier.STATE_DYING)
            {
                soldier = (Soldier)soldierList[i];
                i++;
                if (i == soldierList.Count)
                    break;
            }

            if (soldier.getState() == Soldier.STATE_MELEE_WIN || soldier.getState() == Soldier.STATE_MELEE_LOSS || soldier.getState() == Pikeman.STATE_TUG || soldier.getState() == Soldier.STATE_DYING)
                soldier = null;

            return soldier;
        }

        public int getForwardShotRow()
        {
            Soldier firstNonMeleeSoldier;
            int forwardRow = -1;
            int index = 0;
            float mostforwardX = -1f;
            foreach (ArrayList al in _shotRows)
            {
                firstNonMeleeSoldier = getFirstNonMeleeSoldier(al);
                if (firstNonMeleeSoldier != null)
                {
                    if (firstNonMeleeSoldier._destination.X > mostforwardX)
                    {
                        mostforwardX = firstNonMeleeSoldier._destination.X;
                        forwardRow = index;
                    }
                }
                index++;
            }
            return forwardRow;
        }

        public void increaseWidth()
        {
            if (_width < getBiggestTroopTypeTotal())
            {
                _width++;
                reformFormation();
                resetupFormation();
            }
        }

        public void reduceWidth()
        {
            if (_width > 5)
            {
                _width--;
                reformFormation();
                resetupFormation();
            }
        }

        private int getBiggestTroopTypeTotal()
        {
            int biggestTotal = 0;
            int shotTotal = 0;
            int meleeTotal = 0;
            int cavTotal = 0;
            int supportTotal = 0;

            foreach (ArrayList al in _shotRows)
            {
                shotTotal += al.Count;
            }
            biggestTotal = shotTotal;

            foreach (ArrayList al in _pikeRows)
            {
                meleeTotal += al.Count;
            }
            if (meleeTotal > biggestTotal)
                biggestTotal = meleeTotal;

            /*foreach (ArrayList al in _swingerRows)
            {
                swingerTotal += al.Count;
            }
            if (swingerTotal > biggestTotal)
                biggestTotal = swingerTotal;
            */

            foreach (ArrayList al in _cavRows)
            {
                cavTotal += al.Count;
            }
            if (cavTotal > biggestTotal)
                biggestTotal = cavTotal;

            foreach (ArrayList al in _supportRows)
            {
                supportTotal += al.Count;
            }
            if (supportTotal > biggestTotal)
                biggestTotal = supportTotal;

            return biggestTotal;
        }

        internal int getTotalRows()
        {
            return _cavRows.Count + _meleeRows.Count + _pikeRows.Count + _shotRows.Count + _supportRows.Count /*+ _swingerRows.Count*/;
        }

        internal float getSpeed()
        {
            return _speed;
        }

        internal Vector2 getCenter()
        {
            float x = 0f;
            float y = 0f;

            if (_side == BattleScreen.SIDE_PLAYER)
            {
                // the -1 on the rows is for the fact that the position is behind the first guy
                // the -6 is to get use to the start of the first soldier and not the start of his hitbox
                float value = (((float)getTotalRows()) * 0.5f * Soldier.WIDTH);
                x = _position.X - 6f + Soldier.WIDTH - value;
                y = _position.Y + _width * 0.5f * Soldier.HEIGHT;
            }
            else
            {
                x = _position.X - 6f + (float)getTotalRows() * 0.5f * Soldier.WIDTH;
                y = _position.Y + _width * 0.5f * Soldier.HEIGHT;
            }

            return new Vector2(x, y);
        }

        internal bool hasSoldierOnScreen()
        {
            if (_soldiers.Count <= 0)
                return false;
            else if (!hasAppeared)
                return true;

            foreach (Soldier s in _soldiers)
            {
                if (s._position.X + Soldier.WIDTH * 2f - (_screen.getMapOffset().X) > 0 && s._position.X - Soldier.WIDTH * 2f - (_screen.getMapOffset().X) < PikeAndShotGame.SCREENWIDTH)
                    return true;
            }
            return false;
        }

        internal int getLeastType()
        {
            int shot = 0;
            int pike = 0;
            foreach (ArrayList row in _shotRows)
            {
                foreach (Soldier soldier in row)
                    shot++;
            }
            foreach (ArrayList row in _pikeRows)
            {
                foreach (Soldier soldier in row)
                    pike++;
            }
            if (pike < shot)
                return Soldier.TYPE_PIKE;
            else
                return Soldier.TYPE_SHOT;
        }

        internal void retreat()
        {
            retreated = true;
            foreach(Soldier soldier in _soldiers)
            {
                _screen.addLooseSoldierNext(soldier);
                _soldiersToRemove.Add(soldier);
                soldier.route();
            }
        }

    }

    public class EnemyFormation : Formation, LevelEditorGrabbable
    {
        public List<PatternAction> _pattern;
        private int _currentAction;
        private double _currentDuration;
        private bool _pikesRaised; //Gotta have this flag otherwise enemy formations keep trying to raise all the time and mess up shots
        public Spawner spawner;
        public string name;
        public int index { get; set; }

        public EnemyFormation(string name, List<PatternAction> pattern, BattleScreen screen, float x, float y, int initialCapacity, int side)
            : base(screen, x, y, initialCapacity, side)
        {
            this.name = name;
            _pattern = pattern;
            _currentAction = 0;
            if(pattern != null && pattern.Count > 0)
                _currentDuration = ((PatternAction)pattern[0]).duration;

            _pikesRaised = false;
        }

        public EnemyFormation(string name, List<PatternAction> pattern, BattleScreen screen, float x, float y, int initialCapacity, int side, int index)
            : base(screen, x, y, initialCapacity, side)
        {
            this.name = name;
            _pattern = pattern;
            _currentAction = 0;
            if (pattern != null && pattern.Count > 0)
                _currentDuration = ((PatternAction)pattern[0]).duration;

            _pikesRaised = false;
            this.index = index;
        }

        public override void update(TimeSpan timeSpan)
        {
            base.update(timeSpan);

            if (this.getSoldiers().Count <= 0)
                return;

            if (_pattern == null)
            {
                _position = new Vector2(_screen.getPlayerFormation().getCenter().X, _screen.getPlayerFormation().getCenter().Y);
                foreach (Soldier s in this.getSoldiers())
                {
                    s._destination = new Vector2(_position.X, _position.Y);
                    s.update(timeSpan);
                }
            }
            else
            {
                for (int i = 0; i < ((PatternAction)_pattern[_currentAction]).count(); i++)
                {
                    switch ((int)((PatternAction)_pattern[_currentAction]).actions[i])
                    {
                        case PatternAction.ACTION_LEFT:
                            this.marchLeft(timeSpan.TotalMilliseconds, false);
                            break;
                        case PatternAction.ACTION_UP:
                            this.marchUp(timeSpan.TotalMilliseconds, false);
                            break;
                        case PatternAction.ACTION_RIGHT:
                            this.marchRight(timeSpan.TotalMilliseconds, false);
                            break;
                        case PatternAction.ACTION_DOWN:
                            this.marchDown(timeSpan.TotalMilliseconds, false);
                            break;
                        case PatternAction.ACTION_PIKE:
                            this.pikeAttack();
                            break;
                        case PatternAction.ACTION_RAISE:
                            if (!_pikesRaised)
                            {
                                this.pikeRaise();
                                _pikesRaised = true;
                            }
                            break;
                        case PatternAction.ACTION_SHOT:
                            this.shotAttack();
                            break;
                        case PatternAction.ACTION_CHARGE:
                            this.meleeCharge();
                            break;
                        case PatternAction.ACTION_CAVALRY_HALT:
                            this.haltHorses();
                            break;
                        case PatternAction.ACTION_CAVALRY_TURN:
                            this.turnHorses();
                            break;
                        case PatternAction.ACTION_RELOAD:
                            this.reload();
                            break;
                        case PatternAction.ACTION_SPAWN:
                            this.spawn();
                            break;
                        default:
                            //reformFormation();
                            //resetupFormation();
                            break;
                    }
                }

                _currentDuration -= timeSpan.TotalMilliseconds;

                if (_currentDuration <= 0)
                {
                    if (_currentAction < _pattern.Count - 1)
                        _currentAction++;
                    else
                        _currentAction = 0;

                    _currentDuration = ((PatternAction)_pattern[_currentAction]).duration + _currentDuration;
                    _pikesRaised = false;
                }
            }
        }
    }

    public class ColmillosFormation : Formation
    {
        public const int STATE_INTRO = 300;
        public const int STATE_HOLD = 301;
        public const int STATE_CHARGE = 302;
        public const int STATE_END = 303;

        const float CHARGE_TIME = 1500f;
        const float CHARGE_SPEED = 1.25f;
        const float WOLF_PERIOD = 650f;
        private const float WOLVES_TIME = 3000f;
        private const float WOLVES_INTERVAL = 1000f;
        private const int WOLVES_LAUNCH = 3;
        
        float wolvesTimer;
        Vector2[] destinations;
        Vector2 _delta;
        Vector2 _travel;
        int curDest = 0;
        bool holdUp = true;
        float edgeDist = 150f;
        float holdWaveTimer = 0;
        float patternTimer = 0;
        float chargeRecoverTimer = CHARGE_TIME;
        float wolfSpacing;
        int wolfCount;
        int launches = 0;
        public int numberOfWolves = 20;

        public Colmillos colmillos;

        public ColmillosFormation(BattleScreen screen, float x, float y)
            : base(screen, x, y, 21, BattleScreen.SIDE_ENEMY)
        {
            colmillos = new Colmillos(screen, x, y, BattleScreen.SIDE_ENEMY);
            addSoldier(colmillos);
            for (int i = 0; i < numberOfWolves; i++)
                addSoldier(new Wolf(screen, x, y, BattleScreen.SIDE_ENEMY));

            _state = STATE_INTRO;
            destinations = new Vector2[2];
            destinations[0] = new Vector2(PikeAndShotGame.SCREENWIDTH * 3 / 4, PikeAndShotGame.SCREENHEIGHT - 200);
            destinations[1] = new Vector2(PikeAndShotGame.SCREENWIDTH * 3 / 4, 100);

            _delta = new Vector2(0, 0);
            _travel = new Vector2(0, 0);

            wolfSpacing = (MathHelper.Pi * 2f) / (float)numberOfWolves;
        }

        public override void addSoldier(Soldier soldier)
        {
            base.addSoldier(soldier);
            if (soldier is Wolf)
            {
                ((Wolf)soldier).bossFormation = this;
                soldier.setSpeed(0.24f);
            }
            
        }

        public bool attacked = false;
        bool wolvesArrived = false;
        float wolvesEatTimer = 0;
        int eatCount = 15;
        const float EAT_TIME = 375f;

        public override void update(TimeSpan timeSpan)
        {
 	        base.update(timeSpan);
            _speed = colmillos._speed - 0.04f;

            //boss behaviour code
            if (_state == STATE_CHARGE)
            {
                if (colmillos.hurtTimer > 0f)
                {
                    colmillos.hurtTimer -= (float)timeSpan.TotalMilliseconds;
                    if (colmillos.hurtTimer <= 0f)
                    {
                        colmillos.hurtTimer = 0f;
                    }
                }
                if (((Soldier)_soldiers[_soldiers.Count-1]).getState() != Wolf.STATE_BARK)
                {
                    if (!attacked && colmillos.getState() != Colmillos.STATE_RUN && colmillos.getState() != Colmillos.STATE_ATTACK && colmillos.getState() != Colmillos.STATE_SHIELDBREAK && colmillos.getState() != Colmillos.STATE_DYING)
                    {
                        colmillos.run();

                        float spacing = (float)(PikeAndShotGame.SCREENHEIGHT) / (float)(numberOfWolves);
                        int i = 0;

                        foreach(Soldier s in _soldiers)
                        {
                            if (s is Wolf && s.getState() != Wolf.STATE_SPOOKED && s.getState() != Wolf.STATE_FLEE && !((Wolf)s).flee)
                            {
                                Wolf wolf = (Wolf)s;
                                removeSoldier(wolf);
                                _screen.addLooseSoldier(wolf);
                                wolf.alterDestination(true, -PikeAndShotGame.SCREENWIDTH);
                                if (i % 2 == 0)
                                    wolf._destination.Y = PikeAndShotGame.SCREENHEIGHT / 2 + (i / 2) * spacing;
                                else
                                    wolf._destination.Y = PikeAndShotGame.SCREENHEIGHT / 2 - (i / 2) * spacing;

                                i++;

                                if (wolf.getState() == Wolf.STATE_TURNING)
                                    wolf.setState(Soldier.STATE_READY);

                                if (wolf._turned)
                                    wolf.turn();

                                wolf._runningFeet.setAnimationSpeed(wolf._footSpeed / 0.11f);
                                reduceWolfNumber();
                            }
                        }
                    }

                    Vector2 target;
                    if (colmillos.getState() != Colmillos.STATE_SHIELDBREAK)
                    {
                        if (!attacked)
                        {
                            target = _screen.getPlayerFormation().getCenter();
                            _delta = target - _position;
                        }
                        else
                        {
                            target = new Vector2(_screen.getMapOffset().X + 800, PikeAndShotGame.SCREENHEIGHT / 2);
                            _delta = target - _position;
                            if (_delta.X <= 0)
                            {
                                colmillos.howl();
                                _state = STATE_HOLD;
                                colmillos.setSpeed(0.15f);
                                chargeRecoverTimer = 0;

                                int wolvesToAdd = 20 - numberOfWolves;
                                for (int j = 0; j < wolvesToAdd; j++)
                                {
                                    addSoldier(new Wolf(_screen, _screen.getMapOffset().X + PikeAndShotGame.SCREENWIDTH+20f, _position.Y, BattleScreen.SIDE_ENEMY));
                                    increaseNumberOfWolves();
                                }
                            }
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

                        if (_delta.X > 0)
                        {
                            if (_delta.X - _travel.X >= 0)
                                _position.X += _travel.X;
                            else
                                _position.X = target.X;
                        }
                        else if (_delta.X < 0)
                        {
                            if (_delta.X + _travel.X <= 0)
                                _position.X -= _travel.X;
                            else
                                _position.X = target.X;
                        }

                        if (_delta.Y > 0)
                        {
                            if (_delta.Y - _travel.Y >= 0)
                                _position.Y += _travel.Y;
                            else
                                _position.Y = target.Y;
                        }
                        else if (_delta.Y < 0)
                        {
                            if (_delta.Y + _travel.Y <= 0)
                                _position.Y -= _travel.Y;
                            else
                                _position.Y = target.Y;
                        }


                        if (colmillos.getState() != Colmillos.STATE_RISE && colmillos.getState() != Colmillos.STATE_DYING && colmillos.getState() != Colmillos.STATE_EATEN)
                        {
                            colmillos._destination = _position;
                        }
                    }
                }
            }
            else if (_state == STATE_END)
            {

                Vector2 target;
                if (colmillos.getState() == Colmillos.STATE_STAGGER)
                    target = new Vector2(_screen.getMapOffset().X + 800, PikeAndShotGame.SCREENHEIGHT / 2);
                else
                    target = colmillos.getPosition();

                _delta = target - _position;
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

                if (_delta.X > 0)
                {
                    if (_delta.X - _travel.X >= 0)
                        _position.X += _travel.X;
                    else
                        _position.X = destinations[curDest].X;
                }
                else if (_delta.X < 0)
                {
                    if (_delta.X + _travel.X <= 0)
                        _position.X -= _travel.X;
                    else
                        _position.X = destinations[curDest].X;
                }

                if (_delta.Y > 0)
                {
                    if (_delta.Y - _travel.Y >= 0)
                        _position.Y += _travel.Y;
                    else
                        _position.Y = destinations[curDest].Y;
                }
                else if (_delta.Y < 0)
                {
                    if (_delta.Y + _travel.Y <= 0)
                        _position.Y -= _travel.Y;
                    else
                        _position.Y = destinations[curDest].Y;
                }

                if (colmillos.getState() != Colmillos.STATE_RISE && colmillos.getState() != Colmillos.STATE_DYING && colmillos.getState() != Colmillos.STATE_EATEN)
                    colmillos._destination = _position;

                if (!wolvesArrived)
                {
                    wolfSpacing = (MathHelper.Pi * 2f) / (float)numberOfWolves;

                    float tempPatternTimer = patternTimer;
                    int i = 1;
                    float xDist;
                    float yDist;
                    foreach (Soldier w in _soldiers)
                    {
                        if (w is Wolf)
                        {
                            if (i % 2 > 0)
                            {
                                xDist = 0.75f;
                                yDist = 0.45f;
                            }
                            else
                            {
                                xDist = 0.50f;
                                yDist = 0.30f;
                            }
                            if (w.getState() != Wolf.STATE_SPOOKED)
                            {
                                w._destination.X = colmillos._position.X + this.getWidth() * xDist * Soldier.WIDTH * (float)Math.Cos((double)tempPatternTimer);
                                w._destination.Y = colmillos._position.Y + this.getWidth() * yDist * Soldier.WIDTH * (float)Math.Sin((double)tempPatternTimer);
                            }
                            tempPatternTimer += wolfSpacing;

                            float a = (this.getWidth() * xDist * Soldier.WIDTH * (float)Math.Abs((float)Math.Cos((double)tempPatternTimer))) / (this.getWidth() * xDist * Soldier.WIDTH);

                            if (i % 2 == 0)
                                a = a * 2f / 3f;

                            ((Wolf)w)._runningFeet.setAnimationSpeed((w._footSpeed * 2 - w._footSpeed * (a)) / 0.11f);

                            if (((w._destination.X > w._position.X && !((Wolf)w)._turned) || (w._destination.X < w._position.X && ((Wolf)w)._turned)) && (w.getState() != Wolf.STATE_TURNING))
                            {
                                ((Wolf)w).turn();
                            }
                            i++;
                        }
                    }

                    bool arrived = true;
                    foreach (Soldier wolf in _soldiers)
                    {
                        if (wolf is Wolf && wolf.getPosition().X != wolf.getDestination().X)
                            arrived = false;
                    }
                    if (arrived)
                        wolvesArrived = true;
                }
                else
                {
                    wolvesEatTimer -= (float)timeSpan.TotalMilliseconds;
                    if (wolvesEatTimer <= 0)
                    {
                        wolvesEatTimer = EAT_TIME;
                        if (eatCount > 0)
                        {
                            if (eatCount % 3 == 0)
                                ((Wolf)_soldiers[eatCount - 1]).howl();
                            else
                                ((Wolf)_soldiers[eatCount - 1]).attackColmillos();

                            eatCount--;
                        }
                    }
                    foreach (Soldier wolf in _soldiers)
                    {
                        if (wolf is Wolf)
                        {
                            if (wolf.getPosition() == wolf.getDestination() && wolf.getPosition() == wolf._meleeDestination && wolf.getState() == Soldier.STATE_READY)
                            {
                                ((Wolf)wolf).kill();
                            }
                            else if (((wolf._destination.X > wolf._position.X && !((Wolf)wolf)._turned) || (wolf._destination.X < wolf._position.X && ((Wolf)wolf)._turned)) && (wolf.getState() != Wolf.STATE_TURNING))
                            {
                                ((Wolf)wolf).turn();
                            }
                        }
                    }
                }
            }
            else if (_state == STATE_INTRO)
            {
                Vector2 target;

                target = _screen.getMapOffset() + new Vector2(800f, 500f);
                _delta = target - _position;
               
                if (Math.Abs(_delta.X) == 0)
                {
                    colmillos.howl();
                    _state = STATE_HOLD;
                    colmillos.setSpeed(0.15f);
                    chargeRecoverTimer = 0;
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

                if (_delta.X > 0)
                {
                    if (_delta.X - _travel.X >= 0)
                        _position.X += _travel.X;
                    else
                        _position.X = target.X;
                }
                else if (_delta.X < 0)
                {
                    if (_delta.X + _travel.X <= 0)
                        _position.X -= _travel.X;
                    else
                        _position.X = target.X;
                }

                if (_delta.Y > 0)
                {
                    if (_delta.Y - _travel.Y >= 0)
                        _position.Y += _travel.Y;
                    else
                        _position.Y = target.Y;
                }
                else if (_delta.Y < 0)
                {
                    if (_delta.Y + _travel.Y <= 0)
                        _position.Y -= _travel.Y;
                    else
                        _position.Y = target.Y;
                }


                if (colmillos.getState() != Colmillos.STATE_RISE && colmillos.getState() != Colmillos.STATE_DYING && colmillos.getState() != Colmillos.STATE_EATEN)
                {
                    colmillos._destination = _position;
                }
            }
            else if (_state == STATE_HOLD)
            {
                if (colmillos.hurtTimer > 0f)
                {
                    colmillos.hurtTimer -= (float)timeSpan.TotalMilliseconds;
                    if (colmillos.hurtTimer <= 0f)
                    {
                        colmillos.hurtTimer = 0f;
                    }
                }
                if (colmillos.getState() != Colmillos.STATE_HOWL && chargeRecoverTimer > CHARGE_TIME)
                {
                    attacked = false;
                    holdWaveTimer += (float)timeSpan.TotalSeconds * 2f;

                    if (holdWaveTimer > MathHelper.Pi * 2f)
                        holdWaveTimer -= (MathHelper.Pi * 2f);

                    _delta.X = (float)Math.Cos(this.holdWaveTimer) / 2f * (float)timeSpan.TotalMilliseconds * _speed;
                    _delta.Y = (holdUp ? -1 : 1) * (float)timeSpan.TotalMilliseconds * _speed;

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

                    if (_delta.X > 0)
                    {
                        if (_delta.X - _travel.X >= 0)
                            _position.X += _travel.X;
                        else
                            _position.X = destinations[curDest].X;
                    }
                    else if (_delta.X < 0)
                    {
                        if (_delta.X + _travel.X <= 0)
                            _position.X -= _travel.X;
                        else
                            _position.X = destinations[curDest].X;
                    }

                    if (_delta.Y > 0)
                    {
                        if (_delta.Y - _travel.Y >= 0)
                            _position.Y += _travel.Y;
                        else
                            _position.Y = destinations[curDest].Y;
                    }
                    else if (_delta.Y < 0)
                    {
                        if (_delta.Y + _travel.Y <= 0)
                            _position.Y -= _travel.Y;
                        else
                            _position.Y = destinations[curDest].Y;
                    }

                    if (holdUp)
                    {
                        if (_position.Y < this.edgeDist)
                        {
                            holdUp = !holdUp;
                        }
                    }
                    else
                    {
                        if (_position.Y > PikeAndShotGame.SCREENHEIGHT - this.edgeDist)
                        {
                            holdUp = !holdUp;
                        }
                    }

                    if (colmillos.getState() != Colmillos.STATE_RISE && colmillos.getState() != Colmillos.STATE_DYING && colmillos.getState() != Colmillos.STATE_EATEN)
                        colmillos._destination = _position;

                    patternTimer += (float)timeSpan.TotalMilliseconds / WOLF_PERIOD;
                }
                else
                {
                    chargeRecoverTimer += (float)timeSpan.TotalMilliseconds;
                }

                wolfSpacing = (MathHelper.Pi * 2f) / (float)numberOfWolves;

                float tempPatternTimer = patternTimer;
                int i = 1;
                float xDist;
                float yDist;
                foreach (Soldier w in _soldiers)
                {
                    if (w is Wolf)
                    {
                        if (i % 2 > 0)
                        {
                            xDist = 0.75f;
                            yDist = 0.45f;
                        }
                        else
                        {
                            xDist = 0.50f;
                            yDist = 0.30f;
                        }
                        if (w.getState() != Wolf.STATE_SPOOKED)
                        {
                            w._destination.X = colmillos._position.X + this.getWidth() * xDist * Soldier.WIDTH * (float)Math.Cos((double)tempPatternTimer);
                            w._destination.Y = colmillos._position.Y + this.getWidth() * yDist * Soldier.WIDTH * (float)Math.Sin((double)tempPatternTimer);
                        }
                        tempPatternTimer += wolfSpacing;

                        float a = (this.getWidth() * xDist * Soldier.WIDTH * (float)Math.Abs((float)Math.Cos((double)tempPatternTimer))) / (this.getWidth() * xDist * Soldier.WIDTH);

                        if (i % 2 == 0)
                            a = a * 2f / 3f;

                        ((Wolf)w)._runningFeet.setAnimationSpeed((w._footSpeed * 2 - w._footSpeed * (a)) / 0.11f);

                        if (((w._destination.X > w._position.X && !((Wolf)w)._turned) || (w._destination.X < w._position.X && ((Wolf)w)._turned)) && (w.getState() != Wolf.STATE_TURNING))
                        {
                            ((Wolf)w).turn();
                        }
                        i++;
                    }
                }
                wolvesTimer += (float)timeSpan.TotalMilliseconds;
                if (wolfCount > 0)
                {
                    if (wolvesTimer >= WOLVES_INTERVAL)
                    {
                        wolvesTimer = 0;
                        wolfCount--;
                        launchWolf();
                    }
                }
                else if (wolvesTimer >= WOLVES_TIME)
                {
                    wolvesTimer = 0;
                    wolfCount = WOLVES_LAUNCH - 1;
                    launchWolf();
                }
            }
        }

        void launchWolf()
        {
            float leastX = _screen.getMapOffset().X + PikeAndShotGame.SCREENWIDTH;
            Wolf wolf = null;
            foreach (Soldier s in _soldiers)
            {
                if (s is Wolf && s._position.X < leastX && s.getState() != Wolf.STATE_SPOOKED && s.getState() != Wolf.STATE_FLEE && !((Wolf)s).flee)
                {
                    leastX = s._position.X;
                    wolf = (Wolf)s;
                }
            }
            if (wolf != null)
            {
                launches++;
                if (launches > WOLVES_LAUNCH)
                {
                    launches = 0;
                    attacked = false;
                    //colmillos.howl();
                    foreach (Soldier s in _soldiers)
                    {
                        if (s is Wolf && s.getState() != Wolf.STATE_FLEE && !((Wolf)s).flee)
                        {
                            ((Wolf)s).bark();
                            s.setSpeed(0.2f);
                        }
                    }
                    colmillos.setSpeed(0.2f);
                    _state = STATE_CHARGE;
                }
                else
                {
                    removeSoldier(wolf);
                    _screen.addLooseSoldier(wolf);
                    wolf.alterDestination(true, -PikeAndShotGame.SCREENWIDTH);
                    wolf._destination.Y = wolf.getPosition().Y;
                    wolf.setSpeed(0.24f);

                    if (wolf.getState() == Wolf.STATE_TURNING)
                        wolf.setState(Soldier.STATE_READY);

                    if (wolf._turned)
                        wolf.turn();

                    wolf._runningFeet.setAnimationSpeed(wolf._footSpeed / 0.11f);
                    reduceWolfNumber();
                }
            }
        }

        internal void reduceWolfNumber()
        {
            numberOfWolves--;
            if (numberOfWolves > 0)
                patternTimer += ((float)Math.PI * 2f) / numberOfWolves;
        }

        internal void increaseNumberOfWolves()
        {
            numberOfWolves++;
            if(numberOfWolves>1)
                patternTimer += ((float)Math.PI * 2f) / (numberOfWolves-1);
        }


        internal void setAttacked()
        {
            attacked = true;
            foreach (Soldier s in _soldiers)
            {
                if (s is Wolf)
                    ((Wolf)s).retreatStart();
            }
        }

        internal void setEnd()
        {
            _state = STATE_END;
            _position = colmillos.getPosition();
            _screen.playerInPlay = false;
            foreach (Soldier w in _soldiers)
            {
                if (w is Wolf)
                {
                    ((Wolf)w).fleeStart();
                }
            }
            int wolvesToAdd = 15;
            for (int j = 0; j < wolvesToAdd; j++)
            {
                addSoldier(new Wolf(_screen, _screen.getMapOffset().X + PikeAndShotGame.SCREENWIDTH + 20f, _position.Y, BattleScreen.SIDE_ENEMY));
                increaseNumberOfWolves();
            }
        }
    }

    public class Subformation : Formation
    {
        private Formation _parent;

        public Subformation(Formation parent, BattleScreen screen, float x, float y, int side)
            : base(screen, x, y, 20, side)
        {
            _parent = parent;
        }

        public override void update(TimeSpan timeSpan)
        {
            base.update(timeSpan);
            Vector2 dXdY = _parent.getCenter() - this.getCenter();
            float distanceToParent = (float)Math.Sqrt(Math.Pow(dXdY.X, 2d) + Math.Pow(dXdY.Y, 2d));

            if (distanceToParent > (float)(_parent.getWidth() * Soldier.HEIGHT))
            {

            }

        }

    }
}

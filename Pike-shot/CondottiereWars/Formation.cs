#define DRAW_FORMATION_DOTS

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        private BattleScreen _screen;
        private Vector2 _position;
        private ArrayList _soldiers;
        private ArrayList _soldiersToRemove;
        private ArrayList _soldiersToMove;
        private ArrayList _shotRows;
        private ArrayList _pikeRows;
        private ArrayList _meleeRows;
        //private ArrayList _swingerRows;
        private ArrayList _cavRows;
        private ArrayList _supportRows;
        private ArrayList _enemiesToGuard;
        private ArrayList _enemiesToShoot;
        private ArrayList _shotsToBlock;

        private Vector2 _size;
        private int _side;
        private int _width;
        private float _speed;
        private float avgSpeed;
        private int _state;
        public bool needTriggerUp;
        private bool _soldierDied;
        private bool _allShotsMade;
        private bool _needResetupFormation;
        private bool _addedSoldier;
        private bool DEBUGdangerClose;

        public Formation(BattleScreen screen, float x, float y, int initialCapacity, int side)
        {
            _screen = screen;
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
            if (_side == BattleScreen.SIDE_PLAYER)
                _speed = 0.1f;
                //_speed = 0.2f;

            _state = STATE_SHOT;
            
            _side = side;
            needTriggerUp = false;
            _needResetupFormation = false;
            _addedSoldier = false;
            DEBUGdangerClose = false;
        }

        public int getWidth()
        {
            return _width;
        }

        public virtual void update(TimeSpan timeSpan)
        {
            _soldierDied = false;
            _allShotsMade = true;
            bool isDangerClose = dangerClose();
            bool answer = false;

            dangerBefore();

            foreach (Soldier pike in _soldiers)
            {
                if (pike.guardTarget != null)
                {
                    answer = Math.Abs(pike.guardTarget.getPosition().Y - pike._destination.Y) >= pike.getHeight();

                    if (pike.guardTarget.getState() == Soldier.STATE_DYING || pike.guardTarget.getState() == Soldier.STATE_DEAD
                    || pike.guardTarget.getState() == Soldier.STATE_MELEE_WIN || pike.guardTarget.getState() == Soldier.STATE_MELEE_LOSS
                    || pike.guardTarget.getState() == Soldier.STATE_ROUTE || pike.guardTarget.getState() == Soldier.STATE_ROUTED
                    || pike.guardTarget.getState() == Soldier.STATE_RETREAT
                    || Math.Abs(pike.guardTarget.getPosition().X - this._position.X) > Soldier.WIDTH * 10f
                    || (answer))
                    {
                        pike.guardTarget = null;
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
                            if (pikeman is Pikeman && (pikeman.getState() != Soldier.STATE_MELEE_WIN && pikeman.getState() != Soldier.STATE_MELEE_LOSS))
                                ((Pikeman)pikeman).lower45();
                        }
                        DEBUGdangerClose = true;
                    }
                    else
                    {
                        foreach (Soldier pikeman in (ArrayList)_pikeRows[0])
                        {
                            if (pikeman is Pikeman && (pikeman.getState() != Soldier.STATE_MELEE_WIN && pikeman.getState() != Soldier.STATE_MELEE_LOSS))
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
                        }
                        if (_pikeRows.Count > 1)
                        {
                            foreach (Pikeman pike in (ArrayList)_pikeRows[1])
                            {
                                pike.guardTarget = null;
                            }
                        }
                    }
                }

                if (isDangerClose)
                {
                    foreach (Soldier enemy in _enemiesToGuard)
                    {
                        assignDoppel(enemy);
                    }
                }
            }

            if (_shotRows.Count > 0)
            {
                
                if (_shotRows.Count > 1)
                {
                    ArrayList firstShotRow = (ArrayList)_shotRows[getForwardShotRow()];

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

                        foreach (Soldier enemy in _enemiesToShoot)
                        {
                            assignShooter(enemy);
                        }
                    }
                    else
                    {
                        for (int i = _width - firstShotRow.Count; i > 0; i--)
                        {
                            moveUpShooter();
                        }
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
                if (s.isDead() || s.getState() == Soldier.STATE_DYING || s.getState() == Soldier.STATE_ROUTE || s.getState() == Soldier.STATE_ROUTED || (s.getState() == Soldier.STATE_CHARGING && s.initCharge))
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
            }
            _soldiersToRemove.Clear();

            // if a soldier died then we need to reorganize the formation
            if (_soldierDied)
            {
                reformFormation();
            }

            if (_addedSoldier)
            {
                if (_soldiers.Count > 25)
                {
                    if (_width != (int)Math.Sqrt((double)_soldiers.Count))
                    {
                        _width = (int)Math.Sqrt((double)_soldiers.Count);
                    }
                }
                else
                {
                    if (_width != 5)
                    {
                        _width = 5;
                    }
                }
                _addedSoldier = false;
                resetupFormation();
            }
            _speed = avgSpeed - 0.04f;
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
                        if (guard == null && shot.guardTarget == null)
                            guard = shot;
                        else
                        {
                            bool ySmaller = Math.Abs(enemy.getPosition().Y - shot.getDestination().Y) < Math.Abs(enemy.getPosition().Y - guard.getDestination().Y);
                            if (ySmaller && shot.guardTarget == null)
                                guard = shot;
                        }
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
                        if (guard == null && shot.guardTarget == null)
                            guard = shot;
                        else
                        {
                            bool ySmaller = Math.Abs(enemy.getPosition().Y - shot.getPosition().Y) < Math.Abs(enemy.getPosition().Y - guard.getPosition().Y);
                            if (ySmaller && shot.guardTarget == null)
                                guard = shot;
                        }
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
                    if (guard == null)
                        guard = pike;
                    else
                    {
                        bool ySmaller = Math.Abs(enemy.getPosition().Y - pike.getDestination().Y) < Math.Abs(enemy.getPosition().Y - guard.getDestination().Y);
                        if (ySmaller && pike.guardTarget == null)
                            guard = pike;
                    }
                }
            }

            if (_pikeRows.Count > 1)
            {
                foreach (Pikeman pike in (ArrayList)_pikeRows[1])
                {
                    if (pike.guardTarget == null)
                    {
                        if (guard == null)
                            guard = pike;
                        else
                        {
                            bool ySmaller = Math.Abs(enemy.getPosition().Y - pike.getDestination().Y) < Math.Abs(enemy.getPosition().Y - guard.getDestination().Y);
                            bool xBigger = (pike.getDestination().X - guard.getDestination().X) * _side >= 0;
                            bool ySubstanciallyBigger = Math.Abs(guard.getDestination().Y - pike.getDestination().Y) > (float)Soldier.HEIGHT * 3f;
                            if (ySmaller && (xBigger || ySubstanciallyBigger) && pike.guardTarget == null)
                                guard = pike;
                        }
                    }
                }
            }
            if(guard != null)
                guard.guardTarget = enemy;
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
            _soldiersToRemove.Clear();
            foreach (Soldier s in _soldiers)
            {
                _soldiersToRemove.Add(s);
            }
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

        public Vector2 getScreenPosition()
        {
            return _position - _screen.getMapOffset();
        }

        public void pikeAttack()
        {
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
                }

                if (_pikeRows.Count > 1)
                {
                    foreach (Soldier pikeman in (ArrayList)_pikeRows[1])
                    {
                        if (pikeman is Pikeman && (pikeman.getState() != Soldier.STATE_MELEE_WIN && pikeman.getState() != Soldier.STATE_MELEE_LOSS))
                            pikeman.attackHigh();
                    }
                }

                if (_pikeRows.Count > 2)
                {
                    foreach (Soldier pikeman in (ArrayList)_pikeRows[2])
                    {
                        if (pikeman is Pikeman && (pikeman.getState() != Soldier.STATE_MELEE_WIN && pikeman.getState() != Soldier.STATE_MELEE_LOSS))
                            ((Pikeman)pikeman).lower45();
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
            _state = STATE_MELEE;
            resetupFormation();
            foreach (Soldier pikeman in _soldiers)
            {
                if (pikeman is Pikeman && (pikeman.getState() != Soldier.STATE_MELEE_WIN && pikeman.getState() != Soldier.STATE_MELEE_LOSS))
                    ((Pikeman)pikeman).raise();
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

        }

        /*public void swingAttack()
        {
            Soldier firstSoldier;
            Soldier restOfFormation;

            if (_swingerRows.Count > 0)
            {
                firstSoldier = getFirstNonMeleeSoldier((ArrayList)_swingerRows[0]);
                restOfFormation = getFirstNonMeleeSoldier((ArrayList)_pikeRows[0]);

                if(restOfFormation == null)
                    restOfFormation = getFirstNonMeleeSoldier((ArrayList)_meleeRows[0]);
                if (restOfFormation == null)
                    restOfFormation = getFirstNonMeleeSoldier((ArrayList)_shotRows[0]);

                if (firstSoldier != null && restOfFormation != null && !needTriggerUp)
                {
                    if ((_side * firstSoldier._destination.X - _side * firstSoldier._position.X) > firstSoldier.getSpeed()
                        || ((_side * firstSoldier._destination.X - _side * firstSoldier._position.X) < 0 && (_side * restOfFormation._destination.X - _side * restOfFormation._position.X) > 0))
                    {
                        _position.X = firstSoldier._position.X - _side * Soldier.WIDTH;
                        resetupFormation();
                    }
                    
                    needTriggerUp = true;
                }
                foreach (Soldier soldier in ((ArrayList)_swingerRows[0]))
                {
                    if (soldier.getState() != Soldier.STATE_MELEE_WIN && soldier.getState() != Soldier.STATE_MELEE_LOSS)
                    {
                        soldier._destination.X = _position.X + _side * Soldier.WIDTH;
                        soldier.attack();
                    }
                }
            }
        }

        public void swingRelease()
        {
            Soldier firstSoldier;

            if (_swingerRows.Count > 0)
            {
                firstSoldier = getFirstNonMeleeSoldier((ArrayList)_swingerRows[0]);
                if (firstSoldier != null)
                {
                    if ((_side * firstSoldier._destination.X - _side * firstSoldier._position.X) < -firstSoldier.getSpeed())
                    {
                        _position.X = firstSoldier._position.X + _side * Soldier.WIDTH * _pikeRows.Count;
                    }
                }
            }
            resetupFormation();
        }*/

        public void shotAttack()
        {
            bool didAttack = false;
            ArrayList tempRow;
            ArrayList prevRow;

            if (_shotRows.Count != 0)
            {
                prevRow = (ArrayList)_shotRows[0];

                if (_shotRows.Count == 0)
                    return;

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
            Soldier shot = (Soldier)((ArrayList)_shotRows[0])[0];
            Soldier shot2;
            Soldier melee;

            if (_shotRows.Count > 1)
            {
                shot2 = (Soldier)((ArrayList)_shotRows[_shotRows.Count-1])[0];
                if (!(_side * shot._position.X - _side * shot2._position.X >= Soldier.WIDTH - 0.9))
                    return false;
            }

            if (_pikeRows.Count > 0)
            {
                melee = (Soldier)((ArrayList)_pikeRows[0])[0];
                if (_side * shot._position.X - _side * melee._position.X >= 0)
                    return true;
                else
                    return false;
            }
            else
                return true;
            
        }

        public void draw(SpriteBatch spritebatch)
        {
            foreach (Soldier s in _soldiers)
                s.draw(spritebatch);

            if (_screen.getDrawDots())
            {
                if (DEBUGdangerClose)
                {
                    spritebatch.DrawString(PikeAndShotGame.getSpriteFont(), "HEY!", new Vector2(500f, 500f), Color.White);
                }
                spritebatch.Draw(PikeAndShotGame.getDotTexture(), _position - _screen.getMapOffset(), Color.White);
                spritebatch.Draw(PikeAndShotGame.getDotTexture(), new Vector2(_position.X - _screen.getMapOffset().X + (getTotalRows() * Soldier.WIDTH), _position.Y - _screen.getMapOffset().Y + getTotalRows() * Soldier.HEIGHT), Color.White);
                spritebatch.Draw(PikeAndShotGame.getDotTexture(), this.getCenter() - _screen.getMapOffset(), Color.White);
            }
        }

        public void addSoldier(Soldier soldier)
        {
            _addedSoldier = true;
            switch(soldier.getType())
            {
                case Soldier.TYPE_PIKE:
                    addSoldierToRow(soldier, _pikeRows);
                    break;
                case Soldier.TYPE_SWINGER:
                    addSoldierToRow(soldier, _meleeRows);
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
            soldier.initCharge = false;

            // determine speed
            avgSpeed = 0;

            foreach (Soldier s in _soldiers)
            {
                avgSpeed += s.getSpeed();
            }
            avgSpeed /= (float)_soldiers.Count;
            _speed = avgSpeed - 0.04f;            
            //_speed = avgSpeed - (0.002f * (float)_soldiers.Count);            
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
            int rowsOfType = getTotalSoldiers(soldier.getType()) / _width;
            Soldier prevSoldier;
            ArrayList row;
            float lastRowStartHeight = 0f;

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
                row = findSmallestRow(rows);
                row.Add(soldier);
                prevSoldier = ((Soldier)row[row.Count - 2]);
                soldier._destination = new Vector2(prevSoldier._destination.X, prevSoldier._destination.Y + Soldier.HEIGHT);
            }

            //if (rows.Count > 1)
            //{
            lastRowStartHeight = (float)_width / 2f - (float)(row.Count) / 2f;

            for (int i = 0; i < row.Count; i++)
            {
                ((Soldier)row[i])._destination.Y = _position.Y + Soldier.HEIGHT * ((float)i + lastRowStartHeight);
            }
            //}

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
                //arrangeFormation(_swingerRows, ref i);
                arrangeFormation(_meleeRows, ref i);
                arrangeFormation(_shotRows, ref i);
            }
            else if (_state == STATE_SHOT)
            {
                arrangeFormation(_shotRows, ref i);
                arrangeFormation(_meleeRows, ref i);
                arrangeFormation(_pikeRows, ref i);
                //arrangeFormation(_swingerRows, ref i);
            }
            else // STATE_MELEE
            {
                arrangeFormation(_meleeRows, ref i);
                arrangeFormation(_pikeRows, ref i);
                //arrangeFormation(_swingerRows, ref i);
                arrangeFormation(_shotRows, ref i);
            }

            arrangeFormation(_cavRows, ref i);
            arrangeFormation(_supportRows, ref i);

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

        private int getTotalSoldiers(int type)
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
            _soldiers.Remove(soldier);
        }

        public void removeSoldier(int soldier)
        {
            _soldiers.RemoveAt(soldier);
        }

        public void marchUp(double milliseconds, bool diagonal)
        {
            float amount;

            if (diagonal)
                amount = _speed * (float)milliseconds * 0.708f;// * 1.0f;
            else
                amount = _speed * (float)milliseconds;

            _position.Y -= amount;

            foreach (Soldier s in _soldiers)
            {
                s.alterDestination(false, -amount);
            }
        }

        public void marchDown(double milliseconds, bool diagonal)
        {
            float amount;

            if (diagonal)
                amount = _speed * (float)milliseconds * 0.708f; //* 1.0f;
            else
                amount = _speed * (float)milliseconds;

            _position.Y += amount;
            
            foreach (Soldier s in _soldiers)
            {
                s.alterDestination(false, amount);
            }
        }

        public void marchLeft(double milliseconds, bool diagonal)
        {
            Soldier lastSoldier;
            bool shot = true;
            bool melee = true;
            bool pike = true;
            float amount;

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

                /*if (lastSoldier == null)
                {
                    if (_swingerRows.Count > 0)
                    {
                        lastSoldier = getFirstNonMeleeSoldier((ArrayList)_swingerRows[_swingerRows.Count - 1]);
                        if (lastSoldier != null)
                        {
                            if (!((_side * lastSoldier._destination.X - _side * lastSoldier._position.X) > _speed || (_side * lastSoldier._destination.X - _side * lastSoldier._position.X) < -_speed))
                            {
                                lastSoldier = null;
                                swing = false;
                            }
                        }
                    }
                    else
                        lastSoldier = null;
                }*/

                if (lastSoldier != null)
                {
                    if (_side * lastSoldier._position.X - _side * lastSoldier._destination.X > _speed)
                    {
                        if ((_side * lastSoldier._destination.X - _side * lastSoldier._position.X) > _speed || (_side * lastSoldier._destination.X - _side * lastSoldier._position.X) < -_speed)
                        {
                            _position.X =  lastSoldier._position.X + _side * (_pikeRows.Count - 1 + /*(swing ? _swingerRows.Count : 0) +*/ (shot ? _shotRows.Count : 0) + (melee ? _meleeRows.Count : 0)) * Soldier.WIDTH;
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

                /*if (lastSoldier == null)
                {
                    if (_swingerRows.Count > 0)
                    {
                        lastSoldier = getFirstNonMeleeSoldier((ArrayList)_swingerRows[_swingerRows.Count - 1]);
                        if (lastSoldier != null)
                        {
                            if (!((_side * lastSoldier._destination.X - _side * lastSoldier._position.X) > _speed || (_side * lastSoldier._destination.X - _side * lastSoldier._position.X) < -_speed))
                            {
                                lastSoldier = null;
                                swing = false;
                            }
                        }
                    }
                    else
                        lastSoldier = null;
                }*/

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
                            _position.X = lastSoldier._position.X + _side * ((pike?_pikeRows.Count:0) + /*(swing?_swingerRows.Count:0) +*/ (shot?_shotRows.Count:0) + _meleeRows.Count - 1) * Soldier.WIDTH;
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

                /*if (lastSoldier == null)
                {
                    if (_swingerRows.Count > 0)
                    {
                        lastSoldier = getFirstNonMeleeSoldier((ArrayList)_swingerRows[_swingerRows.Count - 1]);
                        if (lastSoldier != null)
                        {
                            if (!((_side * lastSoldier._destination.X - _side * lastSoldier._position.X) > _speed || (_side * lastSoldier._destination.X - _side * lastSoldier._position.X) < -_speed))
                            {
                                lastSoldier = null;
                                swing = false;
                            }
                        }
                        else
                        {
                            lastSoldier = null;
                        }
                    }
                    else
                        lastSoldier = null;
                }*/

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
                            _position.X = lastSoldier._position.X + _side * ((pike ? _pikeRows.Count : 0) + /*(swing?_swingerRows.Count:0) +*/ (melee?_meleeRows.Count:0) + _shotRows.Count - 1) * Soldier.WIDTH;
                            resetupFormation();
                        }
                    }
                }
                
            }
            
            foreach (Soldier s in _soldiers)
            {
                s.alterDestination(true, -amount);
            }
            
        }

        public void marchRight(double milliseconds, bool diagonal)
        {
            Soldier firstSoldier;
            Soldier lastSoldier;
            float amount;
            float speed = _speed - (float)getSlowedSoldiers();

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

                /*if (lastSoldier == null)
                {
                    if (_swingerRows.Count > 0)
                    {
                        lastSoldier = getFirstNonMeleeSoldier((ArrayList)_swingerRows[_swingerRows.Count - 1]);
                        if (lastSoldier != null)
                        {
                            if (!((_side * lastSoldier._destination.X - _side * lastSoldier._position.X) > _speed || (_side * lastSoldier._destination.X - _side * lastSoldier._position.X) < -_speed))
                                lastSoldier = null;
                        }
                    }
                    else
                        lastSoldier = null;
                }*/

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

                /*if (lastSoldier == null)
                {
                    if (_swingerRows.Count > 0)
                    {
                        lastSoldier = getFirstNonMeleeSoldier((ArrayList)_swingerRows[_swingerRows.Count - 1]);
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
                }*/

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

                /*if (lastSoldier == null)
                {
                    if (_swingerRows.Count > 0)
                    {
                        lastSoldier = getFirstNonMeleeSoldier((ArrayList)_swingerRows[_swingerRows.Count - 1]);
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
                }*/

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
        }

        private Soldier getFirstNonMeleeSoldier(ArrayList soldierList)
        {
            int i = 0;

            Soldier soldier = (Soldier)soldierList[i];

            while (soldier.getState() == Soldier.STATE_MELEE_WIN || soldier.getState() == Soldier.STATE_MELEE_LOSS)
            {
                soldier = (Soldier)soldierList[i];
                i++;
                if (i == soldierList.Count)
                    break;
            }

            if (soldier.getState() == Soldier.STATE_MELEE_WIN || soldier.getState() == Soldier.STATE_MELEE_LOSS)
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
            if (_width > 1)
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
            foreach (Soldier s in _soldiers)
            {
                if (s._position.X + Soldier.WIDTH * 2f - (_screen.getMapOffset().X) > 0 && s._position.X - Soldier.WIDTH * 2f - (_screen.getMapOffset().X) < PikeAndShotGame.SCREENWIDTH)
                    return true;
            }
            return false;
        }
    }

    public class EnemyFormation : Formation
    {
        private List<PatternAction> _pattern;
        private int _currentAction;
        private double _currentDuration;
        private bool _pikesRaised; //Gotta have this flag otherwise enemy formations keep trying to raise all the time and mess up shots

        public EnemyFormation(List<PatternAction> pattern, BattleScreen screen, float x, float y, int initialCapacity, int side)
            : base(screen, x, y, initialCapacity, side)
        {
            _pattern = pattern;
            _currentAction = 0;
            if(pattern.Count > 0)
                _currentDuration = ((PatternAction)pattern[0]).duration;

            _pikesRaised = false;
        }

        public override void update(TimeSpan timeSpan)
        {
            base.update(timeSpan);

            if (this.getSoldiers().Count <= 0)
                return;

            if (!(((Soldier)this.getSoldiers()[0]).getScreen() is LevelEditorScreen))
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
            else
            {
                foreach (Soldier s in this.getSoldiers())
                {
                    s.update(timeSpan);
                }
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

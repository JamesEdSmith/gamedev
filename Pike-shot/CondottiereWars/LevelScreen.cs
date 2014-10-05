using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using System.Collections;

namespace PikeAndShot
{
    public class LevelScreen : BattleScreen, FormListener
    {
        public static float NEXT_SPAWN_POINT = 2000f;
        public static float COIN_METER_FLASH_TIME = 400f;
        public static float COIN_METER_HURT_FLASH_TIME = 400f;

        protected EnemyFormation _newEnemyFormation;
        protected Level _levelData;
        private double _fps = 0;
        private int _draws = 0;
        List<int> _usedFormations;
        float nextSpawnPosition = NEXT_SPAWN_POINT;
        public ArrayList _spawners;
        public ArrayList _deadSpawners;
        private Sprite _coinMeter;
        private Sprite _coinMeterHurt;
        private float _coinMeterTimer;
        private float _coinMeterHurtTimer;

        public LevelScreen(PikeAndShotGame game, Level level)
            : base(game)
        {
            _levelData = level;

            _formation = new Formation(this, 200, 200, 20, SIDE_PLAYER);
            //_formation.addSoldier(new Cavalry(this, 200, 200, BattleScreen.SIDE_PLAYER));
            
            _formation.addSoldier(new Pikeman(this, 200, 200, BattleScreen.SIDE_PLAYER)); 
            _formation.addSoldier(new Pikeman(this, 200, 200, BattleScreen.SIDE_PLAYER));
            _formation.addSoldier(new Pikeman(this, 200, 200, BattleScreen.SIDE_PLAYER));
            _formation.addSoldier(new Pikeman(this, 200, 200, BattleScreen.SIDE_PLAYER));
            _formation.addSoldier(new Pikeman(this, 200, 200, BattleScreen.SIDE_PLAYER));
            _formation.addSoldier(new Pikeman(this, 200, 200, BattleScreen.SIDE_PLAYER));
            _formation.addSoldier(new Pikeman(this, 200, 200, BattleScreen.SIDE_PLAYER));
            
            _formation.addSoldier(new Leader(this, 200, 200, BattleScreen.SIDE_PLAYER));

            _formation.addSoldier(new Arquebusier(this, 200, 200, BattleScreen.SIDE_PLAYER));
            _formation.addSoldier(new Arquebusier(this, 200, 200, BattleScreen.SIDE_PLAYER));
            _formation.addSoldier(new Arquebusier(this, 200, 200, BattleScreen.SIDE_PLAYER));
            _formation.addSoldier(new Arquebusier(this, 200, 200, BattleScreen.SIDE_PLAYER));
            _formation.addSoldier(new Arquebusier(this, 200, 200, BattleScreen.SIDE_PLAYER));
            _formation.addSoldier(new Arquebusier(this, 200, 200, BattleScreen.SIDE_PLAYER));
            _formation.addSoldier(new Arquebusier(this, 200, 200, BattleScreen.SIDE_PLAYER));
            
            _usedFormations = new List<int>(_levelData.formations.Count);
            _spawners = new ArrayList(2);
            _deadSpawners = new ArrayList(2);

            for (int i = 0; i < _coins; i++)
            {
                _coinSprites.Add(new Coin(this, new Vector2(BASE_COIN_POSITION.X, BASE_COIN_POSITION.Y - i * 4f)));
            }
            _coinMeter = new Sprite(PikeAndShotGame.COIN_METER, new Rectangle(0, 0, 36, 134), 36, 134, false, true, 128, Color.Gold);
            _coinMeterHurt = new Sprite(PikeAndShotGame.COIN_METER, new Rectangle(0, 0, 36, 134), 36, 134, false, true, 128, Color.Red);
            _coinMeterTimer = 0f;
        }

        public override void update(GameTime gameTime)
        {
            base.update(gameTime);

            /* formations and terrain are generated on the far right of the screen 
            at their height when the player gets to their spawn trigger point*/
            checkLevelData();

            foreach (Spawner spawny in _spawners)
            {
                spawny.update(gameTime.ElapsedGameTime);
                if (spawny.dead)
                    _deadSpawners.Add(spawny);
            }
            foreach (Spawner deadSpawner in _deadSpawners)
            {
                _spawners.Remove(deadSpawner);
            }
            _deadSpawners.Clear();

            ArrayList coinsDone = new ArrayList();
            for (int i = 0; i < _coinSprites.Count; i++)
            {
                //((Coin)_coinSprites[i]).update(gameTime.ElapsedGameTime);
                if (((Coin)_coinSprites[i])._position.Y >= BASE_COIN_POSITION.Y + 4f)
                {
                    coinsDone.Add(_coinSprites[i]);
                    ((Coin)_coinSprites[i]).setDone();
                }
            }

            foreach (Coin c in coinsDone)
                _coinSprites.Remove(c);

            coinsDone.Clear();

            _fps = (double)_draws / gameTime.ElapsedGameTime.TotalSeconds;
            _draws = 0;
        }

        public bool loseCoin()
        {
            _coinMeterHurtTimer = COIN_METER_HURT_FLASH_TIME;
            if (_coins > 0)
            {
                ((Coin)_coinSprites[_coinSprites.Count - 1]).setDone();
                _coinSprites.RemoveAt(_coinSprites.Count - 1);
                //TODO: make sure this isn't doing some Screen object not getting removed fuckery
                _coins--;

                if (_coins < 1)
                {
                    retreat();
                    return false;
                }
                return true;
            }
            return false;
        }

        public void retreat()
        {
            foreach (Soldier s in _looseSoldiers)
            {
                s.route();
            }
            getPlayerFormation().retreat();
            foreach(EnemyFormation ef in _enemyFormations)
            {
                if (ef.getSide() == SIDE_PLAYER)
                    ef.retreat();
            }
        }

        public void spawnRescue(int type)
        {
            EnemyFormation formation;
            if (PikeAndShotGame.random.Next(100) > 49)
            {
                formation = new EnemyFormation("Reinforcement", null, this,
                    _formation._position.X, (float)PikeAndShotGame.SCREENHEIGHT, 1, SIDE_PLAYER);
            }
            else
            {
                formation = new EnemyFormation("Reinforcement", null, this,
                    _formation._position.X, 0f - (float)Soldier.HEIGHT * 2f, 1, SIDE_PLAYER);
            }
            assignRescue(formation, type);
            _enemyFormations.Add(formation);
        }

        public void collectCoin(Soldier soldier)
        {
            _coinMeterTimer = COIN_METER_FLASH_TIME;
            addAnimation(new Loot(this,soldier.getPosition()));
            if (_coins < MAX_COINS)
            {
                _coinSprites.Add(new Coin(this, new Vector2(BASE_COIN_POSITION.X, BASE_COIN_POSITION.Y - _coins * 4f)));
                _coins++;
            }
            //TODO: otherwise put you into some sort of super coiny state I guess, right?
        }

        public override void draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            base.draw(gameTime, spriteBatch);

            _draws++;

            //draw UI

            for (int i = 0; i < _coinSprites.Count; i++)
            {
                ((Coin)_coinSprites[i]).draw(spriteBatch);
            }

            spriteBatch.Draw(PikeAndShotGame.COIN_METER, COIN_METER_POSITION, Color.White);

            //I draw the flash overtop, I was worried about missing a frame.
            if (_coinMeterTimer > 0)
            {
                _coinMeter.draw(spriteBatch, COIN_METER_POSITION, SIDE_PLAYER, _coinMeterTimer / COIN_METER_FLASH_TIME);
                _coinMeterTimer -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            }

            if (_coinMeterHurtTimer > 0)
            {
                _coinMeterHurt.draw(spriteBatch, COIN_METER_POSITION, SIDE_PLAYER, _coinMeterHurtTimer / COIN_METER_HURT_FLASH_TIME);
                _coinMeterHurtTimer -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            }


            if (getDrawDots())
            {
                spriteBatch.DrawString(PikeAndShotGame.getSpriteFont(), "mapOffset: " + _mapOffset.X + ", " + _mapOffset.Y, new Vector2(5, 35), Color.White);
                spriteBatch.DrawString(PikeAndShotGame.getSpriteFont(), "Loose: " + _looseSoldiers.Count, new Vector2(5, 5), Color.White);
                spriteBatch.DrawString(PikeAndShotGame.getSpriteFont(), "eFormations: " + _enemyFormations.Count, new Vector2(105, 5), Color.White);
                spriteBatch.DrawString(PikeAndShotGame.getSpriteFont(), "shots: " + _shots.Count, new Vector2(205, 5), Color.White);
                spriteBatch.DrawString(PikeAndShotGame.getSpriteFont(), "Objects: " + _screenObjects.Count, new Vector2(305, 5), Color.White);
                spriteBatch.DrawString(PikeAndShotGame.getSpriteFont(), "Colliders: " + _screenColliders.Count, new Vector2(405, 5), Color.White);
                spriteBatch.DrawString(PikeAndShotGame.getSpriteFont(), "fps: " + _fps, new Vector2(405, 35), Color.White);
                //spriteBatch.DrawString(PikeAndShotGame.getSpriteFont(), "terrain: " + _terrain.Count, new Vector2(505, 5), Color.White);
                spriteBatch.DrawString(PikeAndShotGame.getSpriteFont(), "pike: " + _formation.numberOfPikes + "shot: " + _formation.numberOfShots, new Vector2(505, 5), Color.White);
            }
        }

        protected override void getInput(TimeSpan timeSpan)
        {
            keyboardState = Keyboard.GetState();
            gamePadState = GamePad.GetState(PlayerIndex.One);
            
            // Allows the game to exit
            if (keyboardState.IsKeyDown(Keys.Escape) || GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                _game.Exit();

            if (keyboardState.IsKeyDown(Keys.Left) || gamePadState.IsButtonDown(Buttons.DPadLeft) || gamePadState.ThumbSticks.Left.X < 0)
            {
                if ((keyboardState.IsKeyDown(Keys.Up) || gamePadState.IsButtonDown(Buttons.DPadUp) || gamePadState.ThumbSticks.Left.Y < 0) || (keyboardState.IsKeyDown(Keys.Down) || gamePadState.IsButtonDown(Buttons.DPadDown) || gamePadState.ThumbSticks.Left.Y > 0))
                    _formation.marchLeft(timeSpan.TotalMilliseconds, true);
                else
                    _formation.marchLeft(timeSpan.TotalMilliseconds, false);
            }
            if (keyboardState.IsKeyDown(Keys.Right) || gamePadState.IsButtonDown(Buttons.DPadRight) || gamePadState.ThumbSticks.Left.X > 0)
            {
                if ((keyboardState.IsKeyDown(Keys.Up) || gamePadState.IsButtonDown(Buttons.DPadUp) || gamePadState.ThumbSticks.Left.Y < 0) || (keyboardState.IsKeyDown(Keys.Down) || gamePadState.IsButtonDown(Buttons.DPadDown) || gamePadState.ThumbSticks.Left.Y > 0))
                {
                    _formation.marchRight(timeSpan.TotalMilliseconds, true);
                    if (_formation.getCenter().X >= PikeAndShotGame.SCREENWIDTH * BattleScreen.SCROLLPOINT + _mapOffset.X)
                        _mapOffset.X += _formation.getSpeed() * (float)timeSpan.TotalMilliseconds * 0.708f *0.75f;
                }
                else
                {
                    _formation.marchRight(timeSpan.TotalMilliseconds, false);
                    if (_formation.getCenter().X >= PikeAndShotGame.SCREENWIDTH * BattleScreen.SCROLLPOINT + _mapOffset.X)
                        _mapOffset.X += _formation.getSpeed() * (float)timeSpan.TotalMilliseconds * 0.75f;
                }

            }
            else
            {
                if (_formation.getCenter().X >= PikeAndShotGame.SCREENWIDTH * BattleScreen.SCROLLPOINT + _mapOffset.X)
                    _mapOffset.X += getScrollAdjustSpeed() * (float)timeSpan.TotalMilliseconds;
            }
            if (keyboardState.IsKeyDown(Keys.Down) || gamePadState.IsButtonDown(Buttons.DPadDown) || gamePadState.ThumbSticks.Left.Y < 0)
            {
                if ((keyboardState.IsKeyDown(Keys.Left) || gamePadState.IsButtonDown(Buttons.DPadLeft) || gamePadState.ThumbSticks.Left.X < 0) || (keyboardState.IsKeyDown(Keys.Right) || gamePadState.IsButtonDown(Buttons.DPadRight) || gamePadState.ThumbSticks.Left.X > 0))
                {
                    _formation.marchDown(timeSpan.TotalMilliseconds, true);
                    //if (_mapOffset.Y < BattleScreen.BATTLEHEIGHTEXTEND && _formation.getCenter().Y - _mapOffset.Y >= PikeAndShotGame.SCREENHEIGHT * 0.5f)
                       // _mapOffset.Y += _formation.getSpeed() * (float)timeSpan.TotalMilliseconds * 0.708f;
                }
                else
                {
                    _formation.marchDown(timeSpan.TotalMilliseconds, false);
                   // if (_mapOffset.Y < BattleScreen.BATTLEHEIGHTEXTEND && _formation.getCenter().Y - _mapOffset.Y >= PikeAndShotGame.SCREENHEIGHT * 0.5f)
                     //   _mapOffset.Y += _formation.getSpeed() * (float)timeSpan.TotalMilliseconds;
                }
            }
            if (keyboardState.IsKeyDown(Keys.Up) || gamePadState.IsButtonDown(Buttons.DPadUp) || gamePadState.ThumbSticks.Left.Y > 0)
            {
                if ((keyboardState.IsKeyDown(Keys.Left) || gamePadState.IsButtonDown(Buttons.DPadLeft) || gamePadState.ThumbSticks.Left.X < 0) || (keyboardState.IsKeyDown(Keys.Right) || gamePadState.IsButtonDown(Buttons.DPadRight) || gamePadState.ThumbSticks.Left.X > 0))
                {
                    _formation.marchUp(timeSpan.TotalMilliseconds, true);
                    //if (_mapOffset.Y > -1 * BattleScreen.BATTLEHEIGHTEXTEND && _formation.getCenter().Y - _mapOffset.Y <= PikeAndShotGame.SCREENHEIGHT * 0.5f)
                        //_mapOffset.Y -= _formation.getSpeed() * (float)timeSpan.TotalMilliseconds * 0.708f;
                }
                else
                {
                    _formation.marchUp(timeSpan.TotalMilliseconds, false);
                    //if (_mapOffset.Y > -1 * BattleScreen.BATTLEHEIGHTEXTEND && _formation.getCenter().Y - _mapOffset.Y <= PikeAndShotGame.SCREENHEIGHT * 0.5f)
                        //_mapOffset.Y -= _formation.getSpeed() * (float)timeSpan.TotalMilliseconds;
                }
            }
            if ((keyboardState.IsKeyDown(Keys.Z) && previousKeyboardState.IsKeyUp(Keys.Z)) || (gamePadState.IsButtonDown(Buttons.A) && previousGamePadState.IsButtonUp(Buttons.A)))
            {
                _formation.pikeAttack();
            }
            else if ((previousKeyboardState.IsKeyDown(Keys.Z) && keyboardState.IsKeyUp(Keys.Z)) || (previousGamePadState.IsButtonDown(Buttons.A) && gamePadState.IsButtonUp(Buttons.A)))
            {
                _formation.pikeRaise();
            }
            if ((keyboardState.IsKeyDown(Keys.Z) && keyboardState.IsKeyDown(Keys.X) && (previousKeyboardState.IsKeyUp(Keys.Z) || previousKeyboardState.IsKeyUp(Keys.X))) || (gamePadState.IsButtonDown(Buttons.A) && gamePadState.IsButtonDown(Buttons.X) && (previousGamePadState.IsButtonUp(Buttons.A) || previousGamePadState.IsButtonUp(Buttons.X))))
            {
                _formation.meleeCharge();
                //_formation.swingAttack();
            }
            //else if ((keyboardState.IsKeyUp(Keys.Z) || keyboardState.IsKeyUp(Keys.X)) && (previousKeyboardState.IsKeyDown(Keys.Z) && previousKeyboardState.IsKeyDown(Keys.X)))
            //{
            //    _formation.cancelCharge();
            //}
            //else if ((gamePadState.IsButtonUp(Buttons.A) || gamePadState.IsButtonUp(Buttons.X)) && (previousGamePadState.IsButtonDown(Buttons.A) && previousGamePadState.IsButtonDown(Buttons.X)))
            //{
            //    _formation.cancelCharge();
            //}
            //if ((keyboardState.IsKeyDown(Keys.Z) && keyboardState.IsKeyUp(Keys.X) && previousKeyboardState.IsKeyDown(Keys.X)) || (gamePadState.IsButtonDown(Buttons.A) && gamePadState.IsButtonUp(Buttons.X) && previousGamePadState.IsButtonDown(Buttons.X)))
            //{
            //    _formation.swingRelease();
            //}
            if ((keyboardState.IsKeyDown(Keys.X) && !keyboardState.IsKeyDown(Keys.Z)) || (gamePadState.IsButtonDown(Buttons.X) && !gamePadState.IsButtonDown(Buttons.A)))
            {
                _formation.shotAttack();
            }
            if (keyboardState.IsKeyUp(Keys.X) && gamePadState.IsButtonUp(Buttons.X))
            {
                _formation.needTriggerUp = false;
            }
            if (keyboardState.IsKeyDown(Keys.Q) && previousKeyboardState.IsKeyUp(Keys.Q))
            {
                _formation.addSoldier(new Pikeman(this, getMapOffset().X, getMapOffset().Y + PikeAndShotGame.SCREENHEIGHT * 0.5f, SIDE_PLAYER));
            }
            if (keyboardState.IsKeyDown(Keys.W) && previousKeyboardState.IsKeyUp(Keys.W))
            {
                _formation.addSoldier(new Arquebusier(this, getMapOffset().X, getMapOffset().Y + PikeAndShotGame.SCREENHEIGHT * 0.5f, SIDE_PLAYER));
            }
            if (keyboardState.IsKeyDown(Keys.E) && previousKeyboardState.IsKeyUp(Keys.E))
            {
                _formation.addSoldier(new Crossbowman(this, getMapOffset().X, getMapOffset().Y + PikeAndShotGame.SCREENHEIGHT * 0.5f, SIDE_PLAYER));
            }
            if (keyboardState.IsKeyDown(Keys.R) && previousKeyboardState.IsKeyUp(Keys.R))
            {
                _formation.addSoldier(new Slinger(this, getMapOffset().X, getMapOffset().Y + PikeAndShotGame.SCREENHEIGHT * 0.5f, SIDE_PLAYER));
            }
            if (keyboardState.IsKeyDown(Keys.T) && previousKeyboardState.IsKeyUp(Keys.T))
            {
                _formation.addSoldier(new Dopple(this, getMapOffset().X, getMapOffset().Y + PikeAndShotGame.SCREENHEIGHT * 0.5f, SIDE_PLAYER));
            }
            if (keyboardState.IsKeyDown(Keys.Y) && previousKeyboardState.IsKeyUp(Keys.Y))
            {
                _formation.addSoldier(new Berzerker(this, getMapOffset().X, getMapOffset().Y + PikeAndShotGame.SCREENHEIGHT * 0.5f, SIDE_PLAYER));
            }
            if (keyboardState.IsKeyDown(Keys.P) && previousKeyboardState.IsKeyUp(Keys.P))
            {
                _formation.addSoldier(new CrossbowmanPavise(this, getMapOffset().X, getMapOffset().Y + PikeAndShotGame.SCREENHEIGHT * 0.5f, SIDE_PLAYER));
            }
            if (keyboardState.IsKeyDown(Keys.U) && previousKeyboardState.IsKeyUp(Keys.U))
            {
                _formation.addSoldier(new Cavalry(this, getMapOffset().X, getMapOffset().Y + PikeAndShotGame.SCREENHEIGHT * 0.5f, SIDE_PLAYER));
            }
            if (keyboardState.IsKeyDown(Keys.I) && previousKeyboardState.IsKeyUp(Keys.I))
            {
                _formation.addSoldier(new Targeteer(this, getMapOffset().X, getMapOffset().Y + PikeAndShotGame.SCREENHEIGHT * 0.5f, SIDE_PLAYER));
            }
            if (keyboardState.IsKeyDown(Keys.O) && previousKeyboardState.IsKeyUp(Keys.O))
            {
                _formation.addSoldier(new Brigand(this, getMapOffset().X, getMapOffset().Y + PikeAndShotGame.SCREENHEIGHT * 0.5f, SIDE_PLAYER));
            }
            if (keyboardState.IsKeyDown(Keys.C) && previousKeyboardState.IsKeyUp(Keys.C))
            {
                _formation.haltHorses();
            }
            if (keyboardState.IsKeyDown(Keys.V) && previousKeyboardState.IsKeyUp(Keys.V))
            {
                _formation.turnHorses();
            }
            if (keyboardState.IsKeyDown(Keys.A) && previousKeyboardState.IsKeyUp(Keys.A) || (gamePadState.IsButtonDown(Buttons.LeftShoulder) && previousGamePadState.IsButtonUp(Buttons.LeftShoulder)))
            {
                _formation.reduceWidth();
            }
            else if (keyboardState.IsKeyDown(Keys.S) && previousKeyboardState.IsKeyUp(Keys.S) || (gamePadState.IsButtonDown(Buttons.RightShoulder) && previousGamePadState.IsButtonUp(Buttons.RightShoulder)))
            {
                _formation.increaseWidth();
            }
            else if (keyboardState.IsKeyDown(Keys.L) && previousKeyboardState.IsKeyUp(Keys.L))
            {
                //spawnRescue();
            }
            if (keyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.D) && previousKeyboardState.IsKeyUp(Microsoft.Xna.Framework.Input.Keys.D))
                toggleDrawDots();
                
            previousKeyboardState = keyboardState;
            previousGamePadState = gamePadState;
        }

        void FormListener.updateLevel(Level level, int Formation)
        {
            _levelData = level;
        }

        protected void checkLevelData()
        {
            for(int f = 0; f < _levelData.formations.Count; f++)
            {
                if (_levelData.formationTimes[f] <= (double)_mapOffset.X + PikeAndShotGame.SCREENWIDTH && !_usedFormations.Exists(i => i == f))
                {
                    _usedFormations.Add(f);
                    _newEnemyFormation = new EnemyFormation(_levelData.formationNames[f], _levelData.formationActions[f], this, (_levelData.formationPositions[f]).X, (_levelData.formationPositions[f]).Y, 10, _levelData.formationSides[f]);
                    string formationName = _levelData.formationNames[f];                                       
                    float x = _newEnemyFormation.getPosition().X;
                    float y = _newEnemyFormation.getPosition().Y;
                    if (_newEnemyFormation.getSide() == SIDE_PLAYER)
                        assignRescue(_newEnemyFormation, PikeAndShotGame.random.Next(100) < 50 ? Soldier.TYPE_PIKE : Soldier.TYPE_SHOT);
                    else
                    {
                        for (int i = 0; i < _levelData.formations[f].Count; i++)
                        {
                            Soldier.getNewSoldier((_levelData.formations[f])[i], this, _newEnemyFormation, x, y);                           
                        }
                    }
                    if (formationName.StartsWith("spawner:"))
                    {
                        foreach (Soldier s in _newEnemyFormation.getSoldiers())
                            this.removeScreenObject(s);
                        char[] separator = { ':' };
                        EnemyFormation dependantFormation = getDependantFormation(formationName.Split(separator)[2]);
                        Spawner newSpawner = new Spawner(this, _newEnemyFormation, float.Parse(formationName.Split(separator)[1]), dependantFormation);
                        _spawners.Add(newSpawner);
                    }
                    else
                    {
                        _enemyFormations.Add(_newEnemyFormation);
                    }
                }
            }
        }

        private EnemyFormation getDependantFormation(string name)
        {
            foreach (EnemyFormation f in _enemyFormations)
            {
                if (f.name.Equals(name))
                    return f;
            }
            return null;
        }

        private void assignRescue(EnemyFormation formation, int type)
        {
            if (_formation.numberOfPikes < 10 || _formation.numberOfShots < 10)
            {
                Soldier soldier;
                if (type == Soldier.TYPE_PIKE)
                {
                    soldier = new Pikeman(this, formation.getPosition().X, formation.getPosition().Y, SIDE_PLAYER);
                }
                else
                {
                    soldier = new Arquebusier(this, formation.getPosition().X, formation.getPosition().Y, SIDE_PLAYER);
                }
                formation.addSoldier(soldier);
                soldier.setSpeed(0.075f);
            }
                //TODO: HEY MAN, THIS COULD CAUSE PRO-BLEMS
            else
            {
                formation.addSoldier(new Dopple(this, formation.getPosition().X, formation.getPosition().Y, SIDE_PLAYER));
            }
        }

        internal ArrayList shotBeforeFormation()
        {
            ArrayList shots = new ArrayList(_enemyFormations.Count * 10);

            foreach (Shot ef in _shots)
            {
                bool bool1 = ef.getCenter().X - _formation.getCenter().X <= Soldier.WIDTH * 20f;
                bool bool2 = ef.getCenter().X - _formation.getCenter().X > 0;
                bool bool3 = Math.Abs(ef.getCenter().Y - _formation.getCenter().Y) < (float)_formation.getWidth() * Soldier.HEIGHT * 0.5f + (float)ef.getHeight() * 0.5f;
                if (bool1 && bool2 && bool3 && ef.getSide() == SIDE_ENEMY)
                {
                    shots.Add(ef);
                }
            }

            return shots;
        }

        internal ArrayList dangerBeforeFormation()
        {
            ArrayList enemies = new ArrayList(_enemyFormations.Count * 10);
            foreach (EnemyFormation f in _enemyFormations)
            {
                foreach (Soldier ef in f.getSoldiers())
                {
                    bool bool1 = ef.getCenter().X - _formation.getCenter().X <= Soldier.WIDTH * 20f;
                    bool bool2 = ef.getCenter().X - _formation.getCenter().X > 0;
                    bool bool3 = Math.Abs(ef.getCenter().Y - _formation.getCenter().Y) < (float)_formation.getWidth() * Soldier.HEIGHT * 0.5f /*+ (float)ef.getHeight() * 0.5f*/;
                    if (bool1 && bool2 && bool3)
                    {
                        enemies.Add(ef);
                    }
                }
            }

            foreach (Soldier ef in _looseSoldiers)
            {
                bool bool1 = ef.getCenter().X - _formation.getCenter().X <= Soldier.WIDTH * 20f;
                bool bool2 = ef.getCenter().X - _formation.getCenter().X > 0;
                bool bool3 = Math.Abs(ef.getCenter().Y - _formation.getCenter().Y) < (float)_formation.getWidth() * Soldier.HEIGHT * 0.5f + (float)ef.getHeight() * 0.5f;
                if (bool1 && bool2 && bool3 && ef.getState() != Soldier.STATE_DEAD && ef.getState() != Soldier.STATE_DYING && ef.getSide() == SIDE_ENEMY)
                {
                    enemies.Add(ef);
                }
            }

            return enemies;
        }

        internal ArrayList dangerCloseToFormation()
        {
            ArrayList enemies = new ArrayList(_enemyFormations.Count * 10);
            foreach (EnemyFormation f in _enemyFormations)
            {
                foreach(Soldier ef in f.getSoldiers())
                {
                    bool bool1 = ef.getCenter().X - _formation.getCenter().X <= Soldier.WIDTH * 10f;
                    bool bool2 = ef.getCenter().X - _formation.getCenter().X > 0;
                    bool bool3 = Math.Abs(ef.getCenter().Y - _formation.getCenter().Y) < (float)_formation.getWidth() * Soldier.HEIGHT * 0.5f /*+ (float)ef.getHeight() * 0.5f*/;
                    if (ef.getSide() == BattleScreen.SIDE_ENEMY && (bool1 && bool2) && bool3)
                    {
                        enemies.Add(ef);
                    }
                }
            }

            foreach (Soldier ef in _looseSoldiers)
            {
                bool bool1 = ef.getCenter().X - _formation.getCenter().X <= Soldier.WIDTH * 10f;
                bool bool2 = ef.getCenter().X - _formation.getCenter().X > 0;
                bool bool3 = Math.Abs(ef.getCenter().Y - _formation.getCenter().Y) < (float)_formation.getWidth() * Soldier.HEIGHT * 0.5f + (float)ef.getHeight() * 0.5f;
                if ((bool1 && bool2) && bool3 && ef.getState() != Soldier.STATE_DEAD && ef.getState() != Soldier.STATE_DYING && ef.getSide() == SIDE_ENEMY)
                {
                    enemies.Add(ef);
                }
            }

            return enemies;
        }

        internal void restart()
        {
            _mapOffset.X = 0f;
            _mapOffset.Y = 0f;
            _usedFormations.Clear();
            _looseSoldiers.Clear();
            _shots.Clear();
            _enemyFormations.Clear();
            _formation.setPosition(200f, (float)PikeAndShotGame.SCREENHEIGHT * 0.5f);
            _formation.resetupFormation();
            _formation.reformFormation();
            _screenObjects.Clear();
            _screenObjectsToAdd.Clear();
            _screenColliders.Clear();
            _spawners.Clear();
            _screenAnimations.Clear();

            _coins = 10;
            _coinSprites.Clear();
            for(int i = 0; i < _coins; i++)
            {
                _coinSprites.Add(new Coin(this, new Vector2(BASE_COIN_POSITION.X, BASE_COIN_POSITION.Y - i * 4f)));
            }
            
            _formation = new Formation(this, 200, 200, 20, SIDE_PLAYER);
            //_formation.addSoldier(new Cavalry(this, 200, 200, BattleScreen.SIDE_PLAYER));

            _formation.addSoldier(new Pikeman(this, 200, 200, BattleScreen.SIDE_PLAYER));
            _formation.addSoldier(new Pikeman(this, 200, 200, BattleScreen.SIDE_PLAYER));
            _formation.addSoldier(new Pikeman(this, 200, 200, BattleScreen.SIDE_PLAYER));
            _formation.addSoldier(new Pikeman(this, 200, 200, BattleScreen.SIDE_PLAYER));
            _formation.addSoldier(new Pikeman(this, 200, 200, BattleScreen.SIDE_PLAYER));
            _formation.addSoldier(new Pikeman(this, 200, 200, BattleScreen.SIDE_PLAYER));
            _formation.addSoldier(new Pikeman(this, 200, 200, BattleScreen.SIDE_PLAYER));

            _formation.addSoldier(new Leader(this, 200, 200, BattleScreen.SIDE_PLAYER));

            _formation.addSoldier(new Arquebusier(this, 200, 200, BattleScreen.SIDE_PLAYER));
            _formation.addSoldier(new Arquebusier(this, 200, 200, BattleScreen.SIDE_PLAYER));
            _formation.addSoldier(new Arquebusier(this, 200, 200, BattleScreen.SIDE_PLAYER));
            _formation.addSoldier(new Arquebusier(this, 200, 200, BattleScreen.SIDE_PLAYER));
            _formation.addSoldier(new Arquebusier(this, 200, 200, BattleScreen.SIDE_PLAYER));
            _formation.addSoldier(new Arquebusier(this, 200, 200, BattleScreen.SIDE_PLAYER));
            _formation.addSoldier(new Arquebusier(this, 200, 200, BattleScreen.SIDE_PLAYER));

            foreach (Soldier s in _formation.getSoldiers())
            {
                _screenObjects.Add(s);
                s._position = new Vector2(200f, (float)PikeAndShotGame.SCREENHEIGHT * 0.5f);
            }

            _terrain = new ArrayList(20);

            for (int i = 0; i < 100; i++)
            {
                _terrain.Add(new Terrain(this, PikeAndShotGame.ROAD_TERRAIN[PikeAndShotGame.random.Next(7)], SIDE_PLAYER, PikeAndShotGame.random.Next(PikeAndShotGame.SCREENWIDTH), PikeAndShotGame.random.Next(PikeAndShotGame.SCREENHEIGHT)));
            }
        }

    }
}

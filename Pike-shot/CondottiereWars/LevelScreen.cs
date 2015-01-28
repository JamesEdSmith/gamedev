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
    public class LevelScreen : BattleScreen, FormListener, ScreenAnimationListener
    {
        public static int MAX_COINS = 10;
        public static float NEXT_SPAWN_POINT = 2000f;
        public static float COIN_METER_FLASH_TIME = 400f;
        public static float COIN_METER_HURT_FLASH_TIME = 400f;
        public static Vector2 COIN_METER_POSITION = new Vector2(25f, 25f);
        public static float COIN_METER_OFFSET = 92f;
        public static Vector2 BASE_COIN_START_POSITION = new Vector2(COIN_METER_POSITION.X + 6f, COIN_METER_POSITION.Y + 5f);
        public static Vector2 BASE_COIN_POSITION = new Vector2(COIN_METER_POSITION.X + 6f, COIN_METER_POSITION.Y + 48f);
        const float COIN_METER_DROPTIME = 750f;

        protected EnemyFormation _newEnemyFormation;
        protected Level _levelData;
        private double _fps = 0;
        private int _draws = 0;
        List<int> _usedFormations;
        float nextSpawnPosition = NEXT_SPAWN_POINT;
        public ArrayList _spawners;
        public ArrayList _deadSpawners;
        private Sprite _coinMeter;
        private Sprite _doppelMeter;
        private Sprite _coinMeterHurt;
        private Sprite _doppelMeterHurt;
        private float _coinMeterTimer;
        private float _coinMeterHurtTimer;
        LootSpill[] lootSpills;
        protected int _coins;
        protected int _doubleCoins;
        protected ArrayList _coinSprites;
        protected ArrayList _doppelCoinSprites;
        float coinMeterTimer;
        Vector2 coinMeterPosition;
        bool doppel;
        bool doppelType;
        ArrayList coinsDone;

        ArrayList shotSounds;
        int shotSoundsPlayed;

        ArrayList pikeSounds;
        int pikeSoundsPlayed;

        ArrayList meleeSounds;
        int meleeSoundsPlayed;

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

            _coins = MAX_COINS/2;
            _doubleCoins = 0;
            _coinSprites = new ArrayList(20);
            _doppelCoinSprites = new ArrayList(20);
            coinMeterPosition = new Vector2(COIN_METER_POSITION.X, COIN_METER_POSITION.Y);
            
            _usedFormations = new List<int>(_levelData.formations.Count);
            _spawners = new ArrayList(2);
            _deadSpawners = new ArrayList(2);

            for (int i = 0; i < _coins; i++)
            {
                _coinSprites.Add(new Coin(this, BASE_COIN_START_POSITION, new Vector2(BASE_COIN_POSITION.X, BASE_COIN_POSITION.Y - i * 4f)));
            }
            _coinMeter = new Sprite(PikeAndShotGame.COIN_METER, new Rectangle(0, 0, 36, 94), 36, 94, false, true, 128, new Color(Color.Yellow.R, Color.Yellow.G, 100), 2);
            _coinMeterHurt = new Sprite(PikeAndShotGame.COIN_METER, new Rectangle(0, 0, 36, 94), 36, 94, false, true, 128, Color.Red,2);
            _doppelMeter = new Sprite(PikeAndShotGame.DOPPEL_METER, new Rectangle(0, 0, 36, 100), 36, 100, false, true, 80, Color.White, 0.5f);
            _doppelMeterHurt = new Sprite(PikeAndShotGame.DOPPEL_METER, new Rectangle(0, 0, 36, 100), 36, 100, false, true, 80, Color.Red, 2f);
            _coinMeterTimer = 0f;
            lootSpills = new LootSpill[4];
            for (int i = 0; i < 4; i++)
            {
                lootSpills[i] = new LootSpill(this, Vector2.Zero, 0f, Vector2.Zero);
            }

            coinsDone = new ArrayList();

            shotSounds = new ArrayList(5);
            shotSounds.Add(PikeAndShotGame.SHOT_0.CreateInstance());
            shotSounds.Add(PikeAndShotGame.SHOT_1.CreateInstance());
            shotSounds.Add(PikeAndShotGame.SHOT_2.CreateInstance());
            shotSounds.Add(PikeAndShotGame.SHOT_3.CreateInstance());
            shotSounds.Add(PikeAndShotGame.SHOT_4.CreateInstance());

            foreach (SoundEffectInstance sfx in shotSounds)
                sfx.Volume = 0.5f;

            ((SoundEffectInstance)shotSounds[3]).Volume = 0.25f;
            ((SoundEffectInstance)shotSounds[4]).Volume = 0.25f;

            pikeSounds = new ArrayList(6);
            pikeSounds.Add(PikeAndShotGame.PIKE_0.CreateInstance());
            pikeSounds.Add(PikeAndShotGame.PIKE_1.CreateInstance());
            pikeSounds.Add(PikeAndShotGame.PIKE_2.CreateInstance());
            pikeSounds.Add(PikeAndShotGame.PIKE_3.CreateInstance());
            pikeSounds.Add(PikeAndShotGame.PIKE_4.CreateInstance());
            pikeSounds.Add(PikeAndShotGame.PIKE_5.CreateInstance());

            foreach (SoundEffectInstance sfx in pikeSounds)
                sfx.Volume = 0.5f;

            meleeSounds = new ArrayList(6);
            meleeSounds.Add(PikeAndShotGame.MELEE_CLANG_0.CreateInstance());
            meleeSounds.Add(PikeAndShotGame.MELEE_CLANG_1.CreateInstance());
            meleeSounds.Add(PikeAndShotGame.MELEE_CLANG_2.CreateInstance());
            meleeSounds.Add(PikeAndShotGame.MELEE_CLANG_0.CreateInstance());
            meleeSounds.Add(PikeAndShotGame.MELEE_CLANG_1.CreateInstance());
            meleeSounds.Add(PikeAndShotGame.MELEE_CLANG_2.CreateInstance());

            foreach (SoundEffectInstance sfx in meleeSounds)
                sfx.Volume = 0.25f;

            MediaPlayer.IsRepeating = true;
            MediaPlayer.Volume = 0.6f;
            //MediaPlayer.Play(PikeAndShotGame.THEME_1);
            doppelType = true;
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

            foreach(Coin c in _coinSprites)
            {
                if (c.isDone())
                {
                    coinsDone.Add(c);
                }
            }
            foreach (Coin c in coinsDone)
                _coinSprites.Remove(c);

            coinsDone.Clear();

            foreach (Coin c in _doppelCoinSprites)
            {
                if (c.isDone())
                {
                    coinsDone.Add(c);
                }
            }
            foreach (Coin c in coinsDone)
                _doppelCoinSprites.Remove(c);

            coinsDone.Clear();

            if (coinMeterTimer > 0)
            {
                if (doppel)
                {
                    coinMeterPosition.Y = easeInEaseOut(COIN_METER_DROPTIME - coinMeterTimer, COIN_METER_POSITION.Y, COIN_METER_OFFSET, COIN_METER_DROPTIME);
                }
                else
                {
                    coinMeterPosition.Y = easeInEaseOut(COIN_METER_DROPTIME - coinMeterTimer, COIN_METER_POSITION.Y + COIN_METER_OFFSET, -COIN_METER_OFFSET, COIN_METER_DROPTIME);
                }
                coinMeterTimer -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                if (coinMeterTimer <= 0)
                {
                    if (doppel)
                        _doppelMeter.dampening = 2;
                    else
                        _doppelMeter.dampening = 0.5f;
                }
            }
            else
            {
                if (doppel)
                {
                    coinMeterPosition.Y = COIN_METER_POSITION.Y + COIN_METER_OFFSET;
                }
                else
                {
                    coinMeterPosition.Y = COIN_METER_POSITION.Y;
                }
            }

            _fps = (double)_draws / gameTime.ElapsedGameTime.TotalSeconds;
            _draws = 0;
        }

        public void loseCoin(int soldierType)
        {
            _coinMeterHurtTimer = COIN_METER_HURT_FLASH_TIME;
            int i = 0;
            if (_doubleCoins > 0)
            {
                _doubleCoins = 0;
                doppel = false;
                coinMeterTimer = COIN_METER_DROPTIME;
                PikeAndShotGame.DOPPEL_DOWN.Play(0.25f,0,0);

                LootSpill spill;
                foreach (Coin c in _doppelCoinSprites)
                {
                    c.setDone();
                    for (int j = 0; j < 3; j++)
                    {
                        spill = new LootSpill(this, Vector2.Zero, 0f, Vector2.Zero);
                        spill.reset(c._position);
                    }
                }

                foreach (Coin c in _coinSprites)
                {
                    c.finalPosition.Y = COIN_METER_POSITION.Y + 48 - i++ * 4f;
                }
                _coins--;
                ((Coin)_coinSprites[_coinSprites.Count - 1]).setDone();
                foreach (LootSpill spilly in lootSpills)
                {
                    spilly.reset(((Coin)_coinSprites[_coinSprites.Count - 1])._position);
                }
                if (soldierType != Soldier.TYPE_SWINGER)
                {
                    spawnRescue(soldierType);
                }
            }
            else if (_coins > 0)
            {
                _coins--;
                ((Coin)_coinSprites[_coinSprites.Count - 1]).setDone();
                foreach (LootSpill spill in lootSpills)
                {
                    spill.reset(((Coin)_coinSprites[_coinSprites.Count - 1])._position);
                }

                if (_coins < 1)
                {
                    retreat();
                }
                else if (soldierType != Soldier.TYPE_SWINGER)
                {
                    spawnRescue(soldierType);
                }
            }
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
            PikeAndShotGame.GAME_OVER.Play();
        }

        public Vector2 spawnRescue(int type)
        {
            EnemyFormation formation;
            formation = new EnemyFormation("Reinforcement", null, this, getMapOffset().X - Soldier.WIDTH, _formation._position.Y, 1, SIDE_PLAYER);
            assignRescue(formation, type);
            _enemyFormationsToAdd.Add(formation);
            return formation.getPosition();
        }

        public void collectCoin(Soldier soldier)
        {
            Loot loot = new Loot(this, soldier.getPosition());
            LootTwinkle twinkle = new LootTwinkle(this, soldier.getPosition(), 500f, COIN_METER_POSITION + new Vector2(_coinMeter.getBoundingRect().Width/2, 0f));
            loot.addListener(twinkle);
            twinkle.addListener(this);
        }

        public float easeInEaseOut (float t, float b, float c, float d) 
        {
            t /= d/2;
            if (t < 1) return c/2*t*t + b;
            t--;
            return -c/2 * (t*(t-2) - 1) + b;
        }

        public void onAnimationTrigger(ScreenAnimation screenAnimaton)
        {
            if (screenAnimaton is LootTwinkle)
            {
                
                if (!doppel)
                {
                    _coinMeterTimer = COIN_METER_FLASH_TIME;
                    _coinSprites.Add(new Coin(this, BASE_COIN_START_POSITION, new Vector2(BASE_COIN_POSITION.X, BASE_COIN_POSITION.Y - _coins * 4f)));
                    _coins++;
                    if (_coins >= MAX_COINS)
                    {
                        doppel = true;
                        PikeAndShotGame.DOPPEL_UP.Play();
                        coinMeterTimer = COIN_METER_DROPTIME;
                        int i = 0;
                        foreach (Coin c in _coinSprites)
                        {
                            c.finalPosition.Y = COIN_METER_POSITION.Y + COIN_METER_OFFSET + 48 - i++ * 4f;
                        }
                    }
                }
                else if (_doubleCoins < MAX_COINS)
                {
                    _coinMeterTimer = COIN_METER_FLASH_TIME;
                    _doppelCoinSprites.Add(new Coin(this, BASE_COIN_START_POSITION, new Vector2(BASE_COIN_POSITION.X, BASE_COIN_POSITION.Y + 10f - _doubleCoins * 4f)));
                    _doubleCoins++;
                    if (_doubleCoins >= MAX_COINS)
                    {
                        if (((ArrayList)_formation._supportRows[0]).Count < 4)
                        {
                            foreach (Coin c in _doppelCoinSprites)
                            {
                                c.drop(BASE_COIN_POSITION.Y + 14f);
                            }
                            _doubleCoins = 0;
                            spawnRescue(Soldier.TYPE_SWINGER);
                            PikeAndShotGame.POWER_UP.Play();
                        }
                    }
                }
            }
        }

        public override void draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            base.draw(gameTime, spriteBatch);

            _draws++;

            //draw UI
            spriteBatch.Draw(PikeAndShotGame.COIN_METER_BACK, COIN_METER_POSITION, Color.Black);

            for (int i = 0; i < _doppelCoinSprites.Count; i++)
            {
                ((Coin)_doppelCoinSprites[i]).draw(spriteBatch);
            }

            if (doppel)
            {
                if(coinMeterTimer > 0)
                    _doppelMeter.draw(spriteBatch, COIN_METER_POSITION, SIDE_PLAYER, coinMeterTimer / COIN_METER_DROPTIME);
                else
                    _doppelMeter.draw(spriteBatch, COIN_METER_POSITION, SIDE_PLAYER);
            }
            

            spriteBatch.Draw(PikeAndShotGame.COIN_METER_BACK, coinMeterPosition, Color.Black);

            for (int i = 0; i < _coinSprites.Count; i++)
            {
                ((Coin)_coinSprites[i]).draw(spriteBatch);
            }

            spriteBatch.Draw(PikeAndShotGame.COIN_METER, coinMeterPosition, Color.White);

            //I draw the flash overtop, I was worried about missing a frame.
            if (_coinMeterTimer > 0)
            {
                if (doppel && coinMeterTimer <=0)
                    _doppelMeter.draw(spriteBatch, COIN_METER_POSITION, SIDE_PLAYER, _coinMeterTimer / COIN_METER_FLASH_TIME);
                else
                    _coinMeter.draw(spriteBatch, coinMeterPosition, SIDE_PLAYER, _coinMeterTimer / COIN_METER_FLASH_TIME);

                _coinMeterTimer -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            }

            if (_coinMeterHurtTimer > 0)
            {
                _coinMeterHurt.draw(spriteBatch, coinMeterPosition, SIDE_PLAYER, _coinMeterHurtTimer / COIN_METER_HURT_FLASH_TIME);
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
                //_formation.meleeCharge();
                //_formation.swingAttack();
            }
            if ((keyboardState.IsKeyDown(Keys.X) && !keyboardState.IsKeyDown(Keys.Z)) || (gamePadState.IsButtonDown(Buttons.X) && !gamePadState.IsButtonDown(Buttons.A)))
            {
                _formation.shotAttack();
            }
            if (keyboardState.IsKeyUp(Keys.X) && gamePadState.IsButtonUp(Buttons.X))
            {
                _formation.needTriggerUp = false;
            }

            if (PikeAndShotGame.DEBUG)
            {
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
                    //_formation.haltHorses();
                    collectCoin(new Brigand(this, getMapOffset().X, getMapOffset().Y + PikeAndShotGame.SCREENHEIGHT * 0.5f, SIDE_PLAYER));
                }
                if (keyboardState.IsKeyDown(Keys.V) && previousKeyboardState.IsKeyUp(Keys.V))
                {
                    _formation.turnHorses();
                }
                /*if (keyboardState.IsKeyDown(Keys.A) && previousKeyboardState.IsKeyUp(Keys.A) || (gamePadState.IsButtonDown(Buttons.LeftShoulder) && previousGamePadState.IsButtonUp(Buttons.LeftShoulder)))
                {
                    _formation.reduceWidth();
                }
                else if (keyboardState.IsKeyDown(Keys.S) && previousKeyboardState.IsKeyUp(Keys.S) || (gamePadState.IsButtonDown(Buttons.RightShoulder) && previousGamePadState.IsButtonUp(Buttons.RightShoulder)))
                {
                    _formation.increaseWidth();
                }*/
                else if (keyboardState.IsKeyDown(Keys.L) && previousKeyboardState.IsKeyUp(Keys.L))
                {
                    //spawnRescue();
                }
                if (keyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.D) && previousKeyboardState.IsKeyUp(Microsoft.Xna.Framework.Input.Keys.D))
                    toggleDrawDots();
                if (keyboardState.IsKeyDown(Keys.D7) && previousKeyboardState.IsKeyUp(Keys.D7))
                {
                    doppelType = !doppelType;
                }
            }

            base.getInput(timeSpan);
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
                switch(type)
                {
                    case Soldier.TYPE_PIKE:
                        soldier = new Pikeman(this, formation.getPosition().X, formation.getPosition().Y, SIDE_PLAYER);
                        break;
                    case Soldier.TYPE_SHOT:
                        soldier = new Arquebusier(this, formation.getPosition().X, formation.getPosition().Y, SIDE_PLAYER);
                        break;
                    case Soldier.TYPE_SWINGER:
                        if(doppelType)
                            soldier = new Dopple(this, formation.getPosition().X, formation.getPosition().Y, SIDE_PLAYER);
                        else
                            soldier = new CrossbowmanPavise(this, formation.getPosition().X, formation.getPosition().Y, SIDE_PLAYER);
                        break;
                    default:
                        soldier = new Dopple(this, formation.getPosition().X, formation.getPosition().Y, SIDE_PLAYER);
                        break;
                }
                formation.addSoldier(soldier);
                soldier.setSpeed(0.25f);
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

        internal ArrayList dangerOnScreen()
        {
            ArrayList enemies = new ArrayList(_enemyFormations.Count * 10);
            ArrayList aimedShots = new ArrayList(_shots.Count);
            bool addSoldier;
            foreach(Shot shot in _shots)
            {
                if (shot is AimedBolt)
                    aimedShots.Add(shot);
            }
            foreach (EnemyFormation f in _enemyFormations)
            {
                foreach (Soldier s in f.getSoldiers())
                {
                    if (s.getSide() == SIDE_ENEMY
                        && s.getState() != Soldier.STATE_DYING && s.getState() != Soldier.STATE_DEAD
                        && s._position.X + Soldier.WIDTH * 2f - (getMapOffset().X) > 0
                        && s._position.X - Soldier.WIDTH - (getMapOffset().X) < PikeAndShotGame.SCREENWIDTH
                        && s._position.Y + Soldier.HEIGHT - (getMapOffset().Y) > 0
                        && s._position.Y - Soldier.HEIGHT - (getMapOffset().Y) < PikeAndShotGame.SCREENHEIGHT)
                    {
                        addSoldier = true;
                        foreach (AimedBolt ab in aimedShots)
                        {
                            if (ab.targetSoldier == s)
                                addSoldier = false;
                        }
                        if(addSoldier)
                            enemies.Add(s);
                    }
                }
            }

            foreach (Soldier s in _looseSoldiers)
            {
                if (s.getSide() == SIDE_ENEMY
                    && s.getState() != Soldier.STATE_DYING && s.getState() != Soldier.STATE_DEAD
                    && s._position.X + Soldier.WIDTH * 2f - (getMapOffset().X) > 0
                    && s._position.X - Soldier.WIDTH - (getMapOffset().X) < PikeAndShotGame.SCREENWIDTH
                    && s._position.Y + Soldier.HEIGHT - (getMapOffset().Y) > 0
                    && s._position.Y - Soldier.HEIGHT - (getMapOffset().Y) < PikeAndShotGame.SCREENHEIGHT)
                    enemies.Add(s);
            }

            foreach (ArrayList row in _formation._supportRows)
            {
                foreach (Soldier d in row)
                {
                    if ((d is Dopple) && d.guardTarget != null)
                        enemies.Remove(d.guardTarget);
                }
            }

            return enemies;
        }

        internal void restart()
        {
            doppel = false;
            coinMeterPosition = COIN_METER_POSITION;
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

            _coins = MAX_COINS/2;
            _doubleCoins = 0;
            _coinSprites.Clear();
            _doppelCoinSprites.Clear();
            for(int i = 0; i < _coins; i++)
            {
                _coinSprites.Add(new Coin(this, BASE_COIN_START_POSITION, new Vector2(BASE_COIN_POSITION.X, BASE_COIN_POSITION.Y - i * 4f)));
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
            //MediaPlayer.Stop();
            //MediaPlayer.Play(PikeAndShotGame.THEME_1);
        }

        internal void makeShotSound()
        {
            if(shotSoundsPlayed < 3)
            {
                ((SoundEffectInstance)shotSounds[shotSoundsPlayed++]).Play();
            }
        }

        public void clearShotSounds()
        {
            shotSoundsPlayed = 0;
            SoundEffectInstance instance = (SoundEffectInstance)shotSounds[PikeAndShotGame.random.Next(shotSounds.Count -1)];
            shotSounds.Remove(instance);
            shotSounds.Add(instance);
        }

        internal void makePikeSound()
        {
            if (pikeSoundsPlayed < pikeSounds.Count)
            {
                ((SoundEffectInstance)pikeSounds[pikeSoundsPlayed++]).Play();
                if (pikeSoundsPlayed < pikeSounds.Count)
                    clearPikeSounds();
            }
        }

        public void clearPikeSounds()
        {
            pikeSoundsPlayed = 0;
            SoundEffectInstance instance = (SoundEffectInstance)pikeSounds[PikeAndShotGame.random.Next(pikeSounds.Count - 1)];
            pikeSounds.Remove(instance);
            pikeSounds.Add(instance);
        }

        public void makeMeleeSound()
        {
            if (meleeSoundsPlayed >= meleeSounds.Count)
            {
                meleeSoundsPlayed = 0;
            }
            ((SoundEffectInstance)meleeSounds[meleeSoundsPlayed++]).Play();
        }
    }
}

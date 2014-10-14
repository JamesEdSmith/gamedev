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
using System.Xml;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate;


namespace PikeAndShot
{
    class FormationEditorScreen : BattleScreen
    {
        private List<LevelPipeline.PatternActionContent> _pattern;

        private List<int> _actions;
        private double _actionStart;
        private LevelPipeline.PatternActionContent _currPatternAction;
        private int _fileCount;

        public FormationEditorScreen(PikeAndShotGame game): base(game)
        {
            _formation = new Formation(this, 600, 200, 5, SIDE_ENEMY);
            _pattern = new List<LevelPipeline.PatternActionContent>();
            _actions = new List<int>(1);
            _actions.Add(PatternAction.ACTION_IDLE);
            _currPatternAction = new LevelPipeline.PatternActionContent(_actions, 0f);
            _fileCount = 0;
        }

        protected override void getInput(TimeSpan timeSpan)
        {
            keyboardState = Keyboard.GetState();
            gamePadState = GamePad.GetState(PlayerIndex.One);

            if (_pattern.Count == 0)
                _actionStart = timeSpan.TotalSeconds;

            // Allows the game to exit
            if (keyboardState.IsKeyDown(Keys.Escape) || GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                _game.Exit();

            if (keyboardState.IsKeyDown(Keys.Left) || gamePadState.IsButtonDown(Buttons.DPadLeft))
            {
                if ((keyboardState.IsKeyDown(Keys.Up) || gamePadState.IsButtonDown(Buttons.DPadUp)) || (keyboardState.IsKeyDown(Keys.Down) || gamePadState.IsButtonDown(Buttons.DPadDown)))
                    _formation.marchLeft(timeSpan.TotalMilliseconds, true);
                else
                    _formation.marchLeft(timeSpan.TotalMilliseconds, false);
            }
            if (keyboardState.IsKeyDown(Keys.Right) || gamePadState.IsButtonDown(Buttons.DPadRight))
            {
                if ((keyboardState.IsKeyDown(Keys.Up) || gamePadState.IsButtonDown(Buttons.DPadUp)) || (keyboardState.IsKeyDown(Keys.Down) || gamePadState.IsButtonDown(Buttons.DPadDown)))
                {
                    _formation.marchRight(timeSpan.TotalMilliseconds, true);
                    if (_formation.getCenter().X >= PikeAndShotGame.SCREENWIDTH * BattleScreen.SCROLLPOINT + _mapOffset.X)
                        _mapOffset.X += _formation.getSpeed() * (float)timeSpan.TotalMilliseconds * 0.708f * 0.75f;
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
            if (keyboardState.IsKeyDown(Keys.Down) || gamePadState.IsButtonDown(Buttons.DPadDown))
            {
                if ((keyboardState.IsKeyDown(Keys.Left) || gamePadState.IsButtonDown(Buttons.DPadLeft)) || (keyboardState.IsKeyDown(Keys.Right) || gamePadState.IsButtonDown(Buttons.DPadRight)))
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
            if (keyboardState.IsKeyDown(Keys.Up) || gamePadState.IsButtonDown(Buttons.DPadUp))
            {
                if ((keyboardState.IsKeyDown(Keys.Left) || gamePadState.IsButtonDown(Buttons.DPadLeft)) || (keyboardState.IsKeyDown(Keys.Right) || gamePadState.IsButtonDown(Buttons.DPadRight)))
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
            else if ((keyboardState.IsKeyUp(Keys.Z) || keyboardState.IsKeyUp(Keys.X)) && (previousKeyboardState.IsKeyDown(Keys.Z) && previousKeyboardState.IsKeyDown(Keys.X)))
            {
                _formation.cancelCharge();
            }
            else if ((gamePadState.IsButtonUp(Buttons.A) || gamePadState.IsButtonUp(Buttons.X)) && (previousGamePadState.IsButtonDown(Buttons.A) && previousGamePadState.IsButtonDown(Buttons.X)))
            {
                _formation.cancelCharge();
            }
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


            if ((keyboardState.IsKeyDown(Keys.OemQuestion) && previousKeyboardState.IsKeyUp(Keys.OemQuestion)) || (gamePadState.IsButtonDown(Buttons.Start) && previousGamePadState.IsButtonUp(Buttons.Start)))
            {
                writeFormation();
                _pattern.Clear();
                _formation = new Formation(this, 600, 200, 5, SIDE_ENEMY);
                _fileCount++;

            }
            // only when the keys being pressed changes, do we create a new part of the behavoural pattern
            else if (!gamePadState.Equals(previousGamePadState) || !keyboardState.Equals(previousKeyboardState))
            {
                addNextPatternAction();
            }

            previousKeyboardState = keyboardState;
            previousGamePadState = gamePadState;
        }

        private void addNextPatternAction()
        {
            _currPatternAction.duration = _elapsedTime - _actionStart;
            _actionStart = _elapsedTime;

            _pattern.Add(_currPatternAction);
            _actions = new List<int>(5);

            if (keyboardState.IsKeyDown(Keys.Left) || gamePadState.IsButtonDown(Buttons.DPadLeft))
            {
                _actions.Add(PatternAction.ACTION_LEFT);
            }
            if (keyboardState.IsKeyDown(Keys.Right) || gamePadState.IsButtonDown(Buttons.DPadRight))
            {
                _actions.Add(PatternAction.ACTION_RIGHT);
            }
            if (keyboardState.IsKeyDown(Keys.Up) || gamePadState.IsButtonDown(Buttons.DPadUp))
            {
                _actions.Add(PatternAction.ACTION_UP);
            }
            if (keyboardState.IsKeyDown(Keys.Down) || gamePadState.IsButtonDown(Buttons.DPadDown))
            {
                _actions.Add(PatternAction.ACTION_DOWN);
            }
            if (keyboardState.IsKeyDown(Keys.Z) || gamePadState.IsButtonDown(Buttons.A))
            {
                _actions.Add(PatternAction.ACTION_PIKE);
            }
            else
            {
                _actions.Add(PatternAction.ACTION_RAISE);
            }
            if (keyboardState.IsKeyDown(Keys.X) || gamePadState.IsButtonDown(Buttons.X))
            {
                _actions.Add(PatternAction.ACTION_SHOT);
            }
            if (_actions.Count == 0)
                _actions.Add(PatternAction.ACTION_IDLE);

            _currPatternAction = new LevelPipeline.PatternActionContent(_actions, 0f);
        }

        private void writeFormation()
        {
            // phony name
            LevelPipeline.LevelContent testValue = new LevelPipeline.LevelContent("editor" + _fileCount);

            // now we build the level data from our stored formation data
            ArrayList soldiers = _formation.getSoldiers();
            List<int> soldierInts = new List<int>(30);

            foreach (Soldier soldat in soldiers)
            {
                soldierInts.Add(soldat.getClass());
            }
            testValue.formations.Add(soldierInts);

            // remove starting motionlessness
            while (_pattern.ElementAt(0).actions.Count == 1 && _pattern[0].actions[0] == PatternAction.ACTION_RAISE)
                _pattern.RemoveAt(0);

            testValue.formationActions.Add(_pattern);
            testValue.formationPositions.Add(new Vector2(0f, 0f));
            testValue.formationNames.Add("Cock");

            testValue.formationActionNames.Add(new List<string>(32));
            foreach (LevelPipeline.PatternActionContent p in _pattern)
                testValue.formationActionNames[0].Add("Balls");

            testValue.formationTimes.Add(0f);

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;

            using (XmlWriter xmlWriter = XmlWriter.Create("Content\\editor" + _fileCount + ".xml", settings))
            {
                IntermediateSerializer.Serialize(xmlWriter, testValue, null);
            }
        }
    }
}

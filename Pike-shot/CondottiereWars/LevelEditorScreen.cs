using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using System.Collections;
using System.Xml;
using Microsoft.Xna.Framework.Graphics;
using System.Windows.Forms;

namespace PikeAndShot
{
    class LevelEditorScreen : BattleScreen, FormListener
    {
        private Level _levelData;
        private EnemyFormation _newEnemyFormation;
        private EnemyFormation _grabbedFormation;
        private Sprite _pointerSprite;
        private Vector2 _pointerPos;
        protected MouseState mouseState;
        protected MouseState prevMouseState;
        private LevelEditorScreenListner _listener;
        protected Vector2 _oldMousePosition;
        protected Vector2 _startingMousePosition;
        protected Vector2 _endingMousePosition;
        private ArrayList _grabbedFormations;
        private bool _boxSelecting;
        private bool _boxMoving;

        // [dsl] Added zoom stuff
        private float[] _zoomLevels = new float[] { 1.0f, .75f, .50f, .25f, .125f };
        private int _currentZoom = 0;
        
        public LevelEditorScreen(PikeAndShotGame game, LevelConstructorForm form)
            : base(game)
        {
            _levelData = form.getCurrLevel(); 
            _pointerSprite = new Sprite(PikeAndShotGame.SWORD_POINTER, new Rectangle(0, 0, 18,18),18, 18);
            prevMouseState = Mouse.GetState();
            _grabbedFormation = null;
            _grabbedFormations = new ArrayList(30);
            _listener = form;
            _oldMousePosition = new Vector2(0f, 0);
            _boxSelecting = false;
            _boxMoving = false;
        }

        void FormListener.updateLevel(Level level, int selectedFormation)
        {
            _levelData = level;

            foreach (EnemyFormation f in _enemyFormations)
            {
                _deadFormations.Add(f);
            }

            _enemyFormations.Clear();

            for (int f = 0; f < _levelData.formations.Count; f++)
            {
                _newEnemyFormation = new EnemyFormation(_levelData.formationNames[f], _levelData.formationActions[f], this, (_levelData.formationPositions[f]).X, (_levelData.formationPositions[f]).Y, 5, _levelData.formationSides[f]);

                for (int i = 0; i < _levelData.formations[f].Count; i++)
                {
                    switch ((_levelData.formations[f])[i])
                    {
                        case Soldier.CLASS_MERC_PIKEMAN:
                            _newEnemyFormation.addSoldier(new Pikeman(this, _newEnemyFormation.getPosition().X, _newEnemyFormation.getPosition().Y, SIDE_ENEMY));
                            break;
                        case Soldier.CLASS_MERC_ARQUEBUSIER:
                            _newEnemyFormation.addSoldier(new Arquebusier(this, _newEnemyFormation.getPosition().X, _newEnemyFormation.getPosition().Y, SIDE_ENEMY));
                            break;
                        case Soldier.CLASS_MERC_CROSSBOWMAN:
                            _newEnemyFormation.addSoldier(new Crossbowman(this, _newEnemyFormation.getPosition().X, _newEnemyFormation.getPosition().Y, SIDE_ENEMY));
                            break;
                        case Soldier.CLASS_MERC_SOLDIER:
                            _newEnemyFormation.addSoldier(new Targeteer(this, _newEnemyFormation.getPosition().X, _newEnemyFormation.getPosition().Y, SIDE_ENEMY));
                            break;
                        case Soldier.CLASS_GOBLIN_SLINGER:
                            _newEnemyFormation.addSoldier(new Slinger(this, _newEnemyFormation.getPosition().X, _newEnemyFormation.getPosition().Y, SIDE_ENEMY));
                            break;
                        case Soldier.CLASS_MERC_DOPPLE:
                            _newEnemyFormation.addSoldier(new Dopple(this, _newEnemyFormation.getPosition().X, _newEnemyFormation.getPosition().Y, SIDE_ENEMY));
                            break;
                        case Soldier.CLASS_GOBLIN_BERZERKER:
                            _newEnemyFormation.addSoldier(new Berzerker(this, _newEnemyFormation.getPosition().X, _newEnemyFormation.getPosition().Y, SIDE_ENEMY));
                            break;
                        case Soldier.CLASS_GOBLIN_BRIGAND:
                            _newEnemyFormation.addSoldier(new Brigand(this, _newEnemyFormation.getPosition().X, _newEnemyFormation.getPosition().Y, SIDE_ENEMY));
                            break;
                        case Soldier.CLASS_MERC_CROSSBOWMAN_PAVISE:
                            _newEnemyFormation.addSoldier(new CrossbowmanPavise(this, _newEnemyFormation.getPosition().X, _newEnemyFormation.getPosition().Y, SIDE_ENEMY));
                            break;
                        case Soldier.CLASS_MERC_CAVALRY:
                            _newEnemyFormation.addSoldier(new Cavalry(this, _newEnemyFormation.getPosition().X, _newEnemyFormation.getPosition().Y, SIDE_ENEMY));
                            break;
                        case Soldier.CLASS_GOBLIN_WOLF:
                            _newEnemyFormation.addSoldier(new Wolf(this, _newEnemyFormation.getPosition().X, _newEnemyFormation.getPosition().Y, SIDE_ENEMY));
                            break;

                    }
                }
                _enemyFormations.Add(_newEnemyFormation);
            }

            if (selectedFormation != -1 && _enemyFormations.Count > selectedFormation)
            {
                ((EnemyFormation)_enemyFormations[selectedFormation]).selected = true;
            }
        }

        public override void update(GameTime gameTime)
        {
            base.update(gameTime);
            if (_grabbedFormation != null)
            {
                _grabbedFormation.setPosition((float)_pointerPos.X + _mapOffset.X, (float)_pointerPos.Y + _mapOffset.Y);
                _grabbedFormation.resetupFormation();
                _grabbedFormation.reformFormation();
                _grabbedFormation.selected = true;
            }
            if (_boxMoving)
            {
                Vector2 mousePosition = new Vector2(_pointerPos.X, _pointerPos.Y) + _mapOffset;
                _startingMousePosition += mousePosition - _oldMousePosition;
                _endingMousePosition += mousePosition - _oldMousePosition;
                foreach (EnemyFormation ef in _grabbedFormations)
                {
                    ef.setPosition(ef.getPosition() + (mousePosition - _oldMousePosition));
                    ef.resetupFormation();
                    ef.reformFormation();
                    ef.selected = true;
                }
                _oldMousePosition = mousePosition;
            }
        }

        public override void draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            base.draw(gameTime, spriteBatch);
            if (_grabbedFormation != null)
            {
                spriteBatch.DrawString(PikeAndShotGame.getSpriteFont(), "Grab: " + _grabbedFormation.getPosition().X + " " + _grabbedFormation.getPosition().Y, new Vector2(5, 5), Color.White);
                spriteBatch.DrawString(PikeAndShotGame.getSpriteFont(), "Solider: " + ((Soldier)_grabbedFormation.getSoldiers()[0]).getPosition().X + " " + ((Soldier)_grabbedFormation.getSoldiers()[0]).getPosition().Y, new Vector2(5, 20), Color.White);
            }
            else
            {
                spriteBatch.DrawString(PikeAndShotGame.getSpriteFont(), "Grab: " + 0 + " " + 0, new Vector2(5, 5), Color.White);
                spriteBatch.DrawString(PikeAndShotGame.getSpriteFont(), "Solider: " + 0 + " " + 0, new Vector2(5, 20), Color.White);
            }

            spriteBatch.DrawString(PikeAndShotGame.getSpriteFont(), "mapOffset: " + _mapOffset.X + ", " + _mapOffset.Y, new Vector2(5, 35), Color.White);
            spriteBatch.DrawString(PikeAndShotGame.getSpriteFont(), "zoom: " + _currentZoom, new Vector2(5, 50), Color.White);

            int lineThickness = (int)(2.0f / PikeAndShotGame.ZOOM);

            if (_boxSelecting)
            {
                //spriteBatch.Draw(PikeAndShotGame.getDotTexture(), new Vector2(_startingMousePosition.X - _mapOffset.X, _startingMousePosition.Y - _mapOffset.Y), Color.White);
                //spriteBatch.Draw(PikeAndShotGame.getDotTexture(), new Vector2(_startingMousePosition.X - _mapOffset.X, _endingMousePosition.Y - _mapOffset.Y), Color.White);
                //spriteBatch.Draw(PikeAndShotGame.getDotTexture(), new Vector2(_endingMousePosition.X - _mapOffset.X, _startingMousePosition.Y - _mapOffset.Y), Color.White);
                //spriteBatch.Draw(PikeAndShotGame.getDotTexture(), new Vector2(_endingMousePosition.X - _mapOffset.X, _endingMousePosition.Y - _mapOffset.Y), Color.White);

                // [dsl] Actual box
                spriteBatch.Draw(PikeAndShotGame.getDotTexture(),
                    new Rectangle(
                        (int)(Math.Min(_startingMousePosition.X, _endingMousePosition.X) - _mapOffset.X),
                        (int)(Math.Min(_startingMousePosition.Y, _endingMousePosition.Y) - _mapOffset.Y),
                        (int)(Math.Abs(_endingMousePosition.X - _startingMousePosition.X)),
                        lineThickness),
                    Color.White);
                spriteBatch.Draw(PikeAndShotGame.getDotTexture(),
                    new Rectangle(
                        (int)(Math.Min(_startingMousePosition.X, _endingMousePosition.X) - _mapOffset.X),
                        (int)(Math.Max(_startingMousePosition.Y, _endingMousePosition.Y) - _mapOffset.Y),
                        (int)(Math.Abs(_endingMousePosition.X - _startingMousePosition.X)),
                        lineThickness),
                    Color.White);
                spriteBatch.Draw(PikeAndShotGame.getDotTexture(),
                    new Rectangle(
                        (int)(Math.Min(_startingMousePosition.X, _endingMousePosition.X) - _mapOffset.X),
                        (int)(Math.Min(_startingMousePosition.Y, _endingMousePosition.Y) - _mapOffset.Y),
                        lineThickness,
                        (int)(Math.Abs(_endingMousePosition.Y - _startingMousePosition.Y))),
                    Color.White);
                spriteBatch.Draw(PikeAndShotGame.getDotTexture(),
                    new Rectangle(
                        (int)(Math.Max(_startingMousePosition.X, _endingMousePosition.X) - _mapOffset.X),
                        (int)(Math.Min(_startingMousePosition.Y, _endingMousePosition.Y) - _mapOffset.Y),
                        lineThickness,
                        (int)(Math.Abs(_endingMousePosition.Y - _startingMousePosition.Y))),
                    Color.White);
            }
            _pointerSprite.drawWithScale(spriteBatch, _pointerPos, SIDE_ENEMY, 1.0f / PikeAndShotGame.ZOOM);

            // Draw map boundaries, that will help when zoomed out or moved vertically
            spriteBatch.Draw(PikeAndShotGame.getDotTexture(),
                    new Rectangle(
                        0,
                        (int)(-_mapOffset.Y),
                        (int)((float)PikeAndShotGame.viewport.Width / PikeAndShotGame.ZOOM),
                        lineThickness),
                    Color.CornflowerBlue);
            spriteBatch.Draw(PikeAndShotGame.getDotTexture(),
                    new Rectangle(
                        0,
                        PikeAndShotGame.viewport.Height - (int)_mapOffset.Y,
                        (int)((float)PikeAndShotGame.viewport.Width / PikeAndShotGame.ZOOM),
                        lineThickness),
                    Color.CornflowerBlue);
        }

        protected override void getInput(TimeSpan timeSpan)
        {
            keyboardState = Keyboard.GetState();
            gamePadState = GamePad.GetState(PlayerIndex.One);
            mouseState = Mouse.GetState();

            _pointerPos = new Vector2(mouseState.X, mouseState.Y) / PikeAndShotGame.ZOOM;

            if (_game.IsActive)
            {
                // [dsl] Zoom stuff
                if (mouseState.ScrollWheelValue > prevMouseState.ScrollWheelValue)
                    _currentZoom = Math.Max(_currentZoom - 1, 0);
                else if (mouseState.ScrollWheelValue < prevMouseState.ScrollWheelValue)
                    _currentZoom = Math.Min(_currentZoom + 1, _zoomLevels.Count() - 1);
                PikeAndShotGame.ZOOM = _zoomLevels[_currentZoom];

                if (mouseState.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed && prevMouseState.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Released)
                {
                    if (_boxSelecting)
                        moveFormations();
                    else if (!grabFormation())
                        startSelectorBox();
                }
                else if (mouseState.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed && prevMouseState.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed)
                {
                    //When we aren't grabbing a formation we are grabbing the map
                    if (getGrabbedFormation() == null)
                    {
                        if (_boxSelecting && !_boxMoving)
                        {
                            _endingMousePosition = new Vector2(_mapOffset.X + _pointerPos.X, _mapOffset.Y + _pointerPos.Y);
                        }
                    }
                }
                else if (mouseState.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Released && prevMouseState.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed)
                {
                    //release the formation you are holding 
                    if (_grabbedFormation != null)
                    {
                        _listener.updateLevelFromScreen(_enemyFormations.IndexOf(_grabbedFormation), _grabbedFormation.getPosition().X, _grabbedFormation.getPosition().Y);
                        _grabbedFormation = null;
                    }
                    else if (_boxSelecting && !_boxMoving)
                    {
                        grabFormations();
                    }
                    if (_boxMoving)
                    {
                        dropFormations();
                    }
                }
                if (keyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.C) && previousKeyboardState.IsKeyUp(Microsoft.Xna.Framework.Input.Keys.C))
                {
                    if (_boxMoving)
                    {
                        copyFormations();
                    }
                    else if (_grabbedFormation != null)
                    {
                        _listener.copyFormation(_enemyFormations.IndexOf(_grabbedFormation));
                        _grabbedFormation = null;
                    }
                }
                else if (keyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.E) && previousKeyboardState.IsKeyUp(Microsoft.Xna.Framework.Input.Keys.E))
                {
                    if (_boxMoving)
                    {
                        deleteFormations();
                    }
                    else if (_grabbedFormation != null)
                    {
                        _listener.deleteFormation(_enemyFormations.IndexOf(_grabbedFormation));
                        _grabbedFormation = null;
                    }
                }
                else if (keyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Left))
                {
                    _mapOffset.X -= (float)timeSpan.TotalMilliseconds;
                }
                else if (keyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Right))
                {
                    _mapOffset.X += (float)timeSpan.TotalMilliseconds;
                }
                else if (keyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Up))
                {
                    _mapOffset.Y -= (float)timeSpan.TotalMilliseconds;
                }
                else if (keyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Down))
                {
                    _mapOffset.Y += (float)timeSpan.TotalMilliseconds;
                }

                if (keyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.D) && previousKeyboardState.IsKeyUp(Microsoft.Xna.Framework.Input.Keys.D))
                    toggleDrawDots();
            }

            base.getInput(timeSpan);

            previousKeyboardState = keyboardState;
            previousGamePadState = gamePadState;
            prevMouseState = mouseState;
        }

        private void copyFormations()
        {
            ArrayList formationIndices = new ArrayList(_grabbedFormations.Count);
            foreach (EnemyFormation formy in _grabbedFormations)
            {
                formationIndices.Add(_enemyFormations.IndexOf(formy));
            }
            _listener.copyFormations(formationIndices);
            _grabbedFormations.Clear();
            foreach (int i in formationIndices)
            {
                _grabbedFormations.Add(_enemyFormations[i]);
            }
        }

        private void deleteFormations()
        {
            foreach (EnemyFormation formy in _grabbedFormations)
            {
                _listener.deleteFormation(_enemyFormations.IndexOf(formy));
            }
        }

        private void moveFormations()
        {
            bool didCollide = true;

            if (_pointerPos.X + _mapOffset.X < _startingMousePosition.X)
                didCollide = false;
            if (_pointerPos.X + _mapOffset.X > _endingMousePosition.X)
                didCollide = false;
            if (_pointerPos.Y + _mapOffset.Y < _startingMousePosition.Y)
                didCollide = false;
            if (_pointerPos.Y + _mapOffset.Y > _endingMousePosition.Y)
                didCollide = false;

            if (didCollide)
            {
                _boxMoving = true;
                _oldMousePosition = new Vector2(_pointerPos.X + _mapOffset.X, _pointerPos.Y + _mapOffset.Y);
            }
            else
            {
                _boxSelecting = false;
                _grabbedFormations.Clear();
            }

        }

        private void dropFormations()
        {
            _boxMoving = false;
            ArrayList formationsUpdate = new ArrayList(30);
            foreach (EnemyFormation enf in _enemyFormations)
            {
                formationsUpdate.Add(enf);
            }

            foreach (EnemyFormation ef in formationsUpdate)
            {
                _listener.updateLevelFromScreen(formationsUpdate.IndexOf(ef), ef.getPosition().X, ef.getPosition().Y);
            }
        }

        private void grabFormations()
        {
            bool collision = true;
            foreach (EnemyFormation ef in _enemyFormations)
            {
                collision = true;
                if (_endingMousePosition.X < ef.getPosition().X)
                    collision = false;
                else if (_startingMousePosition.X > ef.getPosition().X + ef.getTotalRows() * Soldier.WIDTH)
                    collision = false;
                else if (_endingMousePosition.Y < ef.getPosition().Y)
                    collision = false;
                else if (_startingMousePosition.Y > ef.getPosition().Y + ef.getTotalRows() * Soldier.HEIGHT)
                    collision = false;

                if (collision)
                {
                    _grabbedFormations.Add(ef);
                }
            }
        }

        private void startSelectorBox()
        {
            _startingMousePosition = new Vector2(_mapOffset.X + _pointerPos.X, _mapOffset.Y + _pointerPos.Y);
            _boxSelecting = true;
        }

        public EnemyFormation getGrabbedFormation()
        {
            return _grabbedFormation;
        }

        protected bool grabFormation()
        {
            bool collision = true;
            foreach (EnemyFormation ef in _enemyFormations)
            {
                collision = true;
                if (_pointerPos.X < ef.getScreenPosition().X)
                    collision = false;
                else if (_pointerPos.X > ef.getScreenPosition().X + ef.getTotalRows() * Soldier.WIDTH)
                    collision = false;
                else if (_pointerPos.Y < ef.getScreenPosition().Y)
                    collision = false;
                else if (_pointerPos.Y > ef.getScreenPosition().Y + ef.getTotalRows() * Soldier.HEIGHT)
                    collision = false;

                if (collision)
                {
                    _grabbedFormation = ef;
                    _grabbedFormation.selected = true;
                    return true;
                }
            }
            return false;
        }

    }

    public interface LevelEditorScreenListner
    {
        void updateLevelFromScreen(int formation, float x, float y);
        void copyFormation(int formation);
        void copyFormations(ArrayList formations);
        void deleteFormation(int formation);
    }
}

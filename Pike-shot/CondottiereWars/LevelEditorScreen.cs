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
    public interface LevelEditorGrabbable
    {
        bool selected{get; set;}
        int index{get; set;}
        void setPosition(float x, float y);
        void setPosition(Vector2 pos);
        Vector2 getPosition();
    }

    class LevelEditorScreen : BattleScreen, FormListener
    {
        private Level _levelData;
        private EnemyFormation _newEnemyFormation;
        private LevelEditorGrabbable _grabbedThing;
        private Sprite _pointerSprite;
        private Vector2 _pointerPos;
        protected MouseState mouseState;
        protected MouseState prevMouseState;
        private LevelEditorScreenListner _listener;
        protected Vector2 _oldMousePosition;
        protected Vector2 _startingMousePosition;
        protected Vector2 _endingMousePosition;
        private ArrayList _grabbedThings;
        private ArrayList _grabbedTerrains;
        private bool _boxSelecting;
        private bool _boxMoving;

        // [dsl] Added zoom stuff
        private float[] _zoomLevels = new float[] { 1.0f, .75f, .50f, .25f, .125f };
        private int _currentZoom = 0;
        
        public LevelEditorScreen(PikeAndShotGame game, LevelConstructorForm form)
            : base(game)
        {
            _levelData = form.getCurrLevel(); 
            _pointerSprite = new Sprite(PikeAndShotGame.SWORD_POINTER, new Rectangle(0, 0, 18, 18), 18, 18);
            prevMouseState = Mouse.GetState();
            _grabbedThing = null;
            _grabbedThings = new ArrayList(30);
            _grabbedTerrains = new ArrayList(30);
            _listener = form;
            _oldMousePosition = new Vector2(0f, 0f);
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

            foreach (Terrain t in _terrain)
            {
                _deadThings.Add(t);
            }

            _enemyFormations.Clear();
            _terrain.Clear();

            for (int f = 0; f < _levelData.formations.Count; f++)
            {
                _newEnemyFormation = new EnemyFormation(_levelData.formationNames[f], _levelData.formationActions[f], this, (_levelData.formationPositions[f]).X, (_levelData.formationPositions[f]).Y, 5, _levelData.formationSides[f], f);

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

            for (int f = 0; f < _levelData.terrains.Count; f++)
            {
                Terrain.getNewTerrain(_levelData.terrains[f], this, _levelData.terrainPositions[f].X, _levelData.terrainPositions[f].Y, f);
            }

            if (selectedFormation != -1 && _enemyFormations.Count > selectedFormation)
            {
                ((EnemyFormation)_enemyFormations[selectedFormation]).selected = true;
            }
        }

        public override void update(GameTime gameTime)
        {
            base.update(gameTime);
            if (_grabbedThing != null)
            {
                _grabbedThing.setPosition((float)_pointerPos.X + _mapOffset.X, (float)_pointerPos.Y + _mapOffset.Y);
                _grabbedThing.selected = true;
                if (_grabbedThing is EnemyFormation)
                {
                    ((EnemyFormation)_grabbedThing).resetupFormation();
                    ((EnemyFormation)_grabbedThing).reformFormation();
                }
            }
            if (_boxMoving)
            {
                Vector2 mousePosition = new Vector2(_pointerPos.X, _pointerPos.Y) + _mapOffset;
                _startingMousePosition += mousePosition - _oldMousePosition;
                _endingMousePosition += mousePosition - _oldMousePosition;
                foreach (LevelEditorGrabbable ef in _grabbedThings)
                {
                    ef.setPosition(ef.getPosition() + (mousePosition - _oldMousePosition));
                    if (ef is EnemyFormation)
                    {
                        ((EnemyFormation)ef).resetupFormation();
                        ((EnemyFormation)ef).reformFormation();
                        ef.selected = true;
                    }
                }
                _oldMousePosition = mousePosition;
            }
        }

        public override void draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            base.draw(gameTime, spriteBatch);
            if (_grabbedThing != null)
            {
                spriteBatch.DrawString(PikeAndShotGame.getSpriteFont(), "Grab: " + _grabbedThing.getPosition().X + " " + _grabbedThing.getPosition().Y, new Vector2(5, 5), Color.White);
                if(_grabbedThing is EnemyFormation)
                    spriteBatch.DrawString(PikeAndShotGame.getSpriteFont(), "Solider: " + ((Soldier)((EnemyFormation)_grabbedThing).getSoldiers()[0]).getPosition().X + " " + ((Soldier)((EnemyFormation)_grabbedThing).getSoldiers()[0]).getPosition().Y, new Vector2(5, 20), Color.White);
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
                    else if (!grabThing())
                        startSelectorBox();
                }
                else if (mouseState.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed && prevMouseState.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed)
                {
                    //When we aren't grabbing a formation we are grabbing the map
                    if (getGrabbedThing() == null)
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
                    if (_grabbedThing != null)
                    {
                        if(_grabbedThing is EnemyFormation)
                            _listener.updateLevelFromScreen(_grabbedThing.index, _grabbedThing.getPosition().X, _grabbedThing.getPosition().Y);
                        else
                            _listener.updateLevelFromScreenTerrain(_grabbedThing.index, _grabbedThing.getPosition().X, _grabbedThing.getPosition().Y);

                        _grabbedThing = null;
                    }
                    else if (_boxSelecting && !_boxMoving)
                    {
                        grabThings();
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
                    else if (_grabbedThing != null)
                    {
                        if(_grabbedThing is EnemyFormation)
                            _listener.copyFormation(_grabbedThing.index);
                        else
                            _listener.copyTerrain(_grabbedThing.index);
                        _grabbedThing = null;
                    }
                }
                else if (keyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.E) && previousKeyboardState.IsKeyUp(Microsoft.Xna.Framework.Input.Keys.E))
                {
                    if (_boxMoving)
                    {
                        deleteFormations();
                    }
                    else if (_grabbedThing != null)
                    {
                        if(_grabbedThing is EnemyFormation)
                            _listener.deleteFormation(_grabbedThing.index);
                        else
                            _listener.deleteTerrain(_grabbedThing.index);
                        _grabbedThing = null;
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
            ArrayList formationIndices = new ArrayList(_grabbedThings.Count);
            ArrayList terrainIndices = new ArrayList(_grabbedThings.Count);
            foreach (EnemyFormation formy in _grabbedThings)
            { 
                if(formy is EnemyFormation)
                    formationIndices.Add(formy.index);
                else
                    terrainIndices.Add(formy.index);
            }
            _listener.copyFormations(formationIndices, terrainIndices);
            
            _grabbedThings.Clear();
            /*foreach (int i in formationIndices)
            {
                _grabbedThings.Add(_enemyFormations[i]);
            }*/
        }

        private void deleteFormations()
        {
            foreach (LevelEditorGrabbable formy in _grabbedThings)
            {
                if(formy is EnemyFormation)
                    _listener.deleteFormation(formy.index);
                else
                    _listener.deleteTerrain(formy.index);
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
                _grabbedThings.Clear();
            }

        }

        private void dropFormations()
        {
            _boxMoving = false;
            ArrayList formationsUpdate = new ArrayList(30);
            foreach (LevelEditorGrabbable enf in _grabbedThings)
            {
                formationsUpdate.Add(enf);
            }
            foreach (LevelEditorGrabbable ef in formationsUpdate)
            {
                if(ef is EnemyFormation)
                    _listener.updateLevelFromScreen(ef.index, ef.getPosition().X, ef.getPosition().Y);
                else
                    _listener.updateLevelFromScreenTerrain(ef.index, ef.getPosition().X, ef.getPosition().Y);
            }
        }

        private void grabThings()
        {
            bool collision = true;
            _grabbedThings.Clear();
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
                    _grabbedThings.Add(ef);
                }
            }
            foreach (Terrain t in _terrain)
            {
                collision = true;
                if (_pointerPos.X < t.getPosition().X - getMapOffset().X)
                    collision = false;
                else if (_pointerPos.X > t.getPosition().X - getMapOffset().X + t.getWidth())
                    collision = false;
                else if (_pointerPos.Y < t.getPosition().Y - getMapOffset().Y)
                    collision = false;
                else if (_pointerPos.Y > t.getPosition().Y - getMapOffset().Y + t.getHeight())
                    collision = false;

                if (collision)
                {
                    _grabbedThing = t;
                }
            }

        }

        private void startSelectorBox()
        {
            _startingMousePosition = new Vector2(_mapOffset.X + _pointerPos.X, _mapOffset.Y + _pointerPos.Y);
            _boxSelecting = true;
        }

        public LevelEditorGrabbable getGrabbedThing()
        {
            return _grabbedThing;
        }

        protected bool grabThing()
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
                    _grabbedThing = ef;
                    _grabbedThing.selected = true;
                    return true;
                }
            }
            foreach (Terrain t in _terrain)
            {
                collision = true;
                if (_pointerPos.X < t.getPosition().X - getMapOffset().X)
                    collision = false;
                else if (_pointerPos.X > t.getPosition().X - getMapOffset().X + t.getWidth())
                    collision = false;
                else if (_pointerPos.Y < t.getPosition().Y - getMapOffset().Y)
                    collision = false;
                else if (_pointerPos.Y > t.getPosition().Y - getMapOffset().Y + t.getHeight())
                    collision = false;

                if (collision)
                {
                    _grabbedThing = t;
                    _grabbedThing.selected = true;
                    return true;
                }
            }

            return false;
        }
    }

    public interface LevelEditorScreenListner
    {
        void updateLevelFromScreen(int formation, float x, float y);
        void updateLevelFromScreenTerrain(int terrain, float x, float y);
        void copyFormation(int formation);
        void copyTerrain(int index);
        void copyFormations(ArrayList formations, ArrayList terrains);
        void deleteFormation(int formation);
        void deleteTerrain(int ter);
    }
}

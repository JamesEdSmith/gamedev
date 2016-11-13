using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections;

namespace PikeAndShot
{
    public class Terrain : ScreenObject, LevelEditorGrabbable
    {
        public const int STATE_SHOWN = 1;
        public const int STATE_ANIMATING = 2;

        public const int CLASS_TREE0 = 0;
        public const int CLASS_HORIROAD = 1;
        public const int CLASS_TURNROAD = 2;
        public const int CLASS_MILEROADMARKER = 3;
        public const int CLASS_TURNROADMARKER = 4;
        public const int CLASS_HORIROAD2 = 5;
        public const int CLASS_TREE1 = 6;
        public const int CLASS_TREE2 = 7;
        public const int CLASS_BUSH0 = 8;
        public const int CLASS_BUSH1 = 9;
        public const int CLASS_BUSH2 = 10;

        private Sprite _sprite;
        private float _restTime;
        private float _animationTime;
        public bool selected { get; set; }
        public int index { get; set; }
        public bool generated;

        public Terrain(BattleScreen screen, Texture2D sprite, int side, float x, float y, float restTime, float animationTime)
            : this(screen, sprite, side, x, y)
        {
            _sprite = new Sprite(sprite, new Rectangle(0, 0, 0, 0), 40, 40, true);
            _restTime = restTime;
            _animationTime = animationTime;
            
        }

        public Terrain(BattleScreen screen, Texture2D sprite, int side, float x, float y, float drawingY)
            : this(screen, sprite, side, x, y)
        {
            _drawingY = _position.Y + drawingY;
        }

        public Terrain(BattleScreen screen, Texture2D sprite, int side, float x, float y)
            : base(screen, side)
        {
            _position = new Vector2(x, y);
            _state = STATE_SHOWN;
            _sprite = new Sprite(sprite, new Rectangle(0, 0, 0, 0), sprite.Width, sprite.Height, false);
            _stateTimer = _restTime;
            _drawingY = _position.Y + sprite.Height;
            generated = false;
        }

        public static void getNewTerrain(int terrainClass, BattleScreen screen, float x, float y, int index)
        {
            Terrain newTerrain = null;
            switch (terrainClass)
            {
                case Terrain.CLASS_TREE0:
                    newTerrain = new Terrain(screen, PikeAndShotGame.TREE0, BattleScreen.SIDE_PLAYER, x, y);
                    screen.addTerrain(newTerrain);
                    break;
                case Terrain.CLASS_TREE1:
                    newTerrain = new Terrain(screen, PikeAndShotGame.TREE1, BattleScreen.SIDE_PLAYER, x, y);
                    screen.addTerrain(newTerrain);
                    break;
                case Terrain.CLASS_TREE2:
                    newTerrain = new Terrain(screen, PikeAndShotGame.TREE2, BattleScreen.SIDE_PLAYER, x, y);
                    screen.addTerrain(newTerrain);
                    break;
                case Terrain.CLASS_HORIROAD:
                    newTerrain = new Terrain(screen, PikeAndShotGame.ROAD_HORIZONTAL, BattleScreen.SIDE_PLAYER, x, y, 0);
                    screen.addTerrain(newTerrain);
                    break;
                case Terrain.CLASS_HORIROAD2:
                    newTerrain = new Terrain(screen, PikeAndShotGame.ROAD_HORIZONTAL_2, BattleScreen.SIDE_PLAYER, x, y, 0);
                    screen.addTerrain(newTerrain);
                    break;
                case Terrain.CLASS_TURNROAD:
                    newTerrain = new Terrain(screen, PikeAndShotGame.ROAD_TURN, BattleScreen.SIDE_PLAYER, x, y, 0);
                    screen.addTerrain(newTerrain);
                    break;
                case Terrain.CLASS_TURNROADMARKER:
                    newTerrain = new Terrain(screen, PikeAndShotGame.ROAD_TURN_MARKER, BattleScreen.SIDE_PLAYER, x, y);
                    screen.addTerrain(newTerrain);
                    break;
                case Terrain.CLASS_MILEROADMARKER:
                    newTerrain = new Terrain(screen, PikeAndShotGame.ROAD_MILE_MARKER, BattleScreen.SIDE_PLAYER, x, y);
                    screen.addTerrain(newTerrain);
                    break;
                case Terrain.CLASS_BUSH0:
                    newTerrain = new Terrain(screen, PikeAndShotGame.BUSH0, BattleScreen.SIDE_PLAYER, x, y);
                    screen.addTerrain(newTerrain);
                    break;
                case Terrain.CLASS_BUSH1:
                    newTerrain = new Terrain(screen, PikeAndShotGame.BUSH1, BattleScreen.SIDE_PLAYER, x, y);
                    screen.addTerrain(newTerrain);
                    break;
                case Terrain.CLASS_BUSH2:
                    newTerrain = new Terrain(screen, PikeAndShotGame.BUSH2, BattleScreen.SIDE_PLAYER, x, y);
                    screen.addTerrain(newTerrain);
                    break;

            }
            if (newTerrain != null)
                newTerrain.index = index;
        }

        public void setPosition(float x, float y)
        {
            _position = new Vector2(x, y);
        }

        public void setPosition(Vector2 pos)
        {
            _position = pos;
        }

        public override int getWidth()
        {
            return (int)_sprite.getSize().X;
        }

        public override int getHeight()
        {
            return (int)_sprite.getSize().Y;
        }

        public bool isDead()
        {
            return _state == STATE_DEAD;
        }

        public void update(TimeSpan timeSpan)
        {
            switch (_state)
            {
                case STATE_SHOWN:
                    _stateTimer -= (float)timeSpan.TotalMilliseconds;
                    if(_stateTimer <= 0)
                    {
                        _stateTimer = _animationTime;
                        _state = STATE_ANIMATING;
                    }
                    break;
                case STATE_ANIMATING:
                    int maxFrames = _sprite.getMaxFrames() * 2;
                    float frameTime = _animationTime / (float)maxFrames;
                    int frameNumber = maxFrames - (int)(_stateTimer / frameTime) - 1;

                    if (frameNumber >= _sprite.getMaxFrames())
                        frameNumber = _sprite.getMaxFrames() - (frameNumber - _sprite.getMaxFrames()) - 1;
                    _sprite.setFrame(frameNumber);
                    _stateTimer -= (float)timeSpan.TotalMilliseconds;

                    if (_stateTimer <= 0)
                    {
                        _stateTimer = _restTime;
                        _state = STATE_SHOWN;
                    }
                    break;
            }
        }

        public void draw(SpriteBatch spritebatch)
        {
            _screen.addDrawjob(new DrawJob(_sprite, _position - _screen.getMapOffset(), _side, _drawingY));

            if (_screen.getDrawDots())
            {
                spritebatch.Draw(PikeAndShotGame.getDotTexture(), _position - _screen.getMapOffset(), Color.White);
                spritebatch.Draw(PikeAndShotGame.getDotTexture(), new Vector2(_position.X - _screen.getMapOffset().X + _sprite.getSize().X, _position.Y - _screen.getMapOffset().Y + _sprite.getSize().Y), Color.White);
            }
        }

        internal bool isAnimated()
        {
            return _sprite.getMaxFrames() > 1;
        }
    }

}

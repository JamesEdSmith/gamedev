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
        public const int CLASS_WAGON = 11;
        public const int CLASS_OX_BROWN = 12;
        public const int CLASS_OX_GREY = 13;
        public const int CLASS_OX_DEAD = 14;
        public const int CLASS_WOUNDED_PEASANT = 15;
        public const int CLASS_DEAD_PEASANT = 16;
        public const int CLASS_100_CIRCLE = 17;
        public const int CLASS_TOTEMPOLE = 18;

        protected Sprite _sprite;
        private float _restTime;
        private float _animationTime;
        public bool selected { get; set; }
        public int index { get; set; }
        public bool generated;
        public Rectangle collisionBox;
        public bool collidable = false;
        public Vector2 collisionCenter;

        static Dictionary<Texture2D, List<int>> variantDict;

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

            _sprite.createFlashTexture(screen);
        }

        public Terrain(BattleScreen screen, Texture2D sprite, int side, float x, float y, Rectangle collisionBox)
            : this(screen, sprite, side, x, y)
        {
            this.collisionBox = collisionBox;
            collisionCenter = new Vector2(collisionBox.X + collisionBox.Width / 2, collisionBox.Y + collisionBox.Height / 2);
            collidable = true;
            _drawingY = _position.Y + sprite.Height;
        }

        public Terrain(BattleScreen screen, Texture2D sprite, int side, float x, float y, Rectangle collisionBox, Vector2 spriteDimensions)
            : this(screen, sprite, side, x, y, collisionBox)
        {
            _sprite = new Sprite(sprite, new Rectangle(0, 0, 0, 0), (int)spriteDimensions.X, (int)spriteDimensions.Y, false);

            if (variantDict == null)
            {
                variantDict = new Dictionary<Texture2D, List<int>>();
            }

            if (!variantDict.ContainsKey(sprite))
            {
                List<int> variantList = new List<int>();
                for(int i = 0; i < _sprite.getMaxFrames(); i++)
                {
                    variantList.Add(i);
                }
                variantDict.Add(sprite, variantList);
            }

            if (variantDict[sprite].Count < 1)
            {
                for (int i = 0; i < _sprite.getMaxFrames(); i++)
                {
                    variantDict[sprite].Add(i);
                }
            }

            int variant = PikeAndShotGame.random.Next(variantDict[sprite].Count);
            _sprite.setFrame(variantDict[sprite][variant]);
            variantDict[sprite].RemoveAt(variant);

            _drawingY = _position.Y + spriteDimensions.Y;
            _sprite.createFlashTexture(screen);
        }

        public Terrain(BattleScreen screen, Texture2D sprite, int side, float x, float y, Rectangle collisionBox, Vector2 spriteDimensions, float restTime, float animationTime)
            : this(screen, sprite, side, x, y, collisionBox, spriteDimensions)
        {
            _restTime = restTime;
            _animationTime = animationTime;
        }

        public static void getNewTerrain(int terrainClass, BattleScreen screen, float x, float y, int index)
        {
            Terrain newTerrain = null;
            switch (terrainClass)
            {
                case Terrain.CLASS_TREE0:
                    newTerrain = new Terrain(screen, PikeAndShotGame.TREE0, BattleScreen.SIDE_PLAYER, x, y, new Rectangle((int)(x + PikeAndShotGame.TREE0.Width / 4), (int)(y + PikeAndShotGame.TREE0.Height *2/3), (int)(PikeAndShotGame.TREE0.Width / 2), (int)(PikeAndShotGame.TREE0.Height/8)));
                    screen.addTerrain(newTerrain);
                    break;
                case Terrain.CLASS_TREE1:
                    newTerrain = new Terrain(screen, PikeAndShotGame.TREE1, BattleScreen.SIDE_PLAYER, x, y, new Rectangle((int)x + 20, (int)y + PikeAndShotGame.TREE1.Height / 2, PikeAndShotGame.TREE1.Width - 40, PikeAndShotGame.TREE1.Height/3));
                    screen.addTerrain(newTerrain);
                    break;
                case Terrain.CLASS_TREE2:
                    newTerrain = new Terrain(screen, PikeAndShotGame.TREE2, BattleScreen.SIDE_PLAYER, x, y, new Rectangle((int)x + 20, (int)y + PikeAndShotGame.TREE2.Height / 2, PikeAndShotGame.TREE2.Width - 40, PikeAndShotGame.TREE2.Height / 3));
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
                    newTerrain = new Terrain(screen, PikeAndShotGame.ROAD_TURN_MARKER, BattleScreen.SIDE_PLAYER, x, y, new Rectangle((int)(x + PikeAndShotGame.ROAD_TURN_MARKER.Width / 4), (int)(y + PikeAndShotGame.ROAD_TURN_MARKER.Height / 2), (int)(PikeAndShotGame.ROAD_TURN_MARKER.Width / 2), (int)(PikeAndShotGame.ROAD_TURN_MARKER.Height/8)));
                    screen.addTerrain(newTerrain);
                    break;
                case Terrain.CLASS_MILEROADMARKER:
                    newTerrain = new Terrain(screen, PikeAndShotGame.ROAD_MILE_MARKER, BattleScreen.SIDE_PLAYER, x, y, new Rectangle((int)(x + PikeAndShotGame.ROAD_MILE_MARKER.Width / 4), (int)(y + PikeAndShotGame.ROAD_MILE_MARKER.Height /2), (int)(PikeAndShotGame.ROAD_MILE_MARKER.Width / 2), (int)(PikeAndShotGame.ROAD_MILE_MARKER.Height / 8)));
                    screen.addTerrain(newTerrain);
                    break;
                case Terrain.CLASS_BUSH0:
                    newTerrain = new Terrain(screen, PikeAndShotGame.BUSH0, BattleScreen.SIDE_PLAYER, x, y, new Rectangle((int)x + 8, (int)y + PikeAndShotGame.BUSH0.Height / 6, (int)((float)PikeAndShotGame.BUSH0.Width * 0.6f), PikeAndShotGame.BUSH0.Height* 2 / 3));
                    screen.addTerrain(newTerrain);
                    break;
                case Terrain.CLASS_BUSH1:
                    newTerrain = new Terrain(screen, PikeAndShotGame.BUSH1, BattleScreen.SIDE_PLAYER, x, y, new Rectangle((int)x + 8, (int)y + PikeAndShotGame.BUSH1.Height / 6, (int)((float)PikeAndShotGame.BUSH1.Width * 0.6f), PikeAndShotGame.BUSH1.Height * 2 / 3));
                    screen.addTerrain(newTerrain);
                    break;
                case Terrain.CLASS_BUSH2:
                    newTerrain = new Terrain(screen, PikeAndShotGame.BUSH2, BattleScreen.SIDE_PLAYER, x, y, new Rectangle((int)x + 8, (int)y + PikeAndShotGame.BUSH2.Height / 6, (int)((float)PikeAndShotGame.BUSH2.Width * 0.6f), PikeAndShotGame.BUSH2.Height * 2 / 3));
                    screen.addTerrain(newTerrain);
                    break;
                case Terrain.CLASS_WAGON:
                    newTerrain = new Terrain(screen, PikeAndShotGame.WAGON, BattleScreen.SIDE_PLAYER, x, y, new Rectangle((int)x + 12, (int)y + 4, 60, 25), new Vector2(84, 42));
                    screen.addTerrain(newTerrain);
                    break;
                case Terrain.CLASS_OX_BROWN:
                    newTerrain = new Terrain(screen, PikeAndShotGame.OX_BROWN, BattleScreen.SIDE_PLAYER, x, y, new Rectangle((int)x + 12, (int)y + 6, 70, 20), new Vector2(90, 40), 6000, 3000); 
                    screen.addTerrain(newTerrain);
                    break;
                case Terrain.CLASS_OX_GREY:
                    newTerrain = new Terrain(screen, PikeAndShotGame.OX_GREY, BattleScreen.SIDE_PLAYER, x, y, new Rectangle((int)x + 12, (int)y + 6, 70, 20), new Vector2(90, 40), 6000, 3000);
                    screen.addTerrain(newTerrain);
                    break;
                case Terrain.CLASS_OX_DEAD:
                    newTerrain = new Terrain(screen, PikeAndShotGame.OX_DEAD, BattleScreen.SIDE_PLAYER, x, y, new Rectangle((int)x + 14, (int)y + 22, 32, 10), new Vector2(64, 48));
                    screen.addTerrain(newTerrain);
                    break;
                case Terrain.CLASS_WOUNDED_PEASANT:
                    newTerrain = new Terrain(screen, PikeAndShotGame.WOUNDED_PEASANT, BattleScreen.SIDE_PLAYER, x, y, new Rectangle((int)x + 10, (int)y + 6, 14, 28), new Vector2(32, 46));
                    screen.addTerrain(newTerrain);
                    break;
                case Terrain.CLASS_DEAD_PEASANT:
                    newTerrain = new Terrain(screen, PikeAndShotGame.DEAD_PEASANT, BattleScreen.SIDE_PLAYER, x, y, new Rectangle((int)x + 18, (int)y + 10, 30, 6), new Vector2(56, 40));
                    screen.addTerrain(newTerrain);
                    break;
                case Terrain.CLASS_100_CIRCLE:
                    newTerrain = new CollisionCircle(screen, PikeAndShotGame.getDotTexture(), BattleScreen.SIDE_PLAYER, x, y, 50);
                    screen.addTerrain(newTerrain);
                    break;
                case Terrain.CLASS_TOTEMPOLE:
                    newTerrain = new Terrain(screen, PikeAndShotGame.TOTEMPOLE, BattleScreen.SIDE_PLAYER, x, y, new Rectangle((int)x + 12, (int)y + 84, 14, 34), new Vector2(40, 122));
                    screen.addTerrain(newTerrain);
                    break;
            }

            if (newTerrain != null)
                newTerrain.index = index;

            if (!newTerrain.collidable)
                screen.cancelScreenObject(newTerrain);
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

        virtual public void draw(SpriteBatch spritebatch)
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
            return _animationTime > 0f;
        }

        public Sprite getSprite()
        {
            return _sprite;
        }
    }

    class CollisionCircle : Terrain
    {
        public float radius;

        public CollisionCircle(BattleScreen screen, Texture2D sprite, int side, float x, float y, float radius): base(screen, sprite, side, x, y)
        {
            this.radius = radius;
            collidable = true;
        }

        public override void draw(SpriteBatch spritebatch)
        {
            if (_screen.getDrawDots())
            {
                /*if(selected)
                {
                    spritebatch.Draw(PikeAndShotGame.getDotTexture(), new Rectangle((int)(_position.X - _screen.getMapOffset().X), (int)(_position.Y + radius - _screen.getMapOffset().Y), (int)(radius * 2), PikeAndShotGame.getDotTexture().Height), Color.Fuchsia);
                    spritebatch.Draw(PikeAndShotGame.getDotTexture(), new Rectangle((int)(_position.X + radius - _screen.getMapOffset().X), (int)(_position.Y - _screen.getMapOffset().Y), PikeAndShotGame.getDotTexture().Width, (int)(radius * 2)), Color.Fuchsia);
                }
                else
                {*/
                    spritebatch.Draw(PikeAndShotGame.getDotTexture(), new Rectangle((int)(_position.X - _screen.getMapOffset().X), (int)(_position.Y + radius - _screen.getMapOffset().Y), (int)(radius * 2), PikeAndShotGame.getDotTexture().Height), Color.White);
                    spritebatch.Draw(PikeAndShotGame.getDotTexture(), new Rectangle((int)(_position.X + radius - _screen.getMapOffset().X), (int)(_position.Y - _screen.getMapOffset().Y), PikeAndShotGame.getDotTexture().Width, (int)(radius * 2)), Color.White);
                //}
            }
        }
        
        public override int getWidth()
        {
            return (int)radius * 2;
        }

        public override int getHeight()
        {
            return (int)radius * 2;
        }

        public override Vector2 getCenter()
        {
            return new Vector2(_position.X + radius, _position.Y + radius);
        }
    }

}

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections;

namespace MoleHillMountain
{
    class Stone
    {
        private Vector2 position;
        private Vector2 drawPosition;
        private int vertFacing;
        private int horzFacing;
        private DungeonScreen dungeonScreen;
        int launchOffset = 10;

        Vector2 velocity;
        float speed = 100f;

        Sprite stone;

        public bool dead;

        public Stone(Vector2 position, int vert, int horz, DungeonScreen dungeonScreen)
        {
            dead = false;
            vertFacing = vert;
            horzFacing = horz;
            this.dungeonScreen = dungeonScreen;
            stone = new Sprite(PikeAndShotGame.STONE, new Rectangle(0, 0, 20, 20), 20, 20);
            if (vert == Sprite.DIRECTION_NONE)
            {
                if (horz == Sprite.DIRECTION_LEFT)
                {
                    velocity = new Vector2(-speed, speed);
                    this.position = new Vector2(position.X - launchOffset, position.Y);
                    drawPosition = new Vector2(position.X, position.Y);
                }
                else
                {
                    velocity = new Vector2(speed, speed);
                    this.position = new Vector2(position.X + launchOffset, position.Y);
                    drawPosition = new Vector2(position.X, position.Y);
                }
            }
            else if (vert == Sprite.DIRECTION_UP)
            {
                if (horz == Sprite.DIRECTION_LEFT)
                {
                    velocity = new Vector2(-speed, -speed);
                    this.position = new Vector2(position.X, position.Y - launchOffset);
                    drawPosition = new Vector2(position.X, position.Y);
                }
                else
                {
                    velocity = new Vector2(speed, -speed);
                    this.position = new Vector2(position.X, position.Y - launchOffset);
                    drawPosition = new Vector2(position.X, position.Y);
                }
            }
            else
            {
                if (horz == Sprite.DIRECTION_LEFT)
                {
                    velocity = new Vector2(-speed, speed);
                    this.position = new Vector2(position.X, position.Y + launchOffset);
                    drawPosition = new Vector2(position.X, position.Y);
                }
                else
                {
                    velocity = new Vector2(speed, speed);
                    this.position = new Vector2(position.X, position.Y + launchOffset);
                    drawPosition = new Vector2(position.X, position.Y);
                }
            }
        }

        public void draw(SpriteBatch spritebatch)
        {
            stone.draw(spritebatch, position + DungeonScreen.OFFSET, horzFacing, vertFacing);
        }

        public void update(TimeSpan timeSpan)
        {
            position += velocity * (float)timeSpan.TotalSeconds;
            drawPosition.X = (int)position.X;
            drawPosition.Y = (int)position.Y;

            Tunnel newTunnel = dungeonScreen.getCurrTunnel(position);
            if (newTunnel != null)
            {
                if (newTunnel.right == Tunnel.NOT_DUG && newTunnel.left == Tunnel.NOT_DUG && newTunnel.top == Tunnel.NOT_DUG && newTunnel.bottom == Tunnel.NOT_DUG)
                {
                    velocity.X *= -1;
                    velocity.Y *= -1;
                }
                else
                {

                    if (velocity.X > 0 && newTunnel.right == Tunnel.NOT_DUG && position.X % DungeonScreen.GRID_SIZE >= 17)
                    {
                        velocity.X *= -1;
                    }
                    else if (velocity.X < 0 && newTunnel.left == Tunnel.NOT_DUG && position.X % DungeonScreen.GRID_SIZE <= 3)
                    {
                        velocity.X *= -1;
                    }

                    if (velocity.Y > 0 && newTunnel.bottom == Tunnel.NOT_DUG && position.Y % DungeonScreen.GRID_SIZE >= 17)
                    {
                        velocity.Y *= -1;
                    }
                    else if (velocity.Y < 0 && newTunnel.top == Tunnel.NOT_DUG && position.Y % DungeonScreen.GRID_SIZE <= 3)
                    {
                        velocity.Y *= -1;
                    }
                }

                if (velocity.X > 0)
                {
                    horzFacing = Sprite.DIRECTION_RIGHT;
                }
                else
                {
                    horzFacing = Sprite.DIRECTION_LEFT;
                }

                if (newTunnel.right == Tunnel.NOT_DUG && position.X % DungeonScreen.GRID_SIZE >= 15)
                {
                    stone.setFrame(3);
                }
                else if (newTunnel.left == Tunnel.NOT_DUG && position.X % DungeonScreen.GRID_SIZE <= 5)
                {
                    stone.setFrame(3);
                }

                else if (newTunnel.bottom == Tunnel.NOT_DUG && position.Y % DungeonScreen.GRID_SIZE >= 15)
                {
                    stone.setFrame(1);
                }
                else if (newTunnel.top == Tunnel.NOT_DUG && position.Y % DungeonScreen.GRID_SIZE <= 5)
                {
                    stone.setFrame(1);
                }
                else
                {
                    if (velocity.Y > 0)
                    {
                        stone.setFrame(0);
                    }
                    else
                    {
                        stone.setFrame(2);
                    }
                }

                Mole enemy = dungeonScreen.checkEnemyCollision(position.X, position.Y, 1);
                if (enemy != null)
                {
                    enemy.hit(position);
                    //hack cause of the sprite
                    position.X += 10;
                    position.Y += 10;
                    dungeonScreen.createAnimation(position, 0, 0, AnimationType.stoneImpact);
                    dead = true;
                }
            }
            else
            {
                dungeonScreen.createAnimation(position, 0, 0, AnimationType.stoneImpact);
                dead = true;
            }
        }
    }
}

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections;

namespace MoleHillMountain
{
    public class Projectile
    {
        public Vector2 position;
        public Vector2 drawPosition;
        public int vertFacing;
        protected int horzFacing;
        public DungeonScreen dungeonScreen;

        protected Vector2 velocity;
        protected float speed = 100f;

        protected Sprite stone;

        public bool dead;
        public float launchOffset;
        public Projectile(Vector2 position, int vert, int horz, DungeonScreen dungeonScreen)
        {
            dead = false;
            vertFacing = vert;
            horzFacing = horz;
            this.dungeonScreen = dungeonScreen;
            this.position = new Vector2(position.X, position.Y);
            drawPosition = new Vector2(position.X, position.Y);

        }

        public virtual void draw(SpriteBatch spritebatch)
        {
            stone.draw(spritebatch, position + DungeonScreen.OFFSET, horzFacing, vertFacing);
        }
        public virtual void update(TimeSpan timeSpan) { }
    }

    class Hook : Projectile
    {

        int tail;
        bool crashed;
        float crashTime = 100f;
        float crashTimer = 100f;
        static Vector2 adjustmentX = new Vector2(DungeonScreen.GRID_SIZE, 0);
        static Vector2 adjustmentY = new Vector2(0, DungeonScreen.GRID_SIZE);

        public Hook(Vector2 position, int vert, int horz, DungeonScreen dungeonScreen) : base(position, vert, horz, dungeonScreen)
        {
            stone = new Sprite(PikeAndShotGame.HOOK, new Rectangle(0, 0, 20, 20), 20, 20);
            launchOffset = 10;
            speed = 300f;

            if (vert == Sprite.DIRECTION_NONE)
            {
                if (horz == Sprite.DIRECTION_LEFT)
                {
                    this.position = new Vector2(position.X - launchOffset, position.Y);
                    drawPosition = new Vector2(position.X, position.Y);
                }
                else
                {
                    this.position = new Vector2(position.X + launchOffset, position.Y);
                    drawPosition = new Vector2(position.X, position.Y);
                }
            }
            else if (vert == Sprite.DIRECTION_UP)
            {
                if (horz == Sprite.DIRECTION_LEFT)
                {
                    this.position = new Vector2(position.X, position.Y - launchOffset);
                    drawPosition = new Vector2(position.X, position.Y);
                }
                else
                {
                    this.position = new Vector2(position.X, position.Y - launchOffset);
                    drawPosition = new Vector2(position.X, position.Y);
                }
            }
            else
            {
                if (horz == Sprite.DIRECTION_LEFT)
                {
                    this.position = new Vector2(position.X, position.Y + launchOffset);
                    drawPosition = new Vector2(position.X, position.Y);
                }
                else
                {
                    this.position = new Vector2(position.X, position.Y + launchOffset);
                    drawPosition = new Vector2(position.X, position.Y);
                }
            }

        }

        public override void draw(SpriteBatch spritebatch)
        {
            stone.setFrame(0);
            stone.draw(spritebatch, position + DungeonScreen.OFFSET, horzFacing, vertFacing);
            for (int i = 1; i < tail; i++)
            {
                if (crashed && crashTimer > 0 /*&& crashTimer < crashTime /i*/)
                    stone.setFrame((i % 2) + 2);
                else
                    stone.setFrame(1);

                if (vertFacing == Sprite.DIRECTION_NONE)
                {
                    if (horzFacing == Sprite.DIRECTION_LEFT)
                    {
                        stone.draw(spritebatch, position + (adjustmentX * i) + DungeonScreen.OFFSET, horzFacing, vertFacing);
                    }
                    else
                    {
                        stone.draw(spritebatch, position - (adjustmentX * i) + DungeonScreen.OFFSET, horzFacing, vertFacing);
                    }
                }
                else if (vertFacing == Sprite.DIRECTION_UP)
                {

                    stone.draw(spritebatch, position + (adjustmentY * i) + DungeonScreen.OFFSET, horzFacing, vertFacing);

                }
                else
                {
                    stone.draw(spritebatch, position - (adjustmentY * i) + DungeonScreen.OFFSET, horzFacing, vertFacing);
                }
            }
        }

        Tunnel currTunnel;

        public override void update(TimeSpan timeSpan)
        {
            currTunnel = dungeonScreen.getCurrTunnel(position);

            if (currTunnel == null || crashed)
            {
                if (crashTimer <= 0)
                {
                    if (vertFacing == Sprite.DIRECTION_NONE)
                    {
                        if (horzFacing == Sprite.DIRECTION_LEFT)
                        {
                            dungeonScreen.mole.position.X -= (float)timeSpan.TotalSeconds * speed;
                            if (dungeonScreen.mole.position.X <= position.X + DungeonScreen.GRID_SIZE/1.5f)
                                dead = true;
                        }
                        else
                        {
                            dungeonScreen.mole.position.X += (float)timeSpan.TotalSeconds * speed;
                            if (dungeonScreen.mole.position.X >= position.X - DungeonScreen.GRID_SIZE / 1.5f)
                                dead = true;
                        }
                    }
                    else if (vertFacing == Sprite.DIRECTION_UP)
                    {
                        dungeonScreen.mole.position.Y -= (float)timeSpan.TotalSeconds * speed;
                        if (dungeonScreen.mole.position.Y <= position.Y + DungeonScreen.GRID_SIZE / 1.5f)
                            dead = true;
                    }
                    else
                    {
                        dungeonScreen.mole.position.Y += (float)timeSpan.TotalSeconds * speed;
                        if (dungeonScreen.mole.position.Y >= position.Y - DungeonScreen.GRID_SIZE / 1.5f)
                            dead = true;
                    }
                }
                else
                {
                    crashed = true;
                    crashTimer -= (float)timeSpan.TotalMilliseconds;
                }
            }
            else
            {
                if (vertFacing == Sprite.DIRECTION_NONE)
                {
                    if (horzFacing == Sprite.DIRECTION_LEFT)
                    {
                        position.X = position.X - speed * (float)timeSpan.TotalSeconds;
                        if (currTunnel.right == Tunnel.NOT_DUG)
                        {
                            position.X = (currTunnel.position + DungeonScreen.OFFSET).X + DungeonScreen.GRID_SIZE / 2;
                            crash();
                            
                        }
                    }
                    else
                    {
                        this.position = new Vector2(position.X + speed * (float)timeSpan.TotalSeconds, position.Y);
                        if (currTunnel.left == Tunnel.NOT_DUG)
                        {
                            position.X = (currTunnel.position + DungeonScreen.OFFSET).X - DungeonScreen.GRID_SIZE / 2;
                            crash();
                        }
                    }
                }
                else if (vertFacing == Sprite.DIRECTION_UP)
                {
                    this.position = new Vector2(position.X, position.Y - speed * (float)timeSpan.TotalSeconds);
                    if (currTunnel.bottom == Tunnel.NOT_DUG)
                    {
                        position.Y = (currTunnel.position + DungeonScreen.OFFSET).Y + DungeonScreen.GRID_SIZE ;
                        crash();
                    }
                }
                else
                {
                    this.position = new Vector2(position.X, position.Y + speed * (float)timeSpan.TotalSeconds);
                    if (currTunnel.top == Tunnel.NOT_DUG)
                    {
                        position.Y = (currTunnel.position + DungeonScreen.OFFSET).Y  ;
                        crash();
                    }
                }
            }
            drawPosition.X = position.X;
            drawPosition.Y = position.Y;
            tail = (int)(Math.Abs((float)(dungeonScreen.getMolePosition() - position).Length()) / stone.getBoundingRect().Width) + 1;
        }

        private void crash()
        {
            
            crashed = true;

            dungeonScreen.createAnimation(position + adjustmentX/1.6f + adjustmentY/5f, 0, 0, AnimationType.hookImpact);
        }
    }


    class Stone : Projectile
    {
        public Stone(Vector2 position, int vert, int horz, DungeonScreen dungeonScreen) : base(position, vert, horz, dungeonScreen)
        {

            stone = new Sprite(PikeAndShotGame.STONE, new Rectangle(0, 0, 20, 20), 20, 20);
            launchOffset = 10f;
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

        public override void update(TimeSpan timeSpan)
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

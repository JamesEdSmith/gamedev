using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections;
using System.Collections.Generic;

namespace MoleHillMountain
{
    public class Water
    {
        public const int NONE = 0;
        public const int SHAKING = 1;
        public const int FALLING = 2;
        public const int DEAD = 4;
        public const int MOVING_LEFT = 5;
        public const int MOVING_RIGHT = 6;

        private DungeonScreen dungeonScreen;
        private Sprite waterIdle;
        private Sprite waterFull;
        private Sprite currSprite;
        private Sprite waterBack;

        private float animationTime;
        private float animationTimer;
        private int fallingFrom;
        public Vector2 position;
        public int state;
        float waveMidTime = 650f;
        public int seen;
        public static Vector2 center = new Vector2(DungeonScreen.GRID_SIZE / 2, DungeonScreen.GRID_SIZE / 2);
        private float fallTime = 650f;
        private float shakeTime = 350f;
        private Sprite moving;
        private Sprite falling;
        private float fallSpeed = 78f;
        private float moveSpeed = 68f;
        private bool listingLeft;
        private bool listingRight;
        private static Vector2 leftSide = new Vector2(-DungeonScreen.GRID_SIZE / 2, 0);
        private static Vector2 leftRight = new Vector2(DungeonScreen.GRID_SIZE / 2, 0);
        public Tunnel tunnel;
        Tunnel leftTunnel;
        Tunnel rightTunnel;

        public Water waterBelow = null;
        public Water waterLeft = null;
        public Water waterRight = null;
        public Water waterAbove = null;

        bool groupedLeft;
        bool groupedRight;

        List<Water> group;
        Water leader;

        int crash;

        public Water(float x, float y, DungeonScreen dungeonScreen)
        {
            this.dungeonScreen = dungeonScreen;

            waterIdle = new Sprite(PikeAndShotGame.WATER, new Rectangle(0, 0, 20, 20), 20, 20);
            waterFull = new Sprite(PikeAndShotGame.WATER_FULL, new Rectangle(0, 0, 20, 20), 20, 20);
            falling = new Sprite(PikeAndShotGame.WATER_FALL, new Rectangle(0, 0, 20, 20), 20, 20);
            moving = new Sprite(PikeAndShotGame.WATER_WAVE, new Rectangle(0, 0, 20, 20), 20, 20);
            waterBack = new Sprite(PikeAndShotGame.WATER_BACK, new Rectangle(0, 0, 20, 20), 20, 20);

            currSprite = waterIdle;
            animationTime = waveMidTime;
            position = new Vector2(x, y);

            state = NONE;

            seen = SeenStatus.NOT_SEEN;
            group = new List<Water>(4);
        }
        public void draw(SpriteBatch spritebatch)
        {
            if (state == MOVING_RIGHT)
            {
                currSprite.draw(spritebatch, position + DungeonScreen.OFFSET, Sprite.DIRECTION_RIGHT, Sprite.DIRECTION_NONE);
            }
            else
            {
                currSprite.draw(spritebatch, position + DungeonScreen.OFFSET, 0f);
            }
        }
        public void update(GameTime gameTime)
        {
            if (position.X < 0)
            {
                position.X = 0;
            }
            if (position.X / DungeonScreen.GRID_SIZE >= DungeonScreen.GRID_WIDTH)
            {
                position.X = DungeonScreen.GRID_SIZE * (DungeonScreen.GRID_WIDTH - 1) + 19;
            }

            tunnel = dungeonScreen.getCurrTunnel(position);
            if (tunnel != null)
            {
                seen = dungeonScreen.checkMoleSight(tunnel);
                if (seen == SeenStatus.SEEN)
                {
                    tunnel.water = true;
                }

            }

            if (state == MOVING_LEFT || state == MOVING_RIGHT)
            {
                leftTunnel = dungeonScreen.getCurrTunnel(position + leftSide);
                rightTunnel = dungeonScreen.getCurrTunnel(position + leftRight);

                if (leftTunnel != null)
                {
                    if (dungeonScreen.checkMoleSight(leftTunnel) == SeenStatus.SEEN)
                    {
                        leftTunnel.water = true;
                    }
                }
                if (rightTunnel != null)
                {
                    if (dungeonScreen.checkMoleSight(rightTunnel) == SeenStatus.SEEN)
                    {
                        rightTunnel.water = true;
                    }
                }
            }
            else if (state == FALLING)
            {
                Tunnel topTunnel = dungeonScreen.getTunnelAbove(position);
                Tunnel bottomTunnel = dungeonScreen.getTunnelBelow(position);

                if (topTunnel != null)
                {
                    if (dungeonScreen.checkMoleSight(topTunnel) == SeenStatus.SEEN)
                    {
                        topTunnel.water = true;
                    }
                }
                if (bottomTunnel != null)
                {
                    if (dungeonScreen.checkMoleSight(bottomTunnel) == SeenStatus.SEEN)
                    {
                        bottomTunnel.water = true;
                    }
                }
            }

            Tunnel tunnelBelow = dungeonScreen.getTunnelBelow(position);
            Tunnel tunnelLeft = dungeonScreen.getTunnelLeft(position);
            Tunnel tunnelRight = dungeonScreen.getTunnelRight(position);
            Tunnel tunnelAbove = dungeonScreen.getTunnelAbove(position);
            waterBelow = null;
            waterLeft = null;
            waterRight = null;
            waterAbove = null;

            foreach (Water water in dungeonScreen.waters)
            {
                if (water.tunnel == tunnelBelow && tunnelBelow != null && tunnelBelow.top == Tunnel.DUG)
                    waterBelow = water;
                if (water.position.X < position.X && position.X - water.position.X <= DungeonScreen.GRID_SIZE
                    && Math.Abs(water.position.Y - position.Y) < 5)
                    waterLeft = water;
                if (water.position.X > position.X && water.position.X - position.X <= DungeonScreen.GRID_SIZE
                    && Math.Abs(water.position.Y - position.Y) < 5)
                    waterRight = water;
                if (water.tunnel == tunnelAbove && tunnelAbove != null && tunnelAbove.bottom == Tunnel.DUG)
                    waterAbove = water;
            }

            bool leftOfCenter = position.X < tunnel.position.X + Tunnel.center.X;
            bool rightOfCenter = position.X > tunnel.position.X + Tunnel.center.X;


            if (tunnelBelow != null && tunnelBelow.top != Tunnel.NOT_DUG && (waterBelow == null || waterBelow.state != NONE) && Math.Abs(position.X - (tunnel.position.X + Tunnel.center.X)) < 5)
            {
                fall();
            }
            else if (state == FALLING)
            {
                if (tunnelBelow == null || tunnelBelow.top == Tunnel.NOT_DUG || waterBelow != null)
                {
                    land();
                }
            }
            else if (state == NONE)
            {
                if ((tunnelLeft != null && tunnelLeft.right != Tunnel.NOT_DUG && (waterLeft == null || waterLeft.state == MOVING_LEFT || waterLeft.state == FALLING)) || rightOfCenter)
                {
                    moveLeft();
                }
                else if ((tunnelRight != null && tunnelRight.left != Tunnel.NOT_DUG && (waterRight == null || waterRight.state == MOVING_RIGHT || waterRight.state == FALLING)) || leftOfCenter)
                {
                    moveRight();
                }
                else
                {
                    land();
                }
            }
            else if (state == MOVING_LEFT)
            {
                if ((tunnelLeft != null && tunnelLeft.right != Tunnel.NOT_DUG && (waterLeft == null || waterLeft.state == MOVING_LEFT || waterLeft.state == FALLING)) || rightOfCenter)
                {
                    moveLeft();
                }
                else if ((tunnelRight != null && tunnelRight.left != Tunnel.NOT_DUG) || leftOfCenter)
                {
                    land();
                    if (waterLeft == null || waterLeft.state == MOVING_RIGHT)
                    {
                        crash++;
                        if (crash > 2)
                            state = DEAD;
                    }
                }
                else
                {
                    land();
                }
            }
            else
            {
                if ((tunnelRight != null && tunnelRight.left != Tunnel.NOT_DUG && (waterRight == null || waterRight.state == MOVING_RIGHT || waterRight.state == FALLING)) || leftOfCenter)
                {
                    moveRight();
                }
                else if ((tunnelLeft != null && tunnelLeft.right != Tunnel.NOT_DUG) || rightOfCenter)
                {
                    land();
                    if (waterRight == null || waterRight.state == MOVING_LEFT)
                    {
                        crash++;
                        if (crash > 2)
                            state = DEAD;
                    }
                }
                else
                {
                    land();
                }
            }

            move(gameTime);
            animate(gameTime);

        }

        private void move(GameTime gameTime)
        {
            if (state == FALLING)
            {
                position.Y += fallSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }
            else if (state == MOVING_LEFT && !groupedLeft)
            {
                position.X -= moveSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                int i = 0;
                foreach (Water water in group)
                {
                    i++;
                    water.position.X = position.X + DungeonScreen.GRID_SIZE;
                }
            }
            else if (state == MOVING_RIGHT && !groupedRight)
            {
                position.X += moveSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                int i = 0;
                foreach (Water water in group)
                {
                    i++;
                    water.position.X = position.X - DungeonScreen.GRID_SIZE;
                }
            }
        }

        private void moveLeft()
        {
            if (state != MOVING_LEFT)
            {
                if (group.Count > 0)
                {
                    group[0].giveGroup(group, state);
                    group.Clear();
                }

                state = MOVING_LEFT;
                animationTime = waveMidTime;
                animationTimer = animationTime;
            }

        }

        private void giveGroup(List<Water> group, int state)
        {
            if (group.Contains(this))
            {
                group.Remove(this);
                leader = null;

                this.group = new List<Water>(group);
                foreach (Water water in this.group)
                {
                    water.leader = this;
                }
                groupedLeft = false;
                groupedRight = false;
            }
        }

        private void moveRight()
        {
            if (state != MOVING_RIGHT)
            {
                if (group.Count > 0)
                {
                    group[0].giveGroup(group, state);
                    group.Clear();
                }

                state = MOVING_RIGHT;
                animationTime = waveMidTime;
                animationTimer = animationTime;
            }

        }

        private void fall()
        {
            if (state != FALLING)
            {
                if (group.Count > 0)
                {
                    group[0].giveGroup(group, state);
                    group.Clear();
                }
                groupedLeft = false;
                groupedRight = false;
                leader = null;
                state = FALLING;
                currSprite = falling;
                animationTime = fallTime;
                animationTimer = animationTime;
                fallingFrom = (int)position.Y;
                position.X = tunnel.position.X + Tunnel.center.X;
                crash = 0;
            }
        }


        private void animate(GameTime gameTime)
        {
            Water waterL;
            Water waterR;
            switch (state)
            {
                case NONE:
                    if (waterAbove != null || (waterRight != null && waterRight.isFull(MOVING_RIGHT, new List<Water> { this })) || (waterLeft != null && waterLeft.isFull(MOVING_LEFT, new List<Water> { this })))
                    {
                        currSprite = waterFull;
                    }
                    else
                        currSprite = waterIdle;
                    break;
                case FALLING:
                    currSprite = falling;
                    break;
                case MOVING_LEFT:
                    waterL = dungeonScreen.waterLeft(position, 5);
                    waterR = dungeonScreen.waterRight(position, 5);
                    if (waterL == null || waterL.state == FALLING)
                    {
                        currSprite = moving;
                        if (waterR != null && !group.Contains(waterR) && waterR.state != MOVING_RIGHT && waterR.state != FALLING)
                        {
                            addToGroup(waterR);
                        }
                    }
                    else if (waterR == null || waterR.state == FALLING)
                    {
                        currSprite = waterBack;
                        if (!groupedLeft && waterL.groupedLeft)
                        {
                            waterL.leader.addToGroup(this);
                        }
                        else if (!groupedLeft && waterL.group.Count > 0)
                        {
                            waterL.addToGroup(this);
                        }
                    }
                    else
                    {
                        currSprite = waterIdle;
                        if (!groupedLeft && waterL.groupedLeft)
                        {
                            waterL.leader.addToGroup(this);
                        }
                        else if (!groupedLeft && waterL.group.Count > 0)
                        {
                            waterL.addToGroup(this);
                        }
                    }

                    if (waterAbove != null)
                        currSprite = waterFull;
                    break;
                case MOVING_RIGHT:
                    waterL = dungeonScreen.waterLeft(position, 5);
                    waterR = dungeonScreen.waterRight(position, 5);
                    if (waterR == null || waterR.state == FALLING)
                    {
                        currSprite = moving;

                        if (waterL != null && !group.Contains(waterL) && waterL.state != MOVING_LEFT && waterL.state != FALLING)
                        {
                            addToGroup(waterL);
                        }
                    }
                    else if (waterL == null || waterL.state == FALLING)
                    {
                        currSprite = waterBack;
                        if (!groupedRight && waterR.groupedRight)
                        {
                            waterR.leader.addToGroup(this);
                        }
                        else if (!groupedRight && waterR.group.Count > 0)
                        {
                            waterR.addToGroup(this);
                        }
                    }
                    else
                    {
                        currSprite = waterIdle;
                        if (!groupedRight && waterR.groupedRight)
                        {
                            waterR.leader.addToGroup(this);
                        }
                        else if (!groupedRight && waterR.group.Count > 0)
                        {
                            waterR.addToGroup(this);
                        }
                    }

                    if (waterAbove != null)
                        currSprite = waterFull;
                    break;

            }

            animationTimer -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            if (animationTimer < 0)
            {
                animationTimer = animationTime;
            }

            int maxFrames = currSprite.getMaxFrames();
            float frameTime = animationTime / (float)maxFrames;
            int frameNumber = maxFrames - (int)(animationTimer / frameTime) - 1;
            currSprite.setFrame(frameNumber);
        }

        private void addToGroup(Water water)
        {
            if (leader == water)
            {
                Console.WriteLine("No");

            }
            group.Add(water);
            water.leader = this;
            water.animationTimer = animationTimer;

            group.Sort((w1, w2) => w1.position.X.CompareTo(w2.position.X));
            water.groupedLeft = true;
            water.groupedRight = false;

            if (state == MOVING_RIGHT)
            {
                group.Reverse();
                water.groupedLeft = false;
                water.groupedRight = true;
            }
        }

        private bool isFull(int moving, List<Water> waters)
        {
            if (waters.Contains(this))
            {
                return false;
            }
            else
            {
                waters.Add(this);
            }

            if (moving == MOVING_RIGHT)
            {
                if (waterAbove != null && tunnel.top == Tunnel.DUG && tunnel.left == Tunnel.DUG)
                {
                    return true;
                }
                else if (waterRight != null)
                {
                    return waterRight.isFull(moving, waters);
                }
                else
                {
                    return false;
                }
            }
            else
            {
                if (waterAbove != null && tunnel.top == Tunnel.DUG && tunnel.right == Tunnel.DUG)
                {
                    return true;
                }
                else if (waterLeft != null)
                {
                    return waterLeft.isFull(moving, waters);
                }
                else
                {
                    return false;
                }
            }

        }

        internal void land()
        {
            if (state != NONE && (state != FALLING || position.Y >= tunnel.position.Y + Tunnel.center.Y))
            {
                if (group.Count > 0)
                {
                    group[0].giveGroup(group, state);
                    group.Clear();
                }
                groupedLeft = false;
                groupedRight = false;
                leader = null;
                position = tunnel.position + Tunnel.center;
                state = NONE;
                currSprite.setFrame(0);
                currSprite = waterIdle;
                animationTime = waveMidTime;
                animationTimer = animationTime;
            }
        }
    }
}

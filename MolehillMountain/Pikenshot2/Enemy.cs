using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections;

namespace MoleHillMountain
{
    class Enemy : Mole
    {
        const int DOWN_CLEAR = 1;
        const int LEFT_CLEAR = 2;
        const int UP_CLEAR = 3;
        const int RIGHT_CLEAR = 4;

        Tunnel tunnel;
        ArrayList clearDirections;
        static Random random = new Random();
        int intendingToMove;

        public Enemy(DungeonScreen dungeonScene) : base(dungeonScene)
        {
            walking = new Sprite(PikeAndShotGame.RAT_WALKING, new Rectangle(0, 0, 18, 18), 22, 18);
            
            walkingSprite = walking;
            nudging = new Sprite(PikeAndShotGame.RAT_NUDGE, new Rectangle(0, 0, 18, 18), 22, 18);
            clearDirections = new ArrayList(4);
        }

        public override void update(TimeSpan timeSpan)
        {
            Tunnel newTunnel = dungeonScene.getCurrTunnel(position);
            setDig(false);
            if (tunnel == null || newTunnel != tunnel)
            {
                tunnel = newTunnel;

                clearDirections.Clear();

                if ((tunnel.left == Tunnel.DUG || tunnel.left == Tunnel.HALF_DUG))
                    clearDirections.Add(LEFT_CLEAR);
                if ((tunnel.right == Tunnel.DUG || tunnel.right == Tunnel.HALF_DUG))
                    clearDirections.Add(RIGHT_CLEAR);
                if ((tunnel.top == Tunnel.DUG || tunnel.top == Tunnel.HALF_DUG))
                    clearDirections.Add(UP_CLEAR);
                if ((tunnel.bottom == Tunnel.DUG || tunnel.bottom == Tunnel.HALF_DUG))
                    clearDirections.Add(DOWN_CLEAR);

                if (intendingToMove == MOVING_LEFT && clearDirections.Contains(LEFT_CLEAR))
                {
                    moveLeft();
                }
                else if (intendingToMove == MOVING_RIGHT && clearDirections.Contains(RIGHT_CLEAR))
                {
                    moveRight();
                }
                else if (intendingToMove == MOVING_DOWN && clearDirections.Contains(DOWN_CLEAR))
                {
                    moveDown();
                }
                else if (intendingToMove == MOVING_UP && clearDirections.Contains(UP_CLEAR))
                {
                    moveUp();
                }
                else
                {
                    if(clearDirections.Count > 1)
                    {
                        switch (intendingToMove)
                        {
                            case MOVING_DOWN: clearDirections.Remove(UP_CLEAR); break;
                            case MOVING_LEFT: clearDirections.Remove(RIGHT_CLEAR); break;
                            case MOVING_RIGHT: clearDirections.Remove(LEFT_CLEAR); break;
                            case MOVING_UP: clearDirections.Remove(DOWN_CLEAR); break;
                        }
                    }
                    int choice = random.Next(clearDirections.Count);
                    switch ((int)clearDirections[choice])
                    {
                        case LEFT_CLEAR: moveLeft(); break;
                        case RIGHT_CLEAR: moveRight(); break;
                        case UP_CLEAR: moveUp(); break;
                        case DOWN_CLEAR: moveDown(); break;
                    }
                }
            } else
            {
                switch (intendingToMove)
                {
                    case MOVING_DOWN: moveDown(); break;
                    case MOVING_LEFT: moveLeft(); break;
                    case MOVING_RIGHT: moveRight(); break;
                    case MOVING_UP: moveUp(); break;
                }
            }

            base.update(timeSpan);
        }

        public override void moveLeft()
        {
            intendingToMove = MOVING_LEFT;
            base.moveLeft();
        }

        public override void moveRight()
        {
            intendingToMove = MOVING_RIGHT;
            base.moveRight();
        }
        public override void moveUp()
        {
            intendingToMove = MOVING_UP;
            base.moveUp();
        }
        public override void moveDown()
        {
            intendingToMove = MOVING_DOWN;
            base.moveDown();
        }
    }
}

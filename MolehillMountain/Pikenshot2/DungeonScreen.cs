using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections;

namespace MoleHillMountain
{

    public enum AnimationType
    {
        stoneImpact,
        fightCloud,
        tunnelReveal,
        hookImpact
    }
    public class DungeonScreen : GameScreen
    {
        static Random random = new Random();
        public const int GRID_SIZE = 20;
        public const int GRID_WIDTH = 12;
        public const int GRID_HEIGHT = 9;
        public const int ENEMY_TIME = 3000;
        public const int FIGHT_DIST = 5;

        public static Vector2 OFFSET = new Vector2(8, 0);
        Sprite heart;
        Sprite itemIcon;
        Vector2 heartPosition = new Vector2(20, 181);
        Vector2 heartOffset = new Vector2(12, 0);

        string[] ItemNames = { "S. Shot", "C. Dasher" };

        internal Tunnel getCurrTunnel(Vector2 position)
        {
            if (((int)position.X >= 0 && (int)position.X / GRID_SIZE < GRID_WIDTH) && ((int)position.Y >= 0 && (int)position.Y / GRID_SIZE < GRID_HEIGHT))
            {
                return tunnels[(int)position.X / GRID_SIZE, (int)position.Y / GRID_SIZE];
            }
            else
            {
                return null;
            }
        }

        public PikeAndShotGame _game;
        public Mole mole;
        public Door door;
        public Tunnel[,] tunnels;
        public ArrayList enemies;
        ArrayList vegetables;
        ArrayList pickups;
        ArrayList stones;
        ArrayList deadStuff;
        ArrayList effects;
        ArrayList items;

        protected KeyboardState keyboardState;
        protected KeyboardState previousKeyboardState;
        protected GamePadState gamePadState;
        protected GamePadState previousGamePadState;

        private float pickupTime;
        private float pickupTimer;
        private int pickupSequenceCount;
        bool firstCheck = true;
        public int enemyCount;
        public float enemyTimer;
        private double _fps = 0;
        private double _draws = 0;

        Vector2 fpsPosition;
        Vector2 item1TextPos;
        Vector2 item2TextPos;
        Vector2 item1IconPos;
        Vector2 item2IconPos;
        int[,] combinedTunnels;
        private Vector2 hpPosition;

        public DungeonScreen(PikeAndShotGame game)
        {
            _game = game;
            heart = new Sprite(PikeAndShotGame.HEART, new Rectangle(0, 0, 11, 9), 11, 9);
            itemIcon = new Sprite(PikeAndShotGame.ITEM_ICONS, new Rectangle(0, 0, 9, 9), 9, 9);
            hpPosition = new Vector2(3f, 182f);
            fpsPosition = new Vector2(190f, 182);
            item1TextPos = new Vector2(100f, 182);
            item2TextPos = new Vector2(170f, 182);
            item1IconPos = new Vector2(110f, 181);
            item2IconPos = new Vector2(180f, 181);
            init();
        }

        internal Tunnel getTunnelBelow(Vector2 position)
        {
            int x = (int)(position.X / GRID_SIZE);
            int y = (int)(position.Y / GRID_SIZE) + 1;
            if (x < GRID_WIDTH && y < GRID_HEIGHT && x >= 0 && y >= 0)
                return tunnels[x, y];
            else
                return null;
        }

        internal void fire(int targetDirection, Vector2 position, int pathLength)
        {
            int x = (int)position.X / GRID_SIZE;
            int y = (int)position.Y / GRID_SIZE;

            if (targetDirection == Mole.MOVING_LEFT && x > 0 && (tunnels[x - 1, y].right == Tunnel.DUG || tunnels[x - 1, y].right == Tunnel.HALF_DUG))
            {
                fireSpread(x - 1, y, pathLength - 1, targetDirection, x, y);
            }
            else if (targetDirection == Mole.MOVING_RIGHT && x < GRID_WIDTH - 1 && (tunnels[x + 1, y].left == Tunnel.DUG || tunnels[x + 1, y].left == Tunnel.HALF_DUG))
            {
                fireSpread(x + 1, y, pathLength - 1, targetDirection, x, y);
            }
            else if (targetDirection == Mole.MOVING_UP && y > 0 && (tunnels[x, y - 1].bottom == Tunnel.DUG || tunnels[x, y - 1].bottom == Tunnel.HALF_DUG))
            {
                fireSpread(x, y - 1, pathLength - 1, targetDirection, x, y);
            }
            else if (targetDirection == Mole.MOVING_DOWN && y < GRID_HEIGHT - 1 && (tunnels[x, y + 1].top == Tunnel.DUG || tunnels[x, y + 1].top == Tunnel.HALF_DUG))
            {
                fireSpread(x, y + 1, pathLength - 1, targetDirection, x, y);
            }
        }

        private void fireSpread(int x, int y, int length, int direction, int origX, int origY)
        {
            tunnels[x, y].fire(length, direction);
            if (length > 0)
            {
                if (direction != Mole.MOVING_RIGHT && x > 0 && (x - 1 != origX || origY != y)
                    && (tunnels[x - 1, y].right == Tunnel.DUG || tunnels[x - 1, y].right == Tunnel.HALF_DUG)
                    && !tunnels[x - 1, y].isFire())
                {
                    fireSpread(x - 1, y, length - 1, Mole.MOVING_LEFT, origX, origY);
                }

                if (direction != Mole.MOVING_LEFT && x < GRID_WIDTH - 1 && (x + 1 != origX || origY != y)
                    && (tunnels[x + 1, y].left == Tunnel.DUG || tunnels[x + 1, y].left == Tunnel.HALF_DUG)
                    && !tunnels[x + 1, y].isFire())
                {
                    fireSpread(x + 1, y, length - 1, Mole.MOVING_RIGHT, origX, origY);
                }

                if (direction != Mole.MOVING_UP && y < GRID_HEIGHT - 1 && (x != origX || origY != y + 1)
                    && (tunnels[x, y + 1].top == Tunnel.DUG || tunnels[x, y + 1].top == Tunnel.HALF_DUG)
                    && !tunnels[x, y + 1].isFire())
                {
                    fireSpread(x, y + 1, length - 1, Mole.MOVING_DOWN, origX, origY);
                }

                if (direction != Mole.MOVING_DOWN && y > 0 && (x != origX || origY != y - 1)
                    && (tunnels[x, y - 1].bottom == Tunnel.DUG || tunnels[x, y - 1].bottom == Tunnel.HALF_DUG)
                    && !tunnels[x, y - 1].isFire())
                {
                    fireSpread(x, y - 1, length - 1, Mole.MOVING_UP, origX, origY);
                }
            }
        }

        internal Tunnel getTunnelAbove(Vector2 position)
        {
            int x = (int)(position.X / GRID_SIZE);
            int y = (int)(position.Y / GRID_SIZE) - 1;
            if (x < GRID_WIDTH && y < GRID_HEIGHT && x >= 0 && y >= 0)
                return tunnels[x, y];
            else
                return null;
        }

        internal Tunnel getTunnelLeft(Vector2 position)
        {
            int x = (int)(position.X / GRID_SIZE) - 1;
            int y = (int)(position.Y / GRID_SIZE);
            if (x < GRID_WIDTH && y < GRID_HEIGHT && x >= 0 && y >= 0)
                return tunnels[x, y];
            else
                return null;
        }

        internal Tunnel getTunnelRight(Vector2 position)
        {
            int x = (int)(position.X / GRID_SIZE) + 1;
            int y = (int)(position.Y / GRID_SIZE);
            if (x < GRID_WIDTH && y < GRID_HEIGHT && x >= 0 && y >= 0)
                return tunnels[x, y];
            else
                return null;
        }

        public void createAnimation(Vector2 position, int horz, int vert, AnimationType animationType, float delay = 0)
        {
            Animation returnedEffect = null;

            foreach (Animation effect in effects)
            {
                if (!effect.active && effect.type == animationType)
                {
                    returnedEffect = effect;
                    break;
                }
            }

            if (returnedEffect == null)
            {
                returnedEffect = new Animation(animationType);
                effects.Add(returnedEffect);
            }

            switch (animationType)
            {
                case AnimationType.stoneImpact:
                    returnedEffect.activate((int)position.X, (int)position.Y, horz, vert, delay);
                    break;
                case AnimationType.hookImpact:
                    returnedEffect.activate((int)position.X, (int)position.Y, horz, vert, delay);
                    break;
                case AnimationType.fightCloud:
                    returnedEffect.activate((int)(position.X + OFFSET.X), (int)(position.Y + OFFSET.Y), Sprite.DIRECTION_LEFT, Sprite.DIRECTION_NONE, delay);
                    break;
                case AnimationType.tunnelReveal:
                    returnedEffect.activate((int)(position.X + OFFSET.X), (int)(position.Y + OFFSET.Y), Sprite.DIRECTION_LEFT, Sprite.DIRECTION_NONE, delay);
                    break;
            }

        }
        public Mole checkEnemyCollision(float x, float y, float radius)
        {
            float enemyRadius = 8;
            foreach (Mole enemy in enemies)
            {
                if (enemy.position.X - enemyRadius + radius > x)
                {
                    continue;
                }
                else if (enemy.position.X + enemyRadius - radius < x)
                {
                    continue;
                }
                else if (enemy.position.Y - enemyRadius + radius > y)
                {
                    continue;
                }
                else if (enemy.position.Y + enemyRadius - radius < y)
                {
                    continue;
                }
                return enemy;
            }
            return null;
        }

        public void createStoneImpact(Mole mole)
        {
            createAnimation(mole.position, mole.horzFacing, mole.vertFacing, AnimationType.stoneImpact);
        }

        public void draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            _draws++;
            foreach (Tunnel tunnel in tunnels)
            {
                tunnel.draw(spriteBatch);
            }

            if (door != null)
            {
                door.draw(spriteBatch);
            }

            foreach (Vegetable vegetable in vegetables)
            {
                vegetable.draw(spriteBatch);
            }

            foreach (Grub pickup in pickups)
            {
                pickup.draw(spriteBatch);
            }

            foreach (Rat enemy in enemies)
            {
                enemy.draw(spriteBatch);
            }

            foreach (Projectile stone in stones)
            {
                stone.draw(spriteBatch);
            }

            foreach (Item item in items)
            {
                item.draw(spriteBatch);
            }

            mole.draw(spriteBatch);

            foreach (Tunnel tunnel in tunnels)
            {
                tunnel.drawEffect(spriteBatch);
            }

            foreach (Animation effect in effects)
            {
                if (effect.active)
                {
                    effect.draw(spriteBatch);
                }
            }

            //spriteBatch.Draw(PikeAndShotGame.SANDBOX, new Rectangle((int)OFFSET.X, 80 + (int)OFFSET.Y, 70, 100), new Rectangle(128, 0, 70, 100), Color.White, 0, Vector2.Zero, SpriteEffects.None, 0f);
            //spriteBatch.Draw(PikeAndShotGame.SANDBOX, new Rectangle((int)OFFSET.X, 80 + (int)OFFSET.Y, 72, 20), new Rectangle(0, 1, 72, 20), Color.White, 0, Vector2.Zero, SpriteEffects.None, 0f);
            spriteBatch.DrawString(PikeAndShotGame.GOBLIN_FONT, "HP", hpPosition, Color.Black);
            for (int i = 0; i < mole.con; i++)
            {
                if (mole.health > i)
                {
                    heart.setFrame(0);
                }
                else
                {
                    heart.setFrame(1);
                }
                heart.draw(spriteBatch, heartPosition + heartOffset * i, 0);
            }

            itemIcon.setFrame(0);
            itemIcon.draw(spriteBatch, item1IconPos, 1);
            itemIcon.setFrame(1);
            itemIcon.draw(spriteBatch, item2IconPos, 1);

            spriteBatch.DrawString(PikeAndShotGame.GOBLIN_FONT, "Z:   " + ItemNames[mole.getItem1()], item1TextPos, Color.Black);
            spriteBatch.DrawString(PikeAndShotGame.GOBLIN_FONT, "X:   " + ItemNames[mole.getItem2()], item2TextPos, Color.Black);

            //spriteBatch.DrawString(PikeAndShotGame.GOBLIN_FONT, "fps: " + _fps, fpsPosition, Color.Black);

        }

        internal void spawnHook(Vector2 position, int horzFacing, int vertFacing)
        {
            stones.Add(new Hook(position, vertFacing, horzFacing, this));
        }

        public void update(GameTime gameTime)
        {
            getInput(gameTime.ElapsedGameTime);
            SeenStatus.update(gameTime);
            mole.update(gameTime);
            if (door != null)
            {
                door.update(gameTime);
            }

            foreach (Vegetable vege in vegetables)
            {
                vege.update(gameTime);
                if (vege.state == Vegetable.FALLING)
                {
                    checkCollisions(vege);
                }
                if (vege.state == Vegetable.DEAD)
                {
                    deadStuff.Add(vege);
                }
            }

            foreach (Vegetable vege in vegetables)
            {
                if (vege.leftPushers.Count == 0 && vege.rightPushers.Count == 0 && vege.state == Vegetable.MOVING)
                {
                    vege.state = Vegetable.NONE;
                }
                vege.push((float)gameTime.ElapsedGameTime.TotalSeconds);
            }

            foreach (Vegetable vege in deadStuff)
            {
                vegetables.Remove(vege);
            }
            deadStuff.Clear();

            foreach (Grub pickup in pickups)
            {
                Vector2 distance = pickup.position - mole.position;
                if (distance.Length() <= 5)
                {
                    if (pickupTimer < 0)
                    {
                        pickupSequenceCount = 0;
                    }
                    else
                    {
                        pickupSequenceCount++;
                        if (pickupSequenceCount > 7)
                        {
                            pickupSequenceCount = 0;
                        }
                    }
                    pickupTimer = pickupTime;

                    pickup.collected(pickupSequenceCount);
                }
                pickup.update(gameTime);

                if (pickup.state == Grub.STATE_COLLECTED)
                    deadStuff.Add(pickup);
            }

            foreach (Grub pickup in deadStuff)
            {
                pickups.Remove(pickup);
            }
            deadStuff.Clear();

            //check if level over
            if (pickups.Count <= 0)
            {
                beatLevel();
            }

            foreach (Projectile stone in stones)
            {
                stone.update(gameTime.ElapsedGameTime);
                if (stone.dead)
                    deadStuff.Add(stone);
            }
            foreach (Projectile stone in deadStuff)
            {
                stones.Remove(stone);
            }
            deadStuff.Clear();

            foreach (Animation effect in effects)
            {
                if (effect.active)
                {
                    effect.update(gameTime.ElapsedGameTime);
                }
            }

            foreach (Rat enemy in enemies)
            {
                enemy.update(gameTime);
                if ((enemy.state & Mole.STATE_SQUASHED) == 0 && (enemy.state & Mole.STATE_NUDGING) == 0)
                {
                    updateTunnels(enemy);
                }

                Vector2 diff = enemy.position - mole.position;

                if (Math.Abs(diff.X) <= FIGHT_DIST && Math.Abs(diff.Y) <= FIGHT_DIST
                    && (mole.state & Mole.STATE_SQUASHED) == 0 && (enemy.state & Mole.STATE_SQUASHED) == 0
                    && (mole.state & Mole.STATE_FIGHTING) == 0 && (enemy.state & Mole.STATE_FIGHTING) == 0
                    && (mole.state & Mole.STATE_DIZZY) == 0)
                {
                    createAnimation(mole.position, mole.horzFacing, mole.vertFacing, AnimationType.fightCloud);
                    mole.fight();
                    enemy.fight();
                }

                if ((enemy.state & Mole.STATE_SQUASHED) != 0 && enemy.squashedTimer <= 0)
                {
                    deadStuff.Add(enemy);
                }
            }
            foreach (Rat enemy in deadStuff)
            {
                enemies.Remove(enemy);
            }
            deadStuff.Clear();

            foreach (Item item in items)
            {
                item.update(gameTime);
                Vector2 distance = item.position - mole.position;
                if (distance.Length() <= 5)
                {

                }
            }

            foreach (Tunnel tunnel in tunnels)
            {
                tunnel.update(this, gameTime);
            }

            updateTunnels(mole);
            if (getCurrTunnel(mole.position).seen == SeenStatus.HALF_SEEN)
            {
                revealTunnels();
            }


            pickupTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (enemyCount > 0)
            {
                enemyTimer -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                if (enemyTimer <= 0)
                {
                    enemyTimer = ENEMY_TIME;
                    int pick = random.Next(4);
                    if (pick == 0)
                        enemies.Add(new Rat(this, door.position.X, door.position.Y));
                    else if (pick == 1)
                        enemies.Add(new Beeble(this, door.position.X, door.position.Y));
                    else if (pick == 2)
                        enemies.Add(new Salamando(this, door.position.X, door.position.Y));
                    else
                        enemies.Add(new Mothy(this, door.position.X, door.position.Y));

                    enemyCount--;
                }
            }
            _fps = (double)_draws / gameTime.ElapsedGameTime.TotalSeconds;
            _draws = 0;
        }

        internal void spawnItem(Vegetable vegetable)
        {
            switch (vegetable.dropList[PikeAndShotGame.random.Next(vegetable.dropList.Count)])
            {
                case Item.DROP_SLINGSHOT:
                    items.Add(new Item((int)(vegetable.position.X / GRID_SIZE), (int)(vegetable.position.Y / GRID_SIZE), this));
                    break;
                default:
                    break;
            }
        }

        private void beatLevel()
        {
            init();
        }

        public void spawnStone(Vector2 position, int horzFacing, int vertFacing)
        {
            stones.Add(new Stone(position, vertFacing, horzFacing, this));
        }

        List<Tunnel> openTunnels;
        List<Tunnel> closedTunnels;
        public List<Tunnel> hasPath(Tunnel start, Tunnel end)
        {
            if (openTunnels == null)
            {
                openTunnels = new List<Tunnel>();
                closedTunnels = new List<Tunnel>();
            }
            else
            {
                openTunnels.Clear();
                closedTunnels.Clear();
            }

            openTunnels.AddRange(Array.FindAll(tunnels.Cast<Tunnel>().ToArray(), t => t.left == Tunnel.DUG || t.right == Tunnel.DUG || t.top == Tunnel.DUG || t.bottom == Tunnel.DUG));
            if (!openTunnels.Contains(start) || !openTunnels.Contains(end))
                return null;

            List<Tunnel> path = new List<Tunnel>();

            Tunnel currTunnel = start;
            int x;
            int y;
            Tunnel up;
            Tunnel down;
            Tunnel left;
            Tunnel right;

            while (currTunnel != end)
            {
                if (!closedTunnels.Contains(currTunnel))
                {
                    path.Add(currTunnel);
                }
                if (!closedTunnels.Contains(currTunnel))
                {
                    closedTunnels.Add(currTunnel);
                }
                x = (int)(currTunnel.position.X / GRID_SIZE);
                y = (int)(currTunnel.position.Y / GRID_SIZE);

                up = left = down = right = null;

                if (y > 0 && tunnels[x, y - 1].bottom == Tunnel.DUG)
                {
                    up = closedTunnels.Contains(tunnels[x, y - 1]) ? null : tunnels[x, y - 1];
                }
                if (y < GRID_HEIGHT - 1 && tunnels[x, y + 1].top == Tunnel.DUG)
                {
                    down = closedTunnels.Contains(tunnels[x, y + 1]) ? null : tunnels[x, y + 1];
                }
                if (x > 0 && tunnels[x - 1, y].right == Tunnel.DUG)
                {
                    left = closedTunnels.Contains(tunnels[x - 1, y]) ? null : tunnels[x - 1, y];
                }
                if (x < GRID_WIDTH - 1 && tunnels[x + 1, y].left == Tunnel.DUG)
                {
                    right = closedTunnels.Contains(tunnels[x + 1, y]) ? null : tunnels[x + 1, y];
                }

                int upDist = -1;
                int downDist = -1;
                int leftDist = -1;
                int rightDist = -1;

                if (up != null)
                    upDist = getEstDist(up, start, end);
                if (down != null)
                    downDist = getEstDist(down, start, end);
                if (left != null)
                    leftDist = getEstDist(left, start, end);
                if (right != null)
                    rightDist = getEstDist(right, start, end);

                Tunnel bestTunnel = null;
                int bestDist = -1;

                //TODO: randomize which one is chosen
                if (upDist != -1 && (bestTunnel == null || upDist < bestDist))
                {
                    bestTunnel = up;
                    bestDist = upDist;
                }
                if (downDist != -1 && (bestTunnel == null || downDist < bestDist))
                {
                    bestTunnel = down;
                    bestDist = downDist;
                }
                if (leftDist != -1 && (bestTunnel == null || leftDist < bestDist))
                {
                    bestTunnel = left;
                    bestDist = leftDist;
                }
                if (rightDist != -1 && (bestTunnel == null || rightDist < bestDist))
                {
                    bestTunnel = right;
                    bestDist = rightDist;
                }

                if (bestTunnel != null)
                {
                    currTunnel = bestTunnel;
                }
                else
                {
                    path.Remove(currTunnel);
                    if (path.Count < 1)
                        break;
                    currTunnel = path[path.Count - 1];
                }

            }

            if (currTunnel != end)
            {
                return null;
            }
            else
            {
                return path;
            }

        }

        private int getEstDist(Tunnel up, Tunnel start, Tunnel end)
        {
            int x1 = (int)(up.position.X / GRID_SIZE);
            int y1 = (int)(up.position.Y / GRID_SIZE) + 1;

            int x2 = (int)(start.position.X / GRID_SIZE);
            int y2 = (int)(start.position.Y / GRID_SIZE) + 1;

            int x3 = (int)(end.position.X / GRID_SIZE);
            int y3 = (int)(end.position.Y / GRID_SIZE) + 1;

            return Math.Abs(x1 - x2) + Math.Abs(y1 - y2) + Math.Abs(x1 - x3) + Math.Abs(y1 - y3);
        }

        internal int checkForTarget(Vector2 position, Vector2 enemyPosition, bool mad)
        {
            float yDiff = position.Y - enemyPosition.Y;
            float xDiff = position.X - enemyPosition.X;

            float slope = yDiff / xDiff;
            if (float.IsNegativeInfinity(slope))
                slope = 0;
            float yIntercept = enemyPosition.Y - (slope * enemyPosition.X);

            Tunnel startingTunnel = getCurrTunnel(enemyPosition);
            if (startingTunnel == null)
            {
                return Mole.MOVING_NONE;
            }

            Tunnel tunnel;
            Vector2 vect = Vector2.Zero;
            if (!mad)
            {
                if (yDiff >= 0.5f)
                {
                    for (int y = (int)startingTunnel.position.Y + (int)Tunnel.center.Y; y < position.Y && y < GRID_SIZE * (GRID_HEIGHT - 1); y += GRID_SIZE)
                    {
                        float x = (y - yIntercept) / slope;

                        if (float.IsNaN(x))
                            x = enemyPosition.X;

                        if (x < 0 || x > GRID_SIZE * GRID_WIDTH)
                        {
                            break;
                        }
                        else
                        {
                            vect.X = x;
                            vect.Y = y + 1;
                            tunnel = getCurrTunnel(vect);
                            if (tunnel.bottom == Tunnel.NOT_DUG)
                            {
                                return Mole.MOVING_NONE;
                            }
                            vect.Y = y + GRID_SIZE;
                            tunnel = getCurrTunnel(vect);
                            if (tunnel.top == Tunnel.NOT_DUG)
                            {
                                return Mole.MOVING_NONE;
                            }
                        }
                    }
                }
                else
                {
                    for (int y = (int)startingTunnel.position.Y + (int)Tunnel.center.Y; y > position.Y && y > 0; y -= GRID_SIZE)
                    {
                        float x = (y - yIntercept) / slope;

                        if (float.IsNaN(x))
                            x = enemyPosition.X;

                        if (x < 0 || x > GRID_SIZE * GRID_WIDTH)
                        {
                            break;
                        }
                        else
                        {
                            vect.X = x;
                            vect.Y = y - 1;
                            tunnel = getCurrTunnel(vect);
                            if (tunnel.top == Tunnel.NOT_DUG)
                            {
                                return Mole.MOVING_NONE;
                            }
                            vect.Y = y -GRID_SIZE;
                            tunnel = getCurrTunnel(vect);
                            if (tunnel.bottom == Tunnel.NOT_DUG)
                            {
                                return Mole.MOVING_NONE;
                            }
                        }
                    }
                }
                if (xDiff >= 0.5f)
                {
                    for (int x = (int)startingTunnel.position.X + (int)Tunnel.center.X; x < position.X && x < GRID_SIZE * (GRID_WIDTH - 1); x += GRID_SIZE)
                    {
                        float y = x * slope + yIntercept;
                        if (y < 0 || y > GRID_SIZE * GRID_HEIGHT)
                        {
                            break;
                        }
                        else
                        {
                            vect.X = x + 1;
                            vect.Y = y;
                            tunnel = getCurrTunnel(vect);
                            if (tunnel.right == Tunnel.NOT_DUG)
                            {
                                return Mole.MOVING_NONE;
                            }
                            vect.X = x + GRID_SIZE;
                            tunnel = getCurrTunnel(vect);
                            if (tunnel.left == Tunnel.NOT_DUG)
                            {
                                return Mole.MOVING_NONE;
                            }
                        }
                    }
                }
                else
                {
                    for (int x = (int)startingTunnel.position.X + (int)Tunnel.center.X; x > position.X && x > 0; x -= GRID_SIZE)
                    {
                        float y = x * slope + yIntercept;
                        if (y < 0 || y > GRID_SIZE * GRID_HEIGHT)
                        {
                            break;
                        }
                        else
                        {
                            vect.X = x - 1;
                            vect.Y = y;
                            tunnel = getCurrTunnel(vect);
                            if (tunnel.left == Tunnel.NOT_DUG)
                            {
                                return Mole.MOVING_NONE;
                            }
                            vect.X = x - GRID_SIZE;
                            tunnel = getCurrTunnel(vect);
                            if (tunnel.right == Tunnel.NOT_DUG)
                            {
                                return Mole.MOVING_NONE;
                            }
                        }
                    }
                }
            }

            if (Math.Abs(xDiff) > Math.Abs(yDiff))
            {
                if (xDiff > 0)
                    return Mole.MOVING_RIGHT;
                else
                    return Mole.MOVING_LEFT;
            }
            else
            {
                if (yDiff > 0)
                    return Mole.MOVING_DOWN;
                else
                    return Mole.MOVING_UP;
            }
        }

        internal int checkForTarget(Mole mole, Rat enemy, bool mad)
        {
            float yDiff = mole.position.Y - enemy.position.Y;
            float xDiff = mole.position.X - enemy.position.X;

            float slope = yDiff / xDiff;
            float yIntercept = enemy.position.Y - (slope * enemy.position.X);

            Tunnel startingTunnel = getCurrTunnel(enemy.position);
            if (startingTunnel == null)
            {
                return enemy.moving;
            }

            Tunnel tunnel;
            Vector2 vect = Vector2.Zero;
            if (!mad)
            {
                if (yDiff >= 0)
                {
                    for (int y = (int)startingTunnel.position.Y + GRID_SIZE; y < mole.position.Y && y < GRID_SIZE * (GRID_HEIGHT - 1); y += GRID_SIZE)
                    {
                        float x = (y - yIntercept) / slope;

                        if (float.IsNaN(x))
                            x = enemy.position.X;

                        if (x < 0 || x > GRID_SIZE * GRID_WIDTH)
                        {
                            break;
                        }
                        else
                        {
                            vect.X = x;
                            vect.Y = y - 1;
                            tunnel = getCurrTunnel(vect);
                            if (tunnel.bottom == Tunnel.NOT_DUG)
                            {
                                return Mole.MOVING_NONE;
                            }
                            vect.Y = y + 1;
                            tunnel = getCurrTunnel(vect);
                            if (tunnel.top == Tunnel.NOT_DUG)
                            {
                                return Mole.MOVING_NONE;
                            }
                        }
                    }
                }
                else
                {
                    for (int y = (int)startingTunnel.position.Y; y > mole.position.Y && y > 0; y -= GRID_SIZE)
                    {
                        float x = (y - yIntercept) / slope;

                        if (float.IsNaN(x))
                            x = enemy.position.X;

                        if (x < 0 || x > GRID_SIZE * GRID_WIDTH)
                        {
                            break;
                        }
                        else
                        {
                            vect.X = x;
                            vect.Y = y - 1;
                            tunnel = getCurrTunnel(vect);
                            if (tunnel.bottom == Tunnel.NOT_DUG)
                            {
                                return Mole.MOVING_NONE;
                            }
                            vect.Y = y + 1;
                            tunnel = getCurrTunnel(vect);
                            if (tunnel.top == Tunnel.NOT_DUG)
                            {
                                return Mole.MOVING_NONE;
                            }
                        }
                    }
                }
                if (xDiff >= 0)
                {
                    for (int x = (int)startingTunnel.position.X + GRID_SIZE; x < mole.position.X && x < GRID_SIZE * (GRID_WIDTH - 1); x += GRID_SIZE)
                    {
                        float y = x * slope + yIntercept;
                        if (y < 0 || y > GRID_SIZE * GRID_HEIGHT)
                        {
                            break;
                        }
                        else
                        {
                            vect.X = x - 1;
                            vect.Y = y;
                            tunnel = getCurrTunnel(vect);
                            if (tunnel.right == Tunnel.NOT_DUG)
                            {
                                return Mole.MOVING_NONE;
                            }
                            vect.X = x + 1;
                            tunnel = getCurrTunnel(vect);
                            if (tunnel.left == Tunnel.NOT_DUG)
                            {
                                return Mole.MOVING_NONE;
                            }
                        }
                    }
                }
                else
                {
                    for (int x = (int)startingTunnel.position.X; x > mole.position.X && x > 0; x -= GRID_SIZE)
                    {
                        float y = x * slope + yIntercept;
                        if (y < 0 || y > GRID_SIZE * GRID_HEIGHT)
                        {
                            break;
                        }
                        else
                        {
                            vect.X = x - 1;
                            vect.Y = y;
                            tunnel = getCurrTunnel(vect);
                            if (tunnel.right == Tunnel.NOT_DUG)
                            {
                                return Mole.MOVING_NONE;
                            }
                            vect.X = x + 1;
                            tunnel = getCurrTunnel(vect);
                            if (tunnel.left == Tunnel.NOT_DUG)
                            {
                                return Mole.MOVING_NONE;
                            }
                        }
                    }
                }
            }

            if (Math.Abs(xDiff) > Math.Abs(yDiff))
            {
                if (xDiff > 0)
                    return Mole.MOVING_RIGHT;
                else
                    return Mole.MOVING_LEFT;
            }
            else
            {
                if (yDiff > 0)
                    return Mole.MOVING_DOWN;
                else
                    return Mole.MOVING_UP;
            }
        }

        internal bool vegetableRightClear(Rat mole)
        {
            Vector2 position = mole.position;
            if (!vegetableRight(mole.position, new ArrayList { }, Vegetable.NUDGE_SPACING))
            {
                return true;
            }
            else if ((int)(position.X - 5 + GRID_SIZE * 2) < GRID_WIDTH * GRID_SIZE)
            {
                Tunnel tunnelTwoOver = tunnels[(int)(position.X - 5 + GRID_SIZE * 2) / GRID_SIZE, (int)mole.position.Y / GRID_SIZE];
                if (tunnelTwoOver.left == Tunnel.DUG || tunnelTwoOver.left == Tunnel.HALF_DUG)
                {
                    return true;
                }
            }
            return false;
        }

        Vector2 moleWidth = new Vector2(GRID_SIZE / 2, 0);

        internal bool vegetableLeftClear(Rat mole)
        {
            Vector2 position = mole.position;

            if (!vegetableLeft(mole.position, new ArrayList { }, Vegetable.NUDGE_SPACING))
            {
                return true;
            }
            else if ((int)(position.X + 5 - GRID_SIZE * 2) > 0)
            {
                Tunnel tunnelTwoOver = tunnels[(int)(position.X + 5 - GRID_SIZE * 2) / GRID_SIZE, (int)mole.position.Y / GRID_SIZE];
                if (tunnelTwoOver.right == Tunnel.DUG || tunnelTwoOver.right == Tunnel.HALF_DUG)
                {
                    return true;
                }
            }
            return false;

        }

        internal int checkMoleSight(Vector2 position)
        {
            Tunnel tunnel = getCurrTunnel(position);

            if (tunnel != null && (tunnel.left != Tunnel.NOT_DUG || tunnel.right != Tunnel.NOT_DUG || tunnel.top != Tunnel.NOT_DUG || tunnel.bottom != Tunnel.NOT_DUG))
            {
                return SeenStatus.SEEN;
            }

            float distance = (mole.position - position).Length();

            if (distance <= GRID_SIZE * (mole.per * 0.5f + 0.1f))
            {
                return SeenStatus.SEEN;
            }
            else if (distance <= GRID_SIZE * (mole.per + 0.1f))
            {
                return SeenStatus.HALF_SEEN;
            }
            else
            {
                return SeenStatus.NOT_SEEN;
            }
        }

        internal int checkMoleSight(Tunnel tunnel)
        {
            if (tunnel.revealed)
                return SeenStatus.SEEN;

            float distance = (mole.position - tunnel.position).Length();

            if (distance <= GRID_SIZE * (mole.per * 0.5f + 0.1f))
            {
                return SeenStatus.HALF_SEEN;
            }
            else if (distance <= GRID_SIZE * (mole.per + 0.1f))
            {
                return SeenStatus.HALF_SEEN;
            }
            else
            {
                return SeenStatus.NOT_SEEN;
            }
        }


        void updateTunnels(Mole mole)
        {
            int moleMiddleX = ((int)mole.position.X) / GRID_SIZE;
            int moleMiddleY = ((int)mole.position.Y) / GRID_SIZE;

            int moleLeft = ((int)mole.position.X - GRID_SIZE / 4) / GRID_SIZE;
            int moleRight = ((int)mole.position.X + GRID_SIZE / 4) / GRID_SIZE;
            int moleUp = ((int)mole.position.Y - GRID_SIZE / 4) / GRID_SIZE;
            int moleDown = ((int)mole.position.Y + GRID_SIZE / 4) / GRID_SIZE;

            if (moleLeft < 0 || moleRight >= GRID_WIDTH || moleUp < 0 || moleDown >= GRID_HEIGHT)
            {
                return;
            }

            if (mole.horzFacing == Sprite.DIRECTION_RIGHT)
            {
                if ((tunnels[moleRight, moleMiddleY].left != Tunnel.DUG && tunnels[moleRight, moleMiddleY].right != Tunnel.DUG) || mole.diggingTunnel == tunnels[moleRight, moleMiddleY])
                {
                    mole.setDig(true);
                    mole.diggingTunnel = tunnels[moleRight, moleMiddleY];
                }
                else
                {
                    mole.setDig(false);
                    mole.diggingTunnel = null;
                }
            }
            else if (mole.horzFacing == Sprite.DIRECTION_LEFT)
            {
                if ((tunnels[moleLeft, moleMiddleY].right != Tunnel.DUG && tunnels[moleLeft, moleMiddleY].left != Tunnel.DUG) || mole.diggingTunnel == tunnels[moleLeft, moleMiddleY])
                {
                    mole.setDig(true);
                    mole.diggingTunnel = tunnels[moleLeft, moleMiddleY];
                }
                else
                {
                    mole.setDig(false);
                    mole.diggingTunnel = null;
                }
            }

            if (mole.vertFacing == Sprite.DIRECTION_DOWN)
            {
                if ((tunnels[moleMiddleX, moleDown].top != Tunnel.DUG && tunnels[moleMiddleX, moleDown].bottom != Tunnel.DUG) || (mole.diggingTunnel == tunnels[moleMiddleX, moleDown] && tunnels[moleMiddleX, moleDown].bottom != Tunnel.DUG))
                {
                    mole.setDig(true);
                    mole.diggingTunnel = tunnels[moleMiddleX, moleDown];
                }
                else
                {
                    mole.setDig(false);
                    mole.diggingTunnel = null;
                }
            }
            else if (mole.vertFacing == Sprite.DIRECTION_UP)
            {
                if ((tunnels[moleMiddleX, moleUp].bottom != Tunnel.DUG && tunnels[moleMiddleX, moleUp].top != Tunnel.DUG) || (mole.diggingTunnel == tunnels[moleMiddleX, moleUp] && tunnels[moleMiddleX, moleDown].top != Tunnel.DUG))
                {
                    mole.setDig(true);
                    mole.diggingTunnel = tunnels[moleMiddleX, moleUp];
                }
                else
                {
                    mole.setDig(false);
                    mole.diggingTunnel = null;
                }
            }

            if (moleLeft >= 0 && moleRight < tunnels.GetLength(0) && moleUp >= 0 && moleDown < tunnels.GetLength(1))
            {


                if (tunnels[moleRight, moleMiddleY].left == Tunnel.NOT_DUG && mole.prevMoleRight == moleRight - 1)
                {
                    tunnels[moleRight, moleMiddleY].left = Tunnel.HALF_DUG;
                    tunnels[moleRight - 1, moleMiddleY].right = Tunnel.DUG;
                }
                if (tunnels[moleLeft, moleMiddleY].left == Tunnel.HALF_DUG && mole.prevMoleLeft == moleLeft - 1)
                {
                    tunnels[moleLeft, moleMiddleY].left = Tunnel.DUG;
                }

                if (tunnels[moleLeft, moleMiddleY].right == Tunnel.NOT_DUG && mole.prevMoleLeft == moleLeft + 1)
                {
                    tunnels[moleLeft, moleMiddleY].right = Tunnel.HALF_DUG;
                    tunnels[moleLeft + 1, moleMiddleY].left = Tunnel.DUG;
                }
                if (tunnels[moleRight, moleMiddleY].right == Tunnel.HALF_DUG && mole.prevMoleRight == moleRight + 1)
                {
                    tunnels[moleRight, moleMiddleY].right = Tunnel.DUG;
                }

                mole.prevMoleLeft = moleLeft;
                mole.prevMoleRight = moleRight;

                if (tunnels[moleMiddleX, moleDown].top == Tunnel.NOT_DUG && mole.prevMoleDown == moleDown - 1)
                {
                    tunnels[moleMiddleX, moleDown].top = Tunnel.HALF_DUG;
                    tunnels[moleMiddleX, moleDown - 1].bottom = Tunnel.DUG;
                }
                if (tunnels[moleMiddleX, moleUp].top == Tunnel.HALF_DUG && mole.prevMoleUp == moleUp - 1)
                {
                    tunnels[moleMiddleX, moleUp].top = Tunnel.DUG;
                }

                if (tunnels[moleMiddleX, moleUp].bottom == Tunnel.NOT_DUG && mole.prevMoleUp == moleUp + 1)
                {
                    tunnels[moleMiddleX, moleUp].bottom = Tunnel.HALF_DUG;
                    tunnels[moleMiddleX, moleUp + 1].top = Tunnel.DUG;
                }
                if (tunnels[moleMiddleX, moleDown].bottom == Tunnel.HALF_DUG && mole.prevMoleDown == moleDown + 1)
                {
                    tunnels[moleMiddleX, moleDown].bottom = Tunnel.DUG;
                }

                mole.prevMoleUp = moleUp;
                mole.prevMoleDown = moleDown;

                foreach (Tunnel tunnel in tunnels)
                {
                    if (tunnel.bottom == Tunnel.DUG || tunnel.top == Tunnel.DUG || tunnel.right == Tunnel.DUG || tunnel.left == Tunnel.DUG)
                    {
                        if (tunnel.bottom == Tunnel.HALF_DUG)
                            tunnel.bottom = Tunnel.DUG;
                        if (tunnel.top == Tunnel.HALF_DUG)
                            tunnel.top = Tunnel.DUG;
                        if (tunnel.right == Tunnel.HALF_DUG)
                            tunnel.right = Tunnel.DUG;
                        if (tunnel.left == Tunnel.HALF_DUG)
                            tunnel.left = Tunnel.DUG;
                    }
                }

                if ((mole.state & Mole.STATE_DIGGING) != 0)
                {
                    revealTunnels();
                }
            }
        }

        internal Vector2 getMolePosition()
        {
            return mole.position;
        }

        internal bool moleBelow(Vector2 position)
        {
            Vector2 absPos = (mole.position - position);

            if (Math.Abs(absPos.X) < GRID_SIZE / 2 && ((int)(mole.position.Y - position.Y) > GRID_SIZE / 2 && (int)(mole.position.Y - position.Y) < GRID_SIZE + 5))
            {
                return true;
            }

            return false;
        }

        internal bool moleJustBelow(Vegetable vegetable)
        {
            Vector2 position = vegetable.position;
            Vector2 absPos = (mole.position - position);
            bool squashedSomeone = false;

            if (Math.Abs(absPos.X) < GRID_SIZE / 2 && ((int)(mole.position.Y - position.Y) > 0 && (int)(mole.position.Y - position.Y) <= GRID_SIZE / 4))
            {
                squashMole(vegetable);
                squashedSomeone = true;
            }

            foreach (Rat enemy in enemies)
            {
                absPos = (enemy.position - position);
                if (Math.Abs(absPos.X) < GRID_SIZE / 2 && ((int)(enemy.position.Y - position.Y) >= -GRID_SIZE / 8 && (int)(enemy.position.Y - position.Y) <= GRID_SIZE / 4))
                {
                    enemy.squash(vegetable);
                    squashedSomeone = true;
                }
            }

            foreach (Vegetable vege in vegetables)
            {
                absPos = (vege.position - position);
                if (Math.Abs(absPos.X) < GRID_SIZE / 2 && ((int)(vege.position.Y - position.Y) > 0
                    && (int)(vege.position.Y - position.Y) <= GRID_SIZE * 3 / 4)
                    && vege.state != Vegetable.FALLING)
                {
                    vege.split();
                    vegetable.split();
                    squashedSomeone = true;
                }
            }

            return squashedSomeone;
        }

        internal void squashMole(Vegetable vegetable)
        {
            mole.squash(vegetable);
        }

        internal bool vegetableRight(Vector2 position, ArrayList pushers, float spacing)
        {
            foreach (Vegetable vege in vegetables)
            {
                if ((vege.state == Vegetable.NONE || vege.state == Vegetable.MOVING) && position != vege.position)
                {
                    if (vege.position.X - position.X < GRID_SIZE - spacing && Math.Abs(vege.position.Y - position.Y) < GRID_SIZE - 2 && vege.position.X - position.X > 0)
                    {
                        vege.state = Vegetable.MOVING;
                        vege.leftPushers.AddRange(pushers);
                        vegetableRight(vege.position, pushers, Vegetable.NUDGE_SPACING);
                        moleRight(vege, spacing);
                        return true;
                    }
                }
            }
            return false;
        }

        private void moleRight(Vegetable vegetable, float spacing)
        {
            if (mole.position.X - vegetable.position.X < GRID_SIZE - spacing
                && Math.Abs(mole.position.Y - vegetable.position.Y) < GRID_SIZE - 2
                && mole.position.X - vegetable.position.X > 0
                && !vegetable.rightPushers.Contains(mole))
            {
                vegetable.rightPushers.Add(mole);
            }

            foreach (Rat enemy in enemies)
            {
                if (enemy.position.X - vegetable.position.X < GRID_SIZE - spacing
                && Math.Abs(enemy.position.Y - vegetable.position.Y) < GRID_SIZE - 2
                && enemy.position.X - vegetable.position.X > 0
                && !vegetable.rightPushers.Contains(enemy))
                {
                    vegetable.rightPushers.Add(enemy);
                }
            }
        }

        internal bool vegetableLeft(Vector2 position, ArrayList pushers, float spacing)
        {
            foreach (Vegetable vege in vegetables)
            {
                if ((vege.state == Vegetable.NONE || vege.state == Vegetable.MOVING) && position != vege.position)
                {
                    if (position.X - vege.position.X < GRID_SIZE - spacing && Math.Abs(vege.position.Y - position.Y) < GRID_SIZE - 2 && position.X - vege.position.X > 0)
                    {
                        vege.state = Vegetable.MOVING;
                        vege.rightPushers.AddRange(pushers);
                        vegetableLeft(vege.position, pushers, Vegetable.NUDGE_SPACING);
                        moleLeft(vege, spacing);
                        return true;
                    }
                }
            }
            return false;
        }

        private void moleLeft(Vegetable vegetable, float spacing)
        {
            if (vegetable.position.X - mole.position.X < GRID_SIZE - spacing
                && Math.Abs(vegetable.position.Y - mole.position.Y) < GRID_SIZE - 2
                && vegetable.position.X - mole.position.X > 0
                && !vegetable.leftPushers.Contains(mole))
            {
                vegetable.leftPushers.Add(mole);
            }

            foreach (Rat enemy in enemies)
            {
                if (vegetable.position.X - enemy.position.X < GRID_SIZE - spacing
                && Math.Abs(vegetable.position.Y - enemy.position.Y) < GRID_SIZE - 2
                && vegetable.position.X - enemy.position.X > 0
                && !vegetable.leftPushers.Contains(enemy))
                {
                    vegetable.leftPushers.Add(enemy);
                }
            }
        }

        internal bool vegetableBelow(Mole mole)
        {
            foreach (Vegetable vege in vegetables)
            {
                if (vege.state != Vegetable.SPLITTING)
                {
                    if (Math.Abs(vege.position.X - mole.position.X) < GRID_SIZE - 8 && vege.position.Y - mole.position.Y <= GRID_SIZE && vege.position.Y - mole.position.Y > 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        internal bool vegetableDirectlyBelow(Mole mole)
        {
            foreach (Vegetable vege in vegetables)
            {
                if (vege.state != Vegetable.SPLITTING)
                {
                    if (Math.Abs(vege.position.X - mole.position.X) < GRID_SIZE / 2 - 2 && vege.position.Y - mole.position.Y <= GRID_SIZE && vege.position.Y - mole.position.Y > 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        internal bool vegetableAbove(Mole mole)
        {
            foreach (Vegetable vege in vegetables)
            {
                if (vege.state != Vegetable.SPLITTING)
                {
                    if (Math.Abs(vege.position.X - mole.position.X) < GRID_SIZE - 8 && mole.position.Y - vege.position.Y <= GRID_SIZE && mole.position.Y - vege.position.Y > 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        internal bool vegetableFallingAbove(Mole mole)
        {
            foreach (Vegetable vege in vegetables)
            {
                if (vege.state == Vegetable.FALLING)
                {
                    Vector2 absPos = (mole.position - vege.position);
                    if (Math.Abs(absPos.X) < GRID_SIZE / 2 && mole.position.Y - vege.position.Y <= GRID_SIZE && mole.position.Y - vege.position.Y > 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void checkCollisions(Vegetable vege)
        {
            int middleX = ((int)vege.position.X) / GRID_SIZE;
            int bottomY = ((int)vege.position.Y + GRID_SIZE / 2) / GRID_SIZE;

            if (vege.squashing)
            {
                bottomY = ((int)vege.position.Y + GRID_SIZE * 3 / 4) / GRID_SIZE;
            }

            if (bottomY >= GRID_HEIGHT)
            {
                vege.split();
            }
            else if (tunnels[middleX, bottomY].left != Tunnel.DUG && tunnels[middleX, bottomY].right != Tunnel.DUG
                && tunnels[middleX, bottomY].top != Tunnel.DUG && tunnels[middleX, bottomY].bottom != Tunnel.DUG)
            {
                if (tunnels[middleX, bottomY].top != Tunnel.HALF_DUG)
                {
                    if ((int)vege.position.Y - vege.fallingFrom > GRID_SIZE || vege.gonnaBreak)
                    {
                        vege.split();
                    }
                    else
                    {
                        vege.land();
                    }
                }
            }
            else
            {
                tunnels[middleX, bottomY].top = Tunnel.DUG;
                tunnels[middleX, bottomY - 1].bottom = Tunnel.DUG;
                revealTunnels();
            }
        }
        private void getInput(TimeSpan timeSpan)
        {
            keyboardState = Keyboard.GetState();
            gamePadState = GamePad.GetState(PlayerIndex.One);

            if (!firstCheck)
            {
                // Allows the game to exit
                if (keyboardState.IsKeyDown(Keys.Escape) || GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                    _game.Exit();

                if (mole.alive() && (mole.state & Mole.STATE_USE) == 0)
                {
                    if ((keyboardState.IsKeyDown(Keys.Z) && previousKeyboardState.IsKeyUp(Keys.Z)) || (gamePadState.IsButtonDown(Buttons.A) && previousGamePadState.IsButtonUp(Buttons.A)))
                    {
                        mole.stopMoving();
                        mole.useItem(0);
                    }
                    else if ((keyboardState.IsKeyDown(Keys.X) && previousKeyboardState.IsKeyUp(Keys.X)) || (gamePadState.IsButtonDown(Buttons.X) && previousGamePadState.IsButtonUp(Buttons.X)))
                    {
                        mole.stopMoving();
                        mole.useItem(1);
                    }

                    else
                    {

                        if (keyboardState.IsKeyDown(Keys.Left) || gamePadState.IsButtonDown(Buttons.DPadLeft) || gamePadState.ThumbSticks.Left.X < 0)
                        {
                            mole.moveLeft();
                        }
                        else if (keyboardState.IsKeyDown(Keys.Right) || gamePadState.IsButtonDown(Buttons.DPadRight) || gamePadState.ThumbSticks.Left.X > 0)
                        {
                            mole.moveRight();
                        }
                        else if (keyboardState.IsKeyDown(Keys.Down) || gamePadState.IsButtonDown(Buttons.DPadDown) || gamePadState.ThumbSticks.Left.Y < 0)
                        {
                            mole.moveDown();
                        }
                        else if (keyboardState.IsKeyDown(Keys.Up) || gamePadState.IsButtonDown(Buttons.DPadUp) || gamePadState.ThumbSticks.Left.Y > 0)
                        {
                            mole.moveUp();
                        }
                        else
                        {
                            mole.stopMoving();
                        }
                    }
                }

                if (keyboardState.IsKeyDown(Keys.Q) && previousKeyboardState.IsKeyUp(Keys.Q))
                {
                    enemies.Add(new Rat(this));
                }
                else if (keyboardState.IsKeyDown(Keys.R) && previousKeyboardState.IsKeyUp(Keys.R))
                {
                    init();
                }
            }
            else
            {
                firstCheck = false;
            }

            previousKeyboardState = keyboardState;
            previousGamePadState = gamePadState;
        }

        private void init()
        {
            tunnels = new Tunnel[GRID_WIDTH, GRID_HEIGHT];
            for (int j = 0; j < GRID_HEIGHT; j++)
            {
                for (int i = 0; i < GRID_WIDTH; i++)
                {
                    tunnels[i, j] = new Tunnel(i * GRID_SIZE, j * GRID_SIZE);
                }
            }

            vegetables = new ArrayList(5);
            pickups = new ArrayList(40);
            deadStuff = new ArrayList(5);
            enemies = new ArrayList(10);
            stones = new ArrayList(10);
            effects = new ArrayList(5);
            items = new ArrayList(5);
            for (int i = 0; i < 20; i++)
            {
                effects.Add(new Animation(AnimationType.tunnelReveal));
            }
            effects.Add(new Animation(AnimationType.stoneImpact));
            effects.Add(new Animation(AnimationType.fightCloud));
            effects.Add(new Animation(AnimationType.hookImpact));

            generateLevel();

            mole.prevMoleLeft = ((int)mole.position.X - GRID_SIZE / 4) / GRID_SIZE;
            mole.prevMoleRight = ((int)mole.position.X + GRID_SIZE / 4) / GRID_SIZE;
            mole.prevMoleUp = ((int)mole.position.Y - GRID_SIZE / 4) / GRID_SIZE;
            mole.prevMoleDown = ((int)mole.position.Y + GRID_SIZE / 4) / GRID_SIZE;

            foreach (Mole mole in enemies)
            {
                mole.prevMoleLeft = ((int)mole.position.X - GRID_SIZE / 4) / GRID_SIZE;
                mole.prevMoleRight = ((int)mole.position.X + GRID_SIZE / 4) / GRID_SIZE;
                mole.prevMoleUp = ((int)mole.position.Y - GRID_SIZE / 4) / GRID_SIZE;
                mole.prevMoleDown = ((int)mole.position.Y + GRID_SIZE / 4) / GRID_SIZE;
            }

            pickupTime = 22f / mole.getDigSpeed();
            pickupTimer = -1;
            enemyTimer = ENEMY_TIME;
        }

        private void generateLevel()
        {
            const int generations = 2;
            int tunnelId = 1;
            int[][,] generatedTunnels = new int[generations][,];
            combinedTunnels = new int[GRID_WIDTH, GRID_HEIGHT];
            ArrayList vegetablePlacements = new ArrayList(10);
            ArrayList pickupClusters = new ArrayList(20);

            //generate
            for (int i = 0; i < generations; i++)
            {
                switch (random.Next(7))
                {
                    case 0:
                    case 1:
                        generatedTunnels[i] = generateVerticalLine(tunnelId);
                        break;
                    case 2:
                    case 3:
                        generatedTunnels[i] = generateHorizontalLine(tunnelId);
                        break;
                    case 4:
                        generatedTunnels[i] = generateRoom(tunnelId);
                        break;
                    default:
                        generatedTunnels[i] = generateLoop(tunnelId);
                        break;
                }
                if (random.Next(3) == 0)
                {
                    tunnelId++;
                }
            }

            for (int k = 0; k < generations; k++)
            {
                for (int j = 0; j < GRID_HEIGHT; j++)
                {
                    for (int i = 0; i < GRID_WIDTH; i++)
                    {
                        if (generatedTunnels[k][i, j] != 0)
                        {
                            combinedTunnels[i, j] = generatedTunnels[k][i, j];
                        }
                    }
                }
            }

            //translate to the actual Tunnel array
            for (int j = 0; j < GRID_HEIGHT; j++)
            {
                for (int i = 0; i < GRID_WIDTH; i++)
                {
                    if (combinedTunnels[i, j] != 0)
                    {
                        tunnels[i, j].starting = true;
                        //dig tunnels
                        if (i > 0 && combinedTunnels[i - 1, j] == combinedTunnels[i, j])
                        {
                            tunnels[i, j].left = Tunnel.DUG;
                        }
                        if (i < GRID_WIDTH - 1 && combinedTunnels[i + 1, j] == combinedTunnels[i, j])
                        {
                            tunnels[i, j].right = Tunnel.DUG;
                        }
                        if (j > 0 && combinedTunnels[i, j - 1] == combinedTunnels[i, j])
                        {
                            tunnels[i, j].top = Tunnel.DUG;
                        }
                        if (j < GRID_HEIGHT - 1 && combinedTunnels[i, j + 1] == combinedTunnels[i, j])
                        {
                            tunnels[i, j].bottom = Tunnel.DUG;
                        }
                    }
                    else
                    {
                        //populate possible position vegetable array
                        if (j < GRID_HEIGHT - 2 && j > 0 && combinedTunnels[i, j + 1] == 0 && i > 0 && i < GRID_WIDTH - 1)
                        {
                            bool vegetableVertical = false;
                            foreach (Point point in vegetablePlacements)
                            {
                                if (point.X == i && Math.Abs(point.Y - j) < 2)
                                {
                                    vegetableVertical = true;
                                }
                            }
                            if (!vegetableVertical)
                            {
                                Point addedPoint = new Point(i, j);
                                vegetablePlacements.Add(addedPoint);
                                if (combinedTunnels[i + 1, j] == 0 && combinedTunnels[i - 1, j] == 0 && combinedTunnels[i, j + 2] == 0)
                                {
                                    if (random.Next(5) != 0)
                                    {
                                        vegetablePlacements.Remove(addedPoint);
                                    }
                                }
                                else if (j > GRID_HEIGHT * 0.5f)
                                {
                                    if (random.Next(2) != 0)
                                    {
                                        vegetablePlacements.Remove(addedPoint);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            ArrayList possibleSpawns = new ArrayList(GRID_WIDTH * GRID_HEIGHT);

            for (int j = 0; j < GRID_HEIGHT; j++)
            {
                for (int i = 0; i < GRID_WIDTH; i++)
                {
                    if (combinedTunnels[i, j] != 0)
                    {
                        if (i > 0 && combinedTunnels[i, j] == combinedTunnels[i - 1, j])
                        {
                            possibleSpawns.Add(new Point(i, j));
                        }
                        else if (i < GRID_WIDTH - 1 && combinedTunnels[i, j] == combinedTunnels[i + 1, j])
                        {
                            possibleSpawns.Add(new Point(i, j));
                        }
                        else if (j > 0 && combinedTunnels[i, j] == combinedTunnels[i, j - 1])
                        {
                            possibleSpawns.Add(new Point(i, j));
                        }
                        else if (j < GRID_HEIGHT - 1 && combinedTunnels[i, j] == combinedTunnels[i, j + 1])
                        {
                            possibleSpawns.Add(new Point(i, j));
                        }
                    }
                }
            }

            ArrayList narrowDownSpawns = new ArrayList(GRID_WIDTH * GRID_HEIGHT);
            int distance = 100;
            int xDist;
            int yDist;

            foreach (Point point in possibleSpawns)
            {
                xDist = point.X;
                yDist = point.Y;
                if (point.X > GRID_WIDTH * 0.5f)
                {
                    xDist = GRID_WIDTH - point.X;
                }
                if (point.Y > GRID_HEIGHT * 0.5f)
                {
                    xDist = GRID_HEIGHT - point.Y;
                }

                if (xDist < distance)
                {
                    distance = xDist;
                }
                if (yDist < distance)
                {
                    distance = yDist;
                }
            }

            foreach (Point point in possibleSpawns)
            {
                xDist = point.X;
                yDist = point.Y;
                if (point.X > GRID_WIDTH * 0.5f)
                {
                    xDist = GRID_WIDTH - point.X;
                }
                if (point.Y > GRID_HEIGHT * 0.5f)
                {
                    xDist = GRID_HEIGHT - point.Y;
                }

                if (xDist == distance || yDist == distance)
                {
                    narrowDownSpawns.Add(point);
                }
            }

            Point point2 = (Point)narrowDownSpawns[random.Next(narrowDownSpawns.Count)];

            mole = new Mole(point2.X, point2.Y, this);

            //remove all tunnel identities so they can all just be '1' for "tunnel" for the rest of the proc gen
            for (int j = 0; j < GRID_HEIGHT; j++)
            {
                for (int i = 0; i < GRID_WIDTH; i++)
                {
                    if (combinedTunnels[i, j] != 0 &&
                        (tunnels[i, j].left == Tunnel.DUG ||
                        tunnels[i, j].right == Tunnel.DUG ||
                        tunnels[i, j].top == Tunnel.DUG ||
                        tunnels[i, j].bottom == Tunnel.DUG))
                    {
                        combinedTunnels[i, j] = 1;
                    }
                    else
                    {
                        combinedTunnels[i, j] = 0;
                    }
                }
            }

            //place vegetables
            int totalVegetables = random.Next(4, 6);
            if (vegetablePlacements.Count < totalVegetables)
            {
                totalVegetables = vegetablePlacements.Count;
            }

            for (int i = 0; i < totalVegetables; i++)
            {
                int vegetableSpotIndex = random.Next(vegetablePlacements.Count);
                Point vegetableSpot = (Point)vegetablePlacements[vegetableSpotIndex];
                vegetables.Add(new Vegetable(vegetableSpot.X * GRID_SIZE + GRID_SIZE * 0.5f, vegetableSpot.Y * GRID_SIZE + GRID_SIZE * 0.5f, this));
                vegetablePlacements.RemoveAt(vegetableSpotIndex);
                combinedTunnels[vegetableSpot.X, vegetableSpot.Y] = 2; // 2 for vegetable
            }

            //populate grub
            for (int j = 0; j < GRID_HEIGHT; j++)
            {
                for (int i = 0; i < GRID_WIDTH; i++)
                {
                    if (combinedTunnels[i, j] == 0)
                    {
                        ArrayList paths = new ArrayList();
                        generatePickupClusters(paths, new ArrayList(), i, j);
                        if (paths.Count > 0)
                        {
                            pickupClusters.Add(paths[random.Next(paths.Count)]);
                        }
                    }
                }
            }

            if (pickupClusters.Count < 3)
            {
                //level couldn't populate any pickup clusters(very rare), in future might make this cause something interesting, but for now just redo generation
                init();
            }
            else
            {
                for (int i = 0; i < 3; i++)
                {
                    int pickupClusterIndex = random.Next(pickupClusters.Count);
                    ArrayList pickupCluster = (ArrayList)pickupClusters[pickupClusterIndex];
                    pickupClusters.RemoveAt(pickupClusterIndex);
                    foreach (Point point in pickupCluster)
                    {
                        pickups.Add(new Grub(point.X, point.Y, this));
                    }
                }

                placeEnemies();
            }

            //reveal starting tunnel to player
            revealTunnels(true);
        }

        private int[,] generateRoom(int tunnelId)
        {
            int[,] generatedTunnels = new int[GRID_WIDTH, GRID_HEIGHT];

            int width = random.Next(4, GRID_WIDTH / 2);
            int height = random.Next(4, GRID_HEIGHT / 2);

            int x = random.Next(0, GRID_WIDTH - width);
            int y = random.Next(0, GRID_HEIGHT - height);

            int notchChance = random.Next(2, 10);
            int holeChance = random.Next(4, 10);

            for (int i = x; i < x + width; i++)
            {
                for (int j = y; j < y + height; j++)
                {
                    if ((i == 0 || i == x + width - 1 || j == 0 || j == y + height - 1) && random.Next(notchChance) != 0)
                    {
                        generatedTunnels[i, j] = tunnelId;
                    }
                    else if (random.Next(holeChance) != 0)
                    {
                        generatedTunnels[i, j] = tunnelId;
                    }
                    if (random.Next(random.Next(10, 30)) == 0)
                    {
                        tunnelId++;
                    }
                }
            }

            return generatedTunnels;
        }

        public void revealTunnels(bool start = false)
        {
            if (checkedTunnels == null)
            {
                checkedTunnels = new ArrayList(GRID_WIDTH * GRID_HEIGHT);
                tunnelsToReveal = new ArrayList[GRID_HEIGHT];
                for (int i = 0; i < GRID_HEIGHT; i++)
                {
                    tunnelsToReveal[i] = new ArrayList(GRID_WIDTH);
                }
            }
            else
            {
                checkedTunnels.Clear();
                for (int i = 0; i < GRID_HEIGHT; i++)
                {
                    tunnelsToReveal[i].Clear();
                }
            }

            revealTunnels(start, (int)mole.position.X / GRID_SIZE, (int)mole.position.Y / GRID_SIZE);

            int tracker = 0;
            for (int i = 0; i < GRID_HEIGHT; i++)
            {
                if (tunnelsToReveal[i].Count > 0)
                {
                    foreach (Tunnel tunnel in tunnelsToReveal[i])
                    {
                        createAnimation(tunnel.position + Tunnel.center, 0, 0, AnimationType.tunnelReveal); //tracker * Animation.REVEAL_TIME);
                    }
                    //tracker++;
                }
            }
        }

        private ArrayList[] tunnelsToReveal;

        private void revealTunnels(bool start, int x, int y)
        {
            if (!start && tunnels[x, y].seen != SeenStatus.SEEN && tunnels[x, y].starting)
            {
                tunnelsToReveal[y].Add(tunnels[x, y]);
            }
            tunnels[x, y].seen = SeenStatus.SEEN;
            tunnels[x, y].revealed = true;
            checkedTunnels.Add(tunnels[x, y]);

            if (x > 0 && tunnels[x, y].left == Tunnel.DUG && !checkedTunnels.Contains(tunnels[x - 1, y]))
            {
                revealTunnels(start, x - 1, y);
            }
            if (x < GRID_WIDTH - 1 && tunnels[x, y].right == Tunnel.DUG && !checkedTunnels.Contains(tunnels[x + 1, y]))
            {
                revealTunnels(start, x + 1, y);
            }
            if (y > 0 && tunnels[x, y].top == Tunnel.DUG && !checkedTunnels.Contains(tunnels[x, y - 1]))
            {
                revealTunnels(start, x, y - 1);
            }
            if (y < GRID_HEIGHT - 1 && tunnels[x, y].bottom == Tunnel.DUG && !checkedTunnels.Contains(tunnels[x, y + 1]))
            {
                revealTunnels(start, x, y + 1);
            }
        }

        ArrayList checkedTunnels;

        private void placeEnemies()
        {
            ArrayList[] tunnelBuckets = new ArrayList[10];
            for (int j = 0; j < 10; j++)
            {
                tunnelBuckets[j] = new ArrayList();
            }

            //fill tunnel buckets
            for (int j = 0; j < GRID_HEIGHT; j++)
            {
                for (int i = 0; i < GRID_WIDTH; i++)
                {
                    int adjacent = 0;
                    float debug = (mole.position - new Vector2(i * GRID_SIZE, j * GRID_SIZE)).Length();

                    if ((mole.position - new Vector2(i * GRID_SIZE, j * GRID_SIZE)).Length() > 80)
                    {

                        if (combinedTunnels[i, j] == 1)
                        {
                            adjacent++;
                            if (i > 0 && combinedTunnels[i - 1, j] == 1)
                            {
                                adjacent++;
                                if (j > 0 && combinedTunnels[i - 1, j - 1] == 1)
                                {
                                    adjacent++;
                                }
                                if (j < GRID_HEIGHT - 1 && combinedTunnels[i - 1, j + 1] == 1)
                                {
                                    adjacent++;
                                }
                            }
                            if (i < GRID_WIDTH - 1 && combinedTunnels[i + 1, j] == 1)
                            {
                                adjacent++;
                                if (j > 0 && combinedTunnels[i + 1, j - 1] == 1)
                                {
                                    adjacent++;
                                }
                                if (j < GRID_HEIGHT - 1 && combinedTunnels[i + 1, j + 1] == 1)
                                {
                                    adjacent++;
                                }
                            }
                            if (j > 0 && combinedTunnels[i, j - 1] == 1)
                            {
                                adjacent++;
                            }
                            if (j < GRID_HEIGHT - 1 && combinedTunnels[i, j + 1] == 1)
                            {
                                adjacent++;
                            }
                        }
                    }
                    tunnelBuckets[adjacent].Add(new Point(i, j));
                }
            }

            //place rats
            enemyCount = random.Next(1, 4);
            //for (int i = 0; i < enemyCount; i++)
            //{
            int index = 7;
            ArrayList possibleSpots = new ArrayList();
            while (possibleSpots.Count < 5)
            {
                if (tunnelBuckets[index].Count > 0)
                {
                    int chosen = random.Next(tunnelBuckets[index].Count);
                    possibleSpots.Add(tunnelBuckets[index][chosen]);
                    tunnelBuckets[index].RemoveAt(chosen);
                }
                else
                {
                    index--;
                }
            }
            Point chosenPoint = (Point)possibleSpots[random.Next(possibleSpots.Count)];
            door = new Door(chosenPoint.X, chosenPoint.Y, this);
            //enemies.Add(new Rat(this, chosenPoint.X, chosenPoint.Y));
            //}

        }

        private void generatePickupClusters(ArrayList paths, ArrayList path, int i, int j)
        {
            if (combinedTunnels[i, j] == 0)
            {
                path.Add(new Point(i, j));
                combinedTunnels[i, j] = 3;
                if (path.Count == 8)
                {
                    if (checkForTwoRoutes(path))
                    {
                        paths.Add(path);
                    }
                }
                else
                {
                    if (i > 0 && combinedTunnels[i - 1, j] == 0)
                    {
                        generatePickupClusters(paths, new ArrayList(path), i - 1, j);
                    }
                    if (i < GRID_WIDTH - 1 && combinedTunnels[i + 1, j] == 0)
                    {
                        generatePickupClusters(paths, new ArrayList(path), i + 1, j);
                    }
                    if (j > 0 && combinedTunnels[i, j - 1] == 0)
                    {
                        generatePickupClusters(paths, new ArrayList(path), i, j - 1);
                    }
                    if (j < GRID_HEIGHT - 1 && combinedTunnels[i, j + 1] == 0)
                    {
                        generatePickupClusters(paths, new ArrayList(path), i, j + 1);
                    }
                }
            }
        }

        private bool checkForTwoRoutes(ArrayList path)
        {
            for (int i = 0; i < path.Count; i++)
            {
                Point checkThisPoint = (Point)path[i];
                int tally = 0;

                foreach (Point point in path)
                {
                    if (path.IndexOf(point) != i && Math.Abs(checkThisPoint.X - point.X) <= 1 && Math.Abs(checkThisPoint.Y - point.Y) <= 1)
                    {
                        tally++;
                    }
                }
                if (tally < 3)
                {
                    return false;
                }
            }
            return true;
        }

        private int[,] generateHorizontalLine(int id)
        {
            int[,] generatedTunnels = new int[GRID_WIDTH, GRID_HEIGHT];
            int currX = 0;
            int currY = random.Next(GRID_HEIGHT);
            int result = 0;
            int lastResult;

            while (currX < GRID_WIDTH && generatedTunnels[currX, currY] != id)
            {
                generatedTunnels[currX, currY] = id;
                lastResult = result;
                result = random.Next(6);
                switch (result)
                {
                    case 0:
                        if (currY > 0 && generatedTunnels[currX, currY - 1] != id && lastResult != id)
                        {
                            currY--;
                        }
                        else
                        {
                            currX++;
                        }
                        break;
                    case 1:
                        if (currY < GRID_HEIGHT - 1 && generatedTunnels[currX, currY + 1] != id && lastResult != 0)
                        {
                            currY++;
                        }
                        else
                        {
                            currX++;
                        }
                        break;
                    default:
                        currX++;
                        break;
                }
                if (random.Next(random.Next(10, 30)) == 0)
                {
                    id++;
                }
            }
            return generatedTunnels;
        }

        private int[,] generateLoop(int id)
        {
            int[,] generatedTunnels = new int[GRID_WIDTH, GRID_HEIGHT];

            int radiusX = random.Next(3, (GRID_WIDTH + GRID_HEIGHT) / 4);
            int radiusY = random.Next(3, (GRID_WIDTH + GRID_HEIGHT) / 4);

            int currX = random.Next(radiusX, GRID_WIDTH - radiusX);
            int initialX = currX;
            int currY = random.Next(0, GRID_HEIGHT - radiusY * 2); ;
            int initial = currY;

            while (currX - initialX < radiusX)
            {
                generatedTunnels[currX, currY] = id;

                int downPaths = (int)(((float)currX - (float)initialX) * ((float)radiusY / (float)(radiusX + 1)));
                for (int j = 0; j < downPaths; j++)
                {
                    currY++;
                    if (currY >= GRID_HEIGHT)
                    {
                        currY = GRID_HEIGHT - 1;
                    }
                    generatedTunnels[currX, currY] = id;
                }
                currX++;
                if (currX >= GRID_WIDTH)
                {
                    currX = GRID_WIDTH - 1;
                }
            }

            while (currX - initialX > 0)
            {
                generatedTunnels[currX, currY] = id;

                int downPaths = (int)(((float)currX - (float)initialX) * ((float)radiusY / (float)(radiusX + 1)));
                for (int j = 0; j < downPaths; j++)
                {
                    currY++;
                    if (currY >= GRID_HEIGHT)
                    {
                        currY = GRID_HEIGHT - 1;
                    }
                    generatedTunnels[currX, currY] = id;
                }
                currX--;
                if (currX < 0)
                {
                    currX = 0;
                }
            }

            while (currX - initialX > -radiusX)
            {
                generatedTunnels[currX, currY] = id;

                int downPaths = (int)(((float)currX - (float)initialX) * -1 * ((float)radiusY / (float)(radiusX + 1)));
                for (int j = 0; j < downPaths; j++)
                {
                    currY--;
                    if (currY < 0)
                    {
                        currY = 0;
                    }
                    generatedTunnels[currX, currY] = id;
                }
                currX--;
                if (currX < 0)
                {
                    currX = 0;
                }
            }

            while (currX - initialX < 0)
            {
                generatedTunnels[currX, currY] = id;

                int downPaths = (int)(((float)currX - (float)initialX) * -1 * ((float)radiusY / (float)(radiusX + 1)));
                for (int j = 0; j < downPaths; j++)
                {
                    currY--;
                    if (currY < 0)
                    {
                        currY = 0;
                    }
                    generatedTunnels[currX, currY] = id;
                }
                currX++;
                if (currX >= GRID_WIDTH)
                {
                    currX = GRID_WIDTH - 1;
                }
            }

            if (random.Next(random.Next(10, 30)) == 0)
            {
                id++;
            }
            return generatedTunnels;
        }

        private int[,] generateVerticalLine(int id)
        {
            int[,] generatedTunnels = new int[GRID_WIDTH, GRID_HEIGHT];
            int currX = random.Next(GRID_WIDTH);
            int currY = 0;
            int result = 0;
            int lastResult;

            while (currY < GRID_HEIGHT && generatedTunnels[currX, currY] != id)
            {
                generatedTunnels[currX, currY] = id;
                lastResult = result;
                result = random.Next(6);
                switch (result)
                {
                    case 0:
                        if (currX > 0 && generatedTunnels[currX - 1, currY] != id && lastResult != id)
                        {
                            currX--;
                        }
                        else
                        {
                            currY++;
                        }
                        break;
                    case 1:
                        if (currX < GRID_WIDTH - 1 && generatedTunnels[currX + 1, currY] != id && lastResult != 0)
                        {
                            currX++;
                        }
                        else
                        {
                            currY++;
                        }
                        break;
                    default:
                        currY++;
                        break;
                }
                if (random.Next(random.Next(10, 30)) == 0)
                {
                    id++;
                }
            }
            return generatedTunnels;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PikeAndShot
{
    public class Spawner
    {
        LevelScreen screen;
        EnemyFormation formation;
        float spawnTime;
        float spawnTimer;
        EnemyFormation dependantFormation; //when this formation dies, the spawner dies too

        public bool dead;

        public Spawner(LevelScreen screen, EnemyFormation formation, float spawnTime, EnemyFormation dependantFormation)
        {
            this.screen = screen;
            this.formation = formation;
            this.spawnTime = spawnTime;
            this.dependantFormation = dependantFormation;
            spawnTimer = 0; // start the spawner ready to spawn
            dead = false;
        }

        public void update (TimeSpan timeSpan)
        {
            if (screen.getMapOffset().X + PikeAndShotGame.SCREENWIDTH > formation.getPosition().X)
                dead = true;
            else if(dependantFormation != null && !findDependantFormation(screen._enemyFormations))
                dead = true;
            else{
                spawnTimer -= (float)timeSpan.TotalMilliseconds;
                if (spawnTimer <= 0f)
                {
                    EnemyFormation newFormation = new EnemyFormation(formation.name = " spawn", formation._pattern, screen, formation.getPosition().X, formation.getPosition().Y, 10, formation.getSide());
                    spawnTimer = spawnTime;
                    screen._enemyFormations.Add(newFormation);
                    foreach (Soldier s in formation.getSoldiers())
                        Soldier.getNewSoldier(s.getClass(), screen, newFormation, newFormation.getPosition().X, newFormation.getPosition().Y);
                }
            }
        }

        private bool findDependantFormation(System.Collections.ArrayList arrayList)
        {
            foreach (EnemyFormation ef in arrayList)
            {
                if (ef.name.Equals(dependantFormation.name))
                    return true;
            }
            return false;
        }
    }
}

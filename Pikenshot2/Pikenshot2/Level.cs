using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace PikeAndShot
{
    public class Level
    {
        public string levelName;
        public List<List<int>> formations;
        public List<Vector2> formationPositions;
        public List<double> formationTimes; // times is gonna be spawn points
        public List<List<PatternAction>> formationActions;
        public List<int> terrains;
        public List<Vector2> terrainPositions;
        public List<double> terrainTimes;

        public List<string> formationNames;
        public List<List<string>> formationActionNames;

        public List<int> formationSides;

        public float startingPosition = 0;

        public Level()
        {
            formations = new List<List<int>>(10);
            formationPositions = new List<Vector2>(10);
            formationTimes = new List<double>(10);
            formationActions = new List<List<PatternAction>>(10);

            formationNames = new List<string>(32);
            formationActionNames = new List<List<string>>(32);

            terrains = new List<int>(20);
            terrainPositions = new List<Vector2>(20);
            terrainTimes = new List<double>(20);
            formationSides = new List<int>(32);
        }

        public Level(LevelPipeline.LevelContent content)
        {
            levelName = content.levelName;
            formations = content.formations;
            formationPositions = content.formationPositions;
            formationTimes = content.formationTimes;
            formationSides = content.formationSides;
            formationActions = new List<List<PatternAction>>();
            //List<PatternAction> list;
            //PatternAction patternAction;
            foreach (List<LevelPipeline.PatternActionContent> pac in content.formationActions)
            {
                List<PatternAction> list = new List<PatternAction>();
   
                foreach (LevelPipeline.PatternActionContent pa in pac)
                {
                    PatternAction patternAction = new PatternAction(pa.actions, pa.duration);
                    list.Add(patternAction);
                }
                formationActions.Add(list);
            }
            formationNames = content.formationNames;
            formationActionNames = content.formationActionNames;
            terrains = content.terrains;
            terrainPositions = content.terrainPositions;
            terrainTimes = content.terrainTimes;
            if (terrainTimes != null && terrainPositions != null)
            {
                for (int i = 0; i < terrainTimes.Count; i++)
                {
                    if (terrainPositions.Count > i)
                        terrainTimes[i] = terrainPositions[i].X - 100;
                }
            }
        }
    }

    public class PatternAction
    {
        public const int ACTION_IDLE = 0;
        public const int ACTION_LEFT = 1;
        public const int ACTION_UP = 2;
        public const int ACTION_RIGHT = 3;
        public const int ACTION_DOWN = 4;
        public const int ACTION_PIKE = 5;
        public const int ACTION_RAISE = 6;
        public const int ACTION_SHOT = 7;
        public const int ACTION_CAVALRY_HALT = 8;
        public const int ACTION_CAVALRY_TURN = 9;
        public const int ACTION_CHARGE = 10;
        public const int ACTION_RELOAD = 11;
        public const int ACTION_SPAWN = 12;

        public List<int> actions;  //actions is a list again (a list of ints within a list of Pattern actions) 
                                   //because you could have an overlap of many actions, like marching left and 
                                   //firing simultaneously for instance.
        public double duration;

        public PatternAction()
        {
            actions = new List<int>(5);
        }

        public PatternAction(List<int> actions, double duration)
        {
            this.actions = actions;
            this.duration = duration;
        }

        public void addAction(int i)
        {
            actions.Add(i);
        }

        public int count()
        {
            return this.actions.Count;
        }

        public PatternAction copy()
        {
            PatternAction copy = new PatternAction();
            copy.duration = duration;

            foreach (int action in actions)
                copy.addAction(action);

            return copy;
        }
    }
}

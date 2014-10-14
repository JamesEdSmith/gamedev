using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace LevelPipeline
{
    public class LevelContent
    {
       public string levelName;
        public List<List<int>> formations;
        public List<Vector2> formationPositions;
        public List<double> formationTimes;
        public List<int> formationSides;
        public List<List<PatternActionContent>> formationActions;
        public List<int> terrains;
        public List<Vector2> terrainPositions;
        public List<double> terrainTimes;

        public List<string> formationNames;
        public List<List<string>> formationActionNames;        

        public LevelContent(string name)
        {
            levelName = name;
            formations = new List<List<int>>(32);
            formationPositions = new List<Vector2>(32);
            formationTimes = new List<double>(32);
            formationActions = new List<List<PatternActionContent>>(32);

            formationNames = new List<string>(32);
            formationActionNames = new List<List<string>>(32);

            terrains = new List<int>(20);
            terrainPositions = new List<Vector2>(20);
            terrainTimes = new List<double>(20);
            formationSides = new List<int>(32);
        }

        public LevelContent()
        {
            levelName = "The Broderick Downs";
            formations = new List<List<int>>(2);
            List<int> list1 = new List<int>(1);
            list1.Add(1);
            list1.Add(3);
            List<int> list2 = new List<int>(1);
            list2.Add(2);
            list2.Add(3);
            formations.Add(list1);
            formations.Add(list2);

            formationPositions = new List<Vector2>(2);
            formationPositions.Add(new Vector2(400, 400));
            formationPositions.Add(new Vector2(600, 600));

            formationTimes = new List<double>(2);
            formationTimes.Add(1);
            formationTimes.Add(5);

            formationActions = new List<List<PatternActionContent>>(2);
            PatternActionContent action1 = new PatternActionContent();
            PatternActionContent action2 = new PatternActionContent();
            List<PatternActionContent> list3 = new List<PatternActionContent>();
            list3.Add(action1);
            list3.Add(action2);
            List<PatternActionContent> list4 = new List<PatternActionContent>();
            list4.Add(action1);
            list4.Add(action2);
            formationActions.Add(list3);
            formationActions.Add(list4);

            terrains = new List<int>(2);
            terrains.Add(1);
            terrains.Add(2);

            terrainPositions = new List<Vector2>(2);
            terrainPositions.Add(new Vector2(200, 400));
            terrainPositions.Add(new Vector2(600, 100));

            terrainTimes = new List<double>(2);
            terrainTimes.Add(3);
            terrainTimes.Add(7);
        }
    }

    public class PatternActionContent
    {
        public List<int> actions;
        public double duration;

        public PatternActionContent()
        {
            duration = 1000f;
            actions = new List<int>(1);
            actions.Add(1);
        }

        public PatternActionContent(List<int> actions, double duration)
        {
            this.actions = actions;
            this.duration = duration;
        }
    }
}

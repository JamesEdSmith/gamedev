using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;


namespace PikeAndShot
{
    class LevelReader: ContentTypeReader<Level>
        {
        protected override Level Read(ContentReader input, Level existingInstance)
        {
            Level level = new Level();

            level.levelName = input.ReadString();
            level.formations = input.ReadObject<List<List<int>>>();
            level.formationPositions = input.ReadObject<List<Vector2>>();
            level.formationTimes = input.ReadObject<List<double>>();
            level.formationActions = input.ReadObject<List<List<PatternAction>>>();
            level.terrains = input.ReadObject<List<int>>();
            level.terrainPositions = input.ReadObject<List<Vector2>>();
            level.terrainTimes = input.ReadObject<List<double>>();
            level.formationNames = input.ReadObject<List<string>>();
            level.formationActionNames = input.ReadObject<List<List<string>>>();

            return level;
        }
    
    }

    class PatternActionReader : ContentTypeReader<PatternAction>
    {
        protected override PatternAction Read(ContentReader input, PatternAction existingInstance)
        {
            PatternAction action = new PatternAction();

            action.actions = input.ReadObject<List<int>>();
            action.duration = input.ReadDouble();

            return action;
        }

    }
}

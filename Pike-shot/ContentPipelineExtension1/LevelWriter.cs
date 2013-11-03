using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;

namespace LevelPipeline
{
    /// <summary>
    /// Content Pipeline class for saving ParticleSettings data into XNB format.
    /// </summary>
    [ContentTypeWriter]
    class LevelWriter : ContentTypeWriter<LevelContent>
    {
        protected override void Write(ContentWriter output,
                                      LevelContent value)
        {
            output.Write(value.levelName);
            output.WriteObject(value.formations);
            output.WriteObject(value.formationPositions);
            output.WriteObject(value.formationTimes);
            output.WriteObject(value.formationActions);
            output.WriteObject(value.terrains);
            output.WriteObject(value.terrainPositions);
            output.WriteObject(value.terrainTimes);
            output.WriteObject(value.formationNames);
            output.WriteObject(value.formationActionNames);
        }

        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            return "PikeAndShot.Level, PikeAndShot";
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return "PikeAndShot.LevelReader, PikeAndShot";
        }
    }

    [ContentTypeWriter]
    class PatternActionWriter : ContentTypeWriter<PatternActionContent>
    {
        protected override void Write(ContentWriter output,
                                      PatternActionContent value)
        {
            output.WriteObject(value.actions);
            output.Write(value.duration);
        }

        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            return "PikeAndShot.PatternAction, PikeAndShot";
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return "PikeAndShot.PatternActionReader, PikeAndShot";
        }
    }
}


using System.Collections.Generic;
using PipeSystem;
using Verse;

namespace Neutroflow
{
    public class CompProperties_NeutroamineConsumer : CompProperties_Resource
    {
        public CompProperties_NeutroamineConsumer()
        {
            compClass = typeof(CompNeutroamineConsumer);
        }

        public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
        {
            foreach (var err in base.ConfigErrors(parentDef))
                yield return err;

            if (parentDef.GetModExtension<NeutroamineUsageExtension>() == null)
                yield return $"Neutroflow: {parentDef.defName} has {nameof(CompProperties_NeutroamineConsumer)} but no {nameof(NeutroamineUsageExtension)}.";

            var count = 0;
            foreach (var comp in parentDef.comps)
            {
                if (comp is CompProperties_NeutroamineConsumer)
                    count++;
            }
            if (count > 1)
                yield return $"Neutroflow: {parentDef.defName} has more than one {nameof(CompProperties_NeutroamineConsumer)}.";
        }
    }
}

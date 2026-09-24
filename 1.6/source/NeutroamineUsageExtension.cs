using System.Collections.Generic;
using Verse;

namespace Neutroflow
{
    public enum NeutroamineStarvationMode
    {
        Pause,
        Eject
    }

    public class NeutroamineUsageExtension : DefModExtension
    {
        public float idlePerDay;
        public float activePerDay;
        public string activityMode;
        public NeutroamineStarvationMode starvationMode = NeutroamineStarvationMode.Pause;

        private Def parentDef;

        public override void ResolveReferences(Def parentDef)
        {
            base.ResolveReferences(parentDef);
            this.parentDef = parentDef;
        }

        public override IEnumerable<string> ConfigErrors()
        {
            foreach (var err in base.ConfigErrors())
                yield return err;

            var defName = parentDef?.defName ?? "<unknown>";

            if (idlePerDay < 0f)
                yield return $"Neutroflow: {defName} has a negative idlePerDay ({idlePerDay}).";
            if (activePerDay <= 0f)
                yield return $"Neutroflow: {defName} has a non-positive activePerDay ({activePerDay}).";
            if (activePerDay < idlePerDay)
                yield return $"Neutroflow: {defName} has activePerDay ({activePerDay}) lower than idlePerDay ({idlePerDay}).";
            if (activityMode.NullOrEmpty())
                yield return $"Neutroflow: {defName} has no activityMode set.";
        }
    }
}

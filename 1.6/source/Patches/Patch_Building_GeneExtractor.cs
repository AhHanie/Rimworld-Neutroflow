using HarmonyLib;
using RimWorld;
using Verse;

namespace Neutroflow.Patches
{
    [HarmonyPatch(typeof(Building_GeneExtractor), "Tick")]
    internal static class Patch_Building_GeneExtractor_Tick
    {
        private static readonly AccessTools.FieldRef<Building_GeneExtractor, int> TicksRemaining =
            AccessTools.FieldRefAccess<Building_GeneExtractor, int>("ticksRemaining");

        private static void Prefix(Building_GeneExtractor __instance, out int? __state)
        {
            __state = null;

            if (!__instance.Working)
                return;

            var comp = __instance.TryGetComp<CompNeutroamineConsumer>();
            if (comp?.Extension == null || comp.Extension.activityMode != NeutroflowActivityModes.GeneExtractionWorking)
                return;
            if (!comp.IsStarved)
                return;

            __state = TicksRemaining(__instance);
            TicksRemaining(__instance) += 2;
        }

        private static void Postfix(Building_GeneExtractor __instance, int? __state)
        {
            if (__state.HasValue)
                TicksRemaining(__instance) = __state.Value;
        }
    }
}

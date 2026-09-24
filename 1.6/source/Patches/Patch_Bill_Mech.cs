using HarmonyLib;
using RimWorld;
using Verse;

namespace Neutroflow.Patches
{
    [HarmonyPatch(typeof(Bill_Mech), nameof(Bill_Mech.BillTick))]
    internal static class Patch_Bill_Mech_BillTick
    {
        private static bool Prefix(Bill_Mech __instance)
        {
            if (__instance.State != FormingState.Forming)
                return true;

            var gestator = __instance.Gestator;
            if (gestator == null)
                return true;

            var comp = gestator.TryGetComp<CompNeutroamineConsumer>();
            if (comp?.Extension == null || comp.Extension.activityMode != NeutroflowActivityModes.MechGestationForming)
                return true;

            return !comp.IsStarved;
        }
    }
}

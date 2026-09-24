using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace Neutroflow.Patches
{
    internal static class BiosculpterLiquidUtility
    {
        internal static bool IsLiquidStarved(CompBiosculpterPod pod, out float nudge)
        {
            nudge = 0f;
            var comp = pod.parent.TryGetComp<CompNeutroamineConsumer>();
            if (comp?.Extension == null || comp.Extension.activityMode != NeutroflowActivityModes.BiosculpterOccupied)
                return false;
            if (!comp.IsStarved)
                return false;
            nudge = pod.CycleSpeedFactor * 2f + 1f;
            return true;
        }
    }

    [HarmonyPatch(typeof(CompBiosculpterPod), "CompTick")]
    internal static class Patch_CompBiosculpterPod_CompTick
    {
        private static readonly AccessTools.FieldRef<CompBiosculpterPod, float> CurrentCycleTicksRemaining =
            AccessTools.FieldRefAccess<CompBiosculpterPod, float>("currentCycleTicksRemaining");

        private static void Prefix(CompBiosculpterPod __instance, out float? __state)
        {
            __state = null;
            if (__instance.Occupant == null || !__instance.PowerOn)
                return;
            if (!BiosculpterLiquidUtility.IsLiquidStarved(__instance, out var nudge))
                return;

            __state = CurrentCycleTicksRemaining(__instance);
            CurrentCycleTicksRemaining(__instance) += nudge;
        }

        private static void Postfix(CompBiosculpterPod __instance, float? __state)
        {
            if (__state.HasValue)
                CurrentCycleTicksRemaining(__instance) = __state.Value;
        }
    }

    [HarmonyPatch(typeof(CompBiosculpterPod), nameof(CompBiosculpterPod.CanAcceptOnceCycleChosen))]
    internal static class Patch_CompBiosculpterPod_CanAcceptOnceCycleChosen
    {
        private static void Postfix(CompBiosculpterPod __instance, ref bool __result)
        {
            if (!__result)
                return;
            var comp = __instance.parent.TryGetComp<CompNeutroamineConsumer>();
            if (comp?.Extension == null || comp.Extension.activityMode != NeutroflowActivityModes.BiosculpterOccupied)
                return;
            if (comp.IsStarved)
                __result = false;
        }
    }

    [HarmonyPatch(typeof(JobDriver_EnterBiosculpterPod), "MakeNewToils")]
    internal static class Patch_JobDriver_EnterBiosculpterPod_MakeNewToils
    {
        private static void Prefix(JobDriver_EnterBiosculpterPod __instance)
        {
            __instance.FailOn(() => ShouldFail(__instance));
        }

        private static bool ShouldFail(JobDriver_EnterBiosculpterPod driver)
        {
            var pod = driver.job.targetA.Thing;
            var comp = pod?.TryGetComp<CompNeutroamineConsumer>();
            if (comp?.Extension == null || comp.Extension.activityMode != NeutroflowActivityModes.BiosculpterOccupied)
                return false;
            if (!comp.IsStarved)
                return false;

            NeutroamineJobFailureUtility.Notify_JobFailedNoLiquid(driver.pawn, pod);
            return true;
        }
    }
}

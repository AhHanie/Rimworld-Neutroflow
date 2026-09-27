using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace Neutroflow.Patches
{
    [HarmonyPatch]
    internal static class Patch_CompDraincasket_CompTickInterval
    {
        private static bool Prepare() => DraincasketCompat.Loaded && DraincasketCompat.CompTickIntervalMethod != null && DraincasketCompat.HemogenField != null;

        private static MethodBase TargetMethod() => DraincasketCompat.CompTickIntervalMethod;

        private static void Prefix(ThingComp __instance, int delta, out object __state)
        {
            __state = null;

            var parent = __instance.parent;
            if (!parent.IsHashIntervalTick(60000, delta))
                return;

            var neutro = parent.TryGetComp<CompNeutroamineConsumer>();
            if (neutro?.Extension == null || neutro.Extension.activityMode != NeutroflowActivityModes.DraincasketOccupied)
                return;
            if (neutro.HasActiveWork)
                return;

            var current = DraincasketCompat.HemogenField.GetValue(__instance);
            if (current == null)
                return;

            __state = current;
            DraincasketCompat.HemogenField.SetValue(__instance, null);
        }

        private static void Finalizer(ThingComp __instance, object __state)
        {
            if (__state != null)
                DraincasketCompat.HemogenField.SetValue(__instance, __state);
        }
    }

    [HarmonyPatch]
    internal static class Patch_JobDriver_EnterDraincasket_MakeNewToils
    {
        private static bool Prepare() => DraincasketCompat.Loaded && DraincasketCompat.EnterMakeNewToilsMethod != null;

        private static MethodBase TargetMethod() => DraincasketCompat.EnterMakeNewToilsMethod;

        private static void Postfix(JobDriver __instance)
        {
            __instance.FailOn(() => Patch_Draincasket_Shared.ShouldFail(__instance, TargetIndex.A));
        }
    }

    [HarmonyPatch]
    internal static class Patch_JobDriver_CarryToDraincasket_MakeNewToils
    {
        private static bool Prepare() => DraincasketCompat.Loaded && DraincasketCompat.CarryMakeNewToilsMethod != null;

        private static MethodBase TargetMethod() => DraincasketCompat.CarryMakeNewToilsMethod;

        private static void Prefix(JobDriver __instance)
        {
            __instance.FailOn(() => Patch_Draincasket_Shared.ShouldFail(__instance, TargetIndex.B));
        }
    }

    [HarmonyPatch]
    internal static class Patch_CompDraincasket_TryAcceptPawn
    {
        private static bool Prepare() => DraincasketCompat.Loaded && DraincasketCompat.TryAcceptPawnMethod != null && DraincasketCompat.EjectContentsMethod != null;

        private static MethodBase TargetMethod() => DraincasketCompat.TryAcceptPawnMethod;

        private static void Postfix(ThingComp __instance, bool __result)
        {
            if (__result)
                Patch_Draincasket_Shared.EjectIfConfirmedShortage(__instance);
        }
    }

    [HarmonyPatch]
    internal static class Patch_CompDraincasket_InsertPawn
    {
        private static bool Prepare() => DraincasketCompat.Loaded && DraincasketCompat.InsertPawnMethod != null && DraincasketCompat.EjectContentsMethod != null;

        private static MethodBase TargetMethod() => DraincasketCompat.InsertPawnMethod;

        private static void Postfix(ThingComp __instance, bool __result)
        {
            if (__result)
                Patch_Draincasket_Shared.EjectIfConfirmedShortage(__instance);
        }
    }

    internal static class Patch_Draincasket_Shared
    {
        internal static bool ShouldFail(JobDriver driver, TargetIndex casketIndex)
        {
            var thing = driver.job.GetTarget(casketIndex).Thing;
            var comp = thing?.TryGetComp<CompNeutroamineConsumer>();
            if (comp?.Extension == null || comp.Extension.activityMode != NeutroflowActivityModes.DraincasketOccupied)
                return false;
            if (!comp.IsStarved)
                return false;

            NeutroamineJobFailureUtility.Notify_JobFailedNoLiquid(driver.pawn, thing);
            return true;
        }

        internal static void EjectIfConfirmedShortage(ThingComp comp)
        {
            var parent = comp.parent;
            var neutro = parent.TryGetComp<CompNeutroamineConsumer>();
            if (neutro?.Extension == null || neutro.Extension.activityMode != NeutroflowActivityModes.DraincasketOccupied)
                return;
            if (!neutro.HasConfirmedShortage())
                return;

            DraincasketCompat.Eject(comp, parent.Map);
        }
    }
}

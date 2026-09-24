using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace Neutroflow.Patches
{
    [HarmonyPatch]
    internal static class Patch_CompBioPowerPlant_UpdateDesiredPowerOutput
    {
        private static bool Prepare() => BioReactorCompat.Loaded;

        private static MethodBase TargetMethod() => AccessTools.Method(BioReactorCompat.PowerPlantType, "UpdateDesiredPowerOutput");

        private static void Postfix(CompPowerTrader __instance)
        {
            var comp = __instance.parent.TryGetComp<CompNeutroamineConsumer>();
            if (comp?.Extension == null || comp.Extension.activityMode != NeutroflowActivityModes.BioReactorOccupied)
                return;

            if (!comp.HasActiveWork)
                __instance.PowerOutput = 0f;
        }
    }

    [HarmonyPatch]
    internal static class Patch_CompBioRefuelable_CompTick
    {
        private static bool Prepare() => BioReactorCompat.Loaded;

        private static MethodBase TargetMethod() => AccessTools.Method(BioReactorCompat.RefuelableType, "CompTick");

        private static bool Prefix(ThingComp __instance)
        {
            var comp = __instance.parent.TryGetComp<CompNeutroamineConsumer>();
            if (comp?.Extension == null || comp.Extension.activityMode != NeutroflowActivityModes.BioReactorOccupied)
                return true;

            return !comp.IsStarved;
        }
    }

    [HarmonyPatch]
    internal static class Patch_Building_BioReactor_Tick
    {
        private static bool Prepare() => BioReactorCompat.Loaded;

        private static MethodBase TargetMethod() => AccessTools.Method(BioReactorCompat.BuildingType, "Tick");

        private static void Prefix(Building_Casket __instance, out object __state)
        {
            __state = null;

            if (!ShouldFreeze(__instance, out var current))
                return;

            __state = current;
            BioReactorCompat.StateField.SetValue(__instance, BioReactorCompat.EmptyState);
        }

        private static void Postfix(Building_Casket __instance, object __state)
        {
            if (__state != null)
                BioReactorCompat.StateField.SetValue(__instance, __state);
        }

        private static bool ShouldFreeze(Building_Casket casket, out object current)
        {
            current = null;

            var comp = casket.TryGetComp<CompNeutroamineConsumer>();
            if (comp?.Extension == null || comp.Extension.activityMode != NeutroflowActivityModes.BioReactorOccupied)
                return false;
            if (comp.HasActiveWork)
                return false;

            current = BioReactorCompat.StateField.GetValue(casket);
            var name = current.ToString();
            return name == "StartFilling" || name == "HistolysisStating";
        }
    }

    [HarmonyPatch]
    internal static class Patch_JobDriver_EnterBioReactor_MakeNewToils
    {
        private static bool Prepare() => BioReactorCompat.Loaded;

        private static MethodBase TargetMethod() => AccessTools.Method(BioReactorCompat.JobDriverEnterType, "MakeNewToils");

        private static void Postfix(JobDriver __instance)
        {
            __instance.FailOn(() => ShouldFail(__instance));
        }

        private static bool ShouldFail(JobDriver driver)
        {
            if (!(driver.job.targetA.Thing is Building_Casket casket) || !NeutroamineCasketLiquidUtility.IsLiquidStarved(casket))
                return false;

            NeutroamineJobFailureUtility.Notify_JobFailedNoLiquid(driver.pawn, casket);
            return true;
        }
    }

    [HarmonyPatch]
    internal static class Patch_JobDriver_CarryToBioReactor_MakeNewToils
    {
        private static bool Prepare() => BioReactorCompat.Loaded;

        private static MethodBase TargetMethod() => AccessTools.Method(BioReactorCompat.JobDriverCarryType, "MakeNewToils");

        private static void Prefix(JobDriver __instance)
        {
            __instance.FailOn(() => ShouldFail(__instance));
        }

        private static bool ShouldFail(JobDriver driver)
        {
            if (!(driver.job.GetTarget(TargetIndex.B).Thing is Building_Casket casket) || !NeutroamineCasketLiquidUtility.IsLiquidStarved(casket))
                return false;

            NeutroamineJobFailureUtility.Notify_JobFailedNoLiquid(driver.pawn, casket);
            return true;
        }
    }

    [HarmonyPatch]
    internal static class Patch_Building_BioReactor_TryAcceptThing
    {
        private static bool Prepare() => BioReactorCompat.Loaded;

        private static MethodBase TargetMethod() => AccessTools.Method(BioReactorCompat.BuildingType, "TryAcceptThing");

        private static void Postfix(Building_Casket __instance, bool __result)
        {
            if (!__result)
                return;

            var comp = __instance.TryGetComp<CompNeutroamineConsumer>();
            if (comp?.Extension == null || comp.Extension.activityMode != NeutroflowActivityModes.BioReactorOccupied)
                return;
            if (!comp.HasConfirmedShortage())
                return;

            NeutroflowActivityModes.Notify_BioReactorDenied(__instance);
        }
    }
}

using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace Neutroflow.Patches
{
    [HarmonyPatch(typeof(Building_Casket), nameof(Building_Casket.Accepts))]
    internal static class Patch_Building_Casket_Accepts
    {
        private static void Postfix(Building_Casket __instance, ref bool __result)
        {
            if (!__result)
                return;
            if (NeutroamineCasketLiquidUtility.IsLiquidStarved(__instance))
                __result = false;
        }
    }

    [HarmonyPatch(typeof(JobDriver_EnterCryptosleepCasket), "MakeNewToils")]
    internal static class Patch_JobDriver_EnterCryptosleepCasket_MakeNewToils
    {
        private static void Postfix(JobDriver_EnterCryptosleepCasket __instance)
        {
            __instance.FailOn(() => ShouldFail(__instance));
        }

        private static bool ShouldFail(JobDriver_EnterCryptosleepCasket driver)
        {
            if (!(driver.job.targetA.Thing is Building_CryptosleepCasket casket) || !NeutroamineCasketLiquidUtility.IsLiquidStarved(casket))
                return false;

            NeutroamineJobFailureUtility.Notify_JobFailedNoLiquid(driver.pawn, casket);
            return true;
        }
    }

    [HarmonyPatch(typeof(JobDriver_CarryToCryptosleepCasket), "MakeNewToils")]
    internal static class Patch_JobDriver_CarryToCryptosleepCasket_MakeNewToils
    {
        private static void Prefix(JobDriver_CarryToCryptosleepCasket __instance)
        {
            __instance.FailOn(() => ShouldFail(__instance));
        }

        private static bool ShouldFail(JobDriver_CarryToCryptosleepCasket driver)
        {
            if (!(driver.job.GetTarget(TargetIndex.B).Thing is Building_CryptosleepCasket casket) || !NeutroamineCasketLiquidUtility.IsLiquidStarved(casket))
                return false;

            NeutroamineJobFailureUtility.Notify_JobFailedNoLiquid(driver.pawn, casket);
            return true;
        }
    }
}

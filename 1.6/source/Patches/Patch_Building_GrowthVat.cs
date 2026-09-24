using HarmonyLib;
using RimWorld;
using Verse;

namespace Neutroflow.Patches
{
    internal static class GrowthVatLiquidUtility
    {
        internal static CompNeutroamineConsumer GetStarvedComp(Building_GrowthVat vat)
        {
            if (!vat.Working)
                return null;
            var comp = vat.TryGetComp<CompNeutroamineConsumer>();
            if (comp?.Extension == null || comp.Extension.activityMode != NeutroflowActivityModes.GrowthVatWorking)
                return null;
            return comp.IsStarved ? comp : null;
        }
    }

    [HarmonyPatch(typeof(Building_GrowthVat), "Tick")]
    internal static class Patch_Building_GrowthVat_Tick
    {
        private static readonly AccessTools.FieldRef<Building_GrowthVat, float> ContainedNutrition =
            AccessTools.FieldRefAccess<Building_GrowthVat, float>("containedNutrition");

        private static readonly AccessTools.FieldRef<Building_Enterable, int> StartTick =
            AccessTools.FieldRefAccess<Building_Enterable, int>("startTick");

        private struct State
        {
            public bool active;
            public Pawn originalPawn;
            public float originalNutrition;
        }

        private static void Prefix(Building_GrowthVat __instance, out State __state)
        {
            __state = default;

            if (GrowthVatLiquidUtility.GetStarvedComp(__instance) == null)
                return;

            __state.active = true;
            __state.originalNutrition = ContainedNutrition(__instance);
            ContainedNutrition(__instance) = 999999f;

            if (__instance.SelectedPawn != null)
            {
                __state.originalPawn = __instance.SelectedPawn;
                __instance.SelectedPawn = null;
            }
            else if (__instance.selectedEmbryo != null)
            {
                StartTick(__instance) += 1;
            }
        }

        private static void Postfix(Building_GrowthVat __instance, State __state)
        {
            if (!__state.active)
                return;

            ContainedNutrition(__instance) = __state.originalNutrition;

            if (__state.originalPawn != null)
                __instance.SelectedPawn = __state.originalPawn;
        }
    }

    [HarmonyPatch(typeof(Building_GrowthVat), "TickInterval")]
    internal static class Patch_Building_GrowthVat_TickInterval
    {
        private static void Prefix(Building_GrowthVat __instance, out Pawn __state)
        {
            __state = null;
            if (GrowthVatLiquidUtility.GetStarvedComp(__instance) == null)
                return;
            if (__instance.SelectedPawn == null)
                return;

            __state = __instance.SelectedPawn;
            __instance.SelectedPawn = null;
        }

        private static void Postfix(Building_GrowthVat __instance, Pawn __state)
        {
            if (__state != null)
                __instance.SelectedPawn = __state;
        }
    }
}

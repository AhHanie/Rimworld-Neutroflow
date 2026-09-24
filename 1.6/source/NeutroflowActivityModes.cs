using HarmonyLib;
using RimWorld;
using Verse;

namespace Neutroflow
{
    [StaticConstructorOnStartup]
    public static class NeutroflowActivityModes
    {
        public const string BiosculpterOccupied = "BiosculpterOccupied";
        public const string GrowthVatWorking = "GrowthVatWorking";
        public const string CryptosleepOccupied = "CryptosleepOccupied";
        public const string MechGestationForming = "MechGestationForming";
        public const string GeneExtractionWorking = "GeneExtractionWorking";
        public const string BioReactorOccupied = "BioReactorOccupied";

        private static readonly System.Reflection.MethodInfo GrowthVatFinishPawn =
            AccessTools.Method(typeof(Building_GrowthVat), "FinishPawn");

        private static readonly System.Reflection.MethodInfo GrowthVatOnStop =
            AccessTools.Method(typeof(Building_GrowthVat), "OnStop");

        static NeutroflowActivityModes()
        {
            NeutroamineActivityAdapters.Register(BiosculpterOccupied,
                parent => parent.TryGetComp<CompBiosculpterPod>()?.Occupant != null);

            NeutroamineActivityAdapters.Register(GrowthVatWorking,
                parent => (parent as Building_GrowthVat)?.Working ?? false,
                onActiveDenied: Notify_GrowthVatDenied);

            NeutroamineActivityAdapters.Register(CryptosleepOccupied,
                parent => (parent as Building_CryptosleepCasket)?.HasAnyContents ?? false,
                onActiveDenied: Notify_CasketDenied);

            NeutroamineActivityAdapters.Register(MechGestationForming,
                parent => (parent as Building_MechGestator)?.ActiveMechBill?.State == FormingState.Forming);

            NeutroamineActivityAdapters.Register(GeneExtractionWorking,
                parent => (parent as Building_GeneExtractor)?.Working ?? false);

            NeutroamineActivityAdapters.Register(BioReactorOccupied,
                parent => (parent as Building_Casket)?.HasAnyContents ?? false,
                onActiveDenied: Notify_BioReactorDenied);
        }

        private static void Notify_CasketDenied(Thing parent)
        {
            if (parent is Building_CryptosleepCasket casket && casket.HasAnyContents)
            {
                var occupant = casket.ContainedThing;
                casket.EjectContents();
                Messages.Message("Neutroflow_CasketEjectedNoLiquid".Translate(occupant?.LabelShortCap ?? casket.LabelShortCap), casket, MessageTypeDefOf.NegativeEvent, historical: false);
            }
        }

        internal static void Notify_BioReactorDenied(Thing parent)
        {
            if (parent is Building_Casket casket && casket.HasAnyContents)
            {
                var occupant = casket.ContainedThing;
                casket.EjectContents();
                Messages.Message("Neutroflow_BioReactorEjectedNoLiquid".Translate(occupant?.LabelShortCap ?? casket.LabelShortCap), casket, MessageTypeDefOf.NegativeEvent, historical: false);
            }
        }

        private static void Notify_GrowthVatDenied(Thing parent)
        {
            if (!(parent is Building_GrowthVat vat))
                return;

            if (vat.SelectedPawn != null)
            {
                var pawn = vat.SelectedPawn;
                GrowthVatFinishPawn.Invoke(vat, null);
                Messages.Message("Neutroflow_GrowthVatEjectedNoLiquid".Translate(pawn.LabelShortCap), vat, MessageTypeDefOf.NegativeEvent, historical: false);
            }
            else if (vat.selectedEmbryo != null)
            {
                var embryo = vat.selectedEmbryo;
                vat.innerContainer.Remove(embryo);
                embryo.Destroy();
                GrowthVatOnStop.Invoke(vat, null);
                Messages.Message("Neutroflow_EmbryoDestroyedNoLiquid".Translate(embryo.Label), vat, MessageTypeDefOf.NegativeEvent, historical: false);
            }
        }
    }
}

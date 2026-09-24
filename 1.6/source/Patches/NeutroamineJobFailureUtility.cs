using RimWorld;
using Verse;

namespace Neutroflow.Patches
{
    internal static class NeutroamineJobFailureUtility
    {
        internal static void Notify_JobFailedNoLiquid(Pawn pawn, Thing device)
        {
            Messages.Message("Neutroflow_JobFailedNoLiquid".Translate(pawn.LabelShortCap, device.LabelShortCap), device, MessageTypeDefOf.RejectInput, historical: false);
        }
    }
}

using RimWorld;
using Verse;

namespace Neutroflow.Patches
{
    internal static class NeutroamineCasketLiquidUtility
    {
        internal static bool IsLiquidStarved(Building_Casket casket)
        {
            var comp = casket.TryGetComp<CompNeutroamineConsumer>();
            if (comp?.Extension == null || comp.Extension.starvationMode != NeutroamineStarvationMode.Eject)
                return false;
            return comp.IsStarved;
        }
    }
}

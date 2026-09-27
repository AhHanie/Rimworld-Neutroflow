using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Neutroflow
{
    // Swaps CompFacility.props between the def's real props and a per-def clone with an
    // empty statOffsets list, instead of patching CompFacility.CanBeActive. CanBeActive is
    // read by RimWorld's stat system on every relevant stat request for every facility in
    // the game, so a Harmony postfix there runs far more often than the swap below, which
    // only touches the one comp instance whose supply state actually changed.
    internal static class IVDripFacilityUtility
    {
        private static readonly MethodInfo MemberwiseCloneMethod = AccessTools.Method(typeof(object), "MemberwiseClone");

        private static readonly ConditionalWeakTable<CompProperties_Facility, CompProperties_Facility> InactiveVariants =
            new ConditionalWeakTable<CompProperties_Facility, CompProperties_Facility>();

        internal static void Notify_SupplyChanged(Thing parent, bool isStarved)
        {
            var facility = parent.TryGetComp<CompFacility>();
            var activeProps = parent.def.GetCompProperties<CompProperties_Facility>();
            if (facility == null || activeProps == null)
                return;

            facility.props = isStarved ? InactiveVariants.GetValue(activeProps, CreateInactiveVariant) : activeProps;
        }

        private static CompProperties_Facility CreateInactiveVariant(CompProperties_Facility active)
        {
            var clone = (CompProperties_Facility)MemberwiseCloneMethod.Invoke(active, null);
            clone.statOffsets = null;
            return clone;
        }
    }
}

using System;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace Neutroflow.Patches
{
    internal static class DraincasketCompat
    {
        internal static Type CompType { get; private set; }
        internal static Type JobDriverEnterType { get; private set; }
        internal static Type JobDriverCarryType { get; private set; }

        internal static bool Loaded { get; private set; }

        internal static PropertyInfo OccupantProperty { get; private set; }
        internal static FieldInfo HemogenField { get; private set; }
        internal static MethodInfo EjectContentsMethod { get; private set; }
        internal static MethodInfo TryAcceptPawnMethod { get; private set; }
        internal static MethodInfo InsertPawnMethod { get; private set; }
        internal static MethodInfo CompTickIntervalMethod { get; private set; }
        internal static MethodInfo EnterMakeNewToilsMethod { get; private set; }
        internal static MethodInfo CarryMakeNewToilsMethod { get; private set; }

        internal static void Initialize()
        {
            CompType = AccessTools.TypeByName("VanillaRacesExpandedSanguophage.CompDraincasket");
            Loaded = CompType != null;
            if (!Loaded)
                return;

            JobDriverEnterType = AccessTools.TypeByName("VanillaRacesExpandedSanguophage.JobDriver_EnterDraincasket");
            JobDriverCarryType = AccessTools.TypeByName("VanillaRacesExpandedSanguophage.JobDriver_CarryToDraincasket");

            OccupantProperty = AccessTools.Property(CompType, "Occupant");
            HemogenField = AccessTools.Field(CompType, "compResourceHemogen");
            EjectContentsMethod = AccessTools.Method(CompType, "EjectContents", new[] { typeof(Map) });
            TryAcceptPawnMethod = AccessTools.Method(CompType, "TryAcceptPawn", new[] { typeof(Pawn) });
            InsertPawnMethod = AccessTools.Method(CompType, "InsertPawn", new[] { typeof(Pawn) });
            CompTickIntervalMethod = AccessTools.Method(CompType, "CompTickInterval", new[] { typeof(int) });
            EnterMakeNewToilsMethod = JobDriverEnterType != null ? AccessTools.Method(JobDriverEnterType, "MakeNewToils") : null;
            CarryMakeNewToilsMethod = JobDriverCarryType != null ? AccessTools.Method(JobDriverCarryType, "MakeNewToils") : null;

            var ok = OccupantProperty != null
                && HemogenField != null
                && EjectContentsMethod != null
                && TryAcceptPawnMethod != null
                && InsertPawnMethod != null
                && CompTickIntervalMethod != null
                && EnterMakeNewToilsMethod != null
                && CarryMakeNewToilsMethod != null;

            if (!ok)
            {
                Logger.Error("vanillaracesexpanded.sanguophage is active but one or more expected CompDraincasket members were not found; draincasket compatibility patches may not apply. This likely means Sanguophage was updated in a way Neutroflow does not yet support.");
                return;
            }

            NeutroflowActivityModes.RegisterDraincasketOccupied();
        }

        internal static ThingComp FindComp(Thing parent)
        {
            if (!Loaded || !(parent is ThingWithComps thingWithComps))
                return null;

            var comps = thingWithComps.AllComps;
            for (var i = 0; i < comps.Count; i++)
            {
                if (CompType.IsInstanceOfType(comps[i]))
                    return comps[i];
            }
            return null;
        }

        internal static Pawn GetOccupant(ThingComp comp) => (Pawn)OccupantProperty.GetValue(comp);

        internal static void Eject(ThingComp comp, Map map) => EjectContentsMethod.Invoke(comp, new object[] { map });
    }
}

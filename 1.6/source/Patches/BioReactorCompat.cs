using System;
using System.Reflection;
using HarmonyLib;

namespace Neutroflow.Patches
{
    internal static class BioReactorCompat
    {
        internal static Type BuildingType { get; private set; }
        internal static Type PowerPlantType { get; private set; }
        internal static Type RefuelableType { get; private set; }
        internal static Type JobDriverEnterType { get; private set; }
        internal static Type JobDriverCarryType { get; private set; }

        internal static bool Loaded { get; private set; }

        internal static FieldInfo StateField { get; private set; }
        internal static object EmptyState { get; private set; }

        internal static void Initialize()
        {
            BuildingType = AccessTools.TypeByName("BioReactor.Building_BioReactor");
            Loaded = BuildingType != null;
            if (!Loaded)
                return;

            PowerPlantType = AccessTools.TypeByName("BioReactor.CompBioPowerPlant");
            RefuelableType = AccessTools.TypeByName("BioReactor.CompBioRefuelable");
            JobDriverEnterType = AccessTools.TypeByName("BioReactor.JobDriver_EnterBioReactor");
            JobDriverCarryType = AccessTools.TypeByName("BioReactor.JobDriver_CarryToBioReactor");
            StateField = AccessTools.Field(BuildingType, "state");
            EmptyState = StateField != null ? Enum.ToObject(StateField.FieldType, 0) : null;

            var ok = PowerPlantType != null && AccessTools.Method(PowerPlantType, "UpdateDesiredPowerOutput") != null
                && RefuelableType != null && AccessTools.Method(RefuelableType, "CompTick") != null
                && JobDriverEnterType != null && AccessTools.Method(JobDriverEnterType, "MakeNewToils") != null
                && JobDriverCarryType != null && AccessTools.Method(JobDriverCarryType, "MakeNewToils") != null
                && AccessTools.Method(BuildingType, "Tick") != null
                && AccessTools.Method(BuildingType, "TryAcceptThing") != null
                && StateField != null;

            if (!ok)
                Logger.Error("Mlie.BioReactor is active but one or more expected BioReactor members were not found; BioReactor compatibility patches may not apply. This likely means BioReactor was updated in a way Neutroflow does not yet support.");
        }
    }
}

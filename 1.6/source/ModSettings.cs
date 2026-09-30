using Verse;

namespace Neutroflow
{
    public class ModSettings : Verse.ModSettings
    {
        public static bool PassiveDrainEnabled = true;

        public ModSettings()
        {
            PassiveDrainEnabled = true;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref PassiveDrainEnabled, "passiveDrainEnabled", true);
        }
    }
}

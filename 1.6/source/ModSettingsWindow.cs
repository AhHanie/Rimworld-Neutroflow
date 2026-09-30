using UnityEngine;
using Verse;

namespace Neutroflow
{
    public static class ModSettingsWindow
    {
        public static void Draw(Rect inRect)
        {
            var listing = new Listing_Standard();
            listing.Begin(inRect);
            listing.CheckboxLabeled(
                "Neutroflow.Settings.PassiveDrainEnabled".Translate(), ref ModSettings.PassiveDrainEnabled,
                "Neutroflow.Settings.PassiveDrainEnabled.Tooltip".Translate());
            listing.End();
        }
    }
}

using System.Linq;
using LudeonTK;
using Verse;

namespace Neutroflow
{
    public static class DebugOutputsNeutroflow
    {
        [DebugOutput("Neutroflow")]
        public static void ConsumptionRates()
        {
            var defs = DefDatabase<ThingDef>.AllDefsListForReading
                .Where(d => d.GetModExtension<NeutroamineUsageExtension>() != null)
                .OrderBy(d => d.defName)
                .ToList();

            DebugTables.MakeTablesDialog(
                defs,
                new TableDataGetter<ThingDef>("defName", d => d.defName),
                new TableDataGetter<ThingDef>("label", d => d.label),
                new TableDataGetter<ThingDef>("idle/day", d => Ext(d).idlePerDay),
                new TableDataGetter<ThingDef>("1 item lasts\n(idle)", d => PerItemDuration(Ext(d).idlePerDay)),
                new TableDataGetter<ThingDef>("active/day", d => Ext(d).activePerDay),
                new TableDataGetter<ThingDef>("1 item lasts\n(active)", d => PerItemDuration(Ext(d).activePerDay)),
                new TableDataGetter<ThingDef>("activityMode", d => Ext(d).activityMode),
                new TableDataGetter<ThingDef>("starvation", d => Ext(d).starvationMode.ToString()),
                new TableDataGetter<ThingDef>("has consumer comp", d => d.HasComp(typeof(CompNeutroamineConsumer)) ? "yes" : "NO")
            );
        }

        private static NeutroamineUsageExtension Ext(ThingDef d)
        {
            return d.GetModExtension<NeutroamineUsageExtension>();
        }

        private static string PerItemDuration(float perDay)
        {
            if (perDay <= 0f)
                return "free";

            var days = 1f / perDay;
            return days < 1f ? $"{days * 24f:0.#}h" : $"{days:0.##}d";
        }
    }
}

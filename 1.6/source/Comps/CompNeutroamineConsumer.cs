using System.Text;
using PipeSystem;
using RimWorld;
using Verse;

namespace Neutroflow
{
    public class CompNeutroamineConsumer : CompResource
    {
        private const float Tolerance = 0.0001f;

        public new CompProperties_NeutroamineConsumer Props => (CompProperties_NeutroamineConsumer)props;

        private NeutroamineUsageExtension ext;
        private NeutroamineActivityAdapter adapter;
        private CompPowerTrader powerTrader;
        private CompFlickable flickable;
        private CompBreakdownable breakdownable;

        public bool IsStarved { get; private set; } = true;
        public bool HasActiveWork { get; private set; }

        public NeutroamineUsageExtension Extension => ext;
        public NeutroamineActivityAdapter Adapter => adapter;

        public bool IsReady =>
            parent.Spawned
            && ext != null
            && (powerTrader == null || powerTrader.PowerOn)
            && (flickable == null || flickable.SwitchIsOn)
            && (breakdownable == null || !breakdownable.BrokenDown);

        public bool WantsActive => adapter != null && adapter.isActive(parent);

        public float RequiredPerDay => ext == null ? 0f : (WantsActive ? ext.activePerDay : ext.idlePerDay);

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);

            ext = parent.def.GetModExtension<NeutroamineUsageExtension>();
            powerTrader = parent.GetComp<CompPowerTrader>();
            flickable = parent.GetComp<CompFlickable>();
            breakdownable = parent.GetComp<CompBreakdownable>();

            adapter = null;
            if (ext != null && !NeutroamineActivityAdapters.TryGet(ext.activityMode, out adapter))
                Logger.Error($"{parent.def.defName} has a Neutroflow consumer with unknown activityMode '{ext.activityMode}'; it will never be authorized.");

            IsStarved = true;
            HasActiveWork = false;

            parent.Map.GetComponent<MapComponent_NeutroflowAllocator>()?.Register(this);
        }

        public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
        {
            map.GetComponent<MapComponent_NeutroflowAllocator>()?.Unregister(this);
            base.PostDeSpawn(map, mode);
        }

        internal void Notify_AllocationResult(bool granted, bool wantedActive)
        {
            IsStarved = !granted;
            HasActiveWork = granted && wantedActive;
        }

        internal void Notify_Unready()
        {
            IsStarved = true;
            HasActiveWork = false;
        }

        public bool HasConfirmedShortage()
        {
            if (ext == null)
                return false;

            var net = PipeNet;
            if (net == null)
                return true;

            var amount = ext.activePerDay * 250f / 60000f;
            return net.CurrentStored() + net.OverflowAmount + Tolerance < amount;
        }

        public override string CompInspectStringExtra()
        {
            var sb = new StringBuilder();
            if (ext != null)
            {
                sb.Append("Neutroflow_RequiredRate".Translate((RequiredPerDay).ToString("0.##"), Resource.unit));
                sb.AppendInNewLine("Neutroflow_Status".Translate(
                    IsStarved
                        ? "Neutroflow_StatusStarved".Translate()
                        : (HasActiveWork ? "Neutroflow_StatusActive".Translate() : "Neutroflow_StatusReady".Translate())));
            }
            sb.AppendInNewLine(base.CompInspectStringExtra());
            return sb.ToString().Trim();
        }
    }
}

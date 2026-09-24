using System;
using System.Collections.Generic;
using PipeSystem;
using Verse;

namespace Neutroflow
{
    public class MapComponent_NeutroflowAllocator : MapComponent
    {
        private const int AllocationInterval = 250;
        private const float Tolerance = 0.0001f;

        private readonly List<CompNeutroamineConsumer> registered = new List<CompNeutroamineConsumer>();
        private readonly Dictionary<PipeNet, List<CompNeutroamineConsumer>> groups = new Dictionary<PipeNet, List<CompNeutroamineConsumer>>();

        public MapComponent_NeutroflowAllocator(Map map) : base(map)
        {
        }

        public void Register(CompNeutroamineConsumer comp)
        {
            if (!registered.Contains(comp))
                registered.Add(comp);
        }

        public void Unregister(CompNeutroamineConsumer comp)
        {
            registered.Remove(comp);
        }

        public override void MapComponentTick()
        {
            base.MapComponentTick();
            if (Find.TickManager.TicksGame % AllocationInterval != 0)
                return;

            RunAllocationPass();
        }

        private void RunAllocationPass()
        {
            groups.Clear();

            for (var i = 0; i < registered.Count; i++)
            {
                var comp = registered[i];
                var net = comp.PipeNet;
                if (net == null)
                {
                    var wasWantingActive = comp.WantsActive;
                    comp.Notify_Unready();
                    if (wasWantingActive && comp.Extension?.starvationMode == NeutroamineStarvationMode.Eject)
                        comp.Adapter?.onActiveDenied?.Invoke(comp.parent);
                    continue;
                }
                if (!groups.TryGetValue(net, out var list))
                {
                    list = new List<CompNeutroamineConsumer>();
                    groups[net] = list;
                }
                list.Add(comp);
            }

            foreach (var kv in groups)
            {
                AllocateNet(kv.Key, kv.Value);
            }
        }

        private static readonly Comparison<CompNeutroamineConsumer> SortByActiveThenId = (a, b) =>
        {
            var aActive = a.WantsActive ? 0 : 1;
            var bActive = b.WantsActive ? 0 : 1;
            if (aActive != bActive)
                return aActive - bActive;
            return a.parent.thingIDNumber.CompareTo(b.parent.thingIDNumber);
        };

        private void AllocateNet(PipeNet net, List<CompNeutroamineConsumer> comps)
        {
            comps.Sort(SortByActiveThenId);

            for (var i = 0; i < comps.Count; i++)
            {
                var comp = comps[i];

                if (!comp.IsReady)
                {
                    comp.Notify_Unready();
                    continue;
                }

                var wantedActive = comp.WantsActive;
                var perDay = wantedActive ? comp.Extension.activePerDay : comp.Extension.idlePerDay;
                var amount = perDay * AllocationInterval / 60000f;

                bool granted;
                if (amount <= 0f)
                {
                    granted = true;
                }
                else
                {
                    var available = net.CurrentStored() + net.OverflowAmount;
                    if (available + Tolerance >= amount)
                    {
                        net.DrawAmongStorage(amount, out _, net.storages);
                        granted = true;
                    }
                    else
                    {
                        granted = false;
                    }
                }

                comp.Notify_AllocationResult(granted, wantedActive);

                if (!granted && wantedActive && comp.Extension.starvationMode == NeutroamineStarvationMode.Eject)
                {
                    comp.Adapter?.onActiveDenied?.Invoke(comp.parent);
                }
            }
        }
    }
}

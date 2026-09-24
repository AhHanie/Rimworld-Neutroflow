using System.Collections.Generic;
using Verse;

namespace Neutroflow
{
    public delegate bool NeutroamineActivePredicate(Thing parent);

    public delegate void NeutroamineStarvedHandler(Thing parent);

    public sealed class NeutroamineActivityAdapter
    {
        public readonly string key;
        public readonly NeutroamineActivePredicate isActive;
        public readonly NeutroamineStarvedHandler onActiveDenied;

        public NeutroamineActivityAdapter(string key, NeutroamineActivePredicate isActive, NeutroamineStarvedHandler onActiveDenied)
        {
            this.key = key;
            this.isActive = isActive;
            this.onActiveDenied = onActiveDenied;
        }
    }

    public static class NeutroamineActivityAdapters
    {
        private static readonly Dictionary<string, NeutroamineActivityAdapter> adapters = new Dictionary<string, NeutroamineActivityAdapter>();

        public static void Register(string key, NeutroamineActivePredicate isActive, NeutroamineStarvedHandler onActiveDenied = null)
        {
            adapters[key] = new NeutroamineActivityAdapter(key, isActive, onActiveDenied);
        }

        public static bool TryGet(string key, out NeutroamineActivityAdapter adapter)
        {
            if (key.NullOrEmpty())
            {
                adapter = null;
                return false;
            }
            return adapters.TryGetValue(key, out adapter);
        }
    }
}

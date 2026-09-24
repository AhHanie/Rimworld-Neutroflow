using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Neutroflow
{
    public class Mod : Verse.Mod
    {
        public Mod(ModContentPack content) : base(content)
        {
            LongEventHandler.QueueLongEvent(Init, "Neutroflow.LoadingLabel", doAsynchronously: true, null);
        }

        private void Init()
        {
            Patches.BioReactorCompat.Initialize();
            new Harmony("sk.neutroflow").PatchAll();
        }
    }
}

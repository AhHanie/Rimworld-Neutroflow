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
            GetSettings<ModSettings>();
            LongEventHandler.QueueLongEvent(Init, "Neutroflow.LoadingLabel", doAsynchronously: true, null);
        }

        public override string SettingsCategory()
        {
            return "Neutroflow.Settings.Title".Translate();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            ModSettingsWindow.Draw(inRect);
        }

        private void Init()
        {
            Patches.BioReactorCompat.Initialize();
            Patches.DraincasketCompat.Initialize();
            new Harmony("sk.neutroflow").PatchAll();
        }
    }
}

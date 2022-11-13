using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Reflection.Emit;
using RimWorld;
using HarmonyLib;
using Verse;
using System.Diagnostics.Eventing.Reader;
using UnityEngine;

namespace Advanced_Genes.HarmonyPatches
{
    public static class OptionalPatches
    {
        private static readonly Type patchType = typeof(HarmonyPatches);

        static OptionalPatches() { }
        public static void attemptPatch(Harmony harmony)
        {
            if (ModLister.GetActiveModWithIdentifier("danielwedemeyer.generipper") != null)
            {
                Patch_GeneRipper(harmony);
            }
        }

        private static void Patch_GeneRipper(Harmony harmony)
        {
            harmony.Patch(AccessTools.Method(AccessTools.TypeByName("GeneRipper.Building_GeneRipper"), "CanAcceptPawn"), prefix: new HarmonyMethod(patchType, nameof(Pre_CanAcceptPawnRipper)));
        }
        
        public static bool Pre_CanAcceptPawnRipper(Thing __instance, ref string __result, Pawn pawn)
        {
            foreach (var geneBlocker in pawn.health.hediffSet.hediffs.OfType<Hediff_GeneChangeBlocker>())
            {
                if (geneBlocker.blockPawnScan(ref pawn))
                {
                    __result = "Unable to read " + pawn.Name + "'s genetic sequence";
                    return false;
                }
            }
            return true;
        }
    }
}

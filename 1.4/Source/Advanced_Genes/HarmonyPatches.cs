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

namespace Advanced_Genes
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        private static readonly Type patchType = typeof(HarmonyPatches);

        static HarmonyPatches()
        {
            Harmony harmony = new Harmony(id: "rimworld.smartkar.advanced_genes.main");

            harmony.Patch(AccessTools.Method(typeof(ThingWithComps), nameof(ThingWithComps.PostApplyDamage)),
                postfix: new HarmonyMethod(patchType, nameof(PostPostApplyDamage)));

        }
        public static void PostPostApplyDamage(ThingWithComps __instance, ref DamageInfo dinfo, ref float totalDamageDealt)
        {
            if (!(__instance is Pawn pawn)) return;
            foreach (var detector in pawn.health.hediffSet.hediffs.OfType<HediffWithComps>().SelectMany(hediff => hediff.comps).OfType<HediffComp_AttackDetector>())
            {
                detector.PostApplyDamage(ref dinfo, ref totalDamageDealt);
            }
        }
    }
}

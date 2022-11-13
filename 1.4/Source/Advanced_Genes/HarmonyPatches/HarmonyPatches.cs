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

namespace Advanced_Genes.HarmonyPatches
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {

        static HarmonyPatches()
        {
            Harmony harmony = new Harmony(id: "rimworld.smartkar.advanced_genes.main");
            harmony.PatchAll();
            OptionalPatches.attemptPatch(harmony);
        }

        [HarmonyPatch(typeof(Pawn), "PostApplyDamage")]
        public static class AttackDetector_PostApplyDamage
        {
            static void Postfix(Pawn __instance, ref DamageInfo dinfo, ref float totalDamageDealt)
            {
                foreach (var detector in __instance.health.hediffSet.hediffs.OfType<Hediff_AttackDetector>())
                {
                    detector.PostApplyDamage(ref dinfo, ref totalDamageDealt);
                }
            }
        }

        [HarmonyPatch(typeof(Pawn_GeneTracker), "AddGene", new Type[] { typeof(Gene), typeof(bool) })]
        public static class GeneBlocker_AddGene
        {
            static bool Prefix(Pawn_GeneTracker __instance, ref Gene gene, ref bool addAsXenogene)
            {
                foreach (var geneBlocker in __instance.pawn.health.hediffSet.hediffs.OfType<Hediff_GeneChangeBlocker>())
                {
                    if (geneBlocker.blockGeneChange(ref gene, ref addAsXenogene))
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(Pawn_GeneTracker), "RemoveGene")]
        public static class GeneBlocker_RemoveGene
        {
            static bool Prefix(Pawn_GeneTracker __instance, ref Gene gene)
            {
                foreach (var geneBlocker in __instance.pawn.health.hediffSet.hediffs.OfType<Hediff_GeneChangeBlocker>())
                {
                    if (geneBlocker.blockGeneRemoval(ref gene))
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(Building_GeneExtractor), "CanAcceptPawn")]
        public static class GeneBlocker_Scanner
        {
            static bool Prefix(Pawn_GeneTracker __instance, ref AcceptanceReport __result, ref Pawn pawn)
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

        [HarmonyPatch(typeof(Pawn_InteractionsTracker), "TryInteractWith")]
        public static class InteractionTracker
        {
            static bool Postfix(Pawn_InteractionsTracker __instance, ref bool __result, ref Pawn __pawn, ref Pawn recipient, ref InteractionDef intDef)
            {
                foreach (var hediffTracker in __pawn.health.hediffSet.hediffs.OfType<Hediff_InteractionTracker>())
                {
                    hediffTracker.trackInteraction(ref recipient, ref intDef);
                }

                foreach (var hediffTracker in recipient.health.hediffSet.hediffs.OfType<Hediff_InteractionTracker>())
                {
                    hediffTracker.trackInteraction(ref __pawn, ref intDef);
                }
                return __result;
            }
        }
    }
}

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
            static void Postfix(Pawn __instance, DamageInfo dinfo, ref float totalDamageDealt)
            {
                foreach (Hediff_AttackDetector detector in __instance.health.hediffSet.hediffs.OfType<Hediff_AttackDetector>())
                {
                    detector.PostApplyDamage(ref dinfo, ref totalDamageDealt);
                }
            }
        }

        [HarmonyPatch(typeof(Pawn_GeneTracker), "AddGene", new Type[] { typeof(Gene), typeof(bool) })]
        public static class GeneBlocker_AddGene
        {
            static bool Prefix(Pawn_GeneTracker __instance, Gene gene, ref bool addAsXenogene)
            {
                foreach (Hediff_GeneChangeBlocker geneBlocker in __instance.pawn.health.hediffSet.hediffs.OfType<Hediff_GeneChangeBlocker>())
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
            static bool Prefix(Pawn_GeneTracker __instance, Gene gene)
            {
                foreach (Hediff_GeneChangeBlocker geneBlocker in __instance.pawn.health.hediffSet.hediffs.OfType<Hediff_GeneChangeBlocker>())
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
            static bool Prefix(Pawn_GeneTracker __instance, ref AcceptanceReport __result, Pawn pawn)
            {
                foreach (Hediff_GeneChangeBlocker geneBlocker in pawn.health.hediffSet.hediffs.OfType<Hediff_GeneChangeBlocker>())
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
            static void Postfix(Pawn_InteractionsTracker __instance, ref bool __result, Pawn ___pawn, Pawn recipient, InteractionDef intDef)
            {
                if (!__result)
                {
                    return;
                }

                foreach (Hediff_InteractionTracker hediffTracker in ___pawn.health.hediffSet.hediffs.OfType<Hediff_InteractionTracker>())
                {
                    hediffTracker.trackInteraction(recipient, intDef);
                }

                foreach (Hediff_InteractionTracker hediffTracker in recipient.health.hediffSet.hediffs.OfType<Hediff_InteractionTracker>())
                {
                    hediffTracker.trackInteraction(___pawn, intDef);
                }
            }
        }
    }
}

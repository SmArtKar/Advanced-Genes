using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Advanced_Genes
{
    public class StatPart_DeathGuidanceLearningFactor : StatPart
    {
        public float modifierDecay;
        public int modifierLength;

        public StatPart_DeathGuidanceLearningFactor()
        {
            modifierDecay = LoadedModManager.GetMod<AG_Mod>().GetSettings<AG_Settings>().modifierDecayDeathGuidance;
            modifierLength = LoadedModManager.GetMod<AG_Mod>().GetSettings<AG_Settings>().modifierLengthDeathGuidance;
        }

        public override void TransformValue(StatRequest req, ref float val)
        {
            if (req.HasThing)
            {
                Pawn pawn = req.Thing as Pawn;
                if (pawn != null)
                {
                    var guidanceHediff = pawn.health.hediffSet.GetFirstHediffOfDef(AG_DefOf.Hediff_DeathGuidance, false) as Hediff_DeathGuidance;

                    if (guidanceHediff == null || guidanceHediff.connectedHivemind == null)
                    {
                        return;
                    }

                    Hivemind_DeathGuidance hivemind = guidanceHediff.connectedHivemind as Hivemind_DeathGuidance;
                    int totalDeadCopy = hivemind.totalDead;
                    float modifier = 1f;
                    while (totalDeadCopy > 0)
                    {
                        val -= Math.Min(totalDeadCopy, modifierLength) * modifier * 0.01f;
                        totalDeadCopy -= modifierLength;
                        modifier *= modifierDecay;
                    }
                }
            }
        }

        public override string ExplanationPart(StatRequest req)
        {
            if (req.HasThing)
            {
                Pawn pawn = req.Thing as Pawn;
                if (pawn != null)
                {
                    var guidanceHediff = pawn.health.hediffSet.GetFirstHediffOfDef(AG_DefOf.Hediff_DeathGuidance, false) as Hediff_DeathGuidance;
                    if (guidanceHediff == null || guidanceHediff.connectedHivemind == null)
                    {
                        return null;
                    }

                    Hivemind_DeathGuidance hivemind = guidanceHediff.connectedHivemind as Hivemind_DeathGuidance;
                    int totalDeadCopy = hivemind.totalDead;

                    float modifier = 1f;
                    float skillMod = 0f;
                    string skillListing = "";
                    while (totalDeadCopy > 0)
                    {
                        skillListing = skillListing + " " + skillMod + " " + Math.Min(totalDeadCopy, 10) + " " + modifier + ";";
                        skillMod += Math.Min(totalDeadCopy, 10) * modifier;
                        totalDeadCopy -= 10;
                        modifier /= 2;
                    }

                    return "Absorbed minds: -" + skillMod + "%" + skillListing;
                }
            }
            return null;
        }
    }
}

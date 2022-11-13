﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using VFECore;

namespace Advanced_Genes
{
    public class StatPart_DeathGuidanceMentalBreak : StatPart
    {
        public float modifierDecay;
        public int modifierLength;

        public StatPart_DeathGuidanceMentalBreak()
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
                    if (guidanceHediff == null)
                    {
                        return;
                    }

                    DeathGuidance_Dataset dataset = guidanceHediff.globalSkillbase.datasets[pawn.Faction];
                    int totalDeadCopy = dataset.totalDead;
                    float modifier = 0.5f;
                    while (totalDeadCopy > 0)
                    {
                        val += Math.Min(totalDeadCopy, modifierLength) * modifier * 0.01f;
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
                    if (guidanceHediff == null)
                    {
                        return null;
                    }

                    DeathGuidance_Dataset dataset = guidanceHediff.globalSkillbase.datasets[pawn.Faction];
                    if (dataset.totalDead == 0)
                    {
                        return null;
                    }

                    int totalDeadCopy = dataset.totalDead;
                    float modifier = 1f;
                    float skillMod = 0f;
                    while (totalDeadCopy > 0)
                    {
                        skillMod += Math.Min(totalDeadCopy, 10) * modifier;
                        totalDeadCopy -= 10;
                        modifier /= 2;
                    }

                    return "Absorbed minds: +" + skillMod + "%";
                }
            }
            return null;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using HarmonyLib;
using Verse;
using System.Collections;

namespace Advanced_Genes
{
    public class Hivemind_DeathGuidance : Hivemind
    {
        public Dictionary<SkillDef, float> skillValues = new Dictionary<SkillDef, float>();
        public int totalDead = 0;

        public Hivemind_DeathGuidance()
        {
            foreach (var skillDef in DefDatabase<SkillDef>.AllDefs)
            {
                skillValues[skillDef] = 0f;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref totalDead, "totalDead");
            Scribe_Collections.Look(ref skillValues, "skillValues", LookMode.Def, LookMode.Value);
        }

        public void absorbCorpse(Pawn corpse)
        {
            bool wipeSkills = LoadedModManager.GetMod<AG_Mod>().GetSettings<AG_Settings>().wipeDeathGuidance;
            bool lowerDecay = LoadedModManager.GetMod<AG_Mod>().GetSettings<AG_Settings>().lowerDecayDeathGuidance;

            bool livingPawns = false;
            foreach (var attached in attachedPawns.Keys.ToList())
            {
                if (!attached.health.Dead)
                {
                    livingPawns = true;
                    break;
                }
            }

            if(!livingPawns && wipeSkills)
            {
                foreach (var skillDef in DefDatabase<SkillDef>.AllDefs)
                {
                    skillValues[skillDef] = 0f;
                }

                totalDead += 1;
                foreach (var attached in attachedPawns.Keys.ToList())
                {
                    updatePawnSkills(attached);
                }
                return;
            }

            foreach (var skillDef in DefDatabase<SkillDef>.AllDefs)
            {
                SkillRecord corpseSkill = corpse.skills.GetSkill(skillDef);
                if (!skillValues.ContainsKey(skillDef))
                {
                    skillValues[skillDef] = 0f;
                }

                if (corpseSkill.TotallyDisabled)
                {
                    continue;
                }

                Hediff_DeathGuidance hediff_DeathGuidance = attachedPawns[corpse] as Hediff_DeathGuidance;
                float corpseExpirience = corpseSkill.XpTotalEarned + corpseSkill.xpSinceLastLevel - hediff_DeathGuidance.addedExpirience[skillDef];
                if(lowerDecay && skillValues[skillDef] > corpseExpirience)
                {
                    corpseExpirience = (corpseExpirience * 2 + skillValues[skillDef]) / 3;
                }

                skillValues[skillDef] = (skillValues[skillDef] * totalDead + corpseExpirience) / (totalDead + 1);
            }
            totalDead += 1;
            foreach (var attached in attachedPawns.Keys.ToList())
            {
                updatePawnSkills(attached);
            }
        }

        public override void connectPawn(Pawn pawn, Hediff_Hivemind hediff)
        {
            base.connectPawn(pawn, hediff);
            updatePawnSkills(pawn);
        }

        public override void disconnectPawn(Pawn pawn, Hediff_Hivemind hediff)
        {
            base.disconnectPawn(pawn, hediff);
            Hediff_DeathGuidance heddifGuidance = hediff as Hediff_DeathGuidance;
            foreach (var skillDef in DefDatabase<SkillDef>.AllDefs)
            {
                SkillRecord pawnSkill = pawn.skills.GetSkill(skillDef);
                float pawnExpirience = pawnSkill.XpTotalEarned + pawnSkill.xpSinceLastLevel - heddifGuidance.addedExpirience[skillDef];
                pawnSkill.Level = getSkillLevel(pawnExpirience);
                pawnSkill.xpSinceLastLevel = getLeftoverXP(pawnExpirience, pawnSkill.Level);
            }
        }

        public void updatePawnSkills(Pawn attached)
        {
            if (attached == null)
            {
                return;
            }

            if (attached.health == null)
            {
                return;
            }

            Hediff_DeathGuidance hediff_DeathGuidance = attachedPawns[attached] as Hediff_DeathGuidance;

            foreach (var skillDef in DefDatabase<SkillDef>.AllDefs)
            {
                SkillRecord pawnSkill = attached.skills.GetSkill(skillDef);
                float pawnExpirience = pawnSkill.XpTotalEarned + pawnSkill.xpSinceLastLevel;
                float hiveExpirience = skillValues[skillDef];

                if (pawnExpirience > hiveExpirience)
                {
                    if (hediff_DeathGuidance.addedExpirience[skillDef] > hiveExpirience)
                    {
                        float xpLost = hediff_DeathGuidance.addedExpirience[skillDef] - hiveExpirience;
                        hediff_DeathGuidance.addedExpirience[skillDef] = hiveExpirience;
                        pawnSkill.Level = getSkillLevel(pawnExpirience - xpLost);
                        pawnSkill.xpSinceLastLevel = getLeftoverXP(pawnExpirience - xpLost, pawnSkill.Level);
                    }
                    continue;
                }

                hediff_DeathGuidance.addedExpirience[skillDef] += hiveExpirience - pawnExpirience;
                pawnSkill.Level = getSkillLevel(hiveExpirience);
                pawnSkill.xpSinceLastLevel = getLeftoverXP(hiveExpirience, pawnSkill.Level);
            }
        }
    }
}

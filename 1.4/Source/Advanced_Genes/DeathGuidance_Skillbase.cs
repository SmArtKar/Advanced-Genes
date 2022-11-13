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
    public class GameComponent_DeathGuidance_Skillbase : GameComponent
    {
        public Dictionary<Faction, DeathGuidance_Dataset> datasets = new Dictionary<Faction, DeathGuidance_Dataset>();
        public List<Faction> factionPlaceholder;
        public List<DeathGuidance_Dataset> datasetPlaceholder;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref datasets, "deathGuidanceDatasets", LookMode.Reference, LookMode.Deep, ref factionPlaceholder, ref datasetPlaceholder);
        }

        public GameComponent_DeathGuidance_Skillbase(Game game)
        {
        }

    }

    public class DeathGuidance_Dataset : IExposable
    {
        public Dictionary<SkillDef, float> skillValues = new Dictionary<SkillDef, float>();
        public List<Pawn> attachedPawns = new List<Pawn>();
        public int totalDead = 0;

        public DeathGuidance_Dataset()
        {
            foreach (var skillDef in DefDatabase<SkillDef>.AllDefs)
            {
                skillValues[skillDef] = 0f;
            }
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref totalDead, "totalDead");
            Scribe_Collections.Look(ref attachedPawns, "attachedPawns", LookMode.Reference);
            Scribe_Collections.Look(ref skillValues, "skillValues", LookMode.Def, LookMode.Value);
        }

        public void absorbCorpse(Pawn corpse)
        {
            HediffSet hediffSet = corpse.health.hediffSet;
            var guidanceHediff = hediffSet.GetFirstHediffOfDef(AG_DefOf.Hediff_DeathGuidance, false) as Hediff_DeathGuidance;
            bool wipeSkills = LoadedModManager.GetMod<AG_Mod>().GetSettings<AG_Settings>().wipeDeathGuidance;
            bool lowerDecay = LoadedModManager.GetMod<AG_Mod>().GetSettings<AG_Settings>().lowerDecayDeathGuidance;

            bool livingPawns = false;
            foreach (var attached in attachedPawns)
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
                foreach (var attached in attachedPawns)
                {
                    updatePawnSkills(attached);
                }
                return;
            }


            if (guidanceHediff == null)
            {
                return; //not supposed to happen but I'll add a sanity check just in case
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

                float corpseExpirience = corpseSkill.XpTotalEarned + corpseSkill.xpSinceLastLevel - guidanceHediff.addedExpirience[skillDef];
                if(lowerDecay && skillValues[skillDef] > corpseExpirience)
                {
                    corpseExpirience = (corpseExpirience * 2 + skillValues[skillDef]) / 3;
                }

                skillValues[skillDef] = (skillValues[skillDef] * totalDead + corpseExpirience) / (totalDead + 1);
            }
            totalDead += 1;
            foreach (var attached in attachedPawns)
            {
                updatePawnSkills(attached);
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

            HediffSet hediffSet = attached.health.hediffSet;
            if (hediffSet == null)
            {
                return;
            }

            var guidanceHediff = hediffSet.GetFirstHediffOfDef(AG_DefOf.Hediff_DeathGuidance, false) as Hediff_DeathGuidance;
            
            if(guidanceHediff == null){
                return;
            }


            foreach (var skillDef in DefDatabase<SkillDef>.AllDefs)
            {
                SkillRecord pawnSkill = attached.skills.GetSkill(skillDef);
                float pawnExpirience = pawnSkill.XpTotalEarned + pawnSkill.xpSinceLastLevel;
                float hiveExpirience = skillValues[skillDef];

                if (pawnExpirience > hiveExpirience)
                {
                    if (guidanceHediff.addedExpirience[skillDef] > hiveExpirience)
                    {
                        float xpLost = guidanceHediff.addedExpirience[skillDef] - hiveExpirience;
                        guidanceHediff.addedExpirience[skillDef] = hiveExpirience;
                        pawnSkill.Level = getSkillLevel(pawnExpirience - xpLost);
                        pawnSkill.xpSinceLastLevel = getLeftoverXP(pawnExpirience - xpLost, pawnSkill.Level);
                    }
                    continue;
                }

                guidanceHediff.addedExpirience[skillDef] += hiveExpirience - pawnExpirience;
                pawnSkill.Level = getSkillLevel(hiveExpirience);
                pawnSkill.xpSinceLastLevel = getLeftoverXP(hiveExpirience, pawnSkill.Level);
            }
        }

        public int getSkillLevel(float xp)
        {
            float xpRequired = 0f;
            int currentLevel = 0;
            while (xpRequired <= xp && currentLevel < 20)
            {
                xpRequired += SkillRecord.XpRequiredToLevelUpFrom(currentLevel);
                currentLevel++;
            }

            if(xpRequired > xp)
            {
                currentLevel -= 1;
                xpRequired -= SkillRecord.XpRequiredToLevelUpFrom(currentLevel);
            }

            return currentLevel;
        }

        public float getLeftoverXP(float xp, int level)
        {
            for (int i = 0; i < level; i++)
            {
                xp -= SkillRecord.XpRequiredToLevelUpFrom(i);
            }
            return xp;
        }
    }
}

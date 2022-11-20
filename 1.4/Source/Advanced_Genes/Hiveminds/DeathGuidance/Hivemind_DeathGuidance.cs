using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using Verse;
using static HarmonyLib.Code;

namespace Advanced_Genes
{
    public class Hivemind_DeathGuidance : Hivemind
    {
        public Dictionary<SkillDef, float> skillValues = new Dictionary<SkillDef, float>();
        public int totalDead = 0;

        public Dictionary<string, float> damageMultipliers = new Dictionary<string, float>()
        {
            { "Soulblast", 1f }
        };

        public Dictionary<string, bool> specialAbilties = new Dictionary<string, bool>()
        {
            { "Soulblast", false }
        };

        public Hivemind_DeathGuidance() 
        { 
            overseerCasts = new Dictionary<AbilityDef, int>()
            {
                { AG_DefOf.AG_Ability_Soulblast, 3 }
            };
        }

        public Hivemind_DeathGuidance(string hiveName, Faction hiveFaction)
        {
            foreach (SkillDef skillDef in DefDatabase<SkillDef>.AllDefs)
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

        public override string getHivemindName
        {
            get
            {
                return (LoadedModManager.GetMod<AG_Mod>().GetSettings<AG_Settings>().fiestaMode) ? "4Chan" : "Guidance Of The Dead";
            }
        }

        public override int overseerMemberRequirement
        {
            get
            {
                return Math.Max(1, 3 - totalDead);
            }
        }

        public override string getHivemindIcon
        {
            get
            {
                return "UI/Icons/Genes/Gene_DeathGuidance";
            }
        }

        public override string getOverseerIcon
        {
            get
            {
                return "UI/Icons/Hivemind/OverseerSkull";
            }
        }
        public override HediffDef overseerHediffDef
        {
            get
            {
                return AG_DefOf.Hediff_OverseerDeathGuidance;
            }
        }

        public override Vector2 getRectSize(Pawn pawn)
        {
            return new Vector2(680f, 400f);
        }

        public override void renderHivemindMenu(Rect tabRect, Pawn pawn)
        {
            GUI.BeginGroup(tabRect);
            Listing_Standard listing = new();
            listing.Begin(tabRect);
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(10f, 5f, 300f, 50f), hiveName);
            Text.Font = GameFont.Small;
            renderPawnMenu(new Rect(5f, 35f, 250f, 355f), pawn);
            renderSkillMenu(new Rect(265f, 60f, 120f, 380f), pawn);
            drawVerticalLine(395f, 5f, 375f);
            renderOverseerMenu(new Rect(400f, 10f, 276f, 380f), pawn);
            listing.End();
            GUI.EndGroup();
        }

        public void renderSkillMenu(Rect tabRect, Pawn pawn)
        {
            for (int skillIter = 0; skillIter < skillValues.Count; skillIter++)
            {
                SkillDef skillDef = skillValues.Keys.ToList()[skillIter];
                float rectY = (float)skillIter * 27f + tabRect.yMin;
                Rect skillRect = new Rect(tabRect.xMin, rectY, 120f, 24f);  
                int level = getSkillLevel(skillValues[skillDef]);
                float fillPercent = Mathf.Max(0.01f, (float)level / 20f);
                Widgets.FillableBar(skillRect, fillPercent, SolidColorMaterials.NewSolidColorTexture(new Color(1f, 1f, 1f, 0.1f)), null, doBorder: false);
                Rect textRect = new Rect(tabRect.xMin + 4f, rectY, 230f, 24f);
                GenUI.SetLabelAlign(TextAnchor.MiddleLeft);
                Widgets.Label(textRect, skillDef.skillLabel.CapitalizeFirst() + " - " + level.ToStringCached());
                GenUI.ResetLabelAlign();
            }
        }

        public void absorbCorpse(Pawn corpse)
        {
            bool lowerDecay = LoadedModManager.GetMod<AG_Mod>().GetSettings<AG_Settings>().lowerDecayDeathGuidance;

            foreach (SkillDef skillDef in DefDatabase<SkillDef>.AllDefs)
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

                Hediff_HivemindDeathGuidance hediff_DeathGuidance = attachedPawns[corpse] as Hediff_HivemindDeathGuidance;
                float corpseExpirience = corpseSkill.XpTotalEarned + corpseSkill.xpSinceLastLevel - hediff_DeathGuidance.addedExpirience[skillDef];
                if (lowerDecay && skillValues[skillDef] > corpseExpirience)
                {
                    corpseExpirience = (corpseExpirience * 2 + skillValues[skillDef]) / 3;
                }

                skillValues[skillDef] = (skillValues[skillDef] * totalDead + corpseExpirience) / (totalDead + 1);
            }

            totalDead += 1;

            foreach (Pawn pawn in attachedPawns.Keys.ToList())
            {
                updatePawnSkills(pawn);
            }
        }

        public override void connectPawn(Pawn pawn, Hediff_Hivemind hediff)
        {
            base.connectPawn(pawn, hediff);
            updatePawnSkills(pawn);
        }

        public override void disconnectPawn(Pawn pawn, Hediff_Hivemind hediff)
        {
            disableSkillBoost(pawn);
            base.disconnectPawn(pawn, hediff);
        }

        public void updatePawnSkills(Pawn pawn)
        {
            if (pawn == null)
            {
                return;
            }

            if (pawn.health == null)
            {
                return;
            }

            Hediff_HivemindDeathGuidance hediff_DeathGuidance = attachedPawns[pawn] as Hediff_HivemindDeathGuidance;

            if (hediff_DeathGuidance == null)
            {
                return;
            }

            foreach (SkillDef skillDef in DefDatabase<SkillDef>.AllDefs)
            {
                SkillRecord pawnSkill = pawn.skills.GetSkill(skillDef);
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

        public void disableSkillBoost(Pawn pawn)
        {
            if (pawn == null)
            {
                return;
            }

            if (pawn.health == null)
            {
                return;
            }

            Hediff_HivemindDeathGuidance hediff_DeathGuidance = attachedPawns[pawn] as Hediff_HivemindDeathGuidance;

            if (hediff_DeathGuidance == null)
            {
                return;
            }

            foreach (SkillDef skillDef in DefDatabase<SkillDef>.AllDefs)
            {
                SkillRecord pawnSkill = pawn.skills.GetSkill(skillDef);
                float pawnExpirience = pawnSkill.XpTotalEarned + pawnSkill.xpSinceLastLevel;
                float hiveExpirience = hediff_DeathGuidance.addedExpirience[skillDef];

                pawnSkill.Level = getSkillLevel(pawnExpirience - hiveExpirience);
                pawnSkill.xpSinceLastLevel = getLeftoverXP(pawnExpirience - hiveExpirience, pawnSkill.Level);
                hediff_DeathGuidance.addedExpirience[skillDef] = 0;
            }
        }
    }
}

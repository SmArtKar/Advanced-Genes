using RimWorld;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Advanced_Genes
{
    public class Hediff_DeathGuidance : HediffWithComps
    {
        public Dictionary<SkillDef, float> addedExpirience = new Dictionary<SkillDef, float>();
        public GameComponent_DeathGuidance_Skillbase globalSkillbase = Current.Game.GetComponent<GameComponent_DeathGuidance_Skillbase>();
        
        public Hediff_DeathGuidance()
        {
            foreach (var skillDef in DefDatabase<SkillDef>.AllDefs)
            {
                addedExpirience[skillDef] = 0;
            }
        }

        
        public override void PostAdd(DamageInfo? dinfo)
        {
            base.PostAdd(dinfo);

            if (!globalSkillbase.datasets.ContainsKey(pawn.Faction))
            {
                DeathGuidance_Dataset newDataset = new DeathGuidance_Dataset();
                newDataset.attachedPawns.Add(pawn);
                globalSkillbase.datasets[pawn.Faction] = newDataset;
                return;
            }
            globalSkillbase.datasets[pawn.Faction].attachedPawns.Add(pawn);
            globalSkillbase.datasets[pawn.Faction].updatePawnSkills(pawn);
        }

        
        public override void PostRemoved()
        {
            base.PostRemoved();
            if (!globalSkillbase.datasets.ContainsKey(pawn.Faction))
            {
                return;
            }

        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref addedExpirience, "addedExpirience", LookMode.Def, LookMode.Value);
        }

        public override void Notify_PawnKilled()
        {
            base.Notify_PawnKilled();
            globalSkillbase.datasets[pawn.Faction].absorbCorpse(this.pawn);
        }

        public override void Notify_KilledPawn(Pawn victim, DamageInfo? dinfo)
        {
            base.Notify_KilledPawn(victim, dinfo);
        }
    }
}

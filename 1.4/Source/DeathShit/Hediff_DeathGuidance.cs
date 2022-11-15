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
    public class Hediff_DeathGuidance : Hediff_Hivemind
    {
        public Dictionary<SkillDef, float> addedExpirience = new Dictionary<SkillDef, float>();

        public Hediff_DeathGuidance()
        {
            foreach (var skillDef in DefDatabase<SkillDef>.AllDefs)
            {
                addedExpirience[skillDef] = 0;
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
            if (connectedHivemind != null)
            {
                Hivemind_DeathGuidance deathHivemind = connectedHivemind as Hivemind_DeathGuidance;
                deathHivemind.absorbCorpse(this.pawn);
            }
            disconnectFromHivemind();
        }
        public override void Notify_KilledPawn(Pawn victim, DamageInfo? dinfo)
        {
            base.Notify_KilledPawn(victim, dinfo);
        }
        public override bool disconnectOnDeath
        {
            get
            {
                return false;
            }
        }
    }
}

using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Advanced_Genes
{
    public class Hediff_HivemindDeathGuidance : Hediff_Hivemind
    {
        public Dictionary<SkillDef, float> addedExpirience = new Dictionary<SkillDef, float>();

        public Hediff_HivemindDeathGuidance()
        {
            foreach (SkillDef skillDef in DefDatabase<SkillDef>.AllDefs)
            {
                if (!addedExpirience.ContainsKey(skillDef))
                {
                    addedExpirience[skillDef] = 0;
                }
            }
        }

        public override string getDefaultHivemindIcon
        {
            get
            {
                return "UI/Icons/Genes/Gene_DeathGuidance";
            }
        }

        public override string getDefaultHivemindName
        {
            get
            {
                return (LoadedModManager.GetMod<AG_Mod>().GetSettings<AG_Settings>().fiestaMode) ? "4Chan" : "Guidance Of The Dead";
            }
        }

        public override Hivemind createNewHivemind(string name)
        {
            Hivemind_DeathGuidance newHive = new Hivemind_DeathGuidance(name, pawn.Faction);
            attachToHivemind(newHive);
            return newHive;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref addedExpirience, "addedExpirience", LookMode.Def, LookMode.Value);
        }

        public override void Notify_PawnKilled()
        {
            if (connectedHivemind != null)
            {
                Hivemind_DeathGuidance deathHivemind = connectedHivemind as Hivemind_DeathGuidance;
                deathHivemind.absorbCorpse(this.pawn);
            }
            
            base.Notify_PawnKilled();
        }

        public override void Notify_KilledPawn(Pawn victim, DamageInfo? dinfo)
        {
            base.Notify_KilledPawn(victim, dinfo);
        }
    }
}

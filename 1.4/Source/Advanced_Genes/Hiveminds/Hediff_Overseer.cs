using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Advanced_Genes
{
    public class Hediff_Overseer : HediffWithComps, IExposable
    {
        public GameComponent_Hiveminds hivemindsComponent = Current.Game.GetComponent<GameComponent_Hiveminds>();
        public Hivemind connectedHivemind;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref connectedHivemind, "connectedHivemind");
        }

        public override void Notify_PawnKilled()
        {
            base.Notify_PawnKilled();
            if (connectedHivemind == null) return;
            connectedHivemind.overseerDeath();
        }

        public override void Notify_KilledPawn(Pawn victim, DamageInfo? dinfo)
        {
            base.Notify_KilledPawn(victim, dinfo);
        }

        public override void PostAdd(DamageInfo? dinfo)
        {
            base.PostAdd(dinfo);
        }

        public override void PostRemoved()
        {
            base.PostRemoved();
            removeAbilties();
        }

        public void recalculateAbilities()
        {
            foreach (AbilityDef abilityDef in connectedHivemind.overseerCasts.Keys)
            {
                if (connectedHivemind.canUseAbility(abilityDef) == null) //No need for existance checks because those already exist in Gain/RemoveAbility
                {
                    pawn.abilities.GainAbility(abilityDef);
                }
                else
                {
                    pawn.abilities.RemoveAbility(abilityDef);
                }
            }
        }
        public void removeAbilties()
        {
            foreach (AbilityDef abilityDef in connectedHivemind.overseerCasts.Keys)
            {
                pawn.abilities.RemoveAbility(abilityDef);
            }
        }
    }
}

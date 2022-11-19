using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Advanced_Genes.Hiveminds
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

        public virtual List<AbilityDef> getOverseerCasts()
        {
            return new List<AbilityDef>();
        }

        public override void Notify_PawnKilled()
        {
            base.Notify_PawnKilled();
            if (connectedHivemind == null) return;
            connectedHivemind.overseerDeath(this);
        }

        public override void Notify_KilledPawn(Pawn victim, DamageInfo? dinfo)
        {
            base.Notify_KilledPawn(victim, dinfo);
        }
    }
}

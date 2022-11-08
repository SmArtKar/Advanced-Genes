using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using static UnityEngine.GraphicsBuffer;

namespace Advanced_Genes
{
    public class Gene_BloodDeficiency : Gene
    {
        public override void Tick()
        {
            base.Tick();
            if (this.pawn.IsHashIntervalTick(60))
            {
                HediffSet hediffSet = this.pawn.health.hediffSet;
                Hediff firstHediffOfDef = hediffSet.GetFirstHediffOfDef(HediffDefOf.BloodLoss, false);
                if (firstHediffOfDef != null && hediffSet.BleedRateTotal < 0.1f)
                {
                    HealthUtility.AdjustSeverity(pawn, HediffDefOf.BloodLoss, 0.00022222222f); // By default, 0.00033333333f is lost every 60th tick
                }
            }
        }
    }
}

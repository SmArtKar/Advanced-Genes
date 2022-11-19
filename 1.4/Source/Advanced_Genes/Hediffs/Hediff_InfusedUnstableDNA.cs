using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Advanced_Genes
{
    public class Hediff_InfusedUnstableDNA : Hediff_UnstableDNA
    {
        public Hediff_InfusedUnstableDNA()
        {
            tickInterval = GenDate.TicksPerDay / 24;
        }

        public override void randomizeGenes()
        {
            base.randomizeGenes();
            tickInterval = GenDate.TicksPerDay / 24;
        }
    }
}

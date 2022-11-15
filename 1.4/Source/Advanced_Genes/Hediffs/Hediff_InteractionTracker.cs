using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Advanced_Genes
{
    public class Hediff_InteractionTracker : HediffWithComps
    {
        public virtual void trackInteraction(Pawn recipient, InteractionDef intDef) { }
    }
}

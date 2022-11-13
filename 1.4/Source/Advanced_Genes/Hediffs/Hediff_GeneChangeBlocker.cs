using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Advanced_Genes
{
    internal class Hediff_GeneChangeBlocker : HediffWithComps
    {
        public virtual bool blockGeneChange(ref Gene gene, ref bool addAsXenogene)
        {
            return false;
        }
        public virtual bool blockGeneRemoval(ref Gene gene)
        {
            return false;
        }

        public virtual bool blockPawnScan(ref Pawn pawn)
        {
            return false;
        }
    }
}

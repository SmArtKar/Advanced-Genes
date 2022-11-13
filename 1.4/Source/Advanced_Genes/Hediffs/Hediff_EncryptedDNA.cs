using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Advanced_Genes
{
    internal class Hediff_EncryptedDNA : Hediff_GeneChangeBlocker
    {
        public override bool blockGeneChange(ref Gene gene, ref bool addAsXenogene)
        {
            if (PawnGenerator.IsBeingGenerated(pawn) is true)
            {
                return false;
            }

            return true;
        }

        public override bool blockGeneRemoval(ref Gene gene)
        {
            if (PawnGenerator.IsBeingGenerated(pawn) is true)
            {
                return false;
            }

            return true;
        }

        public override bool blockPawnScan(ref Pawn pawn)
        {
            return true;
        }
    }
}

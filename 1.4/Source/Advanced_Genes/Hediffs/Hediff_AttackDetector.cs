using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace Advanced_Genes
{
    public class Hediff_AttackDetector : HediffWithComps
    {
        public virtual void PostApplyDamage(ref DamageInfo dinfo, ref float totalDamageDealt) { }
    }
}

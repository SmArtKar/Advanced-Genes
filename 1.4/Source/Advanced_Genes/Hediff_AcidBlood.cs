using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using VFECore;
using static HarmonyLib.Code;

namespace Advanced_Genes
{
    internal class Hediff_AcidBlood : HediffComp_AttackDetector
    {
        public override void PostApplyDamage(ref DamageInfo dinfo, ref float totalDamageDealt)
        {
            //Messages.Message("1 PostApplyDamage", MessageTypeDefOf.NeutralEvent);
            if (dinfo.Def.isRanged)
            {
                return;
            }

            //Messages.Message("2 PostApplyDamage", MessageTypeDefOf.NeutralEvent);

            Pawn pawnAttacker = dinfo.Instigator as Pawn;
            if (pawnAttacker == null)
            {
                return;
            }

            //Messages.Message("3 PostApplyDamage", MessageTypeDefOf.NeutralEvent);

            AnimalBehaviours.CompAcidImmunity comp = pawnAttacker.TryGetComp<AnimalBehaviours.CompAcidImmunity>();
            if (comp != null)
            {
                return;
            }

            //Messages.Message("4 PostApplyDamage", MessageTypeDefOf.NeutralEvent);

            pawnAttacker.TakeDamage(new DamageInfo(DefDatabase<DamageDef>.GetNamed("AcidBurn"), dinfo.Amount / 5, 0.3f, -1f, null, null, null, DamageInfo.SourceCategory.ThingOrUnknown, null));
        }
    }
}
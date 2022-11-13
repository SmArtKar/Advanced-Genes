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
    internal class Hediff_AcidBlood : Hediff_AttackDetector
    {
        public float armorPenetration = 0.3f;
        public float damageModifier = 0.2f;

        public override void PostApplyDamage(ref DamageInfo dinfo, ref float totalDamageDealt)
        {
            if (!dinfo.Def.harmsHealth || dinfo.Amount <= 0)
            {
                return;
            }

            if (dinfo.Def.isRanged)
            {
                return;
            }


            Pawn pawnAttacker = dinfo.Instigator as Pawn;
            if (pawnAttacker == null)
            {
                return;
            }


            AnimalBehaviours.CompAcidImmunity comp = pawnAttacker.TryGetComp<AnimalBehaviours.CompAcidImmunity>();
            if (comp != null)
            {
                return;
            }


            pawnAttacker.TakeDamage(new DamageInfo(DefDatabase<DamageDef>.GetNamed("AcidBurn"), dinfo.Amount * damageModifier, armorPenetration, -1f, null, null, null, DamageInfo.SourceCategory.ThingOrUnknown, null));
        }
    }
}
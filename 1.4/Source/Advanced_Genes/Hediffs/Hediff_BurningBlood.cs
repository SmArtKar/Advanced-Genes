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
    internal class Hediff_BurningBlood : Hediff_AttackDetector
    {
        public FloatRange fireRandom = new FloatRange(0f, 1f);
        public float fireChance;
        public float fireChanceSelf;

        public Hediff_BurningBlood()
        {
            fireChance = LoadedModManager.GetMod<AG_Mod>().GetSettings<AG_Settings>().chanceBurningBlood;
            fireChanceSelf = LoadedModManager.GetMod<AG_Mod>().GetSettings<AG_Settings>().chanceSelfBurningBlood;
        }

        public override void PostApplyDamage(ref DamageInfo dinfo, ref float totalDamageDealt)
        {
            if (!dinfo.Def.harmsHealth || dinfo.Amount <= 0)
            {
                return;
            }

            if (fireRandom.RandomInRange < fireChance)
            {
                pawn.TryAttachFire(25f);
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

            if (fireRandom.RandomInRange < fireChance)
            {
                pawnAttacker.TryAttachFire(25f);
            }
        }

        public override void Notify_PawnKilled()
        {
            base.Notify_PawnKilled();
            pawn.TryAttachFire(100f);
            GenExplosion.DoExplosion(pawn.Position, pawn.Map, 3, DamageDefOf.Flame, pawn, (int)(DamageDefOf.Flame.defaultDamage * 1.5f), ignoredThings: new List<Thing> { pawn });
        }
        public override void Notify_KilledPawn(Pawn victim, DamageInfo? dinfo)
        {
            base.Notify_KilledPawn(victim, dinfo);
        }
    }
}
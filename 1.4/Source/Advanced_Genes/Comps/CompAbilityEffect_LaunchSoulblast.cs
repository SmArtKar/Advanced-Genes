using Mono.Unix.Native;
using RimWorld;
using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using static UnityEngine.GraphicsBuffer;

namespace Advanced_Genes
{
    public class CompAbilityEffect_LaunchSoulblast : CompAbilityEffect
    {
        public new CompProperties_AbilityLaunchSoulblast Props => (CompProperties_AbilityLaunchSoulblast)props;

        public int shotsLeft = 0;
        public int shotTicks = 0;

        public LocalTargetInfo target;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            this.target = target;
            shotsLeft = 3;
        }

        public override void CompTick()
        {
            base.CompTick();
            if (shotsLeft > 0)
            {
                shotTicks--;
                if (shotTicks <= 0)
                {
                    shotsLeft--;
                    shotTicks = 10;
                    LaunchProjectile(target);
                }
            }
        }

        private void LaunchProjectile(LocalTargetInfo target)
        {
            Pawn pawn = parent.pawn;
            Soulblast soulblast = GenSpawn.Spawn(Props.projectileDef, pawn.Position, pawn.Map) as Soulblast;
            Hediff_HivemindDeathGuidance hiveHediff = pawn.health.hediffSet.GetFirstHediffOfDef(AG_DefOf.Hediff_DeathGuidance) as Hediff_HivemindDeathGuidance;
            if (hiveHediff == null)
            {
                return;
            }
            soulblast.connectedHivemind = hiveHediff.connectedHivemind as Hivemind_DeathGuidance;
            soulblast.Launch(pawn, pawn.DrawPos, target, target, ProjectileHitFlags.IntendedTarget);
        }

        public override bool AICanTargetNow(LocalTargetInfo target)
        {
            return target.Pawn != null;
        }
    }
}

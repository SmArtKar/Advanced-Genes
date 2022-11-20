using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Advanced_Genes
{
    public class Soulblast : Bullet //Low base damage but quick scaling with soul amount
    {
        public Hivemind_DeathGuidance connectedHivemind;

        public override int DamageAmount
        {
            get
            {
                if (connectedHivemind == null)
                {
                    return (int)(def.projectile.GetDamageAmount(weaponDamageMultiplier));
                }

                return (int)(def.projectile.GetDamageAmount(weaponDamageMultiplier) * (1 + Math.Sqrt(connectedHivemind.totalDead) / 10) * connectedHivemind.damageMultipliers["Soulblast"]);
            }
        }
    }
}

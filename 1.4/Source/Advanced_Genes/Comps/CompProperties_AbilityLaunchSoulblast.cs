using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Advanced_Genes
{
    public class CompProperties_AbilityLaunchSoulblast : CompProperties_AbilityEffect
    {
        public ThingDef projectileDef;

        public CompProperties_AbilityLaunchSoulblast()
        {
            compClass = typeof(CompAbilityEffect_LaunchSoulblast);
        }
    }
}


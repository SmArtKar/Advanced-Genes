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
    public class Thought_PsychicBeacon : Thought_Memory
    {
        public Pawn beacon;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref beacon, "beacon");
        }

        public override bool ShouldDiscard
        {
            get
            {
                if (beacon.health.Dead || beacon.relations == null)
                {
                    return true;
                }

                if (!beacon.health.hediffSet.HasHediff(AG_DefOf.Hediff_PsychicBeacon))
                {
                    return true;
                }

                return base.ShouldDiscard;
            }
        }

        public override float MoodOffset()
        {
            if (ThoughtUtility.ThoughtNullified(pawn, def))
            {
                return 0f;
            }

            if (ShouldDiscard)
            {
                return 0f;
            }

            float num = base.MoodOffset();
            float statValue = beacon.GetStatValue(StatDefOf.PsychicSensitivity, applyPostProcess: true, 1);
            float opinion = beacon.relations.OpinionOf(pawn);
            return num * statValue * opinion * 0.01f;
        }

        public override bool GroupsWith(Thought other)
        {
            if (!(other is Thought_PsychicBeacon thought_PsychicBeacon))
            {
                return false;
            }

            if (thought_PsychicBeacon.beacon == beacon)
            {
                return base.GroupsWith(other);
            }

            return false;
        }
    }
}

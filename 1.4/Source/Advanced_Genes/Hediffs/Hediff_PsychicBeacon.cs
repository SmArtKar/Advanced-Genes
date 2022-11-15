using Mono.Unix.Native;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using static HarmonyLib.Code;

namespace Advanced_Genes
{
    public class Hediff_PsychicBeacon : Hediff_InteractionTracker
    {
        public override void trackInteraction(Pawn recipient, InteractionDef intDef) 
        {
            Thought_PsychicBeacon thoughtBeacon = (Thought_PsychicBeacon)ThoughtMaker.MakeThought(AG_DefOf.ThoughtPsychicBeacon);
            thoughtBeacon.beacon = pawn;
            recipient.needs.mood.thoughts.memories.TryGainMemory(thoughtBeacon, pawn);
        }
    }
}

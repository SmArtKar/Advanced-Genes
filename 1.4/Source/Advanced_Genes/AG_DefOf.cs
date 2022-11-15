using Advanced_Genes;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

[DefOf]
public static class AG_DefOf
{
    //public static HediffDef Hediff_DeathGuidance;
    public static HediffDef Hediff_PsychicBeacon;
    public static ThoughtDef ThoughtPsychicBeacon;

    static AG_DefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(AG_DefOf));
    }
}

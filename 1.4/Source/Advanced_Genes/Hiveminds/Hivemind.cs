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
    public class Hivemind : IExposable
    {
        public GameComponent_Hiveminds hivemindsComponent = Current.Game.GetComponent<GameComponent_Hiveminds>();
        public string hiveName;

        public Faction attachedFaction; //No cross-faction hiveminds
        public Dictionary<Pawn, Hediff_Hivemind> attachedPawns = new Dictionary<Pawn, Hediff_Hivemind>();

        public List<Pawn> pawnPlaceholder;
        public List<Hediff_Hivemind> hediffPlaceholder;

        public Hivemind(string hiveName, Faction hiveFaction)
        {
            this.hiveName = hiveName;
            attachedFaction = hiveFaction;
            hivemindsComponent.hiveminds.Add(this);
        }

        public virtual void ExposeData()
        {
            Scribe_Collections.Look(ref attachedPawns, "attachedPawns", LookMode.Reference, LookMode.Reference, ref pawnPlaceholder, ref hediffPlaceholder);
        }

        public virtual void connectPawn(Pawn pawn, Hediff_Hivemind hediff)
        {
            attachedPawns[pawn] = hediff;
        }

        public virtual void disconnectPawn(Pawn pawn, Hediff_Hivemind hediff)
        {
            attachedPawns.Remove(pawn);
        }

        public virtual Vector2 getRectPosition(Pawn pawn)
        {
            return new Vector2(17f, 17f);
        }
        public virtual Vector2 getRectSize(Pawn pawn)
        {
            return new Vector2(17f, 17f);
        }

        public virtual void renderHivemindMenu(Rect tabRect, Pawn pawn)
        {

        }

        public int getSkillLevel(float xp)
        {
            float xpRequired = 0f;
            int currentLevel = 0;
            while (xpRequired <= xp && currentLevel < 20)
            {
                xpRequired += SkillRecord.XpRequiredToLevelUpFrom(currentLevel);
                currentLevel++;
            }

            if (xpRequired > xp)
            {
                currentLevel -= 1;
                xpRequired -= SkillRecord.XpRequiredToLevelUpFrom(currentLevel);
            }

            return currentLevel;
        }

        public float getLeftoverXP(float xp, int level)
        {
            for (int i = 0; i < level; i++)
            {
                xp -= SkillRecord.XpRequiredToLevelUpFrom(i);
            }
            return xp;
        }
    }
}

using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using Verse;
using static HarmonyLib.Code;

namespace Advanced_Genes
{
    public class Hivemind : IExposable, ILoadReferenceable
    {
        public GameComponent_Hiveminds hivemindsComponent = Current.Game.GetComponent<GameComponent_Hiveminds>();
        public string hiveName;
        public Pawn currentOverseer;
        public int overseerAssignmentTickCooldown = -1;

        public Faction attachedFaction; //No cross-faction hiveminds
        public Dictionary<Pawn, Hediff_Hivemind> attachedPawns = new Dictionary<Pawn, Hediff_Hivemind>();
        public Dictionary<AbilityDef, int> overseerCasts = new Dictionary<AbilityDef, int>();

        public List<Pawn> pawnPlaceholder;
        public List<Hediff_Hivemind> hediffPlaceholder;

        private Vector2 scrollPosition;
        public int loadID = 0;
        private static readonly Color disabledColor = new Color(1f, 1f, 1f, 0.5f);

        public string GetUniqueLoadID()
        {
            return "HivemindInstance_" + loadID;
        }

        public Hivemind() { }

        public Hivemind(string hiveName, Faction hiveFaction)
        {
            this.hiveName = hiveName;
            attachedFaction = hiveFaction;
            hivemindsComponent.hiveminds.Add(this);
            loadID = hivemindsComponent.getNextHiveID();
        }

        public virtual int overseerAssignmentCooldown
        {
            get
            {
                return GenDate.TicksPerDay * 3; // 72 hours
            }
        }

        public virtual int overseerDeathCooldown
        {
            get
            {
                return (int)(GenDate.TicksPerDay * 5f); // 120 hours
            }
        }

        public virtual int overseerMemberRequirement
        {
            get
            {
                return 3;
            }
        }

        public virtual string getHivemindIcon
        {
            get
            {
                return "UI/Icons/Genes/Gene_GestaltConsciousness";
            }
        }
        public virtual string getOverseerIcon
        {
            get
            {
                return "UI/Icons/Hivemind/OverseerGestalt";
            }
        }

        public virtual string getHivemindName
        {
            get
            {
                return (LoadedModManager.GetMod<AG_Mod>().GetSettings<AG_Settings>().fiestaMode) ? "Reddit" : "Gestalt Consciousness";
            }
        }

        public virtual HediffDef overseerHediffDef
        {
            get
            {
                return null;
            }
        }

        public virtual void ExposeData()
        {
            Scribe_Collections.Look(ref attachedPawns, "attachedPawns", LookMode.Reference, LookMode.Reference, ref pawnPlaceholder, ref hediffPlaceholder);
            Scribe_References.Look(ref attachedFaction, "attachedFaction");
            Scribe_Values.Look(ref hiveName, "hiveName");
            Scribe_Values.Look(ref loadID, "loadID", 0);
            Scribe_References.Look(ref currentOverseer, "currentOverseer");
            Scribe_Values.Look(ref overseerAssignmentTickCooldown, "overseerAssignmentTickCooldown");
        }

        public virtual void connectPawn(Pawn pawn, Hediff_Hivemind hediff)
        {
            attachedPawns[pawn] = hediff;

            if (currentOverseer != null)
            {
                Hediff_Overseer overseerHediff = currentOverseer.health.hediffSet.GetFirstHediffOfDef(overseerHediffDef) as Hediff_Overseer;
                overseerHediff.recalculateAbilities();
            }
        }

        public virtual void disconnectPawn(Pawn pawn, Hediff_Hivemind hediff)
        {
            attachedPawns.Remove(pawn);
            if (attachedPawns.Count < overseerMemberRequirement)
            {
                removeOverseer();
            }

            if (currentOverseer != null)
            {
                Hediff_Overseer overseerHediff = currentOverseer.health.hediffSet.GetFirstHediffOfDef(overseerHediffDef) as Hediff_Overseer;
                overseerHediff.recalculateAbilities();
            }

            if (attachedPawns.Count == 0)
            {
                Current.Game.GetComponent<GameComponent_Hiveminds>().hiveminds.Remove(this); //The only thing that could(should) referencing it so doing this should delete it
            }
        }

        public virtual Vector2 getRectPosition(Pawn pawn)
        {
            return new Vector2(5f, 5f);
        }

        public virtual Vector2 getRectSize(Pawn pawn)
        {
            return new Vector2(550f, 400f);
        }

        public virtual void renderHivemindMenu(Rect tabRect, Pawn pawn)
        {
            GUI.BeginGroup(tabRect);
            Listing_Standard listing = new();
            listing.Begin(tabRect);
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(10f, 5f, 300f, 50f), hiveName);
            Text.Font = GameFont.Small;
            renderPawnMenu(new Rect(5f, 35f, 250f, 320f), pawn);
            drawVerticalLine(260f, 5f, 375f);
            renderOverseerMenu(new Rect(265f, 10f, 276f, 380f), pawn);
            listing.End();
            GUI.EndGroup();
        }

        public void drawVerticalLine(float x, float y, float length)
        {
            Color color = GUI.color;
            GUI.color = color * new Color(1f, 1f, 1f, 0.4f);
            Widgets.DrawLineVertical(x, y, length);
            GUI.color = color;
        }

        public virtual bool attemptAssignOverseer(Rect tabRect, Pawn pawn)
        {
            int pawnCount = attachedPawns.Keys.ToList().Where((Pawn x) => !x.health.Dead).Count(); //for hiveminds that don't disconnect their members upon death for some reason
            if (pawnCount < overseerMemberRequirement)
            {
                Rect lockedButton = new(tabRect.xMin + 0f + (tabRect.width - 250f) / 2, tabRect.yMin + 148f, 250f, 50f);
                GUI.color = disabledColor;
                Widgets.ButtonText(lockedButton, "Unable to assign an Overseer: Not enough hivemind members (" + pawnCount + "/" + overseerMemberRequirement + ")", active: false);
                GUI.color = Color.white;
                return false;
            }
            
            if (overseerAssignmentTickCooldown > Find.TickManager.TicksGame)
            {
                Rect lockedButton = new(tabRect.xMin + 0f + (tabRect.width - 250f) / 2, tabRect.yMin + 148f, 250f, 50f);
                GUI.color = disabledColor;
                Widgets.ButtonText(lockedButton, "Unable to assign an Overseer: Wait " + ((int)((overseerAssignmentTickCooldown - Find.TickManager.TicksGame) / GenDate.TicksPerDay * 10)) / 10 + " more days", active: false);
                GUI.color = Color.white;
                return false;
            }
            
            if (attachedPawns.Keys.ToList().Where((Pawn x) => canBecomeOverseer(x)).Count() == 0)
            {
                Rect lockedButton = new(tabRect.xMin + 0f + (tabRect.width - 250f) / 2, tabRect.yMin + 148f, 250f, 50f);
                GUI.color = disabledColor;
                Widgets.ButtonText(lockedButton, "Unable to assign an Overseer: No pawns fitting the requirements", active: false);
                GUI.color = Color.white;
                return false;
            }
            return true;
        }

        public virtual bool attemptReassignOverseer(Rect tabRect, Pawn pawn)
        {
            if (overseerAssignmentTickCooldown > Find.TickManager.TicksGame)
            {
                Rect lockedButton = new(tabRect.xMin + 0f + (tabRect.width - 250f) / 2, tabRect.yMin + 148f, 250f, 50f);
                GUI.color = disabledColor;
                Widgets.ButtonText(lockedButton, "Unable to reassign an Overseer: Wait " + ((int)((overseerAssignmentTickCooldown - Find.TickManager.TicksGame) / GenDate.TicksPerDay * 10)) / 10 + " more days", active: false);
                GUI.color = Color.white;
                return false;
            }
            return true;
        }

        public virtual void renderOverseerMenu(Rect tabRect, Pawn pawn)
        {
            Rect backgroundRect = new(tabRect.xMin + 0f + (tabRect.width - 138f) / 2, tabRect.yMin + 0f, 138f, 138f);
            GUI.DrawTexture(backgroundRect, ContentFinder<Texture2D>.Get("UI/Icons/UI_Background"));
            if (currentOverseer == null)
            {
                Rect overseerImageRect = new(tabRect.xMin + 5f + (tabRect.width - 138f) / 2, tabRect.yMin + 5f, 128f, 128f);
                Texture2D overseerIcon = ContentFinder<Texture2D>.Get(getOverseerIcon);
                GUI.DrawTexture(overseerImageRect, overseerIcon);
                if (attemptAssignOverseer(tabRect, pawn))
                {
                    Rect assignButton = new(tabRect.xMin + 0f + (tabRect.width - 250f) / 2, tabRect.yMin + 148f, 250f, 35f);
                    if(Widgets.ButtonText(assignButton, "Assign an Overseer"))
                    {
                        List<Pawn> potentialOverseers = attachedPawns.Keys.ToList().Where((Pawn x) => canBecomeOverseer(x)).ToList();
                        List<FloatMenuOption> options = new List<FloatMenuOption>();
                        foreach (Pawn selectOverseer in potentialOverseers)
                        {
                            options.Add(new FloatMenuOption(selectOverseer.Name.ToStringFull.CapitalizeFirst(), delegate
                            {
                                selectNewOverseer(selectOverseer);
                            }));
                        }
                        Find.WindowStack.Add(new FloatMenu(options));
                    }
                }
                return;
            }

            Rect imageRect = new(tabRect.xMin + 5f + (tabRect.width - 138f) / 2, tabRect.yMin , 128f, 128f);
            RenderTexture image = PortraitsCache.Get(currentOverseer, new Vector2(128f, 128f), Rot4.South, default(Vector3), healthStateOverride: PawnHealthState.Mobile, cameraZoom: 1f);
            GUI.DrawTexture(imageRect, image);

            if (attemptReassignOverseer(tabRect, pawn))
            {
                Rect assignButton = new(tabRect.xMin + 0f + (tabRect.width - 250f) / 2, tabRect.yMin + 148f, 250f, 35f);
                if (Widgets.ButtonText(assignButton, "Reassign an Overseer"))
                {
                    List<Pawn> potentialOverseers = attachedPawns.Keys.ToList().Where((Pawn x) => canBecomeOverseer(x)).ToList();
                    List<FloatMenuOption> options = new List<FloatMenuOption>();
                    foreach (Pawn selectOverseer in potentialOverseers)
                    {
                        options.Add(new FloatMenuOption(selectOverseer.Name.ToStringFull.CapitalizeFirst(), delegate
                        {
                            reassignOverseer(selectOverseer);
                        }));
                    }
                    Find.WindowStack.Add(new FloatMenu(options));
                }
            }


            Rect abilityRect = new(tabRect.xMin, tabRect.yMin + 193f, tabRect.width, tabRect.height - 203f);
            drawOverseerAbilities(abilityRect, pawn);
        }

        public virtual void drawOverseerAbilities(Rect tabRect, Pawn pawn)
        {
            for (int abilityY = 0; abilityY < 2; abilityY++)
            {

                for (int abilityX = 0; abilityX < 2; abilityX++)
                {
                    Rect backgroundRect = new(tabRect.xMin + abilityX * 79f + (tabRect.width - 148f) / 2, tabRect.yMin + abilityY * 79f + (tabRect.height - 148f) / 2 + 10f, 69f, 69f);
                    GUI.DrawTexture(backgroundRect, ContentFinder<Texture2D>.Get("UI/Icons/UI_Background"));
                    if (overseerCasts.Count() > abilityY * 2 + abilityX)
                    {
                        AbilityDef abilityDef = overseerCasts.Keys.ToList()[abilityY * 2 + abilityX];
                        Rect abilityRect = new(tabRect.xMin + 2f + abilityX * 80.5f + (tabRect.width - 148f) / 2, tabRect.yMin + 2f + abilityY * 80.5f + (tabRect.height - 148f) / 2 + 10f, 64f, 64f);
                        GUI.DrawTexture(abilityRect, ContentFinder<Texture2D>.Get(abilityDef.iconPath));

                        string canUse = canUseAbility(abilityDef);
                        if (canUse != null)
                        {
                            GUI.DrawTexture(backgroundRect, ContentFinder<Texture2D>.Get("UI/Icons/UI_Lock"));
                            TooltipHandler.TipRegion(backgroundRect, canUse);
                        }
                    }
                }
            }
        }

        public virtual string canUseAbility(AbilityDef abilityDef)
        {
            if (overseerCasts[abilityDef] <= attachedPawns.Count)
            {
                return null;
            }

            return "Locked: Not enough hivemind members (" + attachedPawns.Count + "/" + overseerCasts[abilityDef] + ")";
        }

        public virtual bool canBecomeOverseer(Pawn pawn)
        {
            if (LoadedModManager.GetMod<AG_Mod>().GetSettings<AG_Settings>().fiestaMode)
            {
                return !pawn.health.Dead && (pawn.ageTracker.AgeBiologicalYears <= 3f || !pawn.health.Dead);
            }
            return !pawn.health.Downed && !pawn.health.Dead && pawn.ageTracker.AgeBiologicalYears > 3f;
        }

        public virtual void reassignOverseer(Pawn pawn)
        {
            currentOverseer.health.RemoveHediff(currentOverseer.health.hediffSet.GetFirstHediffOfDef(overseerHediffDef));
            currentOverseer = pawn;
            currentOverseer.health.AddHediff(overseerHediffDef);
            Hediff_Overseer hediffOverseer = currentOverseer.health.hediffSet.GetFirstHediffOfDef(overseerHediffDef) as Hediff_Overseer;
            hediffOverseer.connectedHivemind = this;
            hediffOverseer.recalculateAbilities();
            overseerAssignmentTickCooldown = Find.TickManager.TicksGame + overseerAssignmentCooldown;
        }

        public virtual void selectNewOverseer(Pawn pawn)
        {
            currentOverseer = pawn;
            currentOverseer.health.AddHediff(overseerHediffDef);
            Hediff_Overseer hediffOverseer = currentOverseer.health.hediffSet.GetFirstHediffOfDef(overseerHediffDef) as Hediff_Overseer;
            hediffOverseer.connectedHivemind = this;
            hediffOverseer.recalculateAbilities();
            overseerAssignmentTickCooldown = Find.TickManager.TicksGame + overseerAssignmentCooldown;
        }
        public virtual void removeOverseer()
        {
            if (currentOverseer == null)
            {
                return;
            }

            currentOverseer.health.RemoveHediff(currentOverseer.health.hediffSet.GetFirstHediffOfDef(overseerHediffDef));
            currentOverseer = null;
            overseerAssignmentTickCooldown = Find.TickManager.TicksGame + overseerDeathCooldown;
        }

        public virtual void overseerDeath()
        {
            removeOverseer();
        }

        public virtual void renderPawnMenu(Rect tabRect, Pawn pawn)
        {
            Listing_Standard listing = new Listing_Standard();
            Widgets.Label(new Rect(tabRect.xMin + 5f, tabRect.yMin, 300f, 50f), "Current members:");
            Rect pawnRect = new Rect(tabRect.xMin, tabRect.yMin, 250f, 475f);
            Widgets.BeginScrollView(tabRect, ref scrollPosition, pawnRect, false);
            listing.Begin(pawnRect);
            listing.Gap(25f);
            bool colorAlternator = true;
            Color alternativeColor = new Color(1f, 1f, 1f, 0.5f);
            Color lineColor = new Color(1f, 1f, 1f, 0.4f);

            foreach (Pawn member in attachedPawns.Keys)
            {
                Rect labelRect = listing.GetRect(28f);
                Rect textRect = labelRect.ContractedBy(3f);
                if (colorAlternator)
                {
                    Widgets.DrawHighlight(labelRect);
                }
                colorAlternator = !colorAlternator;
                GenUI.SetLabelAlign(TextAnchor.MiddleLeft);
                Widgets.Label(textRect, member.Name.ToStringFull.CapitalizeFirst());
                GenUI.ResetLabelAlign();
            }
            listing.End();
            Widgets.EndScrollView();
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

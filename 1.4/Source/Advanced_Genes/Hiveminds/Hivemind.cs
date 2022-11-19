using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public virtual int overseerAssignmentDeathCooldown
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
        }

        public virtual void disconnectPawn(Pawn pawn, Hediff_Hivemind hediff)
        {
            attachedPawns.Remove(pawn);
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
            var font = Text.Font;
            var anchor = Text.Anchor;
            GUI.BeginGroup(tabRect);
            Listing_Standard listing = new();
            listing.Begin(tabRect);
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(10f, 5f, 300f, 50f), hiveName);
            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(10f, 35f, 300f, 50f), "Current members:");
            renderPawnMenu(new Rect(5f, 60f, 250f, 320f), pawn);
            renderOverseerMenu(new Rect(265f, 10f, 276f, 380f), pawn);
            listing.End();
            GUI.EndGroup();
        }

        public virtual void renderOverseerMenu(Rect tabRect, Pawn pawn)
        {
            Rect backgroundRect = new(tabRect.xMin + 0f + (tabRect.width - 138f) / 2, tabRect.yMin + 0f, 138f, 138f);
            GUI.DrawTexture(backgroundRect, ContentFinder<Texture2D>.Get("UI/Icons/UI_Background"));
            if (currentOverseer == null)
            {
                Rect imageRect = new(tabRect.xMin + 5f + (tabRect.width - 138f) / 2, tabRect.yMin + 5f, 128f, 128f);
                Texture2D overseerIcon = ContentFinder<Texture2D>.Get(getOverseerIcon);
                GUI.DrawTexture(imageRect, overseerIcon);
                int pawnCount = attachedPawns.Keys.ToList().Where((Pawn x) => !x.health.Dead).Count(); //for hiveminds that don't disconnect their members upon death for some reason
                if (pawnCount < overseerMemberRequirement)
                {
                    Rect lockedButton = new(tabRect.xMin + 0f + (tabRect.width - 250f) / 2, tabRect.yMin + 148f, 250f, 50f);
                    GUI.color = disabledColor;
                    Widgets.ButtonText(lockedButton, "Unable to assign an Overseer: Not enough pawns (" + pawnCount + "/" + overseerMemberRequirement + ")", active: false);
                    GUI.color = Color.white;

                }
                else if (overseerAssignmentTickCooldown > Find.TickManager.TicksGame)
                {
                    Rect lockedButton = new(tabRect.xMin + 0f + (tabRect.width - 250f) / 2, tabRect.yMin + 148f, 250f, 50f);
                    GUI.color = disabledColor;
                    Widgets.ButtonText(lockedButton, "Unable to assign an Overseer: Wait " + ((int)((overseerAssignmentTickCooldown - Find.TickManager.TicksGame) / GenDate.TicksPerDay * 10)) / 10 + " more days", active: false);
                    GUI.color = Color.white;
                }
                else if (attachedPawns.Keys.ToList().Where((Pawn x) => canBecomeOverseer(x)).Count() == 0)
                {
                    Rect lockedButton = new(tabRect.xMin + 0f + (tabRect.width - 250f) / 2, tabRect.yMin + 148f, 250f, 50f);
                    GUI.color = disabledColor;
                    Widgets.ButtonText(lockedButton, "Unable to assign an Overseer: Wait " + ((int)((overseerAssignmentTickCooldown - Find.TickManager.TicksGame) / GenDate.TicksPerDay * 10)) / 10 + " more days", active: false);
                    GUI.color = Color.white;
                }
                else
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
            }
            else
            {
                Rect imageRect = new(tabRect.xMin + 5f + (tabRect.width - 138f) / 2, tabRect.yMin , 128f, 128f);
                RenderTexture image = PortraitsCache.Get(currentOverseer, new Vector2(128f, 128f), Rot4.South, default(Vector3), healthStateOverride: PawnHealthState.Mobile, cameraZoom: 1f);
                GUI.DrawTexture(imageRect, image);

                if (overseerAssignmentTickCooldown > Find.TickManager.TicksGame)
                {
                    Rect lockedButton = new(tabRect.xMin + 0f + (tabRect.width - 250f) / 2, tabRect.yMin + 148f, 250f, 50f);
                    GUI.color = disabledColor;
                    Widgets.ButtonText(lockedButton, "Unable to reassign an Overseer: Wait " + ((int)((overseerAssignmentTickCooldown - Find.TickManager.TicksGame) / GenDate.TicksPerDay * 10)) / 10 + " more days", active: false);
                    GUI.color = Color.white;
                }
                else
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
            }
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
            currentOverseer = pawn;
            overseerAssignmentTickCooldown = Find.TickManager.TicksGame + overseerAssignmentCooldown;
        }

        public virtual void selectNewOverseer(Pawn pawn)
        {
            currentOverseer = pawn;
            overseerAssignmentTickCooldown = Find.TickManager.TicksGame + overseerAssignmentCooldown;
        }

        public virtual void renderPawnMenu(Rect tabRect, Pawn pawn)
        {
            Listing_Standard listing = new Listing_Standard();
            Rect pawnRect = new Rect(tabRect.xMin, tabRect.yMin, 250f, 500f);
            Widgets.BeginScrollView(tabRect, ref scrollPosition, pawnRect, false);
            listing.Begin(pawnRect);
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

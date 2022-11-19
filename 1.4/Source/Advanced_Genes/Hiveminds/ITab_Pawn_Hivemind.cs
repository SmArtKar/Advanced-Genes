using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using VanillaGenesExpanded;
using static UnityEngine.Experimental.Rendering.RayTracingAccelerationStructure;
using System.Runtime.InteropServices.ComTypes;
using VFECore;
using VFECore.Abilities;

namespace Advanced_Genes
{
    [StaticConstructorOnStartup]
    public class ITab_Pawn_Hivemind : ITab
    {
        private Pawn pawn;
        private Hediff_Hivemind hediff;

        static ITab_Pawn_Hivemind()
        {
            foreach (var def in DefDatabase<ThingDef>.AllDefs)
                if (def.race is { Humanlike: true })
                {
                    def.inspectorTabs?.Add(typeof(ITab_Pawn_Hivemind));
                    def.inspectorTabsResolved?.Add(InspectTabManager.GetSharedInstance(typeof(ITab_Pawn_Hivemind)));
                }
        }

        public ITab_Pawn_Hivemind()
        {
            labelKey = "Hivemind";
            size = new Vector2(340f, 190f);
        }

        public override bool IsVisible
        {
            get
            {
                return Find.Selector.SingleSelectedThing is Pawn pawn && pawn.health.hediffSet.hediffs.OfType<Hediff_Hivemind>().Count() > 0 && pawn.Faction is { IsPlayer: true };
            }
        }

        public Vector2 Size => size;

        public override void OnOpen()
        {
            base.OnOpen();
            pawn = (Pawn)Find.Selector.SingleSelectedThing;
            InitCache();
        }

        protected override void CloseTab()
        {
            base.CloseTab();
            pawn = null;
            ClearCache();
        }

        public void InitCache()
        {
            pawn = (Pawn)Find.Selector.SingleSelectedThing;
            hediff = pawn.health.hediffSet.GetFirstHediff<Hediff_Hivemind>() as Hediff_Hivemind;
        }

        public void ClearCache()
        {
            hediff = null;
            pawn = null;
        }

        public void updateSize()
        {
            base.UpdateSize();

            if (pawn != (Pawn)Find.Selector.SingleSelectedThing)
            {
                InitCache();
            }

            if (hediff.connectedHivemind != null)
            {
                size = hediff.connectedHivemind.getRectPosition(pawn) + hediff.connectedHivemind.getRectSize(pawn);
                return;
            }
            size = new Vector2(340f, 190f);
        }

        protected override void FillTab()
        {
            if (Find.Selector.SingleSelectedThing is Pawn p && pawn != p)
            {
                InitCache();
            }

            if (pawn == null || hediff == null)
            {
                ClearCache();
                return;
            }

            updateSize();
            if (hediff.connectedHivemind != null)
            {
                Vector2 vectorStart = hediff.connectedHivemind.getRectPosition(pawn);
                Vector2 vectorSize = hediff.connectedHivemind.getRectSize(pawn);
                hediff.connectedHivemind.renderHivemindMenu(new Rect(vectorStart.x, vectorStart.y, vectorSize.x, vectorSize.y), pawn);
                return;
            }

            var font = Text.Font;
            var anchor = Text.Anchor;
            Rect tabRect = new(Vector2.one * 10f, this.size - Vector2.one * 10f);
            GUI.BeginGroup(tabRect);
            Listing_Standard listing = new();
            listing.Begin(tabRect);
            Text.Font = GameFont.Medium;
            listing.Label("No active hivemind connection");
            Text.Font = GameFont.Small;
            Rect imageRect = new(8f, 47f, 96f, 96f);
            Rect backgroundRect = new(0f, 40f, 110f, 110f);
            Rect connectButton = new(120f, 50f, 170f, 35f);
            Rect createButton = new(120f, 105f, 170f, 35f);
            Texture2D hiveIcon = ContentFinder<Texture2D>.Get(hediff.getHivemindIcon);
            GUI.DrawTexture(backgroundRect, ContentFinder<Texture2D>.Get("UI/Icons/UI_Background"));
            GUI.DrawTexture(imageRect, hiveIcon);

            if (Widgets.ButtonText(connectButton, "Connect to a hivemind"))
            {
                GameComponent_Hiveminds hiveComp = hediff.hivemindsComponent;
                List<Hivemind> hiveminds = hiveComp.hivemindsByHediff(hediff);
                if(hiveminds.Count > 0)
                {
                    List<FloatMenuOption> options = new List<FloatMenuOption>();
                    foreach (Hivemind hive in hiveminds)
                    {
                        options.Add(new FloatMenuOption(hive.hiveName, delegate
                        {
                            hediff.attachToHivemind(hive);
                        }));
                    }
                    Find.WindowStack.Add(new FloatMenu(options));
                }
            }

            if (Widgets.ButtonText(createButton, "Create a new hivemind"))
            {
                Find.WindowStack.Add(new Dialog_CreateHivemind(pawn, hediff, hiveIcon));
                CloseTab();
            }

            listing.End();
            GUI.EndGroup();
        }
    }
}

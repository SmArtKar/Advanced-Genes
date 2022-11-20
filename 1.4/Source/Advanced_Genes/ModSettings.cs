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
using Steamworks;

namespace Advanced_Genes
{
    public class AG_Settings : ModSettings
    {
        public bool alphaGenesFound = false;
        public bool compatibilityTab = false;

        public bool lowerDecayDeathGuidance = false;
        public float modifierDecayDeathGuidance = 0.5f;
        public int modifierLengthDeathGuidance = 10;

        public int unstableDNATarget = 15;
        public int unstableDNAChange = 3;
        public int unstableDNADurationMin = 3;
        public int unstableDNADurationMax = 5;

        public float chanceBurningBlood = 0.35f;
        public float chanceSelfBurningBlood = 0.35f;
        public int explosionRadiusBurningBlood = 4;

        public bool fiestaMode = false;

        public PatchSettings mainSettings;
        public PatchSettings lastSettings;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref lowerDecayDeathGuidance, "lowerDecayDeathGuidance");
            Scribe_Values.Look(ref modifierDecayDeathGuidance, "modifierDecayDeathGuidance");
            Scribe_Values.Look(ref modifierLengthDeathGuidance, "modifierLengthDeathGuidance");

            Scribe_Values.Look(ref unstableDNATarget, "unstableDNATarget");
            Scribe_Values.Look(ref unstableDNAChange, "unstableDNAChange");
            Scribe_Values.Look(ref unstableDNADurationMin, "unstableDNADurationMin");
            Scribe_Values.Look(ref unstableDNADurationMax, "unstableDNADurationMax");

            Scribe_Values.Look(ref chanceBurningBlood, "chanceBurningBlood");
            Scribe_Values.Look(ref chanceSelfBurningBlood, "chanceSelfBurningBlood");
            Scribe_Values.Look(ref explosionRadiusBurningBlood, "explosionRadiusBurningBlood");

            Scribe_Values.Look(ref fiestaMode, "fiestaMode");


            Scribe_Deep.Look(ref mainSettings, "patchSettings");

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (mainSettings == null)
                {
                    mainSettings = new PatchSettings();
                }
                lastSettings = new PatchSettings();
            }
        }
    }

    [StaticConstructorOnStartup]

    public class StartupPatcher
    {
        static StartupPatcher()
        {
            bool alphaGenesFound = (ModLister.GetActiveModWithIdentifier("sarg.alphagenes") != null);
            LoadedModManager.GetMod<AG_Mod>().GetSettings<AG_Settings>().alphaGenesFound = alphaGenesFound;
            LoadedModManager.GetMod<AG_Mod>().GetSettings<AG_Settings>().compatibilityTab = alphaGenesFound || false;

            LoadedModManager.GetMod<AG_Mod>().ApplyPatches();
        }
    }

    public class AG_Mod : Mod
    {
        AG_Settings settings;
        private Vector2 scrollPosition;

        public AG_Mod(ModContentPack content) : base(content)
        {
            this.settings = GetSettings<AG_Settings>();
            if (SteamUser.GetSteamID().m_SteamID == 76561198028227150) //Sam's SteamID
            {
                settings.fiestaMode = true;
            }
        }

        public enum TabOption
        {
            Genes,
            Patches,
            Compatibility
        }

        private TabOption CurTab { get; set; } = TabOption.Genes;

        public override void DoSettingsWindowContents(Rect inRect)
        {
            GUI.BeginGroup(inRect);
            Rect tabRect = new Rect(0, TabDrawer.TabHeight, inRect.width, 0);
            Rect menuRect = new Rect(0, TabDrawer.TabHeight, inRect.width, inRect.height - TabDrawer.TabHeight);

            Widgets.DrawMenuSection(menuRect);

            List<TabRecord> tabs = new List<TabRecord>();
            tabs.Add(new TabRecord("Gene Settings", delegate { CurTab = TabOption.Genes; }, CurTab == TabOption.Genes));
            tabs.Add(new TabRecord("Biotech Patches", delegate { CurTab = TabOption.Patches; }, CurTab == TabOption.Patches));
            if (settings.compatibilityTab)
            {
                tabs.Add(new TabRecord("Compatibility Patches", delegate { CurTab = TabOption.Compatibility; }, CurTab == TabOption.Compatibility));
            }

            TabDrawer.DrawTabs(tabRect, tabs);

            switch (CurTab)
            {
                case TabOption.Genes:
                    DrawGenes(menuRect.ContractedBy(15));
                    break;
                case TabOption.Patches:
                    DrawPatches(menuRect.ContractedBy(10));
                    break;
                case TabOption.Compatibility:
                    DrawCompatibility(menuRect.ContractedBy(10));
                    break;
            }

            GUI.EndGroup();
        }

        private void DrawGenes(Rect rect)
        {
            Listing_Standard listing = new Listing_Standard();
            Rect leftRect = rect.ContractedBy(5).Rounded();
            Rect viewRect = new Rect(0f, 0f, rect.width - 35f, 750f);
            Widgets.BeginScrollView(leftRect, ref scrollPosition, viewRect);

            listing.Begin(viewRect);
            
            listing.Label("Unstable DNA Settings".Translate());
            listing.GapLine();
            listing.Label("Game will try to keep amount of your pawn's genes close to this value (Default: 15, Current value: " + settings.unstableDNATarget + ")");
            settings.unstableDNATarget = (int)Widgets.HorizontalSlider(listing.GetRect(15f), settings.unstableDNATarget, 1, 50, roundTo: 1);
            listing.Label("Maximum genes added/removed per activation (Default: 3, Current value: " + settings.unstableDNAChange + ")");
            settings.unstableDNAChange = (int)Widgets.HorizontalSlider(listing.GetRect(15f), settings.unstableDNAChange, 1, 10, roundTo: 1);
            listing.Label("Minimum amount of days between gene changes (Default: 3, Current value: " + settings.unstableDNADurationMin + ")");
            settings.unstableDNADurationMin = (int)Widgets.HorizontalSlider(listing.GetRect(15f), settings.unstableDNADurationMin, 1, settings.unstableDNADurationMax - 2, roundTo: 1);
            listing.Label("Maximum amount of days between gene changes (Default: 5, Current value: " + settings.unstableDNADurationMax + ")");
            settings.unstableDNADurationMax = (int)Widgets.HorizontalSlider(listing.GetRect(15f), settings.unstableDNADurationMax, settings.unstableDNADurationMin + 2, 60, roundTo: 1);
            listing.Gap(30f);

            listing.Label("Guidance Of The Dead Settings".Translate());
            listing.GapLine();
            listing.CheckboxLabeled("Lower skill decay amount", ref settings.lowerDecayDeathGuidance, "Whenever a pawn with a skill lower than the hivemind's dies, hivemind's skill will only drop by 2/3rds of what it would've dropped with this option toggled off.");
            listing.Label("Debuff decay modifier (Default: 0.5, Current value: " + settings.modifierDecayDeathGuidance + ")");
            settings.modifierDecayDeathGuidance = Widgets.HorizontalSlider(listing.GetRect(15f), settings.modifierDecayDeathGuidance, 0.1f, 1f, roundTo: 0.1f);
            listing.Label("Debuff decay length modifier (Default: 10, Current value: " + settings.modifierLengthDeathGuidance + ")");
            settings.modifierLengthDeathGuidance = (int)Widgets.HorizontalSlider(listing.GetRect(15f), settings.modifierLengthDeathGuidance, 1, 20, roundTo: 1);
            listing.Gap(30f);

            listing.Label("Burning Blood Settings".Translate());
            listing.GapLine();
            listing.Label("Burning Blood enemy ignition chance (Default: 35%, Current value: " + (int)(settings.chanceBurningBlood * 100) + "%)");
            settings.chanceBurningBlood = Widgets.HorizontalSlider(listing.GetRect(15f), settings.chanceBurningBlood, 0.05f, 1f, roundTo: 0.01f);
            listing.Label("Burning Blood self ignition chance (Default: 35%, Current value: " + (int)(settings.chanceSelfBurningBlood * 100) + "%)");
            settings.chanceSelfBurningBlood = Widgets.HorizontalSlider(listing.GetRect(15f), settings.chanceSelfBurningBlood, 0.05f, 1f, roundTo: 0.01f);
            listing.Label("Burning Blood explosion radius (Default: 4, Current value: " + settings.explosionRadiusBurningBlood + ")");
            settings.explosionRadiusBurningBlood = (int)Widgets.HorizontalSlider(listing.GetRect(15f), settings.explosionRadiusBurningBlood, 1, 10, roundTo: 1);
            listing.Gap(30f);

            listing.Label("Miscellaneous Settings".Translate());
            listing.GapLine();

            if (SteamUser.GetSteamID().m_SteamID == 76561198028227150) //Sam's SteamID
            {
                listing.Label("The god is dead and I've killed him. Nobody can save you, Sam.");
            } 
            else
            {
                listing.CheckboxLabeled("Enable Fiesta Mode", ref settings.fiestaMode);
            }

            listing.End();
            Widgets.EndScrollView();
        }

        private void DrawPatches(Rect rect)
        {
            PatchSettings mainSettings = settings.mainSettings;
            Listing_Standard listing = new Listing_Standard();
            Rect leftRect = rect.ContractedBy(5).Rounded();
            Rect viewRect = new Rect(0f, 0f, rect.width - 35f, rect.height);
            Widgets.BeginScrollView(leftRect, ref scrollPosition, viewRect);

            listing.Begin(viewRect);

            listing.Label("Superclotting metabolic cost (Default: 1, Recommended: 3, Current value: " + mainSettings.superclottingCost + ")");
            mainSettings.superclottingCost = (int)Widgets.HorizontalSlider(listing.GetRect(15f), mainSettings.superclottingCost, 1, 5, roundTo: 1);
            listing.CheckboxLabeled("Dead Calm applies Iron-Willed/Steadfast", ref mainSettings.deadCalmNerves);
            if (mainSettings.deadCalmNerves)
            {
                if (listing.RadioButton("Iron-Willed", mainSettings.deadCalmIron)) mainSettings.deadCalmIron = true;
                if (listing.RadioButton("Steadfast", !mainSettings.deadCalmIron)) mainSettings.deadCalmIron = false;

                listing.Label("Dead Calm metabolic cost (Default: 1, Recommended: 3, Current value: " + mainSettings.deadCalmCost + ")");
                mainSettings.deadCalmCost = (int)Widgets.HorizontalSlider(listing.GetRect(15f), mainSettings.deadCalmCost, 1, 5, roundTo: 1);
            }

            listing.CheckboxLabeled("Custom Hemogen gene backgrounds", ref mainSettings.hemogenBackgrounds);
            listing.CheckboxLabeled("Genies have the Blood Deficiency gene", ref mainSettings.genieBloodDeficiency);

            listing.End();
            Widgets.EndScrollView();
            CheckPatches();
        }
        private void DrawCompatibility(Rect rect)
        {
            PatchSettings mainSettings = settings.mainSettings;
            Listing_Standard listing = new Listing_Standard();
            Rect leftRect = rect.ContractedBy(5).Rounded();
            Rect viewRect = new Rect(0f, 0f, rect.width - 35f, rect.height);
            Widgets.BeginScrollView(leftRect, ref scrollPosition, viewRect);

            listing.Begin(viewRect);

            if (settings.alphaGenesFound)
            {
                listing.CheckboxLabeled("Revert Alpha Genes backgrounds to vanilla", ref mainSettings.alphaGenesDefaultBackgrounds);
            }

            listing.End();
            Widgets.EndScrollView();
            CheckPatches();
        }

        public void CheckPatches()
        {
            PatchSettings mainSettings = settings.mainSettings;
            PatchSettings lastSettings = settings.lastSettings;

            if (mainSettings.superclottingCost != lastSettings.superclottingCost)
            {
                ApplyPatches();
                return;
            }

            if (mainSettings.deadCalmNerves != lastSettings.deadCalmNerves)
            {
                ApplyPatches();
                return;
            }

            if (mainSettings.deadCalmIron != lastSettings.deadCalmIron)
            {
                ApplyPatches();
                return;
            }

            if (mainSettings.deadCalmCost != lastSettings.deadCalmCost)
            {
                ApplyPatches();
                return;
            }

            if (mainSettings.hemogenBackgrounds != lastSettings.hemogenBackgrounds)
            {
                ApplyPatches();
                return;
            }

            if (mainSettings.genieBloodDeficiency != lastSettings.genieBloodDeficiency)
            {
                ApplyPatches();
                return;
            }

            if (mainSettings.alphaGenesDefaultBackgrounds != lastSettings.alphaGenesDefaultBackgrounds)
            {
                ApplyPatches();
                return;
            }
        }

        public void ApplyPatches()
        {
            PatchSettings mainSettings = settings.mainSettings;
            PatchSettings lastSettings = settings.lastSettings;

            GeneDef coagulation = DefDatabase<GeneDef>.AllDefs.Where((GeneDef x) => x.defName == "Superclotting").ToList().FirstOrDefault();
            if(coagulation != null)
            {
                coagulation.biostatMet = -mainSettings.superclottingCost;
                lastSettings.superclottingCost = mainSettings.superclottingCost;
            }

            GeneDef deadCalm = DefDatabase<GeneDef>.AllDefs.Where((GeneDef x) => x.defName == "Aggression_DeadCalm").ToList().FirstOrDefault();
            if(deadCalm != null)
            {
                if (mainSettings.deadCalmNerves)
                {
                    deadCalm.biostatMet = -mainSettings.deadCalmCost;
                }
                else
                {
                    deadCalm.biostatMet = -1;
                }
                List<GeneticTraitData> forcedTraits = new List<GeneticTraitData>();
                if (deadCalm.forcedTraits != null)
                {
                    forcedTraits = forcedTraits.Concat(deadCalm.forcedTraits).ToList();
                }
                List<GeneticTraitData> NervesTraits = forcedTraits.Where((GeneticTraitData x) => x.def.defName == "Nerves").ToList();
                if (NervesTraits.Count > 0)
                {
                    GeneticTraitData traitData = NervesTraits[0];
                    if (mainSettings.deadCalmNerves)
                    {
                        traitData.degree = mainSettings.deadCalmIron ? 2 : 1;
                    }
                    else if(deadCalm.forcedTraits != null)
                    {
                        deadCalm.forcedTraits.Remove(traitData);
                    }
                }
                else if (mainSettings.deadCalmNerves)
                {
                    GeneticTraitData traitData = new GeneticTraitData();
                    traitData.def = DefDatabase<TraitDef>.AllDefs.Where((TraitDef x) => x.defName == "Nerves").ToList().FirstOrDefault();
                    traitData.degree = mainSettings.deadCalmIron ? 2 : 1;
                    if (traitData.def != null)
                    {
                        if (deadCalm.forcedTraits == null)
                        {
                            deadCalm.forcedTraits = new List<GeneticTraitData>();
                        }
                        deadCalm.forcedTraits.Add(traitData);
                    }
                }
            }

            lastSettings.deadCalmCost = mainSettings.deadCalmCost;
            lastSettings.deadCalmNerves = mainSettings.deadCalmNerves;
            lastSettings.deadCalmIron = mainSettings.deadCalmIron;

            List<string> hemoDefs = new List<string> { "Hemogenic", "HemogenDrain", "Deathrest", "Coagulate", "PiercingSpine", "LongjumpLegs", "Bloodfeeder" };
            foreach (GeneDef hemoGene in DefDatabase<GeneDef>.AllDefs.Where((GeneDef x) => hemoDefs.Contains(x.defName))) {
                if (hemoGene.modExtensions == null)
                {
                    hemoGene.modExtensions = new List<DefModExtension>();
                }
                GeneExtension genEx = hemoGene.GetModExtension<GeneExtension>();
                if (genEx != null)
                {
                    if (mainSettings.hemogenBackgrounds)
                    {
                        genEx.backgroundPathEndogenes = "UI/Icons/Genes/GeneBackground_Endogene_HemoSlush";
                        genEx.backgroundPathXenogenes = "UI/Icons/Genes/GeneBackground_Xenogene_HemoSlush";
                    }
                    else
                    {
                        genEx.backgroundPathEndogenes = null;
                        genEx.backgroundPathXenogenes = null;
                    }
                }
                else
                {
                    genEx = new GeneExtension();
                    genEx.backgroundPathEndogenes = "UI/Icons/Genes/GeneBackground_Endogene_HemoSlush";
                    genEx.backgroundPathXenogenes = "UI/Icons/Genes/GeneBackground_Xenogene_HemoSlush";
                    hemoGene.modExtensions.Add(genEx);
                }
            }
            lastSettings.hemogenBackgrounds = mainSettings.hemogenBackgrounds;

            if (settings.alphaGenesFound)
            {
                foreach (GeneDef alphaGene in DefDatabase<GeneDef>.AllDefs.Where((GeneDef x) => x.GetModExtension<GeneExtension>() != null))
                {
                    GeneExtension genEx = alphaGene.GetModExtension<GeneExtension>();
                    if (genEx.backgroundPathEndogenes == "UI/Icons/Genes/AG_Endogenes")
                    {
                        if (mainSettings.alphaGenesDefaultBackgrounds)
                        {
                            genEx.backgroundPathEndogenes = null;
                            genEx.backgroundPathXenogenes = null;
                        }
                        else
                        {
                            genEx.backgroundPathEndogenes = "UI/Icons/Genes/AG_Endogenes";
                            genEx.backgroundPathXenogenes = "UI/Icons/Genes/AG_Xenogenes";
                        }
                    }
                }
            }

            XenotypeDef genie = DefDatabase<XenotypeDef>.AllDefs.Where((XenotypeDef x) => x.defName == "Genie").ToList().FirstOrDefault();
            if (genie != null)
            {
                GeneDef bloodDeficiency = DefDatabase<GeneDef>.AllDefs.Where((GeneDef x) => x.defName == "AG_BloodDeficiency").ToList().FirstOrDefault();
                if (bloodDeficiency != null)
                {
                    if (mainSettings.genieBloodDeficiency)
                    {
                        if (!genie.genes.Contains(bloodDeficiency))
                        {
                            genie.genes.Add(bloodDeficiency);
                        }
                    }
                    else
                    {
                        if (genie.genes.Contains(bloodDeficiency))
                        {
                            genie.genes.Remove(bloodDeficiency);
                        }
                    }
                }
            }
            lastSettings.genieBloodDeficiency = mainSettings.genieBloodDeficiency;
        }

        public override string SettingsCategory()
        {
            return "Advanced Genes";
        }
    }

    public class PatchSettings : IExposable
    {
        public int superclottingCost = 1;
        public bool deadCalmNerves = false;
        public bool deadCalmIron = true;
        public int deadCalmCost = 1;
        public bool genieBloodDeficiency = false;
        public bool hemogenBackgrounds = true;
        public bool alphaGenesDefaultBackgrounds = true;

        public void ExposeData()
        {
            Scribe_Values.Look(ref superclottingCost, "superclottingCost");
            Scribe_Values.Look(ref deadCalmNerves, "deadCalmNerves");
            Scribe_Values.Look(ref deadCalmIron, "deadCalmIron");
            Scribe_Values.Look(ref deadCalmCost, "deadCalmCost");
            Scribe_Values.Look(ref genieBloodDeficiency, "genieBloodDeficiency");
            Scribe_Values.Look(ref hemogenBackgrounds, "hemogenBackgrounds");
            Scribe_Values.Look(ref alphaGenesDefaultBackgrounds, "alphaGenesDefaultBackgrounds");
        }

    }
}

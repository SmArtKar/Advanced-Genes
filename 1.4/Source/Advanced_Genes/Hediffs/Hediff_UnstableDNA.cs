using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Advanced_Genes
{
    internal class Hediff_UnstableDNA : Hediff_GeneChangeBlocker
    {
        public bool geneEdit = false;
        public int tickInterval = 0;
        public Random rand = new Random();

        public Hediff_UnstableDNA()
        {
            tickInterval = GenDate.TicksPerDay * rand.Next(LoadedModManager.GetMod<AG_Mod>().GetSettings<AG_Settings>().unstableDNADurationMin, LoadedModManager.GetMod<AG_Mod>().GetSettings<AG_Settings>().unstableDNADurationMax);
        }

        public override void Tick()
        {
            base.Tick();
            if (pawn.IsHashIntervalTick(tickInterval))
            {
                randomizeGenes();
            }
        }

        public virtual void randomizeGenes()
        {
            geneEdit = true;
            tickInterval = GenDate.TicksPerDay * rand.Next(LoadedModManager.GetMod<AG_Mod>().GetSettings<AG_Settings>().unstableDNADurationMin, LoadedModManager.GetMod<AG_Mod>().GetSettings<AG_Settings>().unstableDNADurationMax);
            Dictionary<GeneDef, Gene> removableGenes = getRemoveGenes();
            Random random = new Random();
            int genesLeft = pawn.genes.GenesListForReading.Count;
            int geneTarget = LoadedModManager.GetMod<AG_Mod>().GetSettings<AG_Settings>().unstableDNATarget;
            int geneChange = LoadedModManager.GetMod<AG_Mod>().GetSettings<AG_Settings>().unstableDNAChange;

            if (genesLeft > 0 && removableGenes.Count > 0)
            {
                int geneRemoveNum = random.Next(1, Math.Min(removableGenes.Count + 1, Math.Min((int)((genesLeft) * (((float)geneChange) / ((float)geneTarget))) + 1, genesLeft)));
                for (int i = 0; i < geneRemoveNum; i++)
                {
                    GeneDef toRemoveDef = removableGenes.Keys.ToList()[random.Next(removableGenes.Count)];
                    Gene toRemove = removableGenes[toRemoveDef];
                    pawn.genes.RemoveGene(toRemove);
                    removableGenes.Remove(toRemoveDef);
                }
            }

            List<GeneDef> appliableGenes = getAppliableGenes();
            if (appliableGenes.Count > 0)
            {
                int geneApplyNum = random.Next(1, Math.Min(appliableGenes.Count + 1, Math.Max(0, (int)((geneTarget + geneTarget / geneChange - genesLeft) * (((float)geneChange) / ((float)geneTarget))) + 1)));
                for (int i = 0; i < geneApplyNum; i++)
                {
                    GeneDef toAdd = appliableGenes[random.Next(appliableGenes.Count)];
                    pawn.genes.AddGene(toAdd, xenogene: true);
                    appliableGenes.Remove(toAdd);
                }
            }
            pawn.Drawer.renderer.graphics.SetAllGraphicsDirty();
            geneEdit = false;
        }

        public List<GeneDef> getAppliableGenes()
        {
            IEnumerable<GeneDef> allDefs = DefDatabase<GeneDef>.AllDefs;
            List<Gene> genes = pawn.genes.GenesListForReading;
            List<GeneDef> convertedGenes = new List<GeneDef>();
            List<GeneDef> fittingGenes = new List<GeneDef>();

            foreach (var pawnGene in genes)
            {
                convertedGenes.Add(pawnGene.def);
            }

            foreach (var geneDef in allDefs)
            {
                if (convertedGenes.Contains(geneDef) || geneDef.biostatArc > 0)
                {
                    continue;
                }

                if (geneDef.prerequisite != null)
                {
                    if (!convertedGenes.Contains(geneDef.prerequisite))
                    {
                        continue;
                    }
                }

                if (geneDef.exclusionTags != null)
                {
                    bool foundConflict = false;
                    foreach (var pawnGene in convertedGenes)
                    {
                        if (geneDef.ConflictsWith(pawnGene))
                        {
                            foundConflict = true;
                            break;
                        }
                    }
                    if (foundConflict)
                    {
                        continue;
                    }
                }

                fittingGenes.Add(geneDef);
            }

            return fittingGenes;
        }

        public Dictionary<GeneDef, Gene> getRemoveGenes()
        {
            List<Gene> genes = pawn.genes.GenesListForReading;
            Dictionary<GeneDef, Gene> fittingGenes = new Dictionary<GeneDef, Gene>();
            Dictionary<GeneDef, Gene> hairGenes = new Dictionary<GeneDef, Gene>();
            Dictionary<GeneDef, Gene> skinGenes = new Dictionary<GeneDef, Gene>();

            foreach (var pawnGene in genes)
            {
                if (pawnGene.def.defName == "AG_UnstableDNA" || pawnGene.def.defName == "AG_InfusedUnstableDNA")
                {
                    continue;
                }

                if (pawnGene.def.exclusionTags != null && pawnGene.def.exclusionTags.Count > 0)
                {
                    if (pawnGene.def.exclusionTags.Contains("HairColor"))
                    {
                        hairGenes[pawnGene.def] = pawnGene;
                        continue;
                    }

                    if (pawnGene.def.exclusionTags.Contains("SkinColor"))
                    {
                        skinGenes[pawnGene.def] = pawnGene;
                        continue;
                    }
                }

                fittingGenes[pawnGene.def] = pawnGene;
            }

            if (hairGenes.Count > 0)
            {
                Random random = new Random();
                hairGenes.Remove(hairGenes.Keys.ToList()[random.Next(hairGenes.Keys.Count)]);
                fittingGenes.Concat(hairGenes).ToLookup(x => x.Key, x => x.Value).ToDictionary(x => x.Key, g => g.First());
            }

            if (skinGenes.Count > 0)
            {
                Random random = new Random();
                skinGenes.Remove(skinGenes.Keys.ToList()[random.Next(skinGenes.Keys.Count)]);
                fittingGenes.Concat(skinGenes).ToLookup(x => x.Key, x => x.Value).ToDictionary(x => x.Key, g => g.First());
            }

            foreach (var pawnGene in fittingGenes.Keys)
            {
                if (pawnGene.prerequisite != null)
                {
                    if (fittingGenes.ContainsKey(pawnGene.prerequisite)) {
                        fittingGenes.Remove(pawnGene.prerequisite);
                    }
                }
            }

            return fittingGenes;
        }

        public override bool blockGeneChange(ref Gene gene, ref bool addAsXenogene)
        {
            if (PawnGenerator.IsBeingGenerated(pawn) is true)
            {
                return false;
            }

            if (geneEdit)
            {
                return false;
            }

            return true;
        }

        public override bool blockGeneRemoval(ref Gene gene)
        {
            if (PawnGenerator.IsBeingGenerated(pawn) is true)
            {
                return false;
            }

            if (geneEdit)
            {
                return false;
            }

            return true;
        }

        public override bool blockPawnScan(ref Pawn pawn)
        {
            return true;
        }
    }
}

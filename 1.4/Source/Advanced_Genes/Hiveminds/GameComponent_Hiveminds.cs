using RimWorld;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Advanced_Genes
{
    public class GameComponent_Hiveminds : GameComponent
    {
        public List<Hivemind> hiveminds = new List<Hivemind>();

        public List<Hivemind> hivemindsByFaction(Faction faction)
        {
            List<Hivemind> targetHiveminds = hiveminds.Where((Hivemind x) => x.attachedFaction == faction).ToList();
            if (targetHiveminds != null)
            {
                return targetHiveminds;
            }
            return new List<Hivemind>();
        }
        public List<Hivemind> hivemindsByType(Hediff_Hivemind hediff)
        {
            List<Hivemind> targetHiveminds = hiveminds.Where((Hivemind x) => hediff.canConnectTo(x)).ToList();
            if (targetHiveminds != null)
            {
                return targetHiveminds;
            }
            return new List<Hivemind>();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref hiveminds, "globalHiveminds", LookMode.Deep);
        }

        public GameComponent_Hiveminds(Game game)
        {

        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using UnityEngine;
using Verse.AI;

namespace Advanced_Genes 
{
    public class Hediff_Hivemind : HediffWithComps, IExposable
    {
        public GameComponent_Hiveminds hivemindsComponent = Current.Game.GetComponent<GameComponent_Hiveminds>();
        public Hivemind connectedHivemind;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref connectedHivemind, "connectedHivemind");
        }

        public virtual string getHivemindIcon
        {
            get
            {
                if (connectedHivemind != null)
                {
                    return connectedHivemind.getHivemindIcon;
                }

                return getDefaultHivemindIcon;
            }
        }

        public virtual string getDefaultHivemindIcon
        {
            get
            {
                return "UI/Icons/Genes/Gene_GestaltConsciousness";
            }
        }

        public virtual string getHivemindName
        {
            get
            {
                if (connectedHivemind != null)
                {
                    return connectedHivemind.getHivemindName;
                }
                return getDefaultHivemindName;
            }
        }

        public virtual string getDefaultHivemindName
        {
            get
            {
                return (LoadedModManager.GetMod<AG_Mod>().GetSettings<AG_Settings>().fiestaMode) ? "Reddit" : "Gestalt Consciousness";
            }
        }

        public virtual void attachToHivemind(Hivemind newHivemind)
        {
            connectedHivemind = newHivemind;
            connectedHivemind.connectPawn(pawn, this);
        }

        public virtual void disconnectFromHivemind()
        {
            if (connectedHivemind != null)
            {
                connectedHivemind.disconnectPawn(pawn, this);
                connectedHivemind = null;
            }
        }
        public virtual void factionChange()
        {
            disconnectFromHivemind();
        }

        public override void PostRemoved()
        {
            base.PostRemoved();
            disconnectFromHivemind();
        }

        public virtual bool disconnectOnDeath
        {
            get
            {
                return true;
            }
        }

        public virtual bool canConnectTo(Hivemind hivemind)
        {
            return hivemind.attachedFaction == pawn.Faction;
        }

        public virtual Hivemind createNewHivemind(string name)
        {
            Hivemind newHive = new Hivemind(name, pawn.Faction);
            attachToHivemind(newHive);
            return newHive;
        }

        public override void Notify_PawnKilled()
        {
            base.Notify_PawnKilled();
            if (disconnectOnDeath)
            {
                disconnectFromHivemind();
            }
        }

        public override void Notify_KilledPawn(Pawn victim, DamageInfo? dinfo)
        {
            base.Notify_KilledPawn(victim, dinfo);
        }

        public override void Tick()
        {
            base.Tick();
            if (connectedHivemind != null)
            {
                if (pawn.Faction != connectedHivemind.attachedFaction)
                {
                    factionChange();
                }
            }
        }
    }
}

using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;
using VFECore;

namespace MechanoidFoundry
{
    public class CompProperties_BiotechChargingPad : CompProperties
    {
        public float extraChargingPower;
        public float hoursToRecharge = 24;

        public CompProperties_BiotechChargingPad()
        {
            this.compClass = typeof(CompBiotechChargingPad);
        }
    }
    public class CompBiotechChargingPad : ThingComp
    {
        public CompProperties_BiotechChargingPad Props => base.props as CompProperties_BiotechChargingPad;
        public bool forceStay = false;

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            Command_Toggle forceRest = new Command_Toggle
            {
                defaultLabel = "VFEMechForceRecharge".Translate(),
                defaultDesc = "VFEMechForceRechargeDesc".Translate(),
                icon = ContentFinder<Texture2D>.Get("UI/ForceRecharge"),
                toggleAction = delegate
                {
                    foreach (var t in Find.Selector.SelectedObjects)
                    {
                        if (t is ThingWithComps thing && thing is Building_MechanoidPad pad)
                        {
                            var comp = thing.GetComp<CompBiotechChargingPad>();
                            if (comp.forceStay)
                            {
                                comp.forceStay = false;
                            }
                            else
                            {
                                comp.forceStay = true;
                                var mechs = pad.CompAssignableToPawn.AssignedPawns.ToList();
                                foreach (var mech in mechs)
                                {
                                    Job job = GotoPad(mech);
                                    mech.jobs.StopAll();
                                    mech.jobs.TryTakeOrderedJob(job);
                                }
                            }
                        }
                    }
                }
            };
            forceRest.isActive = delegate { return forceStay; };
            yield return forceRest;
        }

        public Job GotoPad(Pawn pawn)
        {
            var buildingPosition = parent.Position;
            if (pawn.Position != buildingPosition)
            {
                if (parent.Spawned && parent.Map == pawn.Map &&
                    pawn.CanReserveAndReach(parent, PathEndMode.OnCell, Danger.Deadly))
                {
                    return JobMaker.MakeJob(JobDefOf.Goto, buildingPosition);
                }
                else
                {
                    return null;
                }
            }
            else
            {
                pawn.Rotation = Rot4.South;
                return JobMaker.MakeJob(JobDefOf.Wait, 300);
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<bool>(ref forceStay, "forceStay");
        }
    }
}


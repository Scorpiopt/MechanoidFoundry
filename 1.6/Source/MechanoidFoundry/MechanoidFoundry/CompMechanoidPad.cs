using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;
using RimWorld;

namespace MechanoidFoundry
{
    public class CompMechanoidPad : ThingComp
    {
        public Pawn HeldPawn;
        public bool forceStay = false;
        
        public CompProperties_MechanoidPad Props => base.props as CompProperties_MechanoidPad;

        public void StartCharging(Pawn pawn)
        {
            HeldPawn = pawn;
        }

        public void StopCharging()
        {
            HeldPawn = null;
        }
        
        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            base.PostDestroy(mode, previousMap);
            
            if (Props != null && !Props.killPawnAfterDestroying && HeldPawn != null)
            {
                HeldPawn.jobs.StopAll();
            }
            else if (Props != null && Props.killPawnAfterDestroying && HeldPawn != null)
            {
                HeldPawn.Kill(null);
            }
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            Command_Toggle forceRest = new Command_Toggle
            {
                defaultLabel = "MF_ForceRecharge".Translate(),
                defaultDesc = "MF_ForceRechargeDesc".Translate(),
                icon = ContentFinder<Texture2D>.Get("UI/Commands/DesirePower", true),
                toggleAction = delegate
                {
                    foreach (var t in Find.Selector.SelectedObjects)
                    {
                        if (t is ThingWithComps thing && thing is Building_MechanoidPad pad)
                        {
                            var comp = thing.GetComp<CompMechanoidPad>();
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
                                    Job job = CreateGotoPadJob(mech, pad);
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

        
        public Job CreateGotoPadJob(Pawn pawn, Building_MechanoidPad pad)
        {
            var buildingPosition = pad.Position;
            if (pawn.Position != buildingPosition)
            {
                if (pad.Spawned && pad.Map == pawn.Map &&
                    pawn.CanReserveAndReach(pad, PathEndMode.OnCell, Danger.Deadly))
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
            Scribe_Values.Look<bool>(ref forceStay, "forceStay", false);
        }
    }
}


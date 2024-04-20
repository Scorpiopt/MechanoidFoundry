using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace MechanoidFoundry
{
    public class WorkGiver_DoBillHacking : WorkGiver_DoBill
    {
        public override Job JobOnThing(Pawn pawn, Thing thing, bool forced = false)
        {
            if (thing is Corpse corpse && !corpse.Position.GetThingList(corpse.Map).Any(x => x is Building_MechanoidPad pad && pad.IsActive))
            {
                var bed = Helpers.GetAvailableMechanoidPad(pawn, corpse.InnerPawn, checkForPower: true);
                if (bed != null)
                {
                    Job job = JobMaker.MakeJob(MechanoidFoundryDefOf.MF_HaulCorpseToPad, corpse, bed.Position);
                    job.count = 1;
                    return job;
                }
            }
            return base.JobOnThing(pawn, thing, forced);
        }
    }

    public class JobDriver_HaulCorpseMechanoidToPad : JobDriver_HaulToCell
    {
        public override IEnumerable<Toil> MakeNewToils()
        {
            foreach (var t in base.MakeNewToils())
            {
                yield return t;
            }
            yield return new Toil
            {
                initAction = delegate
                {
                    var workgiver = new WorkGiver_DoBillHacking();
                    workgiver.def = MechanoidFoundryDefOf.MF_DoBillsMedicalMechanoidOperation;
                    var job = workgiver.JobOnThing(this.pawn, this.TargetA.Thing);
                    if (job != null)
                    {
                        this.pawn.jobs.TryTakeOrderedJob(job);
                    }
                }
            };
        }
    }
}


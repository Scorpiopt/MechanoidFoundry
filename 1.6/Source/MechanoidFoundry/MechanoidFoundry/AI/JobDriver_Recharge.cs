using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace MechanoidFoundry
{
    public class JobDriver_Recharge : JobDriver
    {
        private const TargetIndex ChargerInd = TargetIndex.A;

        private Building_MechanoidPad Charger => (Building_MechanoidPad)job.targetA.Thing;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(Charger, job, 1, -1, null, errorOnFailed);
        }

        public override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedOrNull(TargetIndex.A);
            yield return Toils_Goto.Goto(TargetIndex.A, PathEndMode.InteractionCell);
            var charge = new Toil
            {
                defaultCompleteMode = ToilCompleteMode.Never
            };
            charge.initAction = delegate
            {
                var comp = Charger.GetComp<CompMechanoidPad>();
                comp.StartCharging(pawn);
            };
            charge.tickAction = delegate
            {
                if (pawn.IsHashIntervalTick(60))
                {
                    if (!Charger.IsActive || pawn.Position != Charger.Position)
                    {
                        ReadyForNextToil();
                        return;
                    }
                    
                    var energy = pawn.needs.TryGetNeed<Need_Energy>();
                    if (energy.CurLevelPercentage >= 1.0f)
                    {
                        ReadyForNextToil();
                    }
                }
            };
            charge.AddFinishAction(delegate
            {
                var comp = Charger.GetComp<CompMechanoidPad>();
                if (comp.HeldPawn == pawn)
                {
                    comp.StopCharging();
                }
            });
            yield return charge;
        }
    }
}
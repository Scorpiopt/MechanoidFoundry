using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace MechanoidFoundry
{
    public class FloatMenuOptionProvider_RescueMechanoid : FloatMenuOptionProvider
    {
        public override bool Drafted => true;
        public override bool Undrafted => true;
        public override bool Multiselect => false;

        public override IEnumerable<FloatMenuOption> GetOptionsFor(Pawn clickedPawn, FloatMenuContext context)
        {
            if (clickedPawn.RaceProps.IsMechanoid && clickedPawn.Downed && context.FirstSelectedPawn.CanReserveAndReach(clickedPawn, PathEndMode.OnCell, Danger.Deadly, 1, -1, null, ignoreOtherReservations: true) && !clickedPawn.InBed() && (clickedPawn.IsMechanoidHacked() || clickedPawn.BillStack.Bills.Any(x => x.recipe.Worker is RecipeHackMechanoid)))
            {
                TaggedString taggedString = "Rescue".Translate(clickedPawn.LabelCap, clickedPawn);
                yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(taggedString, delegate
                {
                    var pad = Helpers.GetAvailableMechanoidPad(context.FirstSelectedPawn, clickedPawn);
                    if (pad == null)
                    {
                        Messages.Message("CannotRescue".Translate() + ": " + "NoMechanoidPad".Translate(), clickedPawn, MessageTypeDefOf.RejectInput, historical: false);
                    }
                    else
                    {
                        if (clickedPawn.Destroyed || clickedPawn.Dead || !clickedPawn.Downed)
                        {
                            Messages.Message("CannotRescue".Translate() + ": " + "PawnNoLongerAvailable".Translate(), clickedPawn, MessageTypeDefOf.RejectInput, historical: false);
                            return;
                        }

                        if (pad.Destroyed || !pad.Spawned)
                        {
                            Messages.Message("CannotRescue".Translate() + ": " + "MechanoidPadNoLongerAvailable".Translate(), pad, MessageTypeDefOf.RejectInput, historical: false);
                            return;
                        }

                        Job job = JobMaker.MakeJob(JobDefOf.Rescue, clickedPawn, pad);
                        job.count = 1;
                        context.FirstSelectedPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                    }
                }, MenuOptionPriority.RescueOrCapture, null, clickedPawn), context.FirstSelectedPawn, clickedPawn);
            }
        }
    }
}

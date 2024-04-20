using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace MechanoidFoundry
{
    [HarmonyPatch(typeof(FloatMenuMakerMap), "AddHumanlikeOrders")]
    public static class FloatMenuMakerMap_AddHumanlikeOrders_Patch
    {
        public static void Postfix(Vector3 clickPos, Pawn pawn, ref List<FloatMenuOption> opts)
        {
            if (pawn == null || clickPos == null)
                return;
            IntVec3 c = IntVec3.FromVector3(clickPos);
            if (pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
            {
                foreach (LocalTargetInfo item15 in GenUI.TargetsAt(clickPos, TargetingParameters.ForRescue(pawn), thingsOnly: true))
                {
                    Pawn victim3 = (Pawn)item15.Thing;
                    if (victim3.InBed() || !pawn.CanReserveAndReach(victim3, PathEndMode.OnCell, Danger.Deadly, 1, -1, null, ignoreOtherReservations: true) || victim3.mindState.WillJoinColonyIfRescued)
                    {
                        continue;
                    }
                    if (victim3.IsMechanoidHacked())
                    {
                        TaggedString taggedString = ((HealthAIUtility.ShouldSeekMedicalRest(victim3) 
                            || !victim3.ageTracker.CurLifeStage.alwaysDowned) ? "Rescue".Translate(victim3.LabelCap, victim3) 
                            : "PutSomewhereSafe".Translate(victim3.LabelCap, victim3));
                        opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(taggedString, delegate
                        {
                            var pad = Helpers.GetAvailableMechanoidPad(pawn, victim3);
                            if (pad == null)
                            {
                                string text7 = ((!victim3.RaceProps.Animal) ? ((string)"NoNonPrisonerBed".Translate()) : ((string)"NoAnimalBed".Translate()));
                                Messages.Message("CannotRescue".Translate() + ": " + text7, victim3, MessageTypeDefOf.RejectInput, historical: false);
                            }
                            else
                            {
                                Job job28 = JobMaker.MakeJob(JobDefOf.Rescue, victim3, pad);
                                job28.count = 1;
                                pawn.jobs.TryTakeOrderedJob(job28, JobTag.Misc);
                                PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.Rescuing, KnowledgeAmount.Total);
                            }
                        }, MenuOptionPriority.RescueOrCapture, null, victim3), pawn, victim3));
                    }
                }
            }
        }
    }
}


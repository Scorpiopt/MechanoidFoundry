using RimWorld;
using Verse;
using Verse.AI;

namespace MechanoidFoundry
{
    public class JobGiver_StandIdle : ThinkNode_JobGiver
    {
        public override Job TryGiveJob(Pawn pawn)
        {
            return JobMaker.MakeJob(JobDefOf.Wait_Combat);
        }
    }
}
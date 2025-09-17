using RimWorld;
using Verse;
using Verse.AI;

namespace MechanoidFoundry
{
    public class ThinkNode_Conditional_MechanoidHacked : ThinkNode_Conditional
    {
        public override bool Satisfied(Pawn pawn)
        {
            return pawn.IsMechanoidHacked() && pawn.drafter != null && pawn.drafter.Drafted;
        }
    }
}
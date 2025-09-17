using RimWorld;
using Verse;
using Verse.AI;

namespace MechanoidFoundry
{
    public class ThinkNode_ConditionalHasPower : ThinkNode_Conditional
    {
        public override bool Satisfied(Pawn pawn)
        {
            var energy = pawn.needs.TryGetNeed<Need_Energy>();
            return energy == null || energy.CurLevelPercentage > 0;
        }
    }
}

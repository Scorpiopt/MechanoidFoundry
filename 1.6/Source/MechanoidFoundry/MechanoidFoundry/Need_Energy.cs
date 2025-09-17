using RimWorld;
using Verse;

namespace MechanoidFoundry
{
    public class Need_Energy : Need
    {
        public bool IsCharging => currentCharger != null;

        public Building_MechanoidPad currentCharger;

        public override int GUIChangeArrow
        {
            get
            {
                if (IsCharging)
                {
                    return 1;
                }
                return -1;
            }
        }

        public Need_Energy(Pawn pawn) : base(pawn)
        {
        }

        public override float MaxLevel
        {
            get
            {
                var maxLevel = base.MaxLevel;
                if (pawn.health.hediffSet.GetFirstHediffOfDef(MechanoidFoundryDefOf.MF_BatteryModule) != null)
                {
                    maxLevel *= 1.4f;
                }
                return maxLevel;
            }
        }

        public override void NeedInterval()
        {
            if (!IsFrozen)
            {
                CurLevel -= def.fallPerDay / 400f;
            }
        }

        public override void SetInitialLevel()
        {
            CurLevel = MaxLevel;
        }
    }
}

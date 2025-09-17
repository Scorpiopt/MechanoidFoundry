using RimWorld;
using Verse;

namespace MechanoidFoundry
{
    [DefOf]
    public static class MechanoidFoundryDefOf
    {
        public static ThingDef MF_MechanoidFoundry;
        public static RecipeDef MF_HackMechanoid;
        public static JobDef MF_HaulCorpseToPad;
        public static WorkGiverDef MF_DoBillsMedicalMechanoidOperation;
        public static ThinkTreeDef Downed;
        public static ThinkTreeDef JoinAutoJoinableCaravan;
        public static ThinkTreeDef LordDutyConstant;
        public static HediffDef MF_BatteryModule;
        public static HediffDef MF_MechanoidHacked;
        [MayRequireBiotech] public static NeedDef MechEnergy;
        public static JobDef MF_Recharge;
    }
}

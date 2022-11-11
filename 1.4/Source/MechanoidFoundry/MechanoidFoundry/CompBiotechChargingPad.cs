using Verse;

namespace MechanoidFoundry
{
    public class CompProperties_BiotechChargingPad : CompProperties
    {
        public float extraChargingPower;
        public float hoursToRecharge = 24;

        public CompProperties_BiotechChargingPad()
        {
            this.compClass = typeof(CompBiotechChargingPad);
        }
    }
    public class CompBiotechChargingPad : ThingComp
    {
        public CompProperties_BiotechChargingPad Props => base.props as CompProperties_BiotechChargingPad;
    }
}


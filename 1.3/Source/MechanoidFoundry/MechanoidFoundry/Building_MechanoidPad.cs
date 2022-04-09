using RimWorld;
using System.Collections.Generic;
using Verse;
using VFE.Mechanoids;
using VFE.Mechanoids.Buildings;

namespace MechanoidFoundry
{
    public class Building_MechanoidPad : Building_Bed, IBedMachine
    {
        public CompPowerTrader powerComp;
        public Pawn occupant
        {
            get
            {
                Pawn pawn = this.TryGetComp<CompMachineChargingStation>()?.myPawn;
                if (pawn?.Position == this.Position)
                {
                    return pawn;
                }
                return null;
            }
        }
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            this.powerComp = base.GetComp<CompPowerTrader>();
            this.Medical = false;
        }
        public bool IsActive => this.powerComp.PowerOn;
        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                if (!(gizmo is Command_Toggle toggleCommand && (toggleCommand.icon.name == "AsMedical")))
                {
                    yield return gizmo;
                }
            }
        }
    }
}


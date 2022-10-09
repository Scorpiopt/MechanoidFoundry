﻿using RimWorld;
using System.Collections.Generic;
using Verse;
using VFE.Mechanoids;
using VFE.Mechanoids.Buildings;

namespace MechanoidFoundry
{
    public class Building_MechanoidPad_Big : Building_MechanoidPad
    {
        public override bool CanTake(Pawn pawn)
        {
            return pawn.RaceProps.baseBodySize > 1;
        }
    }
    public class Building_MechanoidPad_Small : Building_MechanoidPad
    {
        public override bool CanTake(Pawn pawn)
        {
            return pawn.RaceProps.baseBodySize <= 1;
        }
    }

    public abstract class Building_MechanoidPad : Building_Bed, IBedMachine
    {
        public override IntVec3 InteractionCell => Position;

        public CompPowerTrader powerComp;
        public Pawn occupant
        {
            get
            {
                var pawn = this.TryGetComp<CompMachineChargingStation>()?.myPawn;
                if (pawn?.Position == Position)
                {
                    return pawn;
                }
                return null;
            }
        }

        public abstract bool CanTake(Pawn pawn);
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            powerComp = base.GetComp<CompPowerTrader>();
            Medical = false;
        }
        public bool IsActive => powerComp.PowerOn;
        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var gizmo in base.GetGizmos())
            {
                if (!(gizmo is Command_Toggle toggleCommand && (toggleCommand.icon.name == "AsMedical")))
                {
                    yield return gizmo;
                }
            }
        }
    }
}


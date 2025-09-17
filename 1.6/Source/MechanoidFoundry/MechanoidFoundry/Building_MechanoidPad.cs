using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

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

    public abstract class Building_MechanoidPad : Building_Bed
    {
        private const int TICKS_PER_HOUR = 2500;
        
        public override IntVec3 InteractionCell => Position;

        public CompPowerTrader powerComp;
        public CompMechanoidPad mechPad;
        public Pawn occupant => this.CurOccupants.Where(x => x.IsMechanoidHacked()).FirstOrDefault();
        
        private float? previousEnergyLevel = null;
        
        public abstract bool CanTake(Pawn pawn);
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            powerComp = base.GetComp<CompPowerTrader>();
            mechPad = base.GetComp<CompMechanoidPad>();
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

        private void HandleHealing(Pawn mech)
        {
            var healthTracker = mech.health;
            if (healthTracker.hediffSet.HasNaturallyHealingInjury())
            {
                float toHeal = 16f;
                if (healthTracker.pawn.GetPosture() != 0)
                {
                    toHeal += 4f;
                    var building_Bed = healthTracker.pawn.CurrentBed();
                    if (building_Bed != null)
                    {
                        toHeal += building_Bed.def.building.bed_healPerDay;
                    }
                }

                foreach (var hediff3 in healthTracker.hediffSet.hediffs)
                {
                    var curStage = hediff3.CurStage;
                    if (curStage != null && curStage.naturalHealingFactor != -1f)
                    {
                        toHeal *= curStage.naturalHealingFactor;
                    }
                }
                var hediffToHeal = (from x in healthTracker.hediffSet.hediffs.OfType<Hediff_Injury>()
                                    where x.CanHealNaturally()
                                    select x).RandomElement();
                if (hediffToHeal != null)
                {
                    hediffToHeal.Heal(toHeal * healthTracker.pawn.HealthScale * 0.01f * healthTracker.pawn.GetStatValue(StatDefOf.InjuryHealingFactor));
                }
            }
            else
            {
                var hediffs = healthTracker.hediffSet.hediffs.OfType<Hediff_MissingPart>().ToList();
                if (hediffs.TryRandomElement(out var missingPart))
                {
                    var part = missingPart.Part;
                    healthTracker.RemoveHediff(missingPart);
                    healthTracker.RestorePart(part);
                }
            }
            if (healthTracker.hediffSet.HasTendedAndHealingInjury())
            {
                var hediff_Injury = (from x in healthTracker.hediffSet.hediffs.OfType<Hediff_Injury>()
                                     where x.CanHealFromTending()
                                     select x).RandomElement();
                if (hediff_Injury != null)
                {
                    float tendQuality = hediff_Injury.TryGetComp<HediffComp_TendDuration>().tendQuality;
                    float num4 = GenMath.LerpDouble(0f, 1f, 0.5f, 1.5f, Mathf.Clamp01(tendQuality));
                    hediff_Injury.Heal(16f * num4 * healthTracker.pawn.HealthScale * 0.01f * healthTracker.pawn.GetStatValue(StatDefOf.InjuryHealingFactor));
                }
            }
        }
 
        public override void Tick()
        {
            base.Tick();
            var mech = occupant;
            if (mech != null && IsActive)
            {
                float chargingRate = 0f;
                if (mechPad.Props.hoursToRecharge > 0)
                {
                    chargingRate = 1.0f / (mechPad.Props.hoursToRecharge * TICKS_PER_HOUR);
                }
                
                var energyNeed = mech.GetEnergyNeed();
                if (previousEnergyLevel.HasValue)
                {
                    energyNeed.CurLevel = previousEnergyLevel.Value;
                }
                
                energyNeed.CurLevel = Mathf.Min(1.0f, energyNeed.CurLevel + chargingRate);
                
                var noBiotechNeed = energyNeed as Need_Energy;
                if (noBiotechNeed != null)
                {
                    noBiotechNeed.currentCharger = this;
                }
                
                previousEnergyLevel = energyNeed.CurLevel;
                powerComp.powerOutputInt = 0 - powerComp.Props.PowerConsumption - mechPad.Props.extraChargingPower;

                if (mech.IsHashIntervalTick(600))
                {
                    HandleHealing(mech);
                }
            }
            else
            {
                previousEnergyLevel = null;
                powerComp.powerOutputInt = -powerComp.Props.PowerConsumption;
            }
        }
    }
}

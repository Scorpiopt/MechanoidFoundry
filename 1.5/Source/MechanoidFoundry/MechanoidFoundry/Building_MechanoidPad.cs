using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
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
        public CompBiotechChargingPad biotechPad;
        public Pawn occupant => this.CurOccupants.Where(x => x.IsMechanoidHacked()).FirstOrDefault();
        public abstract bool CanTake(Pawn pawn);
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            powerComp = base.GetComp<CompPowerTrader>();
            biotechPad = base.GetComp<CompBiotechChargingPad>();
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

        public override void Tick()
        {
            base.Tick();
            var mech = occupant;
            if (mech != null && IsActive)
            {
                if (ModsConfig.BiotechActive)
                {
                    if (mech.needs?.energy != null)
                    {
                        mech.needs.energy.CurLevel += 0.000833333354f;
                        powerComp.powerOutputInt = 0 - powerComp.Props.PowerConsumption - biotechPad.Props.extraChargingPower;
                    }
                }

                if (mech.IsHashIntervalTick(600))
                {
                    var healthTracker = occupant.health;
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
                        hediffToHeal.Heal(toHeal * healthTracker.pawn.HealthScale * 0.01f * healthTracker.pawn.GetStatValue(StatDefOf.InjuryHealingFactor));
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
                        float tendQuality = hediff_Injury.TryGetComp<HediffComp_TendDuration>().tendQuality;
                        float num4 = GenMath.LerpDouble(0f, 1f, 0.5f, 1.5f, Mathf.Clamp01(tendQuality));
                        hediff_Injury.Heal(16f * num4 * healthTracker.pawn.HealthScale * 0.01f * healthTracker.pawn.GetStatValue(StatDefOf.InjuryHealingFactor));
                    }
                }
            }
            else if (ModsConfig.BiotechActive is false)
            {
                powerComp.powerOutputInt = -PowerComp.Props.PowerConsumption;
            }
        }
    }
}


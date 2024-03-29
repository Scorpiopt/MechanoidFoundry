﻿using HarmonyLib;
using RimWorld;
using System.Linq;
using UnityEngine;
using Verse;

namespace MechanoidFoundry
{

    [HarmonyPatch(typeof(Pawn_HealthTracker), "HealthTick")]
    static class Pawn_HealthTracker_HealthTick
    {
        static void Postfix(Pawn_HealthTracker __instance)
        {
            if (__instance.pawn.IsHashIntervalTick(600) && !__instance.pawn.RaceProps.IsFlesh 
				&& __instance.pawn.IsMechanoidHacked() && __instance.pawn.OnMechanoidPad(out bool padIsActive) && padIsActive)
            {
				bool flag2 = false;
				if (__instance.hediffSet.HasNaturallyHealingInjury())
				{
					float toHeal = 16f;
					if (__instance.pawn.GetPosture() != 0)
					{
						toHeal += 4f;
						Building_Bed building_Bed = __instance.pawn.CurrentBed();
						if (building_Bed != null)
						{
							toHeal += building_Bed.def.building.bed_healPerDay;
						}
					}

					foreach (Hediff hediff3 in __instance.hediffSet.hediffs)
					{
						HediffStage curStage = hediff3.CurStage;
						if (curStage != null && curStage.naturalHealingFactor != -1f)
						{
							toHeal *= curStage.naturalHealingFactor;
						}
					}
					var hediffToHeal = (from x in __instance.hediffSet.GetHediffs<Hediff_Injury>()
										where x.CanHealNaturally()
										select x).RandomElement();
					hediffToHeal.Heal(toHeal * __instance.pawn.HealthScale * 0.01f * __instance.pawn.GetStatValue(StatDefOf.InjuryHealingFactor));
					flag2 = true;
				}
				else
				{
					var hediffs = __instance.hediffSet.GetHediffs<Hediff_MissingPart>().ToList();
					if (hediffs.TryRandomElement(out var missingPart))
					{
                        var part = missingPart.Part;
                        __instance.RemoveHediff(missingPart);
                        __instance.RestorePart(part);
                    }
                }
				if (__instance.hediffSet.HasTendedAndHealingInjury())
				{
					Hediff_Injury hediff_Injury = (from x in __instance.hediffSet.GetHediffs<Hediff_Injury>()
												   where x.CanHealFromTending()
												   select x).RandomElement();
					float tendQuality = hediff_Injury.TryGetComp<HediffComp_TendDuration>().tendQuality;
					float num4 = GenMath.LerpDouble(0f, 1f, 0.5f, 1.5f, Mathf.Clamp01(tendQuality));
					hediff_Injury.Heal(16f * num4 * __instance.pawn.HealthScale * 0.01f * __instance.pawn.GetStatValue(StatDefOf.InjuryHealingFactor));
					flag2 = true;
				}
				if (flag2 && !__instance.HasHediffsNeedingTendByPlayer() && !HealthAIUtility.ShouldSeekMedicalRest(__instance.pawn) && !__instance.hediffSet.HasTendedAndHealingInjury() && PawnUtility.ShouldSendNotificationAbout(__instance.pawn))
				{
					Messages.Message("MessageFullyHealed".Translate(__instance.pawn.LabelCap, __instance.pawn), __instance.pawn, MessageTypeDefOf.PositiveEvent);
				}
			}
        }
    }
}


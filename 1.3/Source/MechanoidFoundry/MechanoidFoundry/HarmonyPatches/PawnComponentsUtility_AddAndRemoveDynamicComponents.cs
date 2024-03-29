﻿using HarmonyLib;
using RimWorld;
using Verse;

namespace MechanoidFoundry
{
    [HarmonyPatch(typeof(PawnComponentsUtility), "AddAndRemoveDynamicComponents")]
    public static class PawnComponentsUtility_AddAndRemoveDynamicComponents
    {
        public static void Postfix(Pawn pawn)
        {
            MakeComponentsToHackedMechanoid(pawn);
        }

        public static void MakeComponentsToHackedMechanoid(Pawn pawn)
        {
            if (pawn.IsMechanoidHacked())
            {
                AssignPawnComponents(pawn);
            }
        }

        public static void AssignPawnComponents(Pawn pawn)
        {
            if (pawn.relations == null)
            {
                pawn.relations = new Pawn_RelationsTracker(pawn);
            }
            if (pawn.story == null)
            {
                pawn.story = new Pawn_StoryTracker(pawn);
            }
            if (pawn.ownership == null)
            {
                pawn.ownership = new Pawn_Ownership(pawn);
            }
            if (pawn.skills == null)
            {
                pawn.skills = new Pawn_SkillTracker(pawn);
            }
            if (pawn.workSettings == null)
            {
                pawn.workSettings = new Pawn_WorkSettings(pawn);
                pawn.workSettings.EnableAndInitializeIfNotAlreadyInitialized();
                pawn.workSettings.priorities.SetAll(0);
            }
            if (pawn.interactions == null)
            {
                pawn.interactions = new Pawn_InteractionsTracker(pawn);
            }
            if (pawn.playerSettings != null)
            {
                pawn.playerSettings.medCare = MedicalCareCategory.NoCare;
            }
        }
    }
}


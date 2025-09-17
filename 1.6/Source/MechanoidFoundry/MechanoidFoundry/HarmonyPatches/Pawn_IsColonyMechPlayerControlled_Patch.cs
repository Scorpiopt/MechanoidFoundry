using HarmonyLib;
using MechanoidFoundry;
using System.Collections.Generic;
using Verse;

namespace MechanoidFoundry
{
    [HarmonyPatch(typeof(Pawn), "IsColonyMechPlayerControlled", MethodType.Getter)]
    public static class Pawn_IsColonyMechPlayerControlled_Patch
    {
        public static void Postfix(ref bool __result, Pawn __instance)
        {
            if (!__result && __instance.Spawned && __instance.IsMechanoidHacked())
            {
                __result = true;
            }
        }
    }

    [HarmonyPatch(typeof(MechanitorUtility), "IsColonyMechRequiringMechanitor")]
    public static class MechanitorUtility_IsColonyMechRequiringMechanitor_Patch
    {
        public static void Postfix(ref bool __result, Pawn mech)
        {
            if (mech.IsMechanoidHacked())
            {
                __result = false;
            }
        }
    }

    [HarmonyPatch(typeof(Pawn), "IsColonyMech", MethodType.Getter)]
    public static class Pawn_IsColonyMech_Patch
    {
        public static void Postfix(ref bool __result, Pawn __instance)
        {
            if (!__result && __instance.Spawned && __instance.IsMechanoidHacked())
            {
                __result = true;
            }
        }
    }

    [HarmonyPatch(typeof(MechanitorUtility), nameof(MechanitorUtility.EverControllable))]
    public static class MechanitorUtility_EverControllable_Patch
    {
        public static bool shouldAlter;
        public static void Postfix(Pawn mech, ref bool __result)
        {
            if (mech.IsMechanoidHacked())
            {
                if (shouldAlter)
                {
                    __result = true;
                }
                else
                {
                    __result = false;
                }
            }
        }
    }

    [HarmonyPatch(typeof(VerbTracker), "CreateVerbTargetCommand")]
    public static class VerbTracker_CreateVerbTargetCommand_Patch
    {
        public static void Prefix()
        {
            MechanitorUtility_EverControllable_Patch.shouldAlter = true;
        }

        public static void Postfix()
        {
            MechanitorUtility_EverControllable_Patch.shouldAlter = false;
        }
    }
}


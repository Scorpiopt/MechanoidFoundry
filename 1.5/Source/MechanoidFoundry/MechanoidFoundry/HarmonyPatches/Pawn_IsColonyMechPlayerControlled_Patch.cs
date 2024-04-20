using HarmonyLib;
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
}


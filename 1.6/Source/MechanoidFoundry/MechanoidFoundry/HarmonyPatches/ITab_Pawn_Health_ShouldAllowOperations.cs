using HarmonyLib;
using RimWorld;
using System.Linq;
using Verse;

namespace MechanoidFoundry
{
    [HarmonyPatch(typeof(ITab_Pawn_Health), "ShouldAllowOperations")]
    static class ITab_Pawn_Health_ShouldAllowOperations
    {
        static void Postfix(ITab_Pawn_Health __instance, ref bool __result)
        {
            if (__result is false)
            {
                var pawn = __instance.PawnForHealth;
                if (pawn != null && pawn.Dead && pawn.RaceProps.IsMechanoid)
                {
                    __result = ShouldAllowOperations(pawn);
                }
            }
        }

        public static bool ShouldAllowOperations(Pawn pawn)
        {
            if (!pawn.def.AllRecipes.Any((RecipeDef x) => x.AvailableNow && x.AvailableOnNow(pawn)))
            {
                return false;
            }
            return true;
        }
    }
}


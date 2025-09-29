using HarmonyLib;
using Verse;

namespace MechanoidFoundry
{
    [HarmonyPatch(typeof(Pawn), "CurrentlyUsableForBills")]
    public static class Pawn_CurrentlyUsableForBills_Patch
    {
        public static void Postfix(Pawn __instance, ref bool __result)
        {
            if (__result is false && __instance.BillStack.Bills.Any(x => x.recipe.Worker is RecipeHackMechanoid))
            {
                __result = true;
            }
        }
    }
}


using HarmonyLib;
using RimWorld;
using Verse;

namespace MechanoidFoundry
{
    [HotSwappable]
    [HarmonyPatch(typeof(RestUtility), nameof(RestUtility.CanUseBedNow))]
    public static class RestUtility_CanUseBedNow_Patch
    {
        public static void Postfix(ref bool __result, Pawn sleeper, Thing bedThing)
        {
            if (__result is false && bedThing is Building_MechanoidPad && sleeper.BillStack.Bills.Any(x => x.recipe.Worker is RecipeHackMechanoid))
            {
                __result = true;
            }
        }
    }
}

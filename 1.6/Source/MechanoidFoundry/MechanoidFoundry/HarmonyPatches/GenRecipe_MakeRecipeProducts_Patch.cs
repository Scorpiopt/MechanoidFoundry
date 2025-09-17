using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace MechanoidFoundry
{
    [HarmonyPatch(typeof(GenRecipe), nameof(GenRecipe.MakeRecipeProducts))]
    public static class GenRecipe_MakeRecipeProducts_Patch
    {
        public static IEnumerable<Thing> Postfix(IEnumerable<Thing> __result, RecipeDef recipeDef, Pawn worker, List<Thing> ingredients, Thing dominantIngredient, IBillGiver billGiver, Precept_ThingStyle precept = null)
        {
            if (recipeDef.Worker is RecipeMakeMechanoid recipeWorker)
            {
                recipeWorker.MakeMechanoid(worker);
                yield break;
            }
            else
            {
                foreach (var r in __result)
                {
                    yield return r;
                }
            }
        }
    }

    public class PawnExtension : DefModExtension
    {
        public List<ThingDef> partsToInstall;
    }
}


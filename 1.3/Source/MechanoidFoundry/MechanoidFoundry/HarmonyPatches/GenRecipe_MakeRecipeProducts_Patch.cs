using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace MechanoidFoundry
{
    [HarmonyPatch(typeof(GenRecipe), nameof(GenRecipe.MakeRecipeProducts))]
    public static class GenRecipe_MakeRecipeProducts_Patch
    {
        public static IEnumerable<Thing> Postfix(IEnumerable<Thing> __result, RecipeDef recipeDef, Pawn worker, List<Thing> ingredients, Thing dominantIngredient, IBillGiver billGiver, Precept_ThingStyle precept = null)
        {
            if (recipeDef.workerClass == typeof(RecipeMakeMechanoid))
            {
                SpawnMechanoid(recipeDef, worker);
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

        private static void SpawnMechanoid(RecipeDef recipeDef, Pawn worker)
        {
            var pawnKindDef = PawnKindDef.Named(recipeDef.defName.Replace("MakeMechanoid_", ""));
            var pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(pawnKindDef, Faction.OfMechanoids, newborn: false));
            GenSpawn.Spawn(pawn, worker.Position, worker.Map);
            var eq = pawn.equipment.Primary;
            if (eq != null)
            {
                pawn.equipment.Remove(eq);
            }
            pawn.SetFaction(Faction.OfPlayer);
            if (eq != null)
            {
                pawn.equipment.AddEquipment(eq);
            }
        }
    }
}


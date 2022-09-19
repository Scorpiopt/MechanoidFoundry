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
            var mech = PawnGenerator.GeneratePawn(new PawnGenerationRequest(pawnKindDef, Faction.OfMechanoids));

            var extension = mech.kindDef.GetModExtension<PawnExtension>();
            if (extension != null)
            {
                foreach (var part in extension.partsToInstall)
                {
                    InstallPart(mech, part);
                }
            }

            GenSpawn.Spawn(mech, worker.Position, worker.Map);
            var eq = mech.equipment.Primary;
            if (eq != null)
            {
                mech.equipment.Remove(eq);
            }
            PawnComponentsUtility_AddAndRemoveDynamicComponents.AssignPawnComponents(mech);
            mech.SetFaction(Faction.OfPlayer);
            if (eq != null)
            {
                mech.equipment.AddEquipment(eq);
            }
            mech.needs.AddOrRemoveNeedsAsAppropriate();
        }

        private static void InstallPart(Pawn pawn, ThingDef partDef)
        {
            IEnumerable<RecipeDef> source = DefDatabase<RecipeDef>.AllDefs.Where((RecipeDef x) => x.IsIngredient(partDef) && pawn.def.AllRecipes.Contains(x));
            if (source.Any())
            {
                RecipeDef recipeDef = source.RandomElement();
                BodyPartRecord part = null;
                if (recipeDef.Worker.GetPartsToApplyOn(pawn, recipeDef).TryRandomElement(out var pickedPart))
                {
                    part = pickedPart;
                }
                recipeDef.Worker.ApplyOnPawn(pawn, part, null, new List<Thing>(), null);

            }
        }
    }

    public class PawnExtension : DefModExtension
    {
        public List<ThingDef> partsToInstall;
    }
}


using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace MechanoidFoundry
{
    public class RecipeMakeMechanoid : RecipeWorker
    {
        public void MakeMechanoid(Pawn worker)
        {
            var pawnKindDef = PawnKindDef.Named(this.recipe.defName.Replace("MakeMechanoid_", ""));
            var mech = PawnGenerator.GeneratePawn(new PawnGenerationRequest(pawnKindDef, Faction.OfMechanoids));
            mech.health.AddHediff(MechanoidFoundryDefOf.MF_MechanoidHacked);
            var eq = mech.equipment.Primary;
            if (eq != null)
            {
                mech.equipment.Remove(eq);
            }
            PawnComponentsUtility_AddAndRemoveDynamicComponents.AssignPawnComponents(mech);
            mech.SetFaction(Faction.OfPlayer);
            mech.workSettings.priorities.SetAll(0);
            if (eq != null)
            {
                mech.equipment.AddEquipment(eq);
            }

            var extension = mech.kindDef.GetModExtension<PawnExtension>();
            if (extension != null)
            {
                foreach (var part in extension.partsToInstall)
                {
                    InstallPart(mech, part);
                }
            }
            mech.needs.AddOrRemoveNeedsAsAppropriate();
            GenSpawn.Spawn(mech, worker.Position, worker.Map);
        }

        private static void InstallPart(Pawn pawn, ThingDef partDef)
        {
            var source = DefDatabase<RecipeDef>.AllDefs.Where((RecipeDef x) => x.IsIngredient(partDef) && pawn.def.AllRecipes.Contains(x));
            if (source.Any())
            {
                var recipeDef = source.RandomElement();
                BodyPartRecord part = null;
                if (recipeDef.Worker.GetPartsToApplyOn(pawn, recipeDef).TryRandomElement(out var pickedPart))
                {
                    part = pickedPart;
                }
                recipeDef.Worker.ApplyOnPawn(pawn, part, null, new List<Thing>(), null);

            }
        }
    }
}


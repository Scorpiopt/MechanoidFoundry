using System.Collections.Generic;
using Verse;

namespace MechanoidFoundry
{
    [StaticConstructorOnStartup]
    public static class Startup
    {
        static Startup()
        {
            var recipesOnMechanoids = new List<RecipeDef>();
            foreach (var recipe in DefDatabase<RecipeDef>.AllDefs)
            {
                var extension = recipe.GetModExtension<RecipeInstallOnHackableMechanoid>();
                if (extension != null)
                {
                    recipesOnMechanoids.Add(recipe);
                }
            }
            InitializeMechProps(recipesOnMechanoids);
            InitializeBuildProps();
        }
        public static void InitializeBuildProps()
        {
            foreach (var pawn in DefDatabase<PawnKindDef>.AllDefs)
            {
                if (pawn.RaceProps.IsMechanoid)
                {
                    var buildPropsByDefs = MechanoidFoundryMod.settings.buildPropsByDefs;
                    if (!buildPropsByDefs.ContainsKey(pawn.defName))
                    {
                        var props = new BuildProperties
                        {
                            buildable = pawn.CanBeCreatedAndHacked()
                        };
                        float costMultiplier = GenerateImpliedDefs_PreResolve_Patch.GetMechCostMultiplier(pawn);
                        props.costList["Steel"] = (int)(500 * costMultiplier);
                        props.costList["Plasteel"] = (int)(400 * costMultiplier);
                        props.costList["ComponentIndustrial"] = (int)(20 * costMultiplier);
                        props.costList["ComponentSpacer"] = (int)(5 * costMultiplier);
                        buildPropsByDefs[pawn.defName] = props;
                    }
                }
            }
        }

        private static void InitializeMechProps(List<RecipeDef> recipesOnMechanoids)
        {
            foreach (var pawn in DefDatabase<PawnKindDef>.AllDefs)
            {
                if (pawn.CanBeCreatedAndHacked())
                {
                    foreach (var recipe in recipesOnMechanoids)
                    {
                        if (pawn.race.AllRecipes.Contains(recipe) is false)
                        {
                            pawn.race.AllRecipes.Add(recipe);
                        }
                    }
                    if (pawn.race.AllRecipes.Contains(MechanoidFoundryDefOf.MF_HackMechanoid) is false)
                    {
                        pawn.race.AllRecipes.Add(MechanoidFoundryDefOf.MF_HackMechanoid);
                    }
                    if (pawn.race.race.corpseDef != null && pawn.race.race.corpseDef.recipes.Contains(MechanoidFoundryDefOf.MF_HackMechanoid) is false)
                    {
                        pawn.race.race.corpseDef.recipes.Add(MechanoidFoundryDefOf.MF_HackMechanoid);
                    }
                }
            }
        }
    }
}


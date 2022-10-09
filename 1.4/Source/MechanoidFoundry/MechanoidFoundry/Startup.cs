using AnimalBehaviours;
using System.Collections.Generic;
using Verse;
using VFE.Mechanoids;

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
                    var draftableCompProps = pawn.race.GetCompProperties<CompProperties_Draftable>();
                    if (draftableCompProps is null)
                    {
                        pawn.race.comps.Add(new CompProperties_Draftable());
                    }
                    var compMachineProps = pawn.race.GetCompProperties<CompProperties_Machine>();
                    if (compMachineProps is null)
                    {
                        pawn.race.comps.Add(new CompProperties_Machine
                        {
                            violent = true,
                            canPickupWeapons = true,
                            hoursActive = 100 * pawn.race.race.baseBodySize,
                            compClass = typeof(CompMachine)
                        });
                    }
                    var modExtension = pawn.race.GetModExtension<MechanoidExtension>();
                    if (modExtension is null)
                    {
                        if (pawn.race.modExtensions is null)
                        {
                            pawn.race.modExtensions = new List<DefModExtension>();
                        }
                        pawn.race.modExtensions.Add(new MechanoidExtension
                        {
                            hasPowerNeedWhenHacked = true,
                        });
                    }
                    else
                    {
                        modExtension.hasPowerNeedWhenHacked = true;
                    }
                    foreach (var recipe in recipesOnMechanoids)
                    {
                        pawn.race.AllRecipes.Add(recipe);
                    }

                    pawn.race.AllRecipes.Add(MechanoidFoundryDefOf.MF_HackMechanoid);
                    pawn.race.race.corpseDef.recipes.Add(MechanoidFoundryDefOf.MF_HackMechanoid);
                }
            }
        }
    }
}


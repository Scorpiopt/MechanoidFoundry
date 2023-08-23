using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace MechanoidFoundry
{
    [HarmonyPatch(typeof(WorkGiver_ConstructDeliverResources), "ResetStaticData")]
    public static class WorkGiver_ConstructDeliverResources_ResetStaticData_Patch
    {
        public static void Postfix()
        {
            CreateMechComponents();
        }
        public static void CreateMechComponents()
        {
            foreach (var pawn in DefDatabase<PawnKindDef>.AllDefs)
            {
                if (pawn.CanBeCreatedAndHacked())
                {
                    if (ModsConfig.BiotechActive is false)
                    {
                        var MF_MainMachineBehaviourViolentActive = DefDatabase<ThinkTreeDef>.GetNamed("MF_MainMachineBehaviourViolentActive");
                        if (!pawn.race.race.thinkTreeMain.thinkRoot.subNodes.Any(x => x is ThinkNode_Subtree subtree
                            && subtree.treeDef == MF_MainMachineBehaviourViolentActive))
                        {
                            if (pawn.race.race.thinkTreeConstant != null)
                            {
                                var node = pawn.race.race.thinkTreeConstant.thinkRoot.subNodes
                                    .Find(x => x is ThinkNode_ConditionalCanDoConstantThinkTreeJobNow);
                                if (node != null)
                                {
                                    if (!node.subNodes.Exists(x => x is ThinkNode_Subtree subtree && subtree.treeDef == MechanoidFoundryDefOf.JoinAutoJoinableCaravan))
                                    {
                                        node.subNodes.Add(new ThinkNode_Subtree
                                        {
                                            treeDef = MechanoidFoundryDefOf.JoinAutoJoinableCaravan,
                                        });
                                    }

                                    if (!node.subNodes.Exists(x => x is ThinkNode_Subtree subtree && subtree.treeDef == MechanoidFoundryDefOf.LordDutyConstant))
                                    {
                                        node.subNodes.Add(new ThinkNode_Subtree
                                        {
                                            treeDef = MechanoidFoundryDefOf.LordDutyConstant,
                                        });
                                    }
                                }
                            }
                            else
                            {
                                pawn.race.race.thinkTreeConstant = MechanoidFoundryDefOf.VFE_Mechanoids_Machine_RiddableConstant;
                            }

                            int index = pawn.race.race.thinkTreeMain.thinkRoot.subNodes.FindIndex(x => x is ThinkNode_Subtree subtree
                                && subtree.treeDef == MechanoidFoundryDefOf.Downed);
                            if (index >= 0)
                            {
                                var toAdd = new ThinkNode_Subtree
                                {
                                    treeDef = MF_MainMachineBehaviourViolentActive,
                                };
                                if (index + 1 < pawn.race.race.thinkTreeMain.thinkRoot.subNodes.Count)
                                {
                                    pawn.race.race.thinkTreeMain.thinkRoot.subNodes.Insert(index + 1, toAdd);
                                }
                                else
                                {
                                    pawn.race.race.thinkTreeMain.thinkRoot.subNodes.Add(toAdd);
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(DefGenerator), "GenerateImpliedDefs_PreResolve")]
    public static class GenerateImpliedDefs_PreResolve_Patch
    {
        public static void Prefix()
        {
            foreach (var pawn in DefDatabase<PawnKindDef>.AllDefs)
            {
                if (pawn.CanBeCreatedAndHacked())
                {
                    var recipeDef = GetMechRecipeDef(pawn);
                    recipeDef.PostLoad();
                    DefDatabase<RecipeDef>.Add(recipeDef);
                    Log.Message("Adding recipe " + pawn);
                }
            }
        }
        private static RecipeDef GetMechRecipeDef(PawnKindDef mech)
        {
            float costMultiplier = GetMechCostMultiplier(mech);
            var recipe = new RecipeDef
            {
                recipeUsers = new List<ThingDef>
                {
                    MechanoidFoundryDefOf.MF_MechanoidFoundry
                },
                skillRequirements = new List<SkillRequirement>
                {
                    new SkillRequirement
                    {
                        skill = SkillDefOf.Crafting,
                        minLevel = 8
                    }
                },
                workerClass = typeof(RecipeMakeMechanoid),
                uiIconThing = mech.race,
                workSpeedStat = StatDefOf.GeneralLaborSpeed,
                effectWorking = EffecterDefOf.ConstructMetal,
                soundWorking = SoundDef.Named("Recipe_Machining"),
                unfinishedThingDef = ThingDef.Named("UnfinishedComponent"),
                jobString = "MF.MakingMechanoid".Translate(mech.label),
                workSkill = SkillDefOf.Crafting,
                defName = "MakeMechanoid_" + mech.defName,
                label = "MF.MakeRecipeLabel".Translate(mech.label),
                description = "MF.MakeRecipeDescription".Translate(mech.label),
                workAmount = 10000 * mech.race.race.baseBodySize,
                products = new List<ThingDefCountClass>
                {
                    new ThingDefCountClass
                    {
                        thingDef = mech.race,
                        count = 1,
                    }
                }
            };
            if (MechanoidFoundryMod.settings.buildPropsByDefs.TryGetValue(mech.defName, out var props))
            {
                recipe.fixedIngredientFilter = new ThingFilter
                {
                    thingDefs = new List<ThingDef>()
                };
                recipe.defaultIngredientFilter = new ThingFilter
                {
                    thingDefs = new List<ThingDef>()
                };
                recipe.ingredients = new List<IngredientCount>();
                foreach (var cost in props.costList)
                {
                    var thingDef = DefDatabase<ThingDef>.GetNamedSilentFail(cost.Key);
                    if (thingDef != null)
                    {
                        recipe.fixedIngredientFilter.thingDefs.Add(thingDef);
                        recipe.defaultIngredientFilter.thingDefs.Add(thingDef);
                        recipe.ingredients.Add(NewIngredient(thingDef, cost.Value));
                    }
                }
            }
            else
            {
                recipe.fixedIngredientFilter = new ThingFilter
                {
                    thingDefs = new List<ThingDef>
                    {
                        ThingDef.Named("Steel"),
                        ThingDef.Named("Plasteel"),
                        ThingDef.Named("ComponentIndustrial"),
                        ThingDef.Named("ComponentSpacer"),
                    }
                };
                recipe.defaultIngredientFilter = new ThingFilter
                {
                    thingDefs = new List<ThingDef>
                    {
                        ThingDef.Named("Steel"),
                        ThingDef.Named("Plasteel"),
                        ThingDef.Named("ComponentIndustrial"),
                        ThingDef.Named("ComponentSpacer"),
                    }
                };
                recipe.ingredients = new List<IngredientCount>
                {
                    NewIngredient(ThingDef.Named("Steel"), (int)(500 * costMultiplier)),
                    NewIngredient(ThingDef.Named("Plasteel"), (int)(400 * costMultiplier)),
                    NewIngredient(ThingDef.Named("ComponentIndustrial"), (int)(20 * costMultiplier)),
                    NewIngredient(ThingDef.Named("ComponentSpacer"), (int)(5 * costMultiplier)),
                };
            }
            return recipe;
        }
        public static float GetMechCostMultiplier(PawnKindDef mech)
        {
            float costMultiplier = 1;
            if (mech.combatPower <= 100)
            {
                costMultiplier = 0.25f;
            }
            else if (mech.combatPower <= 200)
            {
                costMultiplier = 0.5f;
            }
            else if (mech.combatPower <= 300)
            {
                costMultiplier = 0.75f;
            }

            return costMultiplier;
        }

        public static IngredientCount NewIngredient(ThingDef ingredient, int baseCount)
        {
            return new IngredientCount
            {
                filter = new ThingFilter
                {
                    thingDefs = new List<ThingDef>
                        {
                            ingredient
                        }
                },
                count = baseCount
            };
        }
    }
}


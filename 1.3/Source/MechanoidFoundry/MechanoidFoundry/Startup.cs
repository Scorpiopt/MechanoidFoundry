using AnimalBehaviours;
using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;

namespace MechanoidFoundry
{
    public class RecipeMakeMechanoid : RecipeWorker
    {
    }

    public class MechanoidFoundryMod : Mod
    {
        public MechanoidFoundryMod(ModContentPack content) : base(content)
        {
            new Harmony("MechanoidFoundry.Mod").PatchAll();
        }
    }

    [StaticConstructorOnStartup]
    public static class Startup
    {
        static Startup()
        {
            foreach (var pawn in DefDatabase<PawnKindDef>.AllDefs)
            {
                if (pawn.RaceProps.IsMechanoid)
                {
                    var compProps = pawn.race.GetCompProperties<CompProperties_Draftable>();
                    if (compProps is null)
                    {
                        pawn.race.comps.Add(new CompProperties_Draftable());
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
                if (pawn.RaceProps.IsMechanoid)
                {
                    var recipeDef = GetMechRecipeDef(pawn);
                    recipeDef.PostLoad();
                    DefDatabase<RecipeDef>.Add(recipeDef);
                }
            }
        }
        private static RecipeDef GetMechRecipeDef(PawnKindDef mech)
        {
            float costMultiplier = 1;
            if (mech.race.race.baseBodySize <= 1)
            {
                costMultiplier = 0.25f;
            }
            else if (mech.race.race.baseBodySize <= 2)
            {
                costMultiplier = 0.5f;
            }
            return new RecipeDef
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
                fixedIngredientFilter = new ThingFilter
                {
                    thingDefs = new List<ThingDef>
                    {
                        ThingDef.Named("Steel"),
                        ThingDef.Named("Plasteel"),
                        ThingDef.Named("ComponentIndustrial"),
                        ThingDef.Named("ComponentSpacer"),
                        ThingDef.Named("Chemfuel"),
                        ThingDef.Named("Gold"),
                        ThingDef.Named("Uranium"),
                    }
                },
                defaultIngredientFilter = new ThingFilter
                {
                    thingDefs = new List<ThingDef>
                    {
                        ThingDef.Named("Steel"),
                        ThingDef.Named("Plasteel"),
                        ThingDef.Named("ComponentIndustrial"),
                        ThingDef.Named("ComponentSpacer"),
                        ThingDef.Named("Chemfuel"),
                        ThingDef.Named("Gold"),
                        ThingDef.Named("Uranium"),
                    }
                },
                defName = "MakeMechanoid_" + mech.defName,
                label = "MF.MakeRecipeLabel".Translate(mech.label),
                description = "MF.MakeRecipeDescription".Translate(mech.label),
                workAmount = 10000 * mech.race.race.baseBodySize,
                ingredients = new List<IngredientCount>
                {
                    NewIngredient(ThingDef.Named("Steel"), (int)(500 * costMultiplier)),
                    NewIngredient(ThingDef.Named("Plasteel"), (int)(400 * costMultiplier)),
                    NewIngredient(ThingDef.Named("ComponentIndustrial"), (int)(100 * costMultiplier)),
                    NewIngredient(ThingDef.Named("ComponentSpacer"), (int)(50 * costMultiplier)),
                    NewIngredient(ThingDef.Named("Chemfuel"), (int)(300 * costMultiplier)),
                    NewIngredient(ThingDef.Named("Gold"), (int)(100 * costMultiplier)),
                    NewIngredient(ThingDef.Named("Uranium"), (int)(250 * costMultiplier)),
                },
                products = new List<ThingDefCountClass>
                {
                    new ThingDefCountClass
                    {
                        thingDef = mech.race,
                        count = 1,
                    }
                }
            };

            IngredientCount NewIngredient(ThingDef ingredient, int baseCount)
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
                    count = Mathf.Max(baseCount, (int)mech.race.race.baseBodySize)
                };
            }
        }
    }
    [HarmonyPatch(typeof(GenDraw), nameof(GenDraw.DrawInteractionCell))]
    public static class GenDraw_DrawInteractionCell_Patch
    {
        public static bool Prefix(ThingDef tDef, IntVec3 center, Rot4 placingRot)
        {
            if (tDef == MechanoidFoundryDefOf.MF_MechanoidFoundry)
            {
                return false;
            }
            return true;
        }
    }
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
            Log.Message("1 pawn: " + pawn + " - " + pawn.equipment.Primary);
            GenSpawn.Spawn(pawn, worker.Position, worker.Map);
            Log.Message("2 pawn: " + pawn + " - " + pawn.equipment.Primary);
            var eq = pawn.equipment.Primary;
            pawn.equipment.Remove(eq);
            pawn.SetFaction(Faction.OfPlayer);
            Log.Message("2 pawn: " + pawn + " - " + pawn.equipment.Primary);
            pawn.equipment.AddEquipment(eq);
            Log.Message("3 pawn: " + pawn + " - " + pawn.equipment.Primary);
        }
    }

    [HarmonyPatch(typeof(PawnComponentsUtility), "AddAndRemoveDynamicComponents")]
    public static class PawnComponentsUtility_AddAndRemoveDynamicComponents
    {
        static void Postfix(Pawn pawn)
        {
            //These two flags detect if the creature is part of the colony and if it has the custom class
            bool flagIsCreatureMine = pawn.Faction != null && pawn.Faction.IsPlayer;
            bool flagIsCreatureDraftable = (pawn.RaceProps.IsMechanoid);
            if (flagIsCreatureMine && flagIsCreatureDraftable)
            {
                if (pawn.drafter is null)
                {
                    pawn.drafter = new Pawn_DraftController(pawn);
                }
                if (pawn.relations == null)
                {
                    pawn.relations = new Pawn_RelationsTracker(pawn);
                }
                if (pawn.story == null)
                {
                    pawn.story = new Pawn_StoryTracker(pawn);
                }
                if (pawn.ownership == null)
                {
                    pawn.ownership = new Pawn_Ownership(pawn);
                }
            }
        }
    }

    [HarmonyPatch(typeof(StatWorker), "IsDisabledFor")]
    static class StatWorker_IsDisabledFor
    {
        static bool Prefix(Thing thing, ref bool __result)
        {

            if (thing is Pawn && ((Pawn)thing).RaceProps.IsMechanoid)
            {
                Pawn pawn = (Pawn)thing;
                if (pawn.Faction == Faction.OfPlayer)
                {
                    __result = false;
                    return false;
                }
            }
            return true;
        }
    }
}


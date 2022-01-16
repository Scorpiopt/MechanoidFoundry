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
using WhatTheHack;
using WhatTheHack.Needs;

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
                        skill = SkillDefOf.Intellectual,
                        minLevel = 6
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
                workAmount = 6666 * mech.race.race.baseBodySize,
                ingredients = new List<IngredientCount>
                {
                    NewIngredient(ThingDef.Named("Steel"), 500),
                    NewIngredient(ThingDef.Named("Plasteel"), 400),
                    NewIngredient(ThingDef.Named("ComponentIndustrial"), 100),
                    NewIngredient(ThingDef.Named("ComponentSpacer"), 50),
                    NewIngredient(ThingDef.Named("Chemfuel"), 300),
                    NewIngredient(ThingDef.Named("Gold"), 100),
                    NewIngredient(ThingDef.Named("Uranium"), 250),
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
            var pawn = PawnGenerator.GeneratePawn(pawnKindDef, Faction.OfPlayer);
            pawn.health.AddHediff(WTH_DefOf.WTH_TargetingHacked);
            GenSpawn.Spawn(pawn, worker.Position, worker.Map);
            pawn.drafter = new Pawn_DraftController(pawn);
            Need_Power powerNeed = (Need_Power)pawn.needs.TryGetNeed(WTH_DefOf.WTH_Mechanoid_Power);
            powerNeed.CurLevel = powerNeed.MaxLevel;
            pawn.health.AddHediff(WTH_DefOf.WTH_BackupBattery, pawn.TryGetReactor());
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
            pawn.Name = PawnBioAndNameGenerator.GeneratePawnName(pawn, NameStyle.Full);
            Find.LetterStack.ReceiveLetter("WTH_Letter_Success_Label".Translate(), "WTH_Letter_Success_Label_Description".Translate(new object[] { worker.Name.ToStringShort, pawn.Name }), LetterDefOf.PositiveEvent, pawn);
            worker.jobs.jobQueue.EnqueueFirst(new Job(WTH_DefOf.WTH_ClearHackingTable, pawn, pawn.CurrentBed()) { count = 1 });
            LessonAutoActivator.TeachOpportunity(WTH_DefOf.WTH_Power, OpportunityType.Important);
            LessonAutoActivator.TeachOpportunity(WTH_DefOf.WTH_Maintenance, OpportunityType.Important);
            LessonAutoActivator.TeachOpportunity(WTH_DefOf.WTH_Concept_MechanoidParts, OpportunityType.Important);
            Log.Message(pawn + " - " + pawn.IsHacked() + " - " + pawn.Faction);
        }
    }
}


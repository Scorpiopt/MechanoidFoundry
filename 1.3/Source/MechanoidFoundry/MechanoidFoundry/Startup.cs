using AnimalBehaviours;
using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

    public class RecipeHackMechanoid : RecipeWorker
    {
        public override bool AvailableOnNow(Thing thing, BodyPartRecord part = null)
        {
            if (thing is Pawn pawn && !pawn.Dead)
            {
                return false;
            }
            return base.AvailableOnNow(thing, part);
        }

        public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            ResurrectionUtility.Resurrect(pawn);
            var eq = pawn.equipment.Primary;
            if (eq != null)
            {
                pawn.equipment.Remove(eq);
            }
            pawn.SetFaction(Faction.OfPlayer);
            PawnComponentsUtility_AddAndRemoveDynamicComponents.MakeComponentsToHackedMechanoid(pawn);
            if (eq != null)
            {
                pawn.equipment.AddEquipment(eq);
            }
        }
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
                    pawn.race.AllRecipes.Add(MechanoidFoundryDefOf.MF_HackMechanoid);
                    pawn.race.race.corpseDef.recipes.Add(MechanoidFoundryDefOf.MF_HackMechanoid);
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

    [HarmonyPatch(typeof(PawnComponentsUtility), "AddAndRemoveDynamicComponents")]
    public static class PawnComponentsUtility_AddAndRemoveDynamicComponents
    {
        static void Postfix(Pawn pawn)
        {
            //These two flags detect if the creature is part of the colony and if it has the custom class
            MakeComponentsToHackedMechanoid(pawn);
        }

        public static void MakeComponentsToHackedMechanoid(Pawn pawn)
        {
            bool flagIsCreatureMine = pawn.Faction != null && pawn.Faction.IsPlayer;
            bool flagIsCreatureDraftable = pawn.RaceProps.IsMechanoid;
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

    [HarmonyPatch(typeof(ITab_Pawn_Health), "ShouldAllowOperations")]
    static class ITab_Pawn_Health_ShouldAllowOperations
    {
        static void Postfix(ITab_Pawn_Health __instance, ref bool __result)
        {
            if (__result is false)
            {
                var pawn = __instance.PawnForHealth;
                if (pawn != null && pawn.Dead && pawn.RaceProps.IsMechanoid)
                {
                    __result = ShouldAllowOperations(pawn);
                }
            }
        }

        public static bool ShouldAllowOperations(Pawn pawn)
        {
            if (!pawn.def.AllRecipes.Any((RecipeDef x) => x.AvailableNow && x.AvailableOnNow(pawn)))
            {
                return false;
            }
            return true;
        }
    }


    [HarmonyPatch(typeof(HealthCardUtility), "DrawPawnHealthCard")]
    static class HealthCardUtility_DrawPawnHealthCard
    {
        static void Prefix(Rect outRect, Pawn pawn, ref bool allowOperations, bool showBloodLoss, Thing thingForMedBills)
        {
            if (allowOperations && pawn.Dead)
            {
                allowOperations = false;
            }
        }
    }

    [HarmonyPatch(typeof(HealthCardUtility), "DrawHealthSummary")]
    static class HealthCardUtility_DrawHealthSummary
    {
        static void Prefix(Rect rect, Pawn pawn, ref bool allowOperations, Thing thingForMedBills)
        {
            if (!allowOperations && pawn.Dead && pawn.RaceProps.IsMechanoid)
            {
                allowOperations = ITab_Pawn_Health_ShouldAllowOperations.ShouldAllowOperations(pawn);
            }
        }
    }

    [HarmonyPatch(typeof(HealthCardUtility), "DrawMedOperationsTab")]
    static class HealthCardUtility_DrawMedOperationsTab
    {
        static bool Prefix(Rect leftRect, Pawn pawn, ref Thing thingForMedBills, float curY, ref float __result)
        {
            if (thingForMedBills is Corpse corpse && corpse.InnerPawn.RaceProps.IsMechanoid)
            {
                DrawMedOperationsTab(leftRect, pawn, corpse, curY);
                return false;
            }
            return true;
        }

        private static float DrawMedOperationsTab(Rect leftRect, Pawn pawn, Corpse corspe, float curY)
        {
            curY += 2f;
            Func<List<FloatMenuOption>> recipeOptionsMaker = delegate
            {
                List<FloatMenuOption> list = new List<FloatMenuOption>();
                var map = corspe.Map;
                foreach (RecipeDef allRecipe in corspe.InnerPawn.def.AllRecipes)
                {
                    if (allRecipe.AvailableNow && allRecipe.AvailableOnNow(pawn))
                    {
                        IEnumerable<ThingDef> enumerable = allRecipe.PotentiallyMissingIngredients(null, map);
                        if (!enumerable.Any((ThingDef x) => x.isTechHediff) && !enumerable.Any((ThingDef x) => x.IsDrug) && (!enumerable.Any() || !allRecipe.dontShowIfAnyIngredientMissing))
                        {
                            if (allRecipe.targetsBodyPart)
                            {
                                foreach (BodyPartRecord item in allRecipe.Worker.GetPartsToApplyOn(pawn, allRecipe))
                                {
                                    if (allRecipe.AvailableOnNow(pawn, item))
                                    {
                                        list.Add(GenerateSurgeryOption(pawn, corspe, allRecipe, enumerable, item));
                                    }
                                }
                            }
                            else
                            {
                                list.Add(GenerateSurgeryOption(pawn, corspe, allRecipe, enumerable));
                            }
                        }
                    }
                }
                return list;
            };
            Rect rect = new Rect(leftRect.x - 9f, curY, leftRect.width, leftRect.height - curY - 20f);
            ((IBillGiver)corspe).BillStack.DoListing(rect, recipeOptionsMaker, ref HealthCardUtility.billsScrollPosition, ref HealthCardUtility.billsScrollHeight);
            return curY;
        }


        private static FloatMenuOption GenerateSurgeryOption(Pawn pawn, Corpse corpse, RecipeDef recipe, IEnumerable<ThingDef> missingIngredients, BodyPartRecord part = null)
        {
            string text = recipe.Worker.GetLabelWhenUsedOn(pawn, part).CapitalizeFirst();
            if (part != null && !recipe.hideBodyPartNames)
            {
                text = text + " (" + part.Label + ")";
            }
            FloatMenuOption floatMenuOption;
            if (missingIngredients.Any())
            {
                text += " (";
                bool flag = true;
                foreach (ThingDef missingIngredient in missingIngredients)
                {
                    if (!flag)
                    {
                        text += ", ";
                    }
                    flag = false;
                    text += "MissingMedicalBillIngredient".Translate(missingIngredient.label);
                }
                text += ")";
                floatMenuOption = new FloatMenuOption(text, null);
            }
            else
            {
                Action action = delegate
                {
                    CreateSurgeryBill(corpse, recipe, part);
                };
                floatMenuOption = ((recipe.Worker is Recipe_AdministerIngestible) ? 
                    new FloatMenuOption(text, action, recipe.ingredients.FirstOrDefault()?.FixedIngredient) : 
                    ((!(recipe.Worker is Recipe_RemoveBodyPart)) ? 
                    new FloatMenuOption(text, action, null) : new FloatMenuOption(text, action, part.def.spawnThingOnRemoved)));
            }
            floatMenuOption.extraPartWidth = 29f;
            floatMenuOption.extraPartOnGUI = (Rect rect) => Widgets.InfoCardButton(rect.x + 5f, rect.y + (rect.height - 24f) / 2f, recipe);
            return floatMenuOption;
        }

        private static void CreateSurgeryBill(Corpse corpse, RecipeDef recipe, BodyPartRecord part)
        {
            Bill_Medical bill_Medical = new Bill_Medical(recipe);
            corpse.BillStack.AddBill(bill_Medical);
            bill_Medical.Part = part;
            if (recipe.conceptLearned != null)
            {
                PlayerKnowledgeDatabase.KnowledgeDemonstrated(recipe.conceptLearned, KnowledgeAmount.Total);
            }
            Map map = corpse.Map;
            if (!map.mapPawns.FreeColonists.Any((Pawn col) => recipe.PawnSatisfiesSkillRequirements(col)))
            {
                Bill.CreateNoPawnsWithSkillDialog(recipe);
            }
        }
    }
    public class WorkGiver_DoBillHacking : WorkGiver_DoBill
    {

    }
}


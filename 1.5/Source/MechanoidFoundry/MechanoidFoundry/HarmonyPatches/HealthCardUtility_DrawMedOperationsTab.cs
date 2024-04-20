using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace MechanoidFoundry
{
    [HarmonyPatch(typeof(HealthCardUtility), "DrawMedOperationsTab")]
    internal static class HealthCardUtility_DrawMedOperationsTab
    {
        private static bool Prefix(Rect leftRect, Pawn pawn, ref Thing thingForMedBills, float curY, ref float __result)
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
            List<FloatMenuOption> recipeOptionsMaker()
            {
                var list = new List<FloatMenuOption>();
                var map = corspe.Map;
                foreach (var allRecipe in corspe.InnerPawn.def.AllRecipes)
                {
                    if (allRecipe.AvailableNow && allRecipe.AvailableOnNow(pawn))
                    {
                        var enumerable = allRecipe.PotentiallyMissingIngredients(null, map);
                        if (!enumerable.Any((ThingDef x) => x.isTechHediff) && !enumerable.Any((ThingDef x) => x.IsDrug) && (!enumerable.Any() || !allRecipe.dontShowIfAnyIngredientMissing))
                        {
                            if (allRecipe.targetsBodyPart)
                            {
                                foreach (var item in allRecipe.Worker.GetPartsToApplyOn(pawn, allRecipe))
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
            }
            var rect = new Rect(leftRect.x - 9f, curY, leftRect.width, leftRect.height - curY - 20f);
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
                foreach (var missingIngredient in missingIngredients)
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
                void action()
                {
                    CreateSurgeryBill(corpse, recipe, part);
                }
                floatMenuOption = (recipe.Worker is Recipe_AdministerIngestible) ?
                    new FloatMenuOption(text, action, recipe.ingredients.FirstOrDefault()?.FixedIngredient) :
                    ((!(recipe.Worker is Recipe_RemoveBodyPart)) ?
                    new FloatMenuOption(text, action, null) : new FloatMenuOption(text, action, part.def.spawnThingOnRemoved));
            }
            floatMenuOption.extraPartWidth = 29f;
            floatMenuOption.extraPartOnGUI = (Rect rect) => Widgets.InfoCardButton(rect.x + 5f, rect.y + ((rect.height - 24f) / 2f), recipe);
            return floatMenuOption;
        }

        private static void CreateSurgeryBill(Corpse corpse, RecipeDef recipe, BodyPartRecord part)
        {
            var bill_Medical = new Bill_Medical(recipe, null);
            corpse.BillStack.AddBill(bill_Medical);
            bill_Medical.Part = part;
            if (recipe.conceptLearned != null)
            {
                PlayerKnowledgeDatabase.KnowledgeDemonstrated(recipe.conceptLearned, KnowledgeAmount.Total);
            }
            var map = corpse.Map;
            if (!map.mapPawns.FreeColonists.Any((Pawn col) => recipe.PawnSatisfiesSkillRequirements(col)))
            {
                Bill.CreateNoPawnsWithSkillDialog(recipe);
            }
        }
    }
}


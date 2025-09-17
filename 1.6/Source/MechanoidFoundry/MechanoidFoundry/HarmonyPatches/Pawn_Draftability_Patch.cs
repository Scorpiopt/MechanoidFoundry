using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.Sound;
using System;
using LudeonTK;

namespace MechanoidFoundry
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class HotSwappableAttribute : Attribute
    {
    }

    [HotSwappable]
    [HarmonyPatch(typeof(Pawn), "GetGizmos")]
    public static class Pawn_GetGizmos_Draftability_Patch
    {
        public static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> __result, Pawn __instance)
        {
            if (__instance.IsMechanoidHacked())
            {
                Command_Toggle draftGizmo = new Command_Toggle();
                draftGizmo.defaultLabel = (__instance.drafter != null && __instance.drafter.Drafted) ? "CommandUndraftLabel".Translate() : "CommandDraftLabel".Translate();
                draftGizmo.defaultDesc = "CommandToggleDraftDesc".Translate();
                draftGizmo.icon = TexCommand.Draft;
                draftGizmo.hotKey = KeyBindingDefOf.Command_ColonistDraft;
                draftGizmo.isActive = (() => __instance.drafter != null && __instance.drafter.Drafted);
                draftGizmo.toggleAction = delegate
                {
                    if (__instance.drafter == null)
                    {
                        __instance.drafter = new Pawn_DraftController(__instance);
                    }
                    __instance.drafter.Drafted = !__instance.drafter.Drafted;
                    if (__instance.drafter.Drafted)
                    {
                        PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.Drafting, KnowledgeAmount.SpecificInteraction);
                        LessonAutoActivator.TeachOpportunity(ConceptDefOf.QueueOrders, OpportunityType.GoodToKnow);
                    }
                };
                draftGizmo.turnOnSound = SoundDefOf.DraftOn;
                draftGizmo.turnOffSound = SoundDefOf.DraftOff;
                if (__instance.Downed)
                {
                    draftGizmo.Disable("IsIncapped".Translate(__instance.LabelShort, __instance));
                }
                if (__instance.drafter != null && __instance.drafter.Drafted)
                {
                    draftGizmo.tutorTag = "Undraft";
                }
                else
                {
                    draftGizmo.tutorTag = "Draft";
                }
                
                yield return draftGizmo;
                foreach (var g in __result)
                {
                    if (g is Command_Toggle command && command.defaultDesc == "CommandToggleDraftDesc".Translate())
                    {
                        continue;
                    }
                    yield return g;
                }
            }
            else
            {
                foreach (var g in __result)
                {
                    yield return g;
                }
            }
        }
    }
    
    [HarmonyPatch(typeof(Pawn), "CanTakeOrder", MethodType.Getter)]
    public static class Pawn_CanTakeOrder_Draftability_Patch
    {
        public static void Postfix(Pawn __instance, ref bool __result)
        {
            if (!__result && __instance.IsMechanoidHacked() && __instance.drafter != null)
            {
                __result = true;
            }
        }
    }
    
    [HarmonyPatch(typeof(Pawn), "IsColonistPlayerControlled", MethodType.Getter)]
    public static class Pawn_IsColonistPlayerControlled_Draftability_Patch
    {
        public static void Postfix(Pawn __instance, ref bool __result)
        {
            if (!__result && __instance.IsMechanoidHacked() && __instance.drafter != null && __instance.drafter.Drafted)
            {
                __result = true;
            }
        }
    }
}

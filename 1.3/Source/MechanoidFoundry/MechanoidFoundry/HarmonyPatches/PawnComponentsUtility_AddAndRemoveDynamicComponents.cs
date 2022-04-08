using HarmonyLib;
using RimWorld;
using Verse;

namespace MechanoidFoundry
{

    [HarmonyPatch(typeof(PawnComponentsUtility), "AddAndRemoveDynamicComponents")]
    public static class PawnComponentsUtility_AddAndRemoveDynamicComponents
    {
        static void Postfix(Pawn pawn)
        {
            MakeComponentsToHackedMechanoid(pawn);
        }

        public static void MakeComponentsToHackedMechanoid(Pawn pawn)
        {
            if (pawn.IsMechanoidHacked())
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
}


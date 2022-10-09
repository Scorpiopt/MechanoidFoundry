using RimWorld;
using System.Collections.Generic;
using Verse;

namespace MechanoidFoundry
{
    public class RecipeModifyMechanoid : RecipeWorker
    {
        public override bool AvailableOnNow(Thing thing, BodyPartRecord part = null)
        {
            return true;
            if (thing is Pawn pawn && !pawn.IsMechanoidHacked())
            {
                return false;
            }
            return base.AvailableOnNow(thing, part);
        }

        public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            if (recipe.addsHediff != null)
            {
                pawn.health.AddHediff(recipe.addsHediff, part, null);
            }
        }
    }

    public class RecipeInstallOnHackableMechanoid : DefModExtension
    {

    }

    public class HediffExtension : DefModExtension
    {
        public List<WorkTypeDef> workTypes;
        public int? skillLevel = -1;
    }
    public class Hediff_MechanoidModification : HediffWithComps
    {
        public override void PostAdd(DamageInfo? dinfo)
        {
            base.PostAdd(dinfo);
            var extension = def.GetModExtension<HediffExtension>();
            if (extension?.workTypes != null)
            {
                if (pawn.story == null)
                {
                    pawn.story = new Pawn_StoryTracker(pawn);
                }

                if (pawn.skills == null)
                {
                    pawn.skills = new Pawn_SkillTracker(pawn);
                }

                if (pawn.workSettings == null)
                {
                    pawn.workSettings = new Pawn_WorkSettings(pawn);
                    pawn.workSettings.EnableAndInitializeIfNotAlreadyInitialized();
                    pawn.workSettings.priorities.SetAll(0);
                }

                foreach (var workType in extension.workTypes)
                {
                    if (extension.skillLevel.HasValue)
                    {
                        foreach (var skill in workType.relevantSkills)
                        {
                            var record = pawn.skills.skills.Find(rec => rec.def == skill);
                            record.levelInt = extension.skillLevel.Value;
                        }
                    }
                    pawn.workSettings.SetPriority(workType, 1);
                }
            }
        }

        public override void PostRemoved()
        {
            base.PostRemoved();
            var extension = def.GetModExtension<HediffExtension>();
            if (extension?.workTypes != null)
            {
                foreach (var workType in extension.workTypes)
                {
                    pawn.workSettings.SetPriority(workType, 0);
                }
            }
        }
    }
}


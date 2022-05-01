using RimWorld;
using System.Collections.Generic;
using System.Linq;
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
            if (this.recipe.addsHediff != null)
            {
                pawn.health.AddHediff(this.recipe.addsHediff, part, null);
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
            var extension = this.def.GetModExtension<HediffExtension>();
            if (extension != null)
            {
                if (extension.workTypes != null)
                {
                    if (pawn.story == null)
                        pawn.story = new Pawn_StoryTracker(pawn);
                    if (pawn.skills == null)
                        pawn.skills = new Pawn_SkillTracker(pawn);
                    if (pawn.workSettings == null)
                    {
                        pawn.workSettings = new Pawn_WorkSettings(pawn);
                        DefMap<WorkTypeDef, int> priorities = new DefMap<WorkTypeDef, int>();
                        priorities.SetAll(0);
                        pawn.workSettings.priorities = priorities;
                    }

                    foreach (WorkTypeDef workType in extension.workTypes)
                    {
                        if (extension.skillLevel.HasValue)
                        {
                            foreach (SkillDef skill in workType.relevantSkills)
                            {
                                SkillRecord record = pawn.skills.skills.Find(rec => rec.def == skill);
                                record.levelInt = extension.skillLevel.Value;
                            }
                        }
                        pawn.workSettings.SetPriority(workType, 1);
                    }
                }
            }
        }

        public override void PostRemoved()
        {
            base.PostRemoved();
            var extension = this.def.GetModExtension<HediffExtension>();
            if (extension.workTypes != null)
            {
                foreach (WorkTypeDef workType in extension.workTypes)
                {
                    pawn.workSettings.SetPriority(workType, 0);
                }
            }
        }
    }
}


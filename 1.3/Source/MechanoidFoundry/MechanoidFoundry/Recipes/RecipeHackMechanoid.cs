using RimWorld;
using System.Collections.Generic;
using Verse;

namespace MechanoidFoundry
{
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
}


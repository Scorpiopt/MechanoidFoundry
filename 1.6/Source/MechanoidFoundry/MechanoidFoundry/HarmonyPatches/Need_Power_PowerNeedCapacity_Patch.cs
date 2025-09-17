using HarmonyLib;
using RimWorld;
using Verse;

namespace MechanoidFoundry
{

    [HarmonyPatch(typeof(HealthAIUtility), "FindBestMedicine")]
    public class HealthAIUtility_FindBestMedicine_Patch
    {
        public static bool Prefix(Pawn healer, Pawn patient)
        {
            if (patient.RaceProps.IsMechanoid)
            {
                return false;
            }
            return true;
        }
    }
}


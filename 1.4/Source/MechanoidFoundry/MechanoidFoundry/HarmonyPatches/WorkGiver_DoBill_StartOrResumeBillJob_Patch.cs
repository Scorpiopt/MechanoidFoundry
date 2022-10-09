using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Verse.AI;

namespace MechanoidFoundry
{
    [HarmonyPatch(typeof(WorkGiver_DoBill), "StartOrResumeBillJob")]
    public static class WorkGiver_DoBill_StartOrResumeBillJob_Patch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            var codes = codeInstructions.ToList();
            var reservationUtilityCanReserveInfo = AccessTools.Method(typeof(ReservationUtility), nameof(ReservationUtility.CanReserve));
            for (int i = 0; i < codes.Count; i++)
            {
                var code = codes[i];
                yield return code;
                if (i > 1 && codes[i - 1].Calls(reservationUtilityCanReserveInfo) && code.opcode == OpCodes.Brtrue_S)
                {
                    yield return new CodeInstruction(OpCodes.Ldloc_S, 4);
                    yield return new CodeInstruction(OpCodes.Call,
                        AccessTools.Method(typeof(WorkGiver_DoBill_StartOrResumeBillJob_Patch), nameof(BillIsNotMechHacking)));
                    yield return new CodeInstruction(OpCodes.Brfalse_S, code.operand);
                }
            }
        }

        public static bool BillIsNotMechHacking(Bill_Medical bill_Medical)
        {
            if (bill_Medical.recipe != MechanoidFoundryDefOf.MF_HackMechanoid)
            {
                return true;
            }
            return false;
        }
    }
}


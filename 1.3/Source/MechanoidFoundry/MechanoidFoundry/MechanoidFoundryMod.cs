using HarmonyLib;
using RimWorld;
using System.Diagnostics;
using UnityEngine;
using Verse;
using Verse.AI;

namespace MechanoidFoundry
{
    public class MechanoidFoundryMod : Mod
    {
        public MechanoidFoundryMod(ModContentPack content) : base(content)
        {
            var harmony = new Harmony("MechanoidFoundry.Mod");
            harmony.PatchAll();
            //var postfix = AccessTools.Method(typeof(MechanoidFoundryMod), "Postfix");
            //foreach (var type2 in typeof(ThinkNode).AllSubclassesNonAbstract())
            //{
            //    var methodToPatch2 = AccessTools.Method(type2, "TryIssueJobPackage");
            //    try
            //    {
            //        harmony.Patch(methodToPatch2, new HarmonyMethod(postfix));
            //    }
            //    catch { }
            //}
            //var postfix2 = AccessTools.Method(typeof(MechanoidFoundryMod), "Postfix2");
            //foreach (var type3 in typeof(ThinkNode_Conditional).AllSubclassesNonAbstract())
            //{
            //    var methodToPatch3 = AccessTools.Method(type3, "Satisfied");
            //    try
            //    {
            //        harmony.Patch(methodToPatch3, new HarmonyMethod(postfix2));
            //    }
            //    catch { }
            //}
            //
            //var postfix3 = AccessTools.Method(typeof(MechanoidFoundryMod), "Postfix3");
            //foreach (var type3 in typeof(WorkGiver_Scanner).AllSubclassesNonAbstract())
            //{
            //    var methodToPatch3 = AccessTools.Method(type3, "HasJobOnThing");
            //    try
            //    {
            //        harmony.Patch(methodToPatch3, new HarmonyMethod(postfix3));
            //    }
            //    catch { }
            //}
            //
            //var postfix4 = AccessTools.Method(typeof(MechanoidFoundryMod), "Postfix4");
            //foreach (var type3 in typeof(WorkGiver_Scanner).AllSubclassesNonAbstract())
            //{
            //    var methodToPatch3 = AccessTools.Method(type3, "JobOnThing");
            //    try
            //    {
            //        harmony.Patch(methodToPatch3, new HarmonyMethod(postfix4));
            //    }
            //    catch { }
            //}
        }

        //private static void Postfix(ThinkNode __instance, ThinkResult __result, Pawn pawn, JobIssueParams jobParams)
        //{
        //    if (pawn.IsColonist)
        //    {
        //        Log.Message(pawn + " gets " + __result.Job + " from " + __instance);
        //        if (__instance is ThinkNode_Subtree subtree)
        //        {
        //            Log.Message("Subtree: " + subtree.treeDef);
        //        }
        //    }
        //}
        //
        //private static void Postfix2(ThinkNode_Conditional __instance, bool __result, Pawn pawn)
        //{
        //    if (pawn.IsColonist)
        //    {
        //        Log.Message(pawn + " gets " + __result + " from " + __instance);
        //    }
        //}
        //
        //private static void Postfix3(WorkGiver_Scanner __instance, bool __result, Pawn pawn)
        //{
        //    if (pawn.IsColonist)
        //    {
        //        Log.Message(pawn + " gets " + __result + " from " + __instance);
        //    }
        //}
        //
        //private static void Postfix4(WorkGiver_Scanner __instance, Job __result, Pawn pawn)
        //{
        //    if (pawn.IsColonist)
        //    {
        //        Log.Message(pawn + " gets " + __result + " from " + __instance);
        //    }
        //}
    }

    //[HarmonyPatch(typeof(Pawn_JobTracker), "StartJob")]
    //public class StartJobPatch
    //{
    //    private static void Postfix(Pawn_JobTracker __instance, Pawn ___pawn, Job newJob, JobTag? tag)
    //    {
    //        if (___pawn.IsColonist)
    //        {
    //            Log.Message(___pawn + " is starting " + newJob);
    //        }
    //    }
    //}
    //
    //
    //[HarmonyPatch(typeof(Pawn_JobTracker), "EndCurrentJob")]
    //public class EndCurrentJobPatch
    //{
    //    private static void Prefix(Pawn_JobTracker __instance, Pawn ___pawn, JobCondition condition, ref bool startNewJob, bool canReturnToPool = true)
    //    {
    //        if (___pawn.IsColonist)
    //        {
    //            Log.Message(___pawn + " is ending " + ___pawn.CurJob);
    //        }
    //    }
    //}
    //
    //[HarmonyPatch(typeof(ThinkNode_JobGiver), "TryIssueJobPackage")]
    //public class TryIssueJobPackage
    //{
    //    private static void Postfix(ThinkNode_JobGiver __instance, ThinkResult __result, Pawn pawn, JobIssueParams jobParams)
    //    {
    //        if (pawn.IsColonist)
    //        {
    //            Log.Message(pawn + " gets " + __result.Job + " from " + __instance);
    //        }
    //    }
    //}
}


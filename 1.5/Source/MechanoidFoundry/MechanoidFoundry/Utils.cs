using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;

namespace MechanoidFoundry
{
    [StaticConstructorOnStartup]
    public static class Utils
    {
        public static List<ThingDef> buildings = new List<ThingDef>();
        public static List<ThingDef> spawnableItems = new List<ThingDef>();
        static Utils()
        {
            buildings = DefDatabase<ThingDef>.AllDefs.Where(x => typeof(Building).IsAssignableFrom(x.thingClass)
                && !typeof(Frame).IsAssignableFrom(x.thingClass)).ToList();
            spawnableItems = DefDatabase<ThingDef>.AllDefs.Where(x => x.Spawnable()).ToList();
        }

        public static MethodInfo ComponentCache_ResetCompCache_Info = AccessTools.Method("PerformanceOptimizer.ComponentCache:ResetCompCache");
        public static void StripBiotechComps(this Pawn mech)
        {
            mech.AllComps.RemoveAll(x => x is CompOverseerSubject);
            ComponentCache_ResetCompCache_Info?.Invoke(null, new object[] { mech });
        }
        public static bool Spawnable(this ThingDef item)
        {
            try
            {
                return (DebugThingPlaceHelper.IsDebugSpawnable(item) || item.Minifiable)
                    && !typeof(Filth).IsAssignableFrom(item.thingClass)
                    && !typeof(Mote).IsAssignableFrom(item.thingClass)
                    && item.category != ThingCategory.Ethereal && item.plant is null
                    && (item.building is null || item.Minifiable);
            }
            catch (Exception ex)
            {
                Log.Error("Caught error processing " + item + ": " + ex.ToString());
                return false;
            }
        }
    }
}


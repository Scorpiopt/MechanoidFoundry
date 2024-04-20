using System.Collections.Generic;
using Verse;

namespace MechanoidFoundry
{
    public class BuildProperties : IExposable
    {
        public Dictionary<string, int> costList = new Dictionary<string, int>();
        public bool buildable;

        public BuildProperties()
        {

        }
        public void ExposeData()
        {
            Scribe_Values.Look(ref buildable, "buildable");
            Scribe_Collections.Look(ref costList, "costList", LookMode.Value, LookMode.Value);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (costList is null)
                {
                    costList = new Dictionary<string, int>();
                }
            }
        }
    }
}


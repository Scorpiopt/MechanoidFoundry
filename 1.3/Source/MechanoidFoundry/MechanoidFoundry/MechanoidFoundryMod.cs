using HarmonyLib;
using Verse;

namespace MechanoidFoundry
{
    public class MechanoidFoundryMod : Mod
    {
        public MechanoidFoundryMod(ModContentPack content) : base(content)
        {
            new Harmony("MechanoidFoundry.Mod").PatchAll();
        }
    }
}


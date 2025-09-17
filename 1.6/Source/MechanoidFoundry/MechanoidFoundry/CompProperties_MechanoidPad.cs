using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace MechanoidFoundry
{
    public class CompProperties_MechanoidPad : CompProperties
    {
        public float extraChargingPower;
        public float hoursToRecharge = 24;
        public bool killPawnAfterDestroying = true;

        public CompProperties_MechanoidPad()
        {
            this.compClass = typeof(CompMechanoidPad);
        }
    }
}
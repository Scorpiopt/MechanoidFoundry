using HarmonyLib;
using UnityEngine;
using Verse;

namespace MechanoidFoundry
{
    [HarmonyPatch(typeof(PawnUIOverlay), "DrawPawnGUIOverlay")]
    class PawnUIOverlay_DrawPawnGUIOverlay
    {
        static void Postfix(Pawn ___pawn)
        {
            if (___pawn.IsMechanoidHacked())
            {
                Vector2 pos = GenMapUI.LabelDrawPosFor(___pawn, -0.6f);
                GenMapUI.DrawPawnLabel(___pawn, pos, 1f, 9999f, null, GameFont.Tiny, true, true);
            }
        }
    }
}


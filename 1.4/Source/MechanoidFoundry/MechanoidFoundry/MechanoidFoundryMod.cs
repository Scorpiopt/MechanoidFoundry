using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace MechanoidFoundry
{
    public class MechanoidFoundryMod : Mod
    {
        public static MechanoidFoundrySettings settings;
        public MechanoidFoundryMod(ModContentPack content) : base(content)
        {
            settings = GetSettings<MechanoidFoundrySettings>();
            Harmony harmony = new Harmony("MechanoidFoundry.Mod");
            harmony.PatchAll();
        }

        public static bool PreventDestructivePrefix(ref bool __result)
        {
            __result = true;
            return false;
        }
        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);
            settings.DoSettingsWindowContents(inRect);
        }
        public override string SettingsCategory()
        {
            return Content.Name;
        }
    }

    public class MechanoidFoundrySettings : ModSettings
    {
        public Dictionary<string, BuildProperties> buildPropsByDefs = new Dictionary<string, BuildProperties>();
        private int scrollHeightCount = 0;
        private Vector2 firstColumnPos;
        private Vector2 scrollPosition;
        private string buf1;
        private string searchKey;
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref buildPropsByDefs, "buildPropsByDefs", LookMode.Value, LookMode.Deep);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (buildPropsByDefs is null)
                {
                    buildPropsByDefs = new Dictionary<string, BuildProperties>();
                }
            }
        }

        public void DoSettingsWindowContents(Rect inRect)
        {
            Text.Anchor = TextAnchor.MiddleLeft;
            Rect searchLabel = new Rect(inRect.x, inRect.y, 60, 24);
            Widgets.Label(searchLabel, "MF.Search".Translate());
            Rect searchRect = new Rect(searchLabel.xMax + 5, searchLabel.y, 200, 24f);
            searchKey = Widgets.TextField(searchRect, searchKey);
            Text.Anchor = TextAnchor.UpperLeft;

            Rect resetButton = new Rect(searchRect.xMax + 15, searchRect.y, 150, 24);
            if (Widgets.ButtonText(resetButton, "Reset".Translate()))
            {
                buildPropsByDefs.Clear();
                Helpers.cachedResults.Clear();

                Startup.InitializeBuildProps();
            }
            List<PawnKindDef> allMechanoids = new List<PawnKindDef>();
            foreach (string key in buildPropsByDefs.Keys.ToList())
            {
                PawnKindDef pawnKind = DefDatabase<PawnKindDef>.GetNamedSilentFail(key);
                if (pawnKind != null)
                {
                    allMechanoids.Add(pawnKind);
                }
            }


            List<PawnKindDef> defs = searchKey.NullOrEmpty() ? allMechanoids : allMechanoids.Where(x => x.label.Contains(searchKey.ToLower())).ToList();
            Rect rect = new Rect(inRect.x, searchRect.yMax + 15, inRect.width, inRect.height - 50);
            Widgets.BeginGroup(rect);
            DrawPage(rect, defs);
            Widgets.EndGroup();
        }

        public void DrawPage(Rect rect, List<PawnKindDef> defs)
        {
            Rect outRect = new Rect(0, 0, rect.width, rect.height);
            Rect viewRect = new Rect(0, 0, rect.width - 30, scrollHeightCount);
            scrollHeightCount = 0;
            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
            Rect removeRect;
            Rect buttonRect = new Rect(rect.x, rect.y, 250, 24);
            foreach (PawnKindDef pawnKind in defs)
            {
                BuildProperties curBuildProps = buildPropsByDefs[pawnKind.defName];
                firstColumnPos.x = 0;
                float prevY = firstColumnPos.y;
                Rect pawnLabel = new Rect(firstColumnPos.x, firstColumnPos.y, 150, 24);
                Widgets.Label(pawnLabel, pawnKind.LabelCap);
                Rect pawnRect = new Rect(firstColumnPos.x, pawnLabel.yMax, 150, 150);
                Graphic graphic = pawnKind.lifeStages.Last().bodyGraphicData.Graphic;
                Material material = graphic.ExtractInnerGraphicFor(null).MatAt(Rot4.South);
                GUI.DrawTexture(pawnRect, (Texture2D)material.mainTexture);
                firstColumnPos.x += pawnRect.width + 15;
                Rect labelRect = DoLabel(ref firstColumnPos, "MF.SetCostList".Translate());
                string toRemove = "";
                TaggedString craftable = "MF.Craftable".Translate();
                Rect buildableRect = new Rect(firstColumnPos.x + buttonRect.width - 30 + 135 + 24 + 15, firstColumnPos.y,
                    Text.CalcSize(craftable).x + 25, 24);
                Widgets.CheckboxLabeled(buildableRect, craftable, ref curBuildProps.buildable);
                foreach (string key in curBuildProps.costList.Keys.ToList())
                {
                    int costCount = curBuildProps.costList[key];
                    Rect costRect = new Rect(firstColumnPos.x, firstColumnPos.y, buttonRect.width - 30, 24);
                    ThingDef def = DefDatabase<ThingDef>.GetNamedSilentFail(key ?? "");
                    if (def != null)
                    {
                        if (curBuildProps.buildable is false)
                        {
                            GUI.color = Color.grey;
                        }
                        if (Widgets.ButtonText(costRect, def.LabelCap, active: curBuildProps.buildable))
                        {
                            Find.WindowStack.Add(new Window_SelectItem<ThingDef>(Utils.spawnableItems.Where(x => x.IsStuff).ToList(),
                            delegate (ThingDef selected)
                            {
                                curBuildProps.costList.Remove(key);
                                curBuildProps.costList.Add(selected.defName, costCount);
                            }, x => x.index, (ThingDef x) => x.LabelCap));
                        }

                        DoInput(costRect.xMax + 5, firstColumnPos.y, "MF.Count".Translate(), ref costCount, ref buf1);
                        curBuildProps.costList[key] = costCount;
                        removeRect = new Rect(costRect.xMax + 135, firstColumnPos.y, 20, 21f);
                        if (Widgets.ButtonImage(removeRect, TexButton.DeleteX))
                        {
                            toRemove = key;
                        }
                        if (curBuildProps.buildable is false)
                        {
                            GUI.color = Color.grey;
                        }
                        firstColumnPos.y += 24;
                    }
                }

                if (!toRemove.NullOrEmpty())
                {
                    curBuildProps.costList.Remove(toRemove);
                }

                buttonRect = DoButton(ref firstColumnPos, "Add".Translate().CapitalizeFirst(), delegate
                {
                    Find.WindowStack.Add(new Window_SelectItem<ThingDef>(Utils.spawnableItems,
                    delegate (ThingDef selected)
                    {
                        curBuildProps.costList.Add(selected.defName, 1);
                    }, x => x.index, (ThingDef x) => x.LabelCap));
                }, active: curBuildProps.buildable);

                firstColumnPos.y += 12;
                firstColumnPos.y = Mathf.Max(prevY + pawnRect.height + pawnLabel.height, firstColumnPos.y);
                GUI.color = Color.white;
            }

            Widgets.EndScrollView();
            scrollHeightCount = (int)Mathf.Max(rect.height, firstColumnPos.y);
            ResetPositions();
        }
        private void DoInput(float x, float y, string label, ref int count, ref string buffer, float width = 50)
        {
            Rect labelRect = new Rect(x, y, width, 24);
            Widgets.Label(labelRect, label);
            Rect inputRect = new Rect(labelRect.xMax, labelRect.y, 75, 24);
            buffer = count.ToString();
            Widgets.TextFieldNumeric<int>(inputRect, ref count, ref buffer);
        }
        private void DoInput(float x, float y, string label, ref float count, ref string buffer, float width = 50)
        {
            Rect labelRect = new Rect(x, y, width, 24);
            Widgets.Label(labelRect, label);
            Rect inputRect = new Rect(labelRect.xMax, labelRect.y, 75, 24);
            buffer = count.ToString();
            Widgets.TextFieldNumeric<float>(inputRect, ref count, ref buffer);
        }
        private void ResetPositions()
        {
            firstColumnPos = new Vector2(0, 0);
        }
        private static Rect DoLabel(ref Vector2 pos, string label)
        {
            Rect labelRect = new Rect(pos.x, pos.y, 250, 24);
            Widgets.Label(labelRect, label);
            pos.y += 24;
            return labelRect;
        }

        private static Rect DoButton(ref Vector2 pos, string label, Action action, bool active = true)
        {
            Rect buttonRect = new Rect(pos.x, pos.y, 250, 24);
            pos.y += 24;
            if (Widgets.ButtonText(buttonRect, label, active: active))
            {
                UI.UnfocusCurrentControl();
                action();
            }
            return buttonRect;
        }

        public void ResetProps()
        {
            buf1 = "";
        }
    }
}


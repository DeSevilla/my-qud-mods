using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using XRL;
using XRL.World;
using XRL.World.Parts;
using XRL.World.Anatomy;

namespace AnimateStatue.HarmonyPatches {

    [HarmonyPatch(typeof(RandomStatue), nameof(RandomStatue.SetCreature))]
    class RandomStatuePatch {
        [HarmonyPostfix]
        static void Postfix(RandomStatue __instance, GameObject gameObject) {
            __instance.ParentObject.SetStringProperty("Animatable", "Yes");
            __instance.ParentObject.SetStringProperty("BodyType", gameObject.Body.Anatomy);
            __instance.ParentObject.SetStringProperty("BodyCategory", "Stone"); //all random statues are currently stone
            //we could make the body category depend on the material of the statue but we'd need to manually define the mapping
            //so we won't for now. if we need to add bronze statues or something we can do it then
        }
    }

    [HarmonyPatch(typeof(AnimateObject), nameof(AnimateObject.Animate))]
    class AnimateObjectPatch {
        static MethodInfo m_getTagOrProp = typeof(GameObject).GetMethod("GetTagOrStringProperty");
        static MethodInfo m_getTag = typeof(GameObject).GetMethod("GetTag");

        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            var found = false;
            foreach (var instruction in instructions)
            {
                if (!found && instruction.Calls(m_getTag))
                {
                    yield return new CodeInstruction(OpCodes.Callvirt, m_getTagOrProp);
                    found = true;
                }
                else {
                    yield return instruction;
                }
            }
        }

        [HarmonyPostfix]
        static void Postfix(GameObject frankenObject) {
            var cat = frankenObject.GetTagOrStringProperty("BodyCategory", "");
            if (cat != "") {
                var code = BodyPartCategory.GetCode(cat);
                frankenObject.Body.CategorizeAll(code);
            }
       }
    }
}
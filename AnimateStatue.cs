using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using XRL;
using XRL.World;
using XRL.World.Parts;

namespace AnimateStatue.HarmonyPatches {



    [HarmonyPatch(typeof(RandomStatue), nameof(RandomStatue.SetCreature))]
    class RandomStatuePatch {
        [HarmonyPostfix]
        static void Postfix(RandomStatue __instance, GameObject gameObject) {
            __instance.ParentObject.SetStringProperty("Animatable", "Yes");
            __instance.ParentObject.SetStringProperty("BodyType", gameObject.Body.Anatomy);
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
    }
}
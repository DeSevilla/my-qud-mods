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
        static void Postfix(RandomStatue __instance, GameObject creatureObject) {
            __instance.ParentObject.SetStringProperty("Animatable", "Yes"); //this may be unnecessary
            __instance.ParentObject.SetStringProperty("BodyType", creatureObject.Body.Anatomy);
            if (!__instance.ParentObject.HasTagOrStringProperty("AnimateStatue_BodyCategory")) {
                __instance.ParentObject.SetStringProperty("AnimateStatue_BodyCategory", "Stone");
            }
        }
    }

    [HarmonyPatch(typeof(AnimateObject), nameof(AnimateObject.Animate))]
    class AnimateObjectPatch {
        static MethodInfo m_getTagOrProp = typeof(GameObject).GetMethod("GetPropertyOrTag");
        static MethodInfo m_getTag = typeof(GameObject).GetMethod("GetTag");

        //the base method calls GetTag for BodyType instead of GetTagOrStringProperty
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
            var cat = frankenObject.GetPropertyOrTag("AnimateStatue_BodyCategory", "a");
            if (cat != "a") {
                var code = BodyPartCategory.GetCode(cat);
                frankenObject.Body.CategorizeAll(code);
            }
       }
    }
}
using HarmonyLib;
using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using XRL;
using XRL.UI;
using XRL.World;
using XRL.World.Parts;
using XRL.World.Parts.Mutation;
using XRL.World.Anatomy;

namespace AnimateStatue.HarmonyPatches {

    [HarmonyPatch(typeof(RandomStatue), nameof(RandomStatue.SetCreature))]
    class RandomStatuePatch {
        [HarmonyPostfix]
        static void Postfix(RandomStatue __instance, GameObject creatureObject) {
            // Popup.Show(creatureObject.Does("exist"));
            if (creatureObject.Body != null) {
                __instance.ParentObject.SetStringProperty("Animatable", "Yes"); //this may be unnecessary
                __instance.ParentObject.SetStringProperty("BodyType", creatureObject.Body.Anatomy);
                if (!__instance.ParentObject.HasTagOrStringProperty("AnimateStatue_BodyCategory")) {
                    __instance.ParentObject.SetStringProperty("AnimateStatue_BodyCategory", "Stone");
                }
                var creatureMutations = creatureObject.GetPart<Mutations>()?.MutationList.Where(mutation => mutation.IsPhysical());
                if (creatureMutations?.Any() == true) {
                    var mutationNames = creatureMutations.Select(mutation => mutation.GetMutationClass());
                    string mutations = String.Join(",", mutationNames);
                    __instance.ParentObject.SetStringProperty("AnimateStatue_AnimatedMutations", mutations);
                }
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
            var mutationList = frankenObject.GetPropertyOrTag("AnimateStatue_AnimatedMutations", "");
            var creatureMutations = frankenObject.GetPart<Mutations>();
            if (!string.IsNullOrEmpty(mutationList) && creatureMutations != null) {
                string[] mutationNames = mutationList.Split(",");
                foreach (string @name in mutationNames) {
                    // var entry = MutationFactory.GetMutationEntryByName(name);
                    // if (entry.Category.Name == "Physical") {
                    creatureMutations.AddMutation(name, 1); //this doesn't handle levels to avoid annoying string parsing
                    // }
                }
            }
       }
    }
}

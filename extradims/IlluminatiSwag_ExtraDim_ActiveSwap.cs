using System;
using XRL.UI;
using XRL.Rules;
using XRL.World.Effects;

namespace XRL.World.Parts.Mutation {

    [Serializable]
    public class IlluminatiSwag_Extradim_ActiveSwap : BaseMutation
    {
        public static readonly string COMMAND_NAME = "CommandIlluminatiSwagExtraDimSwapBody";

        public string ApplyMessage = null;

        public string RemoveMessage = null;

        public IlluminatiSwag_Extradim_ActiveSwap() {
            DisplayName = "Swap Body";
            base.Type = "Mental";
        }

        public override string GetDescription()
        {
            return "You swap bodies with an adjacent creature.";
        }

        public override string GetLevelText(int Level)
        {
            return "Duration: {{rules|100}} rounds\nCooldown: {{rules|" + GetCooldownTurns(Level) + "}} rounds";
        }

        public int GetCooldownTurns(int Level) {
            return Math.Max(750 - 50 * Level, 200);
        }

        public override bool WantEvent(int ID, int cascade)
        {
            return base.WantEvent(ID, cascade) 
                || ID == AIGetOffensiveAbilityListEvent.ID
                || ID == CommandEvent.ID;
        }

        public override bool HandleEvent(AIGetOffensiveAbilityListEvent E)
        {
            if (E.Distance <= 1 && IsMyActivatedAbilityAIUsable(ActivatedAbilityID) && GameObject.Validate(E.Target) 
                    && CheckRealityDistortionAdvisability(Object: E.Actor, Cell: E.Target.CurrentCell, Actor: E.Actor, Mutation: this)) {
                E.Add(COMMAND_NAME);
            }
            return base.HandleEvent(E);
        }

        public override bool HandleEvent(CommandEvent E)
        {
            if (E.Command == COMMAND_NAME) {
                if (!ParentObject.IsRealityDistortionUsable()) {
                    RealityStabilized.ShowGenericInterdictMessage();
                    return false;
                }
                Cell cell = PickDirection("Swap Body");
                if (cell == null) {
                    return false;
                }
                if (cell == ParentObject.CurrentCell) {
                    if (ParentObject.IsPlayer()) {
                        Popup.ShowFail("You may not swap into " + ParentObject.itself + "!");
                    }
                    return false;
                }
                GameObject target = null;
                foreach (GameObject obj in cell.GetObjectsInCell()) {
                    if (obj.HasPart("Combat")) {
                        if (obj.GetPart("MentalMirror") is MentalMirror mirror && mirror.CheckActive()) {
                            mirror.Activate();
                            mirror.ReflectMessage(obj);
                            ParentObject.ApplyEffect(new Confused(Rules.Stat.Roll("3d6"), Level: Level, MentalPenalty: Level));
                        }
                        else {
                            target = obj;
                        }
                        break;
                    }
                }
                if (target == null) {
                    return false;
                }
                if (!target.ApplyEffect(new BodySwapped(OtherBody: ParentObject, Duration: 100, Primary: true))) {
                    return false;
                }
                CooldownMyActivatedAbility(ActivatedAbilityID, GetCooldownTurns(base.Level));
                UseEnergy(1000, "Mental Mutation SwapBody");
            }
            return base.HandleEvent(E);
        }

        public override void Register(GameObject Object)
        {
            Object.RegisterPartEvent(this, COMMAND_NAME);
            base.Register(Object);
        }

        public override bool FireEvent(Event E)
        {
            if (E.ID == COMMAND_NAME)
            {
                if (!ParentObject.IsRealityDistortionUsable()) {
                    RealityStabilized.ShowGenericInterdictMessage();
                    return false;
                }
                Cell cell = PickDirection("Swap Body");
                if (cell == null) {
                    return false;
                }
                if (cell == ParentObject.CurrentCell) {
                    if (ParentObject.IsPlayer()) {
                        Popup.ShowFail("You may not swap into " + ParentObject.itself + "!");
                    }
                    return false;
                }
                GameObject target = null;
                foreach (GameObject obj in cell.GetObjectsInCell()) {
                    if (obj.HasPart("Combat")) {
                        if (obj.GetPart("MentalMirror") is MentalMirror mirror && mirror.CheckActive()) {
                            mirror.Activate();
                            mirror.ReflectMessage(obj);
                            target = ParentObject;
                        }
                        else {
                            target = obj;
                        }
                        break;
                    }
                }
                if (target == null) {
                    return false;
                }
                if (!target.ApplyEffect(new BodySwapped(OtherBody: ParentObject, Duration: 100, Primary: true,
                                                        ApplyMessage: ApplyMessage, RemoveMessage: RemoveMessage))) {
                    return false;
                }
                CooldownMyActivatedAbility(ActivatedAbilityID, GetCooldownTurns(base.Level));
                UseEnergy(1000, "Mental Mutation SwapBody");
            }
            return base.FireEvent(E);
        }

        public override void CollectStats(Templates.StatCollector stats, int Level)
        {
            stats.CollectCooldownTurns(MyActivatedAbility(ActivatedAbilityID), GetCooldownTurns(Level));
        }

        public override bool Mutate(GameObject GO, int Level)
        {
            // MessageQueue.AddPlayerMessage("Adding activated ability Swap Body");
            ActivatedAbilityID = AddMyActivatedAbility(
                Name: "Swap Bodies",
                Command: COMMAND_NAME,
                Class: "Mental Mutation",
                Icon: "*"
            );
            // MessageQueue.AddPlayerMessage("Added activated ability Swap Body " + ActivatedAbilityID.ToString());
            return base.Mutate(GO, Level);
        }

        public override bool Unmutate(GameObject GO)
        {
            RemoveMyActivatedAbility(ref ActivatedAbilityID);
            return base.Unmutate(GO);
        }

        public override bool ChangeLevel(int NewLevel)
        {
            return base.ChangeLevel(NewLevel);
        }
    }
}

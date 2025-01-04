using System;
// using XRL;
// using XRL.World;
using XRL.World.Effects;
using XRL.World.Capabilities;

namespace XRL.World.Parts {

    [Serializable]
    public class IlluminatiSwag_Monsters_Terrifying : IPart
    {
        public override void Register(GameObject Object, IEventRegistrar Registrar)
        {
            Registrar.Register("AfterLookedAt");
            base.Register(Object, Registrar);
        }

        public override bool FireEvent(Event E)
        {
            if (E.ID == "AfterLookedAt")
            {
                GameObject looker = E.GetGameObjectParameter("Looker");
                if (looker != null && !looker.HasEffect("Terrified"))
                {
                    int attackModifier = ParentObject.StatMod("Ego") + ParentObject.GetIntProperty("Persuasion_Intimidate") * 2;
                    Mental.PerformAttack(
                        Handler: Terrified.OfAttacker,
                        Attacker: ParentObject,
                        Defender: looker, 
                        Command: "Terrify Intimidate",
                        Dice: "5d10",
                        Type: 8388610,
                        Magnitude: "2d8".RollCached(),
                        AttackModifier: attackModifier);
                }
            }
            return base.FireEvent(E);
        }
    }
}
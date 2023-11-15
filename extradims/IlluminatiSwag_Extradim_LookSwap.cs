using System;
// using XRL;
// using XRL.World;
using XRL.World.Effects;
// using XRL.World.Capabilities;

namespace XRL.World.Parts {

    [Serializable]
    public class IlluminatiSwag_Extradim_LookSwap : IPart
    {

        public override void Register(GameObject Object)
        {
            Object.RegisterPartEvent(this, "AfterLookedAt");
            base.Register(Object);
        }

        public override bool FireEvent(Event E)
        {
            if (E.ID == "AfterLookedAt")
            {
                GameObject looker = E.GetGameObjectParameter("Looker");
                if (looker != null && looker != ParentObject)
                {
                    looker.ApplyEffect(new BodySwapped(OtherBody: ParentObject, Duration: 30, Primary: true));
                }
            }
            return base.FireEvent(E);
        }
    }
}

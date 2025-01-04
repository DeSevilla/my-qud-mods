using System;
using XRL.Messages;
using XRL.UI;
using XRL.World.AI;
using XRL.World.Parts;
using XRL.World.Parts.Mutation;
using XRL;

namespace XRL.World.Effects {
    public class BodySwapped : Effect
    {
        public GameObject OtherBody;

        public bool Primary;

        public int XPAwarded;
        
        public bool Metempsychosis;

        public bool FromOriginalPlayerBody;

        public string ApplyMessage = "=subject.t's= mind swaps bodies with =object.t=!";

        public string RemoveMessage = "=object.t's= mind returns to =objpronouns.possessive= body.";

        public BodySwapped()
        {
            base.DisplayName = "body-swapped";
        }

        public BodySwapped(GameObject OtherBody, int Duration, bool Primary=false, string ApplyMessage=null, string RemoveMessage=null)
            : this()
        {
            this.OtherBody = OtherBody;
            if (ApplyMessage != null) {
                this.ApplyMessage = ApplyMessage;
            }
            // else {
            //     this.ApplyMessage = "Your mind is sucked into the body of {0}!";
            // }
            if (RemoveMessage != null) {
                this.RemoveMessage = RemoveMessage;
            }

            this.Primary = Primary;
            FromOriginalPlayerBody = OtherBody.IsOriginalPlayerBody();
            base.Duration = Duration;
        }

        public override int GetEffectType()
        {
            return 2;
        }

        public override bool SameAs(Effect e)
        {
            return false;
        }

        public override bool WantEvent(int ID, int cascade)
        {
            return base.WantEvent(ID, cascade)
                || ID == BeforeDeathRemovalEvent.ID
                || ID == AwardedXPEvent.ID;
        }

        public override bool HandleEvent(AwardedXPEvent E)
        {
            XPAwarded += E.Amount;
            return base.HandleEvent(E);
        }

        public override string GetDetails()
        {
            return "Bound within another creature's body.";
        }

        public static void BodySwap(GameObject Source, GameObject Target, string Message = null) {
            Brain targetBrain = Target.pBrain.DeepCopy(Target) as Brain;
            targetBrain.InitFromFactions(); // until deepcopy is fixed we'll do it manually
            // Brain thisBrain = Source.pBrain.DeepCopy(Target) as Brain;
            GameObject newPlayerBody = null;
            if (Target.IsPlayer()) {
                newPlayerBody = Source;
            }
            else if (Source.IsPlayer()) {
                newPlayerBody = Target;
            }
            if (newPlayerBody != null) {
                SoundManager.PlaySound(Clip: "spooky", Effect: SoundRequest.SoundEffectType.FullPanRightToLeft);
                GameManager.Instance.Spin = 2f;
                DeepDream.TransitionIn(Options.ModernUI ? 2f : 0f);
                Popup.Show(GameText.VariableReplace(Message: Message, Subject: Source, Object: Target));
                The.Game.Player.Body = newPlayerBody;
                // if (!Options.ModernUI)
				// {
				// 	DeepDream.ClassicFade();
				// }
                // Popup.Show(String.Format(Message, newPlayerBody.DefiniteArticle() + newPlayerBody.DisplayName));
                // if (!Return && ApplyMessage != null) {
                //     Popup.Show(GameText.VariableReplace(Message: Message, Subject: Source, Object: Target));
                //     // Popup.Show(Source.Poss("mind") + " is transferred to " + Target.Poss("body") + "!");
                // }
                // else if (Return && ReturnMessage != null) {
                //     Popup.Show(GameText.VariableReplace(Message: ApplyMessage, Subject: Source, Object: Target));
                //     // Popup.Show(Source.Poss("mind") + " returns to " + Target.Poss("body") + ".");
                // }
            }
            GiveAffiliations(Target, Source, Source.pBrain);
            GiveAffiliations(Source, Target, targetBrain);
            SwapConversations(Source, Target);
        }

        public static void GiveAffiliations(GameObject Target, GameObject Source, Brain SourceBrain) {
            //separate Brain parameter is to support deepcopy before player transfer in case that messes w things
            bool gotTargetOpinion = SourceBrain.ObjectMemory.TryGetValue(Target, out var opinionOfTarget);
            Target.pBrain.TakeOnAttitudesOf(SourceBrain, CopyLeader: true, CopyTarget: false);
            if (gotTargetOpinion) {
                Target.pBrain.ObjectMemory.Remove(Target);
                Target.pBrain.ObjectMemory.Add(Source, new ObjectOpinion(opinionOfTarget));
            }
        }

        public static void SwapConversations(GameObject Target, GameObject Source) {
            if (Target.GetPart("ConversationScript") is ConversationScript targetConv) {
                if (Source.GetPart("ConversationScript") is ConversationScript sourceConv) {
                    var targetId = targetConv.ConversationID;
                    targetConv.ConversationID = sourceConv.ConversationID;
                    sourceConv.ConversationID = targetId;
                }
                else {
                    Source.RequirePart<ConversationScript>().ConversationID = targetConv.ConversationID;
                    Target.RemovePart<ConversationScript>();
                }
            }
            else if (Source.GetPart("ConversationScript") is ConversationScript sourceConv) {
                Target.RequirePart<ConversationScript>().ConversationID = sourceConv.ConversationID;
                Source.RemovePart<ConversationScript>();
            }
        }

        public override bool Apply(GameObject Object)
        {
            if (Object.HasEffect(typeof(BodySwapped))
                    || Object.HasPart<MentalShield>()
                    || !ApplyEffectEvent.Check(Object, "BodySwapped", this)) {
                return false;
            }
            else if (Primary) {
                if (!OtherBody.ApplyEffect(new BodySwapped(OtherBody: Object, Duration: Effect.DURATION_INDEFINITE, Primary: false))) {
                    return false;
                }
                BodySwap(Source: OtherBody, Target: Object, Message: ApplyMessage);
            }
            return base.Apply(Object);
        }

        public bool IsOurTargetEffect(Effect FX) {
            if (FX is BodySwapped swapped) {
                return swapped.OtherBody == base.Object;
            }
            else {
                return false;
            }
        }

        public override void Remove(GameObject Object) {
            Object.PullDown();
            if (GameObject.Validate(ref OtherBody) && !OtherBody.IsNowhere() && !Metempsychosis) {
                if (Primary) {
                    BodySwap(Source: Object, Target: OtherBody, Message: RemoveMessage);
                    OtherBody.RemoveEffect(IsOurTargetEffect);
                }
                OtherBody.AwardXP(XPAwarded);
            }
            else if (Object.IsPlayer()) {
                Domination.Metempsychosis(Object, FromOriginalPlayerBody);
            }
        }

        public override void Register(GameObject Object)
        {
            Object.RegisterEffectEvent(this, "EndAction");
            base.Register(Object);
        }

        public override void Unregister(GameObject Object)
        {
            Object.UnregisterEffectEvent(this, "EndAction");
            base.Unregister(Object);
        }

        public override bool UseStandardDurationCountdown()
        {
            return true;
        }

        public override bool FireEvent(Event E)
        {
            if (E.ID == "EndAction" && base.Duration <= 0)
            {
                base.Object.RemoveEffect(this);
            }
            return base.FireEvent(E);
        }

        public override bool HandleEvent(BeforeDeathRemovalEvent E)
        {
            BodySwapped effect = OtherBody.GetEffect<BodySwapped>();
            if (effect != null && effect.OtherBody == base.Object)
            {
                Metempsychosis = true;
                effect.Metempsychosis = true;
                if (base.Object.IsPlayer()) {
                    OtherBody.GiveProperName(base.Object.BaseDisplayNameStripped);
                }
                if (Primary) {
                    base.Object.RemoveEffect(this);
                }
                else {
                    OtherBody.RemoveEffect(effect);
                }
                E.RequestInterfaceExit();
            }
            return base.HandleEvent(E);
        }
    }
}
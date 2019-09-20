using System.Globalization;
using Bencodex.Types;
using Libplanet;
using Libplanet.Action;
using LibplanetUnity.Action;
using MinorityGame.Model;
using UniRx;
using BDictionary = Bencodex.Types.Dictionary;

namespace MinorityGame.Action
{
    [ActionType(nameof(GameTurnAction))]
    public class GameTurnAction : ActionBase
    {
        public override IValue PlainValue => BDictionary.Empty;

        public static readonly Subject<ActionEvaluation> RenderSubject =
            new Subject<ActionEvaluation>();

        public override void LoadPlainValue(IValue plainValue)
        {
        }

        public override IAccountStateDelta Execute(IActionContext context)
        {
            var states = context.PreviousStates;
            var bettingPhase = context.BlockIndex % 2 == 0;

            if (!bettingPhase) return states;

            // reward
            IValue serializedBoardState = states.GetState(BoardState.Address);
            if (serializedBoardState != null)
            {
                BoardState boardState = new BoardState(serializedBoardState);
                foreach (BoardState.Item item in boardState.Items)
                {
                    if (item.Bettors.Count == 0)
                        continue;

                    var dividedStake = item.Stake / item.Bettors.Count;
                    foreach (Address bettor in item.Bettors)
                    {
                        decimal balance = states.GetState(bettor) is Text v ? decimal.Parse(v) : 0m;
                        states = states.SetState(bettor,
                            (Text) (balance + dividedStake).ToString(CultureInfo.InvariantCulture));
                    }
                }
            }

            // new turn
            BoardState newBoardState = new BoardState(null, context.Random);
            states = states.SetState(BoardState.Address, newBoardState.Serialize());

            return states;
        }

        public override void Render(IActionContext context, IAccountStateDelta nextStates)
        {
            RenderSubject.OnNext(new ActionEvaluation(this, context, nextStates));
        }

        public override void Unrender(IActionContext context, IAccountStateDelta nextStates)
        {
        }
    }
}

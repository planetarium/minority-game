using System;
using Bencodex.Types;
using Libplanet.Action;
using LibplanetUnity.Action;
using MinorityGame.Model;

namespace MinorityGame.Action
{
    [ActionType(nameof(BetAction))]
    public class BetAction : ActionBase
    {
        public long BlockIndex { get; private set; }
        public int BetIndex { get; private set; }

        public override IValue PlainValue =>
            Bencodex.Types.Dictionary.Empty
                .Add("block_index", BlockIndex)
                .Add("bet_index", BetIndex);

        public BetAction()
            : this(0, 0)
        {
        }

        public BetAction(long blockIndex, int betIndex)
        {
            BlockIndex = blockIndex;
            BetIndex = betIndex;
        }

        public override void LoadPlainValue(IValue plainValue)
        {
            if (plainValue is Bencodex.Types.Dictionary dict)
            {
                BlockIndex = (Bencodex.Types.Integer) dict["block_index"];
                BetIndex = (Bencodex.Types.Integer) dict["bet_index"];
            }
            else
            {
                throw new ArgumentException("Expected a Bencodex dictionary.", nameof(plainValue));
            }
        }

        public override IAccountStateDelta Execute(IActionContext context)
        {
            var states = context.PreviousStates;
            if (context.Rehearsal)
                return states.SetState(BoardState.Address, ActionBase.MarkChanged);

            if (context.BlockIndex > BlockIndex + 2) // ?
                return states;

            var serializedBoardState = states.GetState(BoardState.Address);
            var boardState = serializedBoardState is null ? new BoardState() : new BoardState(serializedBoardState);
            var newBoardState = boardState.Bet(BetIndex, context.Signer);
            return states.SetState(BoardState.Address, newBoardState.Serialize());
        }

        public override void Render(IActionContext context, IAccountStateDelta nextStates)
        {
        }

        public override void Unrender(IActionContext context, IAccountStateDelta nextStates)
        {
        }
    }
}

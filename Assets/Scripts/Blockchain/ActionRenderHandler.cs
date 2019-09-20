using System;
using System.Collections.Generic;
using Bencodex.Types;
using Libplanet;
using Libplanet.Action;
using MinorityGame.Action;
using MinorityGame.Extensions;
using MinorityGame.Model;
using UniRx;
using UnityEngine;

namespace Nekoyume.BlockChain
{
    public class ActionRenderHandler
    {
        private readonly Address _playerAddress;

        private readonly List<IDisposable> _disposables = new List<IDisposable>();

        public static readonly Subject<(BoardState, GamePhase)> BoardStateSubject =
            new Subject<(BoardState, GamePhase)>();

        public static readonly Subject<long> BlockIndexSubject = new Subject<long>();
        public static readonly Subject<decimal> BalanceSubject = new Subject<decimal>();

        public ActionRenderHandler(Address playerAddress)
        {
            _playerAddress = playerAddress;
        }

        public void Start()
        {
            HandleGameTurnAction();
        }

        public void Stop()
        {
            _disposables.DisposeAllAndClear();
        }

        private void UpdateBoardState(ActionEvaluation evaluation)
        {
            UpdateBoardState(evaluation.OutputStates.GetState, evaluation.InputContext.BlockIndex);
        }

        public void UpdateBoardState(AccountStateGetter getState, long blockIndex)
        {
            GamePhase gamePhase = blockIndex % 2 == 0 ? GamePhase.Bet : GamePhase.Close;
            BoardStateSubject.OnNext((new BoardState(getState(BoardState.Address)), gamePhase));
            BlockIndexSubject.OnNext(blockIndex);
            var state = getState(_playerAddress);
            Debug.LogFormat("State at {0} (block #{1}): {2}", _playerAddress.ToString(), blockIndex,
                state is null ? "(NULL)" : state.ToString());
            var balance = state is Text v ? decimal.Parse(v) : 0m;
            BalanceSubject.OnNext(balance);
        }

        private void HandleGameTurnAction()
        {
            GameTurnAction.RenderSubject
                .ObserveOnMainThread()
                .Subscribe(UpdateBoardState).AddTo(_disposables);
        }
    }
}

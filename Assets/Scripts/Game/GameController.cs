using System;
using System.Collections.Generic;
using Blockchain;
using LibplanetUnity;
using MinorityGame.Action;
using MinorityGame.Extensions;
using MinorityGame.UI;
using Nekoyume.BlockChain;
using UniRx;

namespace MinorityGame.Game
{
    public class GameController
    {
        private readonly GamePanel _gamePanel;
        private readonly List<IDisposable> _disposables;

        public GameController(GamePanel gamePanel)
        {
            _gamePanel = gamePanel;
            _disposables = new List<IDisposable>();

            var i = 0;
            foreach (var item in _gamePanel.itemManager.items)
            {
                item.itemIndex = i++;
                item.onClick.Subscribe(OnClickItem).AddTo(_disposables);
            }

            ActionRenderHandler.BoardStateSubject.Subscribe(pair =>
                {
                    _gamePanel.SetGamePhase(pair.Item2);
                    _gamePanel.DrawBoard(pair.Item1);
                })
                .AddTo(_disposables);
            ActionRenderHandler.BlockIndexSubject.Subscribe(value =>
                {
                    _gamePanel.DrawBlockIndex(value);
                    _gamePanel.DrawRound(value / 2 + 1);
                })
                .AddTo(_disposables);
            ActionRenderHandler.BalanceSubject.Subscribe(_gamePanel.DrawBalance)
                .AddTo(_disposables);
        }

        ~GameController()
        {
            _disposables.DisposeAllAndClear();
        }

        private void OnClickItem(ItemController item)
        {
            var blockIndex = AgentHelper.InnerAgent.BlockIndex;
            var bet = new BetAction(blockIndex - (blockIndex % 2), item.itemIndex);
            Agent.instance.MakeTransaction(new BetAction[] {bet});
            _gamePanel.SelectedIndex = item.itemIndex;
        }
    }
}

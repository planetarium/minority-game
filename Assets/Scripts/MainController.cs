using System.Collections;
using Blockchain;
using Libplanet.Net;
using LibplanetUnity;
using MinorityGame.Game;
using Nekoyume.BlockChain;
using Serilog;
using UnityEngine;

namespace MinorityGame
{
    public class MainController : Pattern.MonoSingleton<MainController>
    {
        public PanelManager panelManager;
        public GameController gameController;
        public ActionRenderHandler actionRenderHandler;

        protected override void Awake()
        {
            base.Awake();
            StartCoroutine(CoInitialize());
        }

        protected override void OnDestroy()
        {
            actionRenderHandler.Stop();
            base.OnDestroy();
        }

        private IEnumerator CoInitialize()
        {
//#if !UNITY_EDITOR
            yield return new WaitForSeconds(2f);
//#endif

            panelManager.introPanel.Hide();
            var agentInitialized = false;
            InitializeLogger();
            Agent.Initialize();
            var innerAgent = AgentHelper.InnerAgent;
            innerAgent.PreloadProcessed += (sender, state) =>
            {
                switch (state)
                {
                    case BlockDownloadState s:
                        panelManager.loadingPanel.progressCircle.ProgressValue =
                            (float) s.ReceivedBlockCount / s.TotalBlockCount * .5f;
                        break;
                }
            };
            innerAgent.PreloadEnded += (sender, args) =>
            {
                Debug.Log($"{nameof(MainController)}: {nameof(innerAgent)}.{nameof(innerAgent.PreloadEnded)} called.");
                panelManager.loadingPanel.progressCircle.ProgressValue = .5f;
                agentInitialized = true;
            };
            yield return new WaitUntil(() => agentInitialized);
            yield return new WaitUntil(() => innerAgent.BlockIndex > 0);
            while (panelManager.loadingPanel.progressCircle.ProgressValue < 1f)
            {
                panelManager.loadingPanel.progressCircle.ProgressValue += Time.deltaTime * .2f;
                yield return null;
            }

            gameController = new GameController(panelManager.gamePanel);
            actionRenderHandler = new ActionRenderHandler(innerAgent.Address);
            actionRenderHandler.UpdateBoardState(innerAgent.GetState, innerAgent.BlockIndex);
            actionRenderHandler.Start();

            panelManager.loadingPanel.Hide();
        }


        private static void InitializeLogger()
        {
            var loggerConfiguration = new LoggerConfiguration()
                .MinimumLevel.Verbose();

            loggerConfiguration = loggerConfiguration
                .WriteTo.Sink(new UnityDebugSink());

            Log.Logger = loggerConfiguration.CreateLogger();
        }
    }
}

using MinorityGame.UI;
using UnityEngine;

namespace MinorityGame
{
    public class PanelManager : MonoBehaviour
    {
        public IntroPanel introPanel;
        public LoadingPanel loadingPanel;
        public GamePanel gamePanel;

        private void Reset()
        {
            introPanel = GetComponentInChildren<IntroPanel>(true);
            loadingPanel = GetComponentInChildren<LoadingPanel>(true);
            gamePanel = GetComponentInChildren<GamePanel>(true);
        }
    }
}

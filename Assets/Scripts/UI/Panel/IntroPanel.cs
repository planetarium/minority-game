using UnityEngine;

namespace MinorityGame.UI
{
    public class IntroPanel : MonoBehaviour
    {
        public CanvasGroup canvasGroup;

        private void Reset()
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}

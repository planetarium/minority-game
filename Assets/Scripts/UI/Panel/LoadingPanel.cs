using TMPro;
using UnityEngine;

namespace MinorityGame.UI
{
    public class LoadingPanel : MonoBehaviour
    {
        public ProgressCircle progressCircle;
        public TextMeshProUGUI messageText;

        private void Reset()
        {
            progressCircle = GetComponentInChildren<ProgressCircle>();
            messageText = transform.Find("MessageText").GetComponent<TextMeshProUGUI>();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}

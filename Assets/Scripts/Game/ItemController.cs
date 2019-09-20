using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace MinorityGame.Game
{
    public class ItemController : MonoBehaviour
    {
        public Image iconImage;
        public TextMeshProUGUI countText;
        public Button button;

        public int itemIndex;

        public readonly Subject<ItemController> onClick = new Subject<ItemController>();

        private void Awake()
        {
            button.OnClickAsObservable()
                .Subscribe(_ => onClick.OnNext(this))
                .AddTo(gameObject);
        }

        private void Reset()
        {
            iconImage = transform.Find("IconImage").GetComponent<Image>();
            countText = transform.Find("CountText").GetComponent<TextMeshProUGUI>();
            button = GetComponent<Button>();
        }
    }
}

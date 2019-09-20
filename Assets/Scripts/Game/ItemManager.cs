using System.Collections.Generic;
using System.Globalization;
using MinorityGame.Model;
using UnityEngine;

namespace MinorityGame.Game
{
    public class ItemManager : MonoBehaviour
    {
        public CanvasGroup canvasGroup;
        public List<ItemController> items;

        private void Reset()
        {
            items = new List<ItemController>(GetComponentsInChildren<ItemController>(true));
        }

        public void DrawBoard(BoardState boardState)
        {
            if (boardState is null)
            {
                // Is it okay?
                return;
            }

            int i = 0;
            foreach (var item in boardState.Items)
            {
                items[i].countText.text = item.Stake.ToString(CultureInfo.InvariantCulture);
                i++;
            }
        }
    }
}

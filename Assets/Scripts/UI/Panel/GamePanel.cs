using MinorityGame.Game;
using MinorityGame.Model;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MinorityGame.UI
{
    public class GamePanel : MonoBehaviour
    {
        public ItemManager itemManager;
        public Image selectionImage;
        public TextMeshProUGUI blockIndexText;
        public TextMeshProUGUI roundText;
        public TextMeshProUGUI balanceText;

        private int? _selectedIndex;

        public int? SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                _selectedIndex = value;
                SelectItem(value);
            }
        }

        private void Reset()
        {
            itemManager = GetComponentInChildren<ItemManager>(true);
        }

        public void SetGamePhase(GamePhase value)
        {
            if (value == GamePhase.Bet)
            {
                SelectedIndex = null;
            }
        }

        public void DrawBoard(BoardState value)
        {
            itemManager.DrawBoard(value);
        }

        public void DrawBlockIndex(long value)
        {
            blockIndexText.text = $"BlockIndex: {value}";
        }

        public void DrawRound(long value)
        {
            roundText.text = $"Round: {value}";
        }

        public void DrawBalance(decimal value)
        {
            balanceText.text = $"Balance: {value}";
        }

        private void SelectItem(int? selectedIndex)
        {
            if (selectedIndex is int idx)
            {
                selectionImage.transform.position = itemManager.items[idx].transform.position;
                selectionImage.enabled = true;
                itemManager.canvasGroup.interactable = false;
            }
            else
            {
                selectionImage.enabled = false;
                itemManager.canvasGroup.interactable = true;
            }
        }
    }
}

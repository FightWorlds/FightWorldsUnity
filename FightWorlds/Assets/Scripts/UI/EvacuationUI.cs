using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace FightWorlds.UI
{
    public class EvacuationUI : MonoBehaviour
    {
        [SerializeField] private Image baseHpBar;
        [SerializeField] private Button evacuationButton;
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private TextMeshProUGUI buttonText;
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private Sprite[] hpBarSprites;
        [SerializeField] private Color textColor;
        [SerializeField] private GameObject activeStatus;
        [SerializeField] private GameObject activeTimer;
        [SerializeField] private GameObject activeButton;

        private Dictionary<EvacuationState, string> statesStatus;
        private Dictionary<EvacuationState, string> statesButton;

        public void SwitchButtonState(EvacuationState state, UnityAction action)
        {
            evacuationButton.onClick.RemoveAllListeners();
            if (action != null)
            {
                evacuationButton.onClick.AddListener(action);
                SwitchOn(false);
            }
            else
                SwitchOn(true);
            activeStatus.SetActive(true);
            statusText.color = textColor;
            statusText.text = statesStatus[state];
            buttonText.text = statesButton[state];
        }

        public void AddListenerOnCall(UnityAction act)
        {
            evacuationButton.onClick.RemoveAllListeners();
            evacuationButton.onClick.AddListener(act);
        }

        public void ChangeTimeText(float time)
        {
            int sec = Mathf.FloorToInt(time);
            float mil = time - sec;
            string sc = sec < 10 ? "0" + sec.ToString() : sec.ToString();
            string ml = mil == 0f ? "00" : mil.ToString()[2..4];
            timerText.text = $"00:{sc}:{ml}";
        }

        public void UpdateBaseHpBar(float value, int spriteIndex)
        {
            baseHpBar.sprite = hpBarSprites[spriteIndex];
            baseHpBar.fillAmount = value;
        }

        private void Awake()
        {
            statesStatus = new(){
                {EvacuationState.None, "WARNING"},
                {EvacuationState.Warn, "WARNING"},
                {EvacuationState.Land, "ARRIVING IN"},
                {EvacuationState.Load, "LOADING"},
                {EvacuationState.Evacuate, "EVACUATE IN"},
            };
            statesButton = new(){
                {EvacuationState.None, "EVACUATING"},
                {EvacuationState.Warn, "CALL\nEVACUATION"},
                {EvacuationState.Land, ""},
                {EvacuationState.Load, "TAKE OFF\nTO SAVE"},
                {EvacuationState.Evacuate, ""},
            };
        }

        private void SwitchOn(bool isTimer)
        {
            activeTimer.SetActive(isTimer);
            activeButton.SetActive(!isTimer);
            timerText.gameObject.SetActive(isTimer);
        }
    }
}
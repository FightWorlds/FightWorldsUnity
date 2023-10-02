using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace FightWorlds.UI
{
    public class EvacuationUI : MonoBehaviour
    {
        [SerializeField] private Slider baseHpSlider;
        [SerializeField] private Button callButton;
        [SerializeField] private Button evacuationButton;
        [SerializeField] private Text timerText;
        [SerializeField] private Button restartButton;


        public void AddListenerOnRestart(UnityAction act) =>
            restartButton.onClick.AddListener(act);

        public void AddListenerOnCall(UnityAction act) =>
            callButton.onClick.AddListener(act);

        public void AddListenerOnUp(UnityAction act) =>
            evacuationButton.onClick.AddListener(act);

        public void SwitchCallButtonState(bool value) =>
            callButton.gameObject.SetActive(value);

        public void SwitchEvacuationButtonState(bool value) =>
            evacuationButton.gameObject.SetActive(value);

        public void SwitchEvacuationTimerState(bool value) =>
            timerText.transform.parent.gameObject.SetActive(value);

        public void ChangeTimeText(float time)
        {
            int sec = Mathf.FloorToInt(time);
            float mil = time - sec;
            string sc = sec < 10 ? "0" + sec.ToString() : sec.ToString();
            string ml = mil == 0f ? "00" : mil.ToString()[2..4];
            timerText.text = $"00:{sc}:{ml}";
        }

        public void UpdateBaseHpBar(float value) => baseHpSlider.value = value;

    }
}
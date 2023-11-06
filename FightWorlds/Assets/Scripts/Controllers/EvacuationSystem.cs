using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using FightWorlds.Placement;

namespace FightWorlds.Controllers
{
    public class EvacuationSystem : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private int loadAmount;
        [SerializeField] private float evacuateOperationTime;
        [SerializeField] private float landingTime;
        [SerializeField] private GameObject particles;
        public PlacementSystem placement;
        public bool IsGameFinished;

        private bool isFlying;
        private bool isShuttleCalled;
        private int collectedArtifacts;
        private float leftTime;

        public void FinishGame()
        {
            if (IsGameFinished) return;
            IsGameFinished = true;
            StopAllCoroutines();
            if (isShuttleCalled && !isFlying)
            {
                StartCoroutine(ShuttleEvacuating());
                return;
            };
            StopGame();
        }

        private void Start()
        {
            placement.ui.SwitchButtonState(UI.EvacuationState.Warn, () =>
            {
                if (!isShuttleCalled)
                    StartCoroutine(EvacuatingPipeline());
            });
        }

        private void Update()
        {
            if (isFlying)
            {
                placement.ui.ChangeTimeText(leftTime);
                leftTime -= Time.deltaTime;
            }
        }

        private IEnumerator EvacuatingPipeline()
        {
            yield return ShuttleLanding();
            yield return CollectArtifacts();
            yield return ShuttleEvacuating();
        }

        private IEnumerator CollectArtifacts()
        {
            int artifactsPerOperation;
            particles.SetActive(true);
            while (!IsGameFinished)
            {
                artifactsPerOperation =
                (int)(loadAmount * (1 - placement.HpPercent));
                if (!placement.player.UseResources(artifactsPerOperation,
                ResourceType.Artifacts, false))
                    break;
                collectedArtifacts += artifactsPerOperation;
                yield return new WaitForSeconds(evacuateOperationTime);
            }
        }

        private IEnumerator ShuttleLanding()
        {
            isShuttleCalled = true;
            placement.ui.SwitchButtonState(UI.EvacuationState.Land, null);
            animator.SetBool("Landing", true);
            yield return FlyShuttle();
            animator.SetBool("Landing", false);
            placement.ui.SwitchButtonState(UI.EvacuationState.Load, FinishGame);
        }

        private IEnumerator ShuttleEvacuating()
        {
            particles.SetActive(false);
            animator.SetBool("Evacuating", true);
            placement.ui.SwitchButtonState(UI.EvacuationState.Evacuate, null);
            yield return FlyShuttle();
            StopGame();
        }

        private IEnumerator FlyShuttle()
        {
            leftTime = landingTime;
            isFlying = true;
            yield return new WaitForSeconds(landingTime);
            isFlying = false;
        }

        private void StopGame()
        {
            placement.ui.SwitchButtonState(UI.EvacuationState.None, () => { });
            placement.ui.SetDefaultLayout();
            placement.ui.FinishGamePopUp(collectedArtifacts, RestartGame);
            placement.player.SavePlayerResult(collectedArtifacts);
            Time.timeScale = 0f;
        }

        private void RestartGame()
        {
            placement.soundFeedback.PlaySound(Audio.SoundType.SceneRestart);
            SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name);
            Time.timeScale = 1f;
        }
    }
}
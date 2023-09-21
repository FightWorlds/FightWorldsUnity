using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EvacuationSystem : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private int loadAmount;
    [SerializeField] private float evacuateOperationTime;
    [SerializeField] private float landingTime;
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
        if (isShuttleCalled) return;
        StopGame();
    }

    private void Start()
    {
        placement.ui.SwitchCallButtonState(true);
        placement.ui.AddListenerOnCall(() =>
        {
            if (!isShuttleCalled)
                StartCoroutine(EvacuatingPipeline());
        });
        placement.ui.AddListenerOnUp(FinishGame);
    }

    private void Update()
    {
        if (isFlying)
        {
            leftTime -= Time.deltaTime;
            placement.ui.ChangeTimeText(leftTime);
        }
    }

    private IEnumerator EvacuatingPipeline()
    {
        yield return ShuttleLanding();
        yield return CollectArtifacts();
        yield return ShuttleEvacuating();
        StopGame();
    }

    private IEnumerator CollectArtifacts()
    {
        int artifactsPerOperation;
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
        placement.ui.SwitchCallButtonState(false);
        animator.SetBool("Landing", true);
        yield return FlyShuttle();
        animator.SetBool("Landing", false);
        placement.ui.SwitchEvacuationButtonState(true);
    }

    private IEnumerator ShuttleEvacuating()
    {
        animator.SetBool("Evacuating", true);
        placement.ui.SwitchEvacuationButtonState(false);
        yield return FlyShuttle();
    }

    private IEnumerator FlyShuttle()
    {
        leftTime = landingTime;
        isFlying = true;
        placement.ui.SwitchEvacuationTimerState(isFlying);
        yield return new WaitForSeconds(landingTime);
        isFlying = false;
        placement.ui.SwitchEvacuationTimerState(isFlying);
    }

    private void StopGame()
    {
        placement.ui.FinishGamePopUp(collectedArtifacts);
        placement.player.SavePlayerResult(collectedArtifacts);
        Time.timeScale = 0f;
        placement.ui.AddListenerOnRestart(RestartGame);
    }

    private void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        Time.timeScale = 1f;
    }
}

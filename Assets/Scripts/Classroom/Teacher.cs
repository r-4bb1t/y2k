using System.Collections;
using UnityEngine;

public class Teacher : MonoBehaviour
{
    public bool isWatching = false;

    private Animator animator;

    [SerializeField] private float minSleepTime = 2f;
    [SerializeField] private float maxSleepTime = 5f;
    [SerializeField] private float warningTime = 1f;
    [SerializeField] private float minWatchTime = 0.5f;
    [SerializeField] private float maxWatchTime = 3f;

    private void Start()
    {
        animator = GetComponentInChildren<Animator>();
        StartCoroutine(StateLoop());
    }

    private IEnumerator StateLoop()
    {
        while (true)
        {
            animator.SetInteger("status", 0);
            isWatching = false;
            float sleepTime = Random.Range(minSleepTime, maxSleepTime);
            yield return new WaitForSeconds(sleepTime);

            animator.SetInteger("status", 1);
            yield return new WaitForSeconds(warningTime);

            animator.SetInteger("status", 2);
            isWatching = true;
            float watchTime = Random.Range(minWatchTime, maxWatchTime);
            yield return new WaitForSeconds(watchTime);
        }
    }
}

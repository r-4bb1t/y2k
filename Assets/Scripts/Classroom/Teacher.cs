using System.Collections;
using UnityEngine;

public class Teacher : MonoBehaviour
{
    public bool isWatching = false;
    private Player player;


    private Animator animator;

    [SerializeField] private float minSleepTime = 2f;
    [SerializeField] private float maxSleepTime = 5f;
    [SerializeField] private float warningTime = 1f;
    [SerializeField] private float minWatchTime = 0.5f;
    [SerializeField] private float maxWatchTime = 3f;

    private void Start()
    {
        animator = GetComponentInChildren<Animator>();
        player = FindObjectOfType<Player>();
        StartCoroutine(StateLoop());
    }

    private IEnumerator StateLoop()
    {
        while (true)
        {
            animator.SetInteger("status", 0);
            isWatching = false;
            yield return new WaitForSeconds(Random.Range(minSleepTime, maxSleepTime));

            if (player != null && player.isGameOver)
                yield break;

            animator.SetInteger("status", 1);
            yield return new WaitForSeconds(warningTime);

            if (player != null && player.isGameOver)
                yield break;

            animator.SetInteger("status", 2);
            isWatching = true;
            yield return new WaitForSeconds(Random.Range(minWatchTime, maxWatchTime));
        }
    }


}

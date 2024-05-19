using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class onHoverEvents : MonoBehaviour
{

    IEnumerator getBiggerCoroutine;
    IEnumerator getSmallerCoroutine;
    public void OnHoverEnter()
    {
        getBiggerCoroutine = changeScale(transform, new Vector3(1.25f, 1.25f, 1.25f), true);
        if (getSmallerCoroutine != null)
        {
            StopCoroutine(getSmallerCoroutine);
            getSmallerCoroutine = null;
        }
        StartCoroutine(getBiggerCoroutine);
    }

    public void OnHoverExit()
    {
        getSmallerCoroutine = changeScale(transform, new Vector3(1f, 1f, 1f), false);
        if (getBiggerCoroutine != null)
        {
            StopCoroutine(getBiggerCoroutine);
            getBiggerCoroutine = null;
        }
        StartCoroutine(getSmallerCoroutine);
    }

    // function to change localScale of transform over time
    IEnumerator changeScale(Transform transform, Vector3 targetScale, bool isGettingBigger)
    {
        Vector3 startScale = transform.localScale;
        float time = 0;
        float duration = .2f;
        while (time < duration)
        {
            time += Time.deltaTime;
            transform.localScale = Vector3.Lerp(startScale, targetScale, time / duration);
            yield return null;
        }
        transform.localScale = targetScale;
        if (isGettingBigger)
        {
            getBiggerCoroutine = null;
        }
        else
        {
            getSmallerCoroutine = null;
        }
    }
}

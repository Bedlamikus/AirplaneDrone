using System.Collections;
using UnityEngine;

public class RotateTransform : MonoBehaviour
{
    [SerializeField] private float speed = 30f;
    [SerializeField] private Vector3 LocalAxis = Vector3.forward;

    private void Start()
    {
        StartCoroutine(Rotate());
    }

    private IEnumerator Rotate()
    {
        while (true)
        {
            float needTime = 360 / speed;
            float timer = 0f;

            while (timer < needTime)
            {
                timer += Time.deltaTime;
                transform.localEulerAngles = Vector3.Lerp(
                    Vector3.zero, LocalAxis * 360f,
                    timer / needTime);

                yield return null;
            }
            yield return null;
        }
    }
}

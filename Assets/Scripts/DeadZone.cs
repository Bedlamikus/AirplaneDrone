using UnityEngine;

public class DeadZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        var airplane = other.GetComponent<AirplaneController>();
        if (airplane == null) return;

        airplane.Die();
    }
}

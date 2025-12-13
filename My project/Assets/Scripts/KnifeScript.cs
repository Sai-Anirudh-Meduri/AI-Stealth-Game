using UnityEngine;

public class KnifeScript : MonoBehaviour
{
    [SerializeField, Range(0f, 180f)]
    private float backstabAngle = 60f; // cone behind the enemy that counts as backstab

    private void OnTriggerEnter(Collider other)
    {
        EnemyStateMachineController enemy = other.GetComponent<EnemyStateMachineController>();

        Debug.Log($"Hit enemy: {enemy}");

        if (enemy != null)
        {
            // Direction from enemy to knife
            Vector3 toKnife = (transform.position - enemy.Trans.position).normalized;

            // Enemy forward direction
            Vector3 enemyForward = enemy.Trans.forward;

            // Angle between enemy forward and the direction to knife
            float angle = Vector3.Angle(enemyForward, toKnife);

            if (angle > 180f - backstabAngle / 2f)
            {
                // We are behind the enemy
                enemy.Damage(10, true);
            }
            else
            {
                enemy.Damage(10, false);
            }
        }
    }
}

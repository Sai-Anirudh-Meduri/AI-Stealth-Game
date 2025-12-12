using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnerController : MonoBehaviour
{
    [SerializeField] List<Vector3> _spawnPoints; //A list of spots to spawn enemies. Assign in editor. Number of points == number of enemies to spawn.
    public int activated = 0; //How many enemies has this spawner spawned recently?
    [SerializeField] GameObject enemyPrefab; //Holds information about enemies to spawn.

    void Awake()
    {
        GetComponent<Renderer>().enabled = false; //Make invisible
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        foreach (var p in _spawnPoints)
        {
            Gizmos.DrawSphere(p, 5.0f);
        }
    }

    public void spawnEnemies(Vector3 playerPos)
    {
        if (activated == 0 && _spawnPoints.Count > 0)
        {
            activated = _spawnPoints.Count;

            /*bool leaderSpawn = true;
            GameObject leader = EnemyStateMachineController.SpawnEnemy(enemyPrefab, _spawnPoints[0], Quaternion.identity).gameObject;
            leader.GetComponent<EnemyStateMachineController>().InitializeState(EnemyStateType.SoloPursuit);*/

            foreach (Vector3 p in _spawnPoints)
            {
                //Skip the first point, then spawn formation enemies for every other point
                /*if(leaderSpawn)
                {
                    leaderSpawn = false;
                } else
                {
                    EnemyStateMachineController enemy = EnemyStateMachineController.SpawnEnemy(enemyPrefab, p, Quaternion.identity);
                    enemy.InitializeFormationState(leader.transform, enemy.transform.position - leader.transform.position);
                }*/
                EnemyStateMachineController enemy = EnemyStateMachineController.SpawnEnemy(enemyPrefab, p, Quaternion.identity);
                enemy.InitializeSpawnerState(p, this);
            }
        }
    }
}

using System;
using UnityEngine;

public class DoorController : MonoBehaviour
{
    [SerializeField] private bool _open = false;
    [SerializeField] private float _openSpeed = 1.0f;
    [SerializeField] private int _doorID = 1;

    // Update is called once per frame
    void Update()
    {
        if (_open)
        {
            //Door is nw open, "animate" it moving down
            transform.position += Vector3.down * _openSpeed * Time.deltaTime;

            if(transform.position.y < -100f)
            {
                //Doors never close, so instead of moving this down forever, destroy it eventually
                Destroy(gameObject);
            }
        }
    }
    void OnEnable()
    {
        KeyController.OnKeyPickup += OpenDoor;
    }

    void OnDisable()
    {
        KeyController.OnKeyPickup -= OpenDoor;
    }

    private void OpenDoor(int id)
    {
        if (id == _doorID) { _open = true; }
    }
}

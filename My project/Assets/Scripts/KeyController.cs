using System.Collections;
using UnityEngine;

public class KeyController : MonoBehaviour
{
    public delegate void KeyPickup(int id);
    public static event KeyPickup OnKeyPickup;

    [SerializeField] private int _keyID;
    private AudioSource _audioPlayer;

    private void Start()
    {
        _audioPlayer = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log($"Key {_keyID} triggered by {other.name}");
            StartCoroutine(PickupRoutine());
        }
    }

    private IEnumerator PickupRoutine()
    {
        _audioPlayer.Play();

        float duration = _audioPlayer.clip.length;

        // Hide key immediately
        GetComponent<MeshRenderer>().enabled = false;
        GetComponent<Collider>().enabled = false;

        yield return new WaitForSeconds(duration);

        // Now notify doors
        OnKeyPickup?.Invoke(_keyID);

        Destroy(gameObject);
    }
}

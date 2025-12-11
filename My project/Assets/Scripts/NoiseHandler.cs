using System;
using UnityEngine;

//Defines events to be used to create noises. Mostly used staticly.
public class NoiseHandler : MonoBehaviour
{
    //Identifiers for the source of a noise
    public enum NoiseID
    {
        Laser,
        Clap,
        Run,
        Walk,
        Landing
    }

    //Event construction for subscribing to listen for noises directly
    public delegate void Noise(NoiseID id, Transform origin, double range); 
    public static event Noise OnNoise;

    //Function that sends events for noises. Should be called to make event listeners react to a noise.
    public static void InvokeNoise(NoiseID id, Transform origin, double range)
    {
        if (OnNoise != null)
        {
            OnNoise?.Invoke(id, origin, range);
        }
    }
}

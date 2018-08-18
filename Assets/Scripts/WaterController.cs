using UnityEngine;

public class WaterController : MonoBehaviour
{
    public bool isMoving;

    public float scale = 0.1f;
    public float speed = 1.0f;
    public float waveDistance = 1f;
    public float noiseStrength = 1f;
    public float noiseWalk = 1f;

    public static WaterController Instance { get; private set; }

    private void Start()
    {
        if (Instance == null)
        {
            Instance = this; 
        }
        else
        {
            Debug.LogWarning("Duplicate WaterController found, deleting this one.");
            Destroy(this);
        }
    }

    public float GetWaveYPos(Vector3 position, float timeSinceStart)
    {
        return 0f;
    }

    public float DistanceToWater(Vector3 position, float timeSinceStart)
    {
        float waterHeight = GetWaveYPos(position, timeSinceStart);

        return position.y - waterHeight;
    }
}
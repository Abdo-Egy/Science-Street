using UnityEngine;
using DG.Tweening;
using Unity.VisualScripting;

public class FloatingRotating : MonoBehaviour
{
    [Header("Floating Settings")]
    public float floatStrength = 0.5f;  // How high it moves up/down
    public float floatDuration = 1.5f;  // Time to move up/down

    [Header("Rotation Settings")]
    public Vector3 rotationSpeed = new Vector3(0, 90, 0); // Degrees per second

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;

        // Floating (Yoyo up/down loop)
        transform.DOMoveY(startPos.y + floatStrength, floatDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);

        // Rotation (infinite spin)
        transform.DORotate(rotationSpeed, 1f, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Incremental);
    }

}
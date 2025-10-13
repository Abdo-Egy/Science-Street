using UnityEngine;
using DG.Tweening;

public class ItemPosition : MonoBehaviour
{
    [SerializeField] GameObject HomeHighlights;
    [SerializeField] string EndPositionAreaTag;
    [SerializeField] float AnimationDuration = 5;
    [Header("End Transform")]
    [SerializeField] Vector3 EndPosition;
    [SerializeField] Quaternion EndRotation;
    [SerializeField] Vector3 EndScale;
    [Header("Start Transform")]
    Vector3 StartPosition;
    Quaternion StartRotation;
    Vector3 StartScale;
    [HideInInspector] public bool IsDragable;

    public bool inHome;
    private void Start()
    {
        StartPosition = transform.position;
        StartRotation = transform.rotation;
        StartScale = transform.localScale;

        ExperimentManager.Instance.Items.Add(this);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(EndPositionAreaTag))
        {
            inHome = true;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(EndPositionAreaTag))
        {
            inHome = false;
        }
    }
    private void Update()
    {
        if (inHome)
        {
            if (!IsDragable)
            {
                transform.position = Vector3.Lerp(transform.position, EndPosition, AnimationDuration * Time.deltaTime);
                transform.localScale = Vector3.Lerp(transform.localScale, EndScale, AnimationDuration * Time.deltaTime);
                transform.rotation = Quaternion.Lerp(transform.rotation, EndRotation, AnimationDuration * Time.deltaTime);
            }
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, StartPosition, AnimationDuration * Time.deltaTime);
            transform.localScale = Vector3.Lerp(transform.localScale, StartScale, AnimationDuration * Time.deltaTime);
            transform.rotation = Quaternion.Lerp(transform.rotation, StartRotation, AnimationDuration * Time.deltaTime);
        }
    }
    public void ActiveFade(bool status)
    {
        HomeHighlights.SetActive(status);

    }
}

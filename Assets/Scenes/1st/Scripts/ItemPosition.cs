using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;
using System.Collections;
using System;

public class ItemPosition : MonoBehaviour
{
    [SerializeField] UnityEvent EventEnter;
    [SerializeField] UnityEvent EventExit;
    [SerializeField] GameObject HomeHighlights;
    [SerializeField] AreaEnum EndPositionAreaName;
    [SerializeField] float AnimationDuration = 5;
    [Header("End Transform")]
    [SerializeField] Vector3 EndPosition;
    [SerializeField] Quaternion EndRotation;
    [SerializeField] Vector3 EndScale;
    [Header("Start Transform")]
    Vector3 StartPosition;
    Quaternion StartRotation;
    Vector3 StartScale;
    [SerializeField] public bool IsDragable;

    public bool inHome;
    bool startAction;


    [SerializeField] bool inHomePosition;

    private void Start()
    {
        inHome = false;
        StartPosition = transform.position;
        StartRotation = transform.rotation;
        StartScale = transform.localScale;

        ExperimentManager.Instance.Items.Add(this);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<HomeName>(out HomeName HomeHighlights))
        {
            if (EndPositionAreaName == HomeHighlights.AreaName)
            {
                inHome = true;
                EventEnter?.Invoke();
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<HomeName>(out HomeName HomeHighlights))
        {
            if (EndPositionAreaName == HomeHighlights.AreaName)
            {
                if (TryGetComponent<ItemAction>(out ItemAction action))
                {
                    action.StopAction();
                }
                EventExit?.Invoke();
                inHome = false;
                startAction = false;
            }
        }
    }
    private void Update()
    {
        if (inHome)
        {
            inHomePosition = Vector3.Distance(transform.position, EndPosition) < .2f;
            if (inHomePosition)
            {
                StartActionInHome();
            }
            if (!IsDragable)
            {
                transform.position = Vector3.Lerp(transform.position, EndPosition, AnimationDuration * Time.deltaTime);
                transform.localScale = Vector3.Lerp(transform.localScale, EndScale, AnimationDuration * Time.deltaTime);
                transform.rotation = Quaternion.Lerp(transform.rotation, EndRotation, AnimationDuration * Time.deltaTime);
                //  if(ChildCollidor != null) ChildCollidor.enabled = true;

            }

        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, StartPosition, AnimationDuration * Time.deltaTime);
            transform.localScale = Vector3.Lerp(transform.localScale, StartScale, AnimationDuration * Time.deltaTime);
            transform.rotation = Quaternion.Lerp(transform.rotation, StartRotation, AnimationDuration * Time.deltaTime);
          //  if (ChildCollidor != null) ChildCollidor.enabled = false;
        }

    }

    void StartActionInHome()
    {
        if (TryGetComponent<ItemAction>(out ItemAction action))
        {
            if (!startAction)
            {
                Debug.Log("StartActionInHome");
                action.StartAction();
                startAction = true;
            }
        }

    }

    public void ActiveFade(bool status)
    {
        HomeHighlights.SetActive(status);

    }
    public void ReturnToHome()
    {
        startAction = false;
        inHome = false;
        EventExit?.Invoke();
    }
}

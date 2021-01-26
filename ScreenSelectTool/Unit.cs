using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour, ISelectable, ITargetable
{

    public bool isSelectable;
    public bool isTargetable;

    public bool Selectable
    {
        get
        {
            return isSelectable;
        }
        set
        {
            isSelectable = value;
        }
    }
    public bool Targetable
    {
        get
        {
            return isTargetable;
        }
        set
        {
            isTargetable = value;
        }
    }
    public Transform Transform
    {
        get
        {
            if (this != null)
                return this.transform;
            else
            {
                return null;
            }
        }
    }
    public HashSet<ISelectable> Container
    {
        get
        {
            return ScreenObjPicker.screenObjPicker.selectableObjects;
        }
    }
    public HashSet<ITargetable> TargetableList
    {
        get
        {
            return ScreenObjPicker.screenObjPicker.targetableObjects;
        }

    }

    public void BeingChosen()
    {
        if (outline != null)
            outline.enabled = true;
    }
    public void BeingDischosen()
    {
        if (outline != null)
            outline.enabled = false;
    }

    Outline outline;  //EXAMPLE  *DELETE IT IF YOUR ARE NOT USING IT :https://assetstore.unity.com/packages/tools/particles-effects/quick-outline-115488*

    private void Start()
    {
        outline = gameObject.GetComponent<Outline>();
        outline.enabled = false;

        Container.Add(this);
        TargetableList.Add(this);

    }
    private void OnDestroy()
    {

    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
//按鈕
public class OptionButton : MonoBehaviour
{
    public void Clicked()
    {
        StoryReader.ReturnInput(gameObject.name);
    }
}

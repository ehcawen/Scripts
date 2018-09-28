using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateFrame : MonoBehaviour {

    public int targetFrameRate = 1;

    //当程序唤醒时
    void Awake()
    {
        //修改当前的FPS
        Application.targetFrameRate = targetFrameRate;
    }
}

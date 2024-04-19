using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager : MonoBehaviour
{
    #region Singleton

    public static Manager instance;
    private void Awake()
    {
        instance = this;
    }

    #endregion

    public GameObject player;
}

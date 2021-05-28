using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Entities;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DebugJoinUIController : MonoBehaviour
{
    public TextMeshProUGUI RoomIdText;
    public Button JoinButton;

    // Start is called before the first frame update
    void Start()
    {
        JoinButton.onClick.AddListener(HandleJoin);
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void Awake()
    {
        // World.DefaultGameObjectInjectionWorld
    }

    void HandleJoin()
    {
        NetworkController.roomIdOverride = RoomIdText.text;
        SceneManager.LoadScene("SampleScene");
    }
}

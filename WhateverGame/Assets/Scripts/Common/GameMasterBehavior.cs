using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMasterBehavior : MonoBehaviour
{
    #region singleton
    public static GameMasterBehavior Instance;

    void Awake()
    {
        if (Instance == null)
        {

            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        CommonRefManager.Instance.startMenu.SetActive(true);
        CommonRefManager.Instance.gameOverMenu.SetActive(false);
        CommonRefManager.Instance.playerStateInfo.SetActive(false);
    }

    public void QuitGame()
    {
        // save any game data here
#if UNITY_EDITOR
        // Application.Quit() does not work in the editor so
        // UnityEditor.EditorApplication.isPlaying need to be set to false to end the game
        UnityEditor.EditorApplication.isPlaying = false;
#else
         Application.Quit();
#endif
    }

    public void OnStartGame()
    {
        CommonRefManager.Instance.startMenu.SetActive(false);
        CommonRefManager.Instance.gameOverMenu.SetActive(false);
        CommonRefManager.Instance.playerStateInfo.SetActive(true);
    }

    public void OnGameOver()
    {
        CommonRefManager.Instance.startMenu.SetActive(false);
        CommonRefManager.Instance.gameOverMenu.SetActive(true);
        CommonRefManager.Instance.playerStateInfo.SetActive(false);
    }
}

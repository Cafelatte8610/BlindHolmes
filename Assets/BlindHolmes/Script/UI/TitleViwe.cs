using System;
using System.Threading;
using BlindHolmes.MVP;
using Cysharp.Threading.Tasks;
using UnityEngine;
using R3;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class TitleViwe : MonoBehaviour
{
    [SerializeField] private CustomButton m_startButton;

    [SerializeField] private CustomButton m_quitButton;
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        m_startButton.ClickAsObservable.Subscribe(_ => StartGame()).AddTo(this);
        m_quitButton.ClickAsObservable.Subscribe(_ => QuitGame()).AddTo(this);
    }

    private void StartGame()
    {
        SceneManager.LoadScene("Stage1");
    }

    private void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif    
    }

}

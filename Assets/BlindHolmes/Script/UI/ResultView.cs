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

public class ResultView : MonoBehaviour
{
    [SerializeField] private GameObject m_ContentPanel;
    [SerializeField] private TMP_Text m_answerText;
    [SerializeField] private TMP_Text m_deduceText;
    [SerializeField] private TMP_Text m_resultText;
    [SerializeField] private CustomButton m_rePlayButton;

    private void Start()
    {
        m_rePlayButton.ClickAsObservable
            .Subscribe(_ =>
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            });
        m_ContentPanel.SetActive(false);
    }

    public void OpenResult(int answer,string deduce)
    {
        m_ContentPanel.SetActive(true);
        if (answer == 3)
        {
            m_resultText.text = "正解！";
        }
        else
        {
            m_resultText.text = "不正解！";
        }

        switch (answer)
        {
            case -1 :
                m_answerText.text = "犯人を絞り込めなかった…";
                break;
            case 1:
                m_answerText.text = "メアリー";
                break;     
            case 2:
                m_answerText.text = "ジェイク";
                break;  
            case 3:
                m_answerText.text = "ミラー";
                break;  
        }
        m_deduceText.text = deduce;
    }
}

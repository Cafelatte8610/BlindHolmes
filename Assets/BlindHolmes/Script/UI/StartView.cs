using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using R3;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

namespace BlindHolmes.MVP
{
    public class StartView : MonoBehaviour
    {
        [SerializeField] private GameObject m_ContentPanel;
        [SerializeField] private Image m_assistantImage;
        [SerializeField] private Image m_ditectiveImage;
        [SerializeField] private TMP_Text m_speakerText;
        [SerializeField] private TMP_Text m_talkText;
        [SerializeField] private float m_delayDuration = 0.05f;


        [SerializeField] private GameManager m_gameManager;
        private CancellationTokenSource _cts;
        private CancellationTokenSource _talkCts;

        private void OnDestroy()
        {
            _talkCts?.Cancel();
            _talkCts?.Dispose();
            _cts?.Cancel();
            _cts?.Dispose();
        }

        public void Open()
        {
            m_ContentPanel.SetActive(true);
            m_assistantImage.gameObject.SetActive(true);
            m_ditectiveImage.gameObject.SetActive(true);

            _talkCts?.Cancel();
            _talkCts = new CancellationTokenSource();

            // 会話全体を開始
            TalkEvidenceAsync(_talkCts.Token).Forget();
        }

        public void Close()
        {
            _talkCts?.Cancel();
            m_ContentPanel.SetActive(false);
        }
        
        
        private bool IsClicked()
        {
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) return true;
            return false;
        }

        // 会話のAsync
        private async UniTask TalkEvidenceAsync(CancellationToken token)
        {
            try
            {
                m_speakerText.text = "探偵";
                m_assistantImage.color = Color.gray;
                m_ditectiveImage.color = Color.white;
                
                await ShowMessageAndWaitClick("急に呼び出して悪いね助手君", token);

                m_speakerText.text = "助手";
                m_assistantImage.color = Color.white;
                m_ditectiveImage.color = Color.gray;
                await ShowMessageAndWaitClick("高名な盲目探偵　ブラインドホームズに協力できるならどこへでも行きます！", token);

                
                m_speakerText.text = "探偵";
                m_assistantImage.color = Color.gray;
                m_ditectiveImage.color = Color.white;

                await ShowMessageAndWaitClick("今回の事件は港町オハイオのレンガ通りで起きた事件だ", token);
                await ShowMessageAndWaitClick("被害者は郵便配達員のボブ、遺体は警察が回収したが遺体があった場所は記してあるので調べてくれたまえ。", token);
                await ShowMessageAndWaitClick("簡単な警察の調査で犯行推定時刻に証明されたアリバイが無かった３人が今回の事件の容疑者だ。隣人のミラー、被害者の妻のメアリー、被害者の息子のボブだ", token);
                await ShowMessageAndWaitClick("君はご存じの通り眼が見えない私の代わりに事件現場を調査し、犯人の手がかりになりそうな情報を私に持ってきてくれ。私はここで待っているから何かわかったら話しかけてくれ。", token);
                await ShowMessageAndWaitClick("左クリックで証拠になりそうなものを細かく見れるのでつかうといい。", token);
                await ShowMessageAndWaitClick("調査が修了したら私に話して「推理する」のボタンを押してくれ。", token);
                await ShowMessageAndWaitClick("では私の代わりに調査頼むよ助手君", token);
                
                m_speakerText.text = "助手";
                m_assistantImage.color = Color.white;
                m_ditectiveImage.color = Color.gray;
                await ShowMessageAndWaitClick("了解しました！", token);
                
                m_gameManager.CloseUI();
                Close();
            }
            catch (OperationCanceledException)
            {
            }
        }


        // テキスト表示とクリック待機
        private async UniTask ShowMessageAndWaitClick(string message, CancellationToken parentToken)
        {
            _cts?.Cancel();
            _cts?.Dispose();

            _cts = CancellationTokenSource.CreateLinkedTokenSource(parentToken);
            var linkedToken = _cts.Token;

            await WriteMessageAsync(message, linkedToken);

            await UniTask.WaitUntil(() => IsClicked(), cancellationToken: linkedToken);            
            await UniTask.Yield(linkedToken);
        }

        // 文字送り
        private async UniTask WriteMessageAsync(string message, CancellationToken token)
        {
            m_talkText.text = message;
            m_talkText.maxVisibleCharacters = 0;

            int totalLength = message.Length;

            for (int i = 1; i <= totalLength; i++)
            {
                m_talkText.maxVisibleCharacters = i;

                float currentWait = 0f;
                while (currentWait < m_delayDuration)
                {
                    if (IsClicked())
                    {
                        m_talkText.maxVisibleCharacters = totalLength;
                        await UniTask.Yield(token);
                        return;
                    }

                    currentWait += Time.deltaTime;
                    await UniTask.Yield(token);
                }
            }
            
            m_talkText.maxVisibleCharacters = totalLength;
        }
    } 
}
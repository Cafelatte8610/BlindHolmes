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
    public class HolmesView : MonoBehaviour
    {
        [SerializeField] private GameObject m_ContentPanel;
        [SerializeField] private Image m_assistantImage;
        [SerializeField] private Image m_ditectiveImage;
        [SerializeField] private TMP_Text m_speakerText;
        [SerializeField] private TMP_Text m_talkText;
        [SerializeField] private float m_delayDuration = 0.05f;
        [SerializeField] private GameObject m_SubmitPanel;
        [SerializeField] private TMP_InputField m_submitText;
        [SerializeField] private CustomButton m_submitButton;
        [SerializeField] private CustomButton m_finButton;
        [SerializeField] private CustomButton m_closeButton;
        [SerializeField] private GeminiChat m_geminiChat;
        [SerializeField] private ResultView m_resultView;

        public readonly Subject<Unit> CloseHolmesSubject = new Subject<Unit>();

        public Observable<Unit> CloseButtonAsObservable
        {
            get { return m_closeButton.ClickAsObservable; }
        }

        private CancellationTokenSource _cts;
        private CancellationTokenSource _talkCts;

        private void Start()
        {
            m_ContentPanel.SetActive(false);
            m_submitButton.ClickAsObservable.Subscribe(_ =>
            {
                SendEvidence();
            }).AddTo(this);;
            m_finButton.ClickAsObservable.Subscribe(_ =>
            {
                FinDeduce();
            }).AddTo(this);;
        }

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
            m_SubmitPanel.SetActive(false);

            _talkCts?.Cancel();
            _talkCts = new CancellationTokenSource();

            // 会話全体を開始
            TalkEvidenceAsync(_talkCts.Token).Forget();
        }

        public void Close()
        {
            _talkCts?.Cancel();
            m_ContentPanel.SetActive(false);
            CloseHolmesSubject.OnNext(Unit.Default);
        }

        // Geminiにメッセージを送る
        private void SendEvidence()
        {
            string message = m_submitText.text;
            if (string.IsNullOrEmpty(message)) return;
            m_geminiChat.SendMessageToGemini(message, (reply) => 
            {
                Debug.Log("探偵の反応: " + reply); 
            });
        }

        // 推理の結果
        private void FinDeduce()
        {
            m_geminiChat.AccuseCulprit((text, id) =>
            {
                string displayText = text.Trim();
                int lastNewLineIndex = displayText.LastIndexOf('\n');
                if (lastNewLineIndex > 0)
                {
                    string lastLine = displayText.Substring(lastNewLineIndex + 1).Trim();
                    if (int.TryParse(lastLine, out int _))
                    {
                        displayText = displayText.Substring(0, lastNewLineIndex).Trim();
                    }
                }
                
                Debug.Log("探偵の推理: " + displayText);
                if (id == 3)
                {
                    Debug.Log("【ゲームクリア】正解！犯人はミラーだ！");
                }
                else if (id == 1)
                {
                    Debug.Log("【ゲームオーバー】冤罪だ！妻を疑ってしまった...");
                }
                else if (id == 2)
                {
                    Debug.Log("【ゲームオーバー】冤罪だ！息子を疑ってしまった...");
                }
                else
                {
                    Debug.Log("【失敗】探偵は犯人を絞り込めなかったようだ（情報不足）");
                }
                m_resultView.OpenResult(id, displayText);
            });
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
                m_SubmitPanel.SetActive(false);
                m_assistantImage.color = Color.gray;
                m_ditectiveImage.color = Color.white;
                
                await ShowMessageAndWaitClick("なにか発見はあったかな？", token);

                m_speakerText.text = "助手";
                m_submitText.text = "";
                m_SubmitPanel.SetActive(true);
                m_assistantImage.color = Color.white;
                m_ditectiveImage.color = Color.gray;

                await m_submitButton.ClickAsObservable.FirstAsync(token);

                m_speakerText.text = "探偵";
                m_SubmitPanel.SetActive(false);
                m_assistantImage.color = Color.gray;
                m_ditectiveImage.color = Color.white;

                await ShowMessageAndWaitClick("なるほど、それが証拠というわけだね。", token);

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
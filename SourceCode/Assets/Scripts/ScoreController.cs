using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ScoreController : MonoBehaviour
{
    #region UI
    [Header("UI")]
    public TMPro.TextMeshProUGUI redScore;
    public TMPro.TextMeshProUGUI blueScore;
    public TMPro.TextMeshProUGUI redOverScore;
    public TMPro.TextMeshProUGUI blueOverScore;
    public TMPro.TextMeshProUGUI speedText;
    public TMPro.TextMeshProUGUI winnerText;
    public TMPro.TextMeshProUGUI countdownText;
    public TMPro.TextMeshProUGUI timerText;
    public Button returnButton;
    public GameObject scorePanel;
    public GameObject countdownPanel;
    public Camera cameraToShake;
    private CanvasGroup panelCanvasGroupScore;
    private CanvasGroup panelCanvasGroupCountdown;
    #endregion

    [SerializeField]
    private float shakeStrength = 0.5f;

    [SerializeField]
    private float shakeDuration = 0.3f;

    [SerializeField]
    private float scoreEaseInDuration = 0.15f;

    private float TimeLeft = 300f;
    private static bool TimerOn = false;

    private List<GameObject> players;

    void Start()
     {
        LeanTween.init();
        panelCanvasGroupScore = scorePanel.GetComponent<CanvasGroup>();
        panelCanvasGroupCountdown = countdownPanel.GetComponent<CanvasGroup>();
        players = new List<GameObject>();
    }

    void Update()
    {
        if(TimeLeft >0)
        {
            if(TimerOn)TimeLeft -= Time.deltaTime;
            if(TimeLeft >=298f)
            {
                GameObject[] eventProviders = GameObject.FindGameObjectsWithTag("Player");
                if(eventProviders.Length == 0)return;
                for(int i = 0; i < eventProviders.Length; ++i)
                {
                    if(players.Contains(eventProviders[i]))continue;
                    eventProviders[i].GetComponent<NetworkGamePlayer>().PointScored  +=  ShowScore;
                    eventProviders[i].GetComponent<NetworkGamePlayer>().GameStarting +=  ShowCountdown;
                    players.Add(eventProviders[i]);
                }
            }
        }
        else
        {
            TimerOn = false;
            TimeLeft = 0f;
            NetworkGamePlayer.isInputAvaliable = false;
            ShowScorePanel();
            speedText.text = "";
            int currentRedScore = int.Parse(redScore.text);
            int currentBlueScore = int.Parse(blueScore.text);
            if(currentRedScore > currentBlueScore)winnerText.text = "RED WON";
            else if(currentRedScore < currentBlueScore)winnerText.text = "BLUE WON";
            else winnerText.text = "TIE";
            returnButton.gameObject.SetActive(true);
        }
        UpdateTimer();
    }

    private void UpdateTimer()
    {
        float currentTime = TimeLeft;
        if(currentTime != 0f && TimerOn)++currentTime;
        float minutes = Mathf.FloorToInt(currentTime / 60);
        float seconds = Mathf.FloorToInt(currentTime % 60);
        timerText.text = string.Format("{0:00}:{1:00}",minutes,seconds);
    }

    public void ShowScore(int redPoints,int bluePoints,float ballSpeed,string finalText)
    {
        if(!TimerOn)return;
        blueScore.text = bluePoints.ToString();
        blueOverScore.text = bluePoints.ToString();
        redScore.text = redPoints.ToString();
        redOverScore.text = redPoints.ToString();
        speedText.text = $"{ballSpeed * 5}".Substring(0,5)+ " KM/H";
        ShowScorePanel();
        if(string.IsNullOrEmpty(finalText))
        {
            StartCoroutine(HideScore(3f,panelCanvasGroupScore));
        }
        else
        {
            TimerOn = false;
            winnerText.text = finalText;
            returnButton.gameObject.SetActive(true);
        }
        LeanTween.move(cameraToShake.gameObject, cameraToShake.transform.position + new Vector3(shakeStrength, shakeStrength, 0f), shakeDuration)
            .setEase(LeanTweenType.easeShake)
            .setLoopPingPong(1);
    }
    
    private void ShowScorePanel()
    {
        ConfettiController.instance.SprayConfetti();
        LeanTween.alphaCanvas(panelCanvasGroupScore,1f,scoreEaseInDuration);
    }

    public void ShowCountdown()
    {
        LeanTween.alphaCanvas(panelCanvasGroupCountdown,1f,scoreEaseInDuration);
        StartCoroutine(ChangeCountdownText(1f,"2"));
        StartCoroutine(ChangeCountdownText(2f,"1"));
        StartCoroutine(ChangeCountdownText(3f,"Begin"));
        StartCoroutine(HideScore(3.5f,panelCanvasGroupCountdown));
        StartCoroutine(EnableTimer());
    }

    private IEnumerator ChangeCountdownText(float time, string text)
    {
        yield return new WaitForSeconds(time);
        countdownText.text = text;
    }

    private IEnumerator EnableTimer()
    {
        yield return new WaitForSeconds(3.5f);
        TimerOn = true;
    }

    private IEnumerator HideScore(float time,CanvasGroup panel)
    {
        yield return new WaitForSeconds(time);
        if(string.IsNullOrEmpty(winnerText.text))
        {
            LeanTween.alphaCanvas(panel,0f,scoreEaseInDuration);
            NetworkGamePlayer.isInputAvaliable = true;
        }
    }
}

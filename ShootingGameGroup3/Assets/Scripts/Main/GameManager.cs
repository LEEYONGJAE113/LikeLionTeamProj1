using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Objects")]
    public MouseManager MouseManager;
    public Player Player;
    public WeaponManager WeaponManager;
    public LYJ_SpawnManager SpawnManager;

    [Header("Scene Clear")]
    // 씬 클리어 상태 변수
    public bool isSceneCleared = false;

    // 씬 전환 순서 배열
    public string[] sceneOrder = new string[] { "MainScene", "CHW", "LHG", "LSM", "LYJ", "SDH" };
    public int stage { get; private set; } = 0;

    [Header("Fade Settings")]
    // 전체 화면을 덮는 UI Image (Inspector에서 연결)
    public Image fadeImage;

    // 페이드 효과에 걸리는 시간 (초)
    public float fadeDuration = 1f;

    private bool isFading = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject); // 중복 방지
        }

        string sceneName = SceneManager.GetActiveScene().name;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // 새로운 씬이 로드되면 isSceneCleared를 false로 초기화하고, stage 값을 업데이트한 뒤 페이드 인 실행
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 씬이 로드될 때마다 새로운 Player 찾아서 연결
        Player = FindObjectOfType<Player>();

        if (fadeImage != null)
        {
            Color color = fadeImage.color;
            color.a = 1f;
            fadeImage.color = color;
        }

        isSceneCleared = false;
        UpdateStage(scene.name);
        StartCoroutine(FadeIn());
    }

    private void UpdateStage(string sceneName)
    {
        switch (sceneName)
        {
            case "MainScene":
                stage = 0;
                break;
            case "CHW":
                stage = 1;
                break;
            case "LHG":
                stage = 2;
                break;
            case "LSM":
                stage = 3;
                break;
            case "LYJ":
                stage = 4;
                break;
            case "SDH":
                stage = 5;
                break;
            default:
                stage = 0;
                break;
        }
    }

    void Update()
    {
        // 씬 클리어 상태이고, 현재 페이드 효과가 실행 중이 아니면 씬 전환 시작
        if (isSceneCleared && !isFading)
        {
            StartCoroutine(TransitionToNextScene());
        }
        // 'K' 키를 눌렀을 때 씬 클리어 호출
        if (Input.GetKeyDown(KeyCode.K)) // 'K' 키 사용
        {
            ClearScene();
        }
    }

    // 외부에서 씬 클리어 처리를 요청할 때 호출
    public void ClearScene()
    {
        isSceneCleared = true;
    }

    // 현재 씬의 다음 씬을 로드하는 함수
    void LoadNextScene()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        int currentIndex = System.Array.IndexOf(sceneOrder, currentSceneName);

        if (currentIndex < 0)
        {
            Debug.LogError("현재 씬이 sceneOrder 배열에 존재하지 않습니다: " + currentSceneName);
            return;
        }

        int nextIndex = currentIndex + 1;

        if (nextIndex < sceneOrder.Length)
        {
            SceneManager.LoadScene(sceneOrder[nextIndex]);
        }
        else
        {
            Debug.Log("모든 씬을 완료했습니다.");
            // 추가 처리 가능
        }
    }

    // 씬 전환 시 페이드 아웃 -> 씬 로드 -> 페이드 인 순서로 진행
    private IEnumerator TransitionToNextScene()
    {
        yield return StartCoroutine(FadeOut());
        LoadNextScene();
        // 씬 로드 후 OnSceneLoaded()에서 FadeIn()이 호출됨
    }

    // 페이드 아웃: 화면이 서서히 검게 변하며, 그동안 게임 진행은 중지됨
    private IEnumerator FadeOut()
    {
        if (fadeImage == null)
        {
            yield break;
        }

        isFading = true;
        Time.timeScale = 0f;
        float timer = 0f;
        Color color = fadeImage.color;

        while (timer < fadeDuration && fadeImage != null)
        {
            timer += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(0f, 1f, timer / fadeDuration);
            color.a = alpha;
            fadeImage.color = color;
            yield return null;
        }

        if (fadeImage != null)
        {
            color.a = 1f;
            fadeImage.color = color;
        }
        yield return null;
    }

    // 페이드 인: 씬 로드 후 화면이 서서히 밝아지며, 완료되면 게임 진행을 재개
    private IEnumerator FadeIn()
    {
        if (fadeImage == null)
        {
            Time.timeScale = 1f;
            isFading = false;
            yield break;
        }

        float timer = 0f;
        Color color = fadeImage.color;

        while (timer < fadeDuration && fadeImage != null)
        {
            timer += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
            color.a = alpha;
            fadeImage.color = color;
            yield return null;
        }

        if (fadeImage != null)
        {
            color.a = 0f;
            fadeImage.color = color;
        }

        Time.timeScale = 1f;
        isFading = false;
        yield return null;
    }

    float currentStageTime;
    public float CurrentStageTime => currentStageTime;
    bool isTimeGoing;

    public void StopGame()
    {
        isTimeGoing = false;
        Time.timeScale = 0;
    }

    public void ResumeGame()
    {
        isTimeGoing = true;
        Time.timeScale = 1;
    }
}

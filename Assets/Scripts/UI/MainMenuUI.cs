using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Button gameStartButton;

    private const string GAME_SCENE = "SampleScene";

    private void Start()
    {
        if (gameStartButton != null)
            gameStartButton.onClick.AddListener(OnGameStart);
    }

    private void OnGameStart()
    {
        SceneManager.LoadScene(GAME_SCENE);
    }
}

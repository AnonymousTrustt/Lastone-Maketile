using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuPlaceholderActions : MonoBehaviour
{
    [SerializeField] private string gameSceneName = "SampleScene";
    [SerializeField] private string gameScenePath = "Assets/Scenes/SampleScene.unity";

    public void StartNewGame()
    {
        if (!Application.CanStreamedLevelBeLoaded(gameSceneName))
        {
#if UNITY_EDITOR
            UnityEditor.SceneManagement.EditorSceneManager.LoadSceneInPlayMode(gameScenePath, new LoadSceneParameters(LoadSceneMode.Single));
#else
            Debug.LogError($"Cannot load scene '{gameSceneName}'. Add it to File > Build Profiles > Scene List.");
#endif
            return;
        }

        SceneManager.LoadScene(gameSceneName);
    }

    public void LoadSave()
    {
        Debug.Log("Load Save clicked. Placeholder action only.");
    }

    public void QuitGame()
    {
        Debug.Log("Quit clicked.");
        Application.Quit();
    }
}

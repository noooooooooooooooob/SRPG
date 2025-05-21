using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    // 인스펙터에서 넣어줄 씬 이름
    [SerializeField] private string sceneToLoad;

    public void LoadScene()
    {
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            SceneManager.LoadScene(sceneToLoad);
        }
        else
        {
            Debug.LogWarning("로드할 씬 이름이 설정되지 않았습니다!");
        }
    }

    // 현재 씬 다시 로드할 때 쓸 수도 있음
    public void ReloadCurrentScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
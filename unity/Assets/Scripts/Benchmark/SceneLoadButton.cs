using UnityEngine;
using UnityEngine.SceneManagement;

namespace Benchmark
{
    public class SceneLoadButton : MonoBehaviour
    {
        public void LoadScene(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}

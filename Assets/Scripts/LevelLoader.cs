using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Unity.BizimKodlar
{
    public class LevelLoader : MonoBehaviour
    {
        [SerializeField]
        private Animator animator;

        public float transitionTime = .5f;

        public void LoadNextLevel()
        {
            StartCoroutine(LoadLevel(SceneManager.GetActiveScene().buildIndex + 1));
        }

        public void LoadLevelAtIndex(int idx)
        {
            StartCoroutine(LoadLevel(idx));
        }

        public IEnumerator LoadLevel(int levelIndex)
        {
            animator.SetTrigger("start");
            yield return new WaitForSeconds(transitionTime);
            SceneManager.LoadScene(levelIndex);
        }


    }
}

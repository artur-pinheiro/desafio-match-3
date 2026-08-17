using UnityEngine;
using UnityEngine.SceneManagement;

namespace Gazeus.DesafioMatch3 {
    public class MenuController : MonoBehaviour {

        

        public void PlayGame(int gameMode) {
            PlayerPrefs.SetInt("GameMode", gameMode);

            SceneManager.LoadScene("Gameplay");
        }

        public void SelectMatchMode(int matchMode) {
            PlayerPrefs.SetInt("MatchMode", matchMode);
        }
    }
}

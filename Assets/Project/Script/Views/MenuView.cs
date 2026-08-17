using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gazeus.DesafioMatch3
{
    public class MenuView : MonoBehaviour {

        [SerializeField] private TMP_Dropdown _matchModeDropdown;

        void Start() {
            _matchModeDropdown.value = PlayerPrefs.GetInt("MatchMode", 0);
        }

    }
}

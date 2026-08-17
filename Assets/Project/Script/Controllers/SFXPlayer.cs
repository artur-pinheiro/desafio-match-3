using UnityEngine;
using static Gazeus.DesafioMatch3.AudioController;

namespace Gazeus.DesafioMatch3 {
    public class SFXPlayer : MonoBehaviour     {

        public SFXType sfx;

        public void PlayEffect() {
            AudioController.Instance?.PlaySfx(sfx);
        }

    }
}

using System;
using UnityEngine;

namespace Gazeus.DesafioMatch3 {
    public class EventSystem : MonoBehaviour {

        public static Action OnTileDestroyed;

        public static Action<int> OnScoreUpdated;

        public static Action<int> OnComboPerformed;

    }
}

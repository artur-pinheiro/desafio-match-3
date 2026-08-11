using System;
using UnityEngine;

namespace Gazeus.DesafioMatch3
{
    public class ScoreController : MonoBehaviour {

        [SerializeField] private int _scorePerTile = 10;

        private int _currentScore;

        void Start() {
            EventSystem.OnTileDestroyed += IncreaseScore;
        }

        private void OnDestroy() {
            EventSystem.OnTileDestroyed -= IncreaseScore;
        }

        private void IncreaseScore() {
            _currentScore += _scorePerTile;
            print("New Score: " + _currentScore);
        }
    }
}

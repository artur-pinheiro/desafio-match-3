using TMPro;
using UnityEngine;
using DG.Tweening;

namespace Gazeus.DesafioMatch3 {
    public class ScoreView : MonoBehaviour     {

        [SerializeField] private float _size; 
        [SerializeField] private float _rotation;
        [SerializeField] private float _duraiton;
        [SerializeField] private TextMeshProUGUI _scoreText; 
        private Vector3 originalScale;
        private Vector3 originalRotation;

        private void Awake() {
            originalScale = _scoreText.transform.localScale;
            originalRotation = _scoreText.transform.localEulerAngles;

            EventSystem.OnScoreUpdated += AnimateScoreText;
        }

        private void OnDestroy() {
            EventSystem.OnScoreUpdated -= AnimateScoreText;
        }

        private void AnimateScoreText(int score) {
            _scoreText.text = score.ToString();

            _scoreText.transform.localScale = originalScale;
            _scoreText.transform.localEulerAngles = originalRotation;

            _scoreText.transform
                .DOScale(originalScale * _size, _duraiton) 
                .SetEase(Ease.OutBack) 
                .OnComplete(() => {
                    _scoreText.transform.DOScale(originalScale, 0.1f)
                        .SetEase(Ease.InBack);
                });

            _scoreText.transform
                .DORotate(originalRotation + new Vector3(0, 0, _rotation), _duraiton) 
                .OnComplete(() => {
                    _scoreText.transform.DORotate(originalRotation, 0.1f);
                });
        }
    }
}

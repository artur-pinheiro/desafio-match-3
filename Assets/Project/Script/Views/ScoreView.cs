using TMPro;
using UnityEngine;
using DG.Tweening;

namespace Gazeus.DesafioMatch3 {
    public class ScoreView : MonoBehaviour     {

        [SerializeField] private float _size; 
        [SerializeField] private float _rotation;
        [SerializeField] private float _duration;
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

            StopAnimation();
            _scoreText.text = score.ToString();

            _scoreText.transform.localScale = originalScale;
            _scoreText.transform.localEulerAngles = originalRotation;

            Sequence scoreAnimation = DOTween.Sequence();

            Tween scaleUp = _scoreText.transform.DOScale(originalScale * _size, _duration).SetEase(Ease.OutBack);
            Tween rotate = _scoreText.transform.DORotate(originalRotation + new Vector3(0, 0, _rotation), _duration);

            scoreAnimation.Append(scaleUp);
            scoreAnimation.Join(rotate);

            Tween scaleDown = _scoreText.transform.DOScale(originalScale, 0.1f).SetEase(Ease.InBack);
            Tween rotateBack = _scoreText.transform.DORotate(originalRotation, 0.1f);

            scoreAnimation.Append(scaleDown);
            scoreAnimation.Join(rotateBack);

            scoreAnimation.Play();
        }

        public void StopAnimation() {
            DOTween.Kill(_scoreText);
            _scoreText.transform.DOKill();
        }
    }
}

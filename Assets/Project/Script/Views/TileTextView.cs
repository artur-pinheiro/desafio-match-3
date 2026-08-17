using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Gazeus.DesafioMatch3 {
    public class TileTextView : MonoBehaviour {
        [SerializeField] private float _fadeDuration = 1f;
        [SerializeField] private float _delayBeforeFadeOut = 2f;
        [SerializeField] private float _maxScale = 1.2f;
        [SerializeField] private float _scaleDuration = 0.5f;
        [SerializeField] private TextMeshProUGUI _tileText;


        private Vector3 _originalScale;
        private Color _originalColor;
        private int _brokenTiles = 0;

        void Start() {
            _originalScale = _tileText.transform.localScale;
            _originalColor = _tileText.color;
            _tileText.color = new Color(_originalColor.r, _originalColor.g, _originalColor.b, 0f);

            EventSystem.OnTileDestroyed += PlayAnimation;
        }

        private void OnDestroy() {
            EventSystem.OnTileDestroyed -= PlayAnimation;
        }

        public void PlayAnimation() {

            _brokenTiles++;

            StopAnimation();
            _tileText.text = (_brokenTiles) + " Tiles!!";

            _tileText.color = new Color(_originalColor.r, _originalColor.g, _originalColor.b, 0f);
            _tileText.transform.localScale = _originalScale;

            Sequence sequence = DOTween.Sequence();

            sequence.Append(DOTween.To(() => _tileText.color.a, x => _tileText.color = new Color(_originalColor.r, _originalColor.g, _originalColor.b, x), 1f, _fadeDuration));
            sequence.Join(_tileText.transform.DOScale(_originalScale * _maxScale, _scaleDuration));

            sequence.AppendInterval(_delayBeforeFadeOut);

            sequence.Append(DOTween.To(() => _tileText.color.a, x => _tileText.color = new Color(_originalColor.r, _originalColor.g, _originalColor.b, x), 0f, _fadeDuration));
            sequence.Join(_tileText.transform.DOScale(_originalScale, _scaleDuration));

            sequence.onComplete += InvokeReset;

        }

        private void InvokeReset() {
            Invoke(nameof(ResetTileCounter), 2f);
        }

        private void ResetTileCounter() {
            _brokenTiles = 0;
        }


        public void StopAnimation() {
            DOTween.Kill(_tileText);
            _tileText.transform.DOKill();
        }

    }
}

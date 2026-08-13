using DG.Tweening;
using System.Xml;
using TMPro;
using UnityEngine;

namespace Gazeus.DesafioMatch3 {
    public class ComboView : MonoBehaviour {

        [SerializeField] private float _fadeDuration = 1f;
        [SerializeField] private float _delayBeforeFadeOut = 2f;
        [SerializeField] private float _maxScale = 1.2f;
        [SerializeField] private float _scaleDuration = 0.5f;
        [SerializeField] private TextMeshProUGUI _comboText;


        private Vector3 _originalScale;
        private Color _originalColor;

        void Start() {
            _originalScale = _comboText.transform.localScale;
            _originalColor = _comboText.color;
            _comboText.color = new Color(_originalColor.r, _originalColor.g, _originalColor.b, 0f);

            EventSystem.OnComboPerformed += PlayAnimation;
        }

        private void OnDestroy() {
            EventSystem.OnComboPerformed -= PlayAnimation;
        }

        public void PlayAnimation(int combo) {

            if (combo <= 1) {
                return;
            }

            StopAnimation();
            _comboText.text = (combo)+"x Combo!!";

            _comboText.color = new Color(_originalColor.r, _originalColor.g, _originalColor.b, 0f);
            _comboText.transform.localScale = _originalScale;

            Sequence sequence = DOTween.Sequence();

            sequence.Append(DOTween.To(() => _comboText.color.a, x => _comboText.color = new Color(_originalColor.r, _originalColor.g, _originalColor.b, x), 1f, _fadeDuration));
            sequence.Join(_comboText.transform.DOScale(_originalScale * _maxScale, _scaleDuration));

            sequence.AppendInterval(_delayBeforeFadeOut);

            sequence.Append(DOTween.To(() => _comboText.color.a, x => _comboText.color = new Color(_originalColor.r, _originalColor.g, _originalColor.b, x), 0f, _fadeDuration));
            sequence.Join(_comboText.transform.DOScale(_originalScale, _scaleDuration));

        }

        public void StopAnimation() {
            DOTween.Kill(_comboText);
            _comboText.transform.DOKill();
        }

    }
}

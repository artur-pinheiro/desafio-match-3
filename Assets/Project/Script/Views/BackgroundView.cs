using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;


namespace Gazeus.DesafioMatch3 { 
    public class BackgroundView : MonoBehaviour {

        [SerializeField] private float _duration = 2f;
        [Range(0f, 1f)] [SerializeField] private float _saturation = 0.7f;

        [Range(0f, 1f)] [SerializeField] private float _brightness = 0.9f;

        private Image _targetImage;
        private Sequence _sequence;

        void Awake() {
            _targetImage = GetComponent<Image>();
            StartLoop();
        }

        void StartLoop() {
            _sequence = DOTween.Sequence();

            _sequence.Append(DOTween.To(
                () => 0f,
                x => {
                    Color newColor = Color.HSVToRGB(x, _saturation, _brightness);
                    _targetImage.color = newColor;
                },
                1f,
                _duration
            )).SetEase(Ease.Linear);

            _sequence.SetLoops(-1, LoopType.Restart);
        }

        void OnDestroy() {
            if ( _sequence != null && _sequence.IsActive() ) {
                _sequence.Kill();
            }
        }

        public void StopLoop() {
            if ( _sequence != null && _sequence.IsActive() ) {
                _sequence.Kill();
            }
        }

        public void InitiateLoop() {
            if ( _sequence != null ) {
                _sequence.Kill();
            }
            StartLoop();
        }
    }
}
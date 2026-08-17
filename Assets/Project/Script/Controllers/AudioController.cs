using System.Collections.Generic;
using UnityEngine;

namespace Gazeus.DesafioMatch3 {
    public class AudioController : MonoBehaviour {
        public enum SFXType {
            Button,
            Break,
            Move,
            Spawn,
            Error
        }

        [SerializeField] private AudioSource _audioSource;
        [SerializeField] List<AudioClip> _buttonClickSfx;
        [SerializeField] List<AudioClip> _tileBreakSfx;
        [SerializeField] List<AudioClip> _tileMoveSfx;
        [SerializeField] List<AudioClip> _tileSpawnSfx;
        [SerializeField] List<AudioClip> _errorSfx;

        public static AudioController Instance;

        void Start() {
            if ( Instance == null )
                Instance = this;
            else
                Destroy(gameObject);

            DontDestroyOnLoad(this);
        }

        public void PlayButtonClick() {
            PlaySfx(SFXType.Button);
        }

        public void PlaySfx(SFXType type) {

            AudioClip clip;
            switch ( type ) {
                case SFXType.Button:
                    clip = _buttonClickSfx[Random.Range(0, _buttonClickSfx.Count)];
                    break;
                case SFXType.Break:
                    clip = _tileBreakSfx[Random.Range(0, _tileBreakSfx.Count)];
                    break;
                case SFXType.Move:
                    clip = _tileMoveSfx[Random.Range(0, _tileMoveSfx.Count)];
                    break;
                case SFXType.Spawn:
                    clip = _tileSpawnSfx[Random.Range(0, _tileSpawnSfx.Count)];
                    break;
                case SFXType.Error:
                    clip = _errorSfx[Random.Range(0, _errorSfx.Count)];
                    break;
                default:
                    clip = _buttonClickSfx[Random.Range(0, _buttonClickSfx.Count)];
                    break;
            }

            _audioSource.PlayOneShot(clip);

        }
    }
}


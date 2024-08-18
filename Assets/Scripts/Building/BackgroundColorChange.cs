using UnityEngine;

namespace masterland.Building
{
    public class BackgroundColorChange : MonoBehaviour
    {
        [SerializeField] private Camera _camera;
        [SerializeField] private Gradient _gradient;
        [SerializeField] private float _timeUntilChange;

        [SerializeField] private  float _currentTime;
        [SerializeField] private  float _currentColorValue;

        private void Start()
        {
            _currentTime = 0;
            _currentColorValue = 0f;
        }
        private void Update()
        {
            _currentTime += Time.deltaTime;
            _currentColorValue += 0.01f;

            if (_currentTime >= _timeUntilChange)
            {

                _camera.backgroundColor = _gradient.Evaluate(_currentColorValue);
                _currentTime = 0;
            }

            if (_currentColorValue > 1)
            {
                _currentColorValue = 0;
            }
        }
    }
}

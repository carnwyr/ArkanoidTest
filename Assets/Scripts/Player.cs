using System;
using UnityEngine;
using UniRx;
using Object = UnityEngine.Object;

namespace ArkanoidTest
{
    public class Player : BallReflector
    {
        private readonly Vector2 _playerStartPosition;
        private readonly Vector2 _ballStartPosition;
        private readonly Rigidbody2D _ball;
        private readonly float _ballSpeed;
        private readonly float _playerSpeed;
        private readonly Camera _camera;
        private readonly IReactiveProperty<Vector2> _ballVelocity;
        
        private bool _isBallFired;
        private IDisposable _inputSubscription;

        public Player(Collider2D playerPrefab, Rigidbody2D ballPrefab, float ballSpeed, float playerSpeed, Camera camera, IReactiveProperty<Vector2> ballVelocity)
        {
            _ballSpeed = ballSpeed;
            _playerSpeed = playerSpeed;
            _camera = camera;
            _ballVelocity = ballVelocity;

            _playerStartPosition = playerPrefab.transform.position;
            _ballStartPosition = ballPrefab.transform.position;

            _collider = Object.Instantiate(playerPrefab);
            _ball = Object.Instantiate(ballPrefab);
            
            SubscribeOnBallCollision(ballVelocity);
            SubscribeOnInput();
        }

        private void SubscribeOnInput()
        {
#if UNITY_EDITOR
            _inputSubscription = Observable.EveryUpdate()
                .Where(_ => Input.GetMouseButton(0))
                .Select(_ => (Vector2)_camera.ScreenToWorldPoint(Input.mousePosition))
                .Subscribe(x => ProcessInput(x, _ballVelocity));
#else
            _inputSubscription = Observable.EveryUpdate()
                .Where(_ => Input.touchCount > 0)
                .Select(_ => (Vector2)_camera.ScreenToWorldPoint(Input.GetTouch(0).position))
                .Subscribe(x => ProcessInput(x, _ballVelocity));
#endif
        }

        private void ProcessInput(Vector2 touchPosition, IReactiveProperty<Vector2> ballVelocity)
        {
            if (!_isBallFired)
            {
                _ball.velocity = _ballSpeed * (touchPosition - (Vector2) _ball.transform.position).normalized;
                ballVelocity.Value = _ball.velocity;
                _isBallFired = true;
                return;
            }

            var step = _playerSpeed * Time.deltaTime;
            var target = new Vector2(touchPosition.x, _collider.transform.position.y);
            _collider.transform.position = Vector3.MoveTowards(_collider.transform.position, target, step);
        }

        public void Reset()
        {
            _inputSubscription?.Dispose();
            
            _collider.transform.position = _playerStartPosition;
            _ball.velocity = Vector2.zero;
            _ball.transform.position = _ballStartPosition;
            _isBallFired = false;
            
            Observable.EveryUpdate()
                .Where(_ => !Input.GetMouseButton(0))
                .Take(1)
                .Subscribe(_ => SubscribeOnInput());
        }
    }
}
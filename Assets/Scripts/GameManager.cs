using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace ArkanoidTest
{
    public class GameManager
    {
        private const string _configPath = "config";
        private const string _cameraPath = "Main Camera";
        private const string _playerPath = "Player";
        private const string _ballPath = "Ball";
        private const string _brickPath = "Brick";

        private const int _rows = 2;
        private const int _columns = 20;
        private const float _brickMargin = 0.1f;
        
        private static float _ballSpeed = 10;
        private static float _playerSpeed = 10;

        private static Camera _mainCamera;
        private static Vector2 _screenUnits;
        private static Player _player;
        private static List<Brick> _bricks = new List<Brick>();
        private static IReactiveProperty<int> _removedBricksCount = new ReactiveProperty<int>();

        private static IReactiveProperty<Vector2> _ballVelocity = new ReactiveProperty<Vector2>();
        
        [RuntimeInitializeOnLoadMethod]
        private static void Init()
        {
            LoadConfig();
            CreateCamera();
            CreateBounds();
            CreatePlayer();
            CreateBricks();
            _removedBricksCount
                .Where(x => x == _rows * _columns)
                .Subscribe(x => RestartGame());
        }

        private static void LoadConfig()
        {
            var config = Resources.Load<TextAsset>(_configPath);
            
            if (config == null) return;
            
            var data = JsonUtility.FromJson<ConfigData>(config.text);
            _ballSpeed = data.BallSpeed;
            _playerSpeed = data.PlayerSpeed;
        }

        private static void CreateCamera()
        {
            var cameraPrefab = Resources.Load<Camera>(_cameraPath);
            _mainCamera = Object.Instantiate(cameraPrefab);
            _screenUnits = (Vector2)_mainCamera.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0));
        }

        private static void CreateBounds()
        {
            var leftBound = new Bound(_screenUnits, Vector2.left, _ballVelocity);
            var rightBound = new Bound(_screenUnits, Vector2.right, _ballVelocity);
            var topBound = new Bound(_screenUnits, Vector2.up, _ballVelocity);
            var bottomBound = new Bound(_screenUnits, Vector2.down, _ballVelocity, true);
            bottomBound.BallCollision.Subscribe(_ => RestartGame());
        }

        private static void CreatePlayer()
        {
            var playerPrefab = Resources.Load<Collider2D>(_playerPath);
            var ballPrefab = Resources.Load<Rigidbody2D>(_ballPath);
            _player = new Player(playerPrefab, ballPrefab, _ballSpeed, _playerSpeed, _mainCamera, _ballVelocity);
        }

        private static void CreateBricks()
        {
            var brickPrefab = Resources.Load<Collider2D>(_brickPath);

            var brickHeight = brickPrefab.transform.localScale.y;
            var brickWidth = (_screenUnits.x * 2 - (_columns + 1) * _brickMargin) / (float)_columns;

            for (var i = 0; i < _rows; i++)
            {
                for (var j = 0; j < _columns; j++)
                {
                    var posY = _screenUnits.y - 1 - i * (brickHeight + _brickMargin);
                    var posX = -_screenUnits.x  + _brickMargin + brickWidth / 2 + j * (brickWidth + _brickMargin);
                    var brick = new Brick(brickPrefab, posY, posX, brickHeight, brickWidth, _ballVelocity);
                    brick.RemoveBrick.Subscribe(_ => _removedBricksCount.Value++);
                    _bricks.Add(brick);
                }
            }
        }

        private static void RestartGame()
        {
            _removedBricksCount.Value = 0;
            _player.Reset();
            foreach (var brick in _bricks)
            {
                brick.Reset();
            }
        }
    }
}
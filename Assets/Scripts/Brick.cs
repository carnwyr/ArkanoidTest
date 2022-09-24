using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace ArkanoidTest
{
    public class Brick : BallReflector
    {
        public readonly IReactiveCommand<Unit> RemoveBrick = new ReactiveCommand<Unit>();
        
        public Brick(Collider2D brickPrefab, float posY, float posX, float height, float width, IReactiveProperty<Vector2> ballVelocity)
        {
            _collider = Object.Instantiate(brickPrefab);
            _collider.transform.localScale = new Vector3(width, height, 1);
            _collider.transform.position = new Vector3(posX, posY, 0);
            
            SubscribeOnBallCollision(ballVelocity);

            _collider.OnCollisionEnter2DAsObservable()
                .Where(x => x.gameObject.CompareTag(_ballTag))
                .Do(_ => _collider.gameObject.SetActive(false))
                .Subscribe(_ => RemoveBrick.Execute(Unit.Default));
        }

        public void Reset()
        {
            _collider.gameObject.SetActive(true);
        }
    }
}
using System;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace ArkanoidTest
{
    public class Bound : BallReflector
    {
        public IObservable<Unit> BallCollision;
        
        public Bound(Vector2 screenUnits, Vector2 direction, IReactiveProperty<Vector2> ballVelocity, bool gameOverOnTouch = false)
        {
            var gameObject = new GameObject();
            
            _collider = gameObject.AddComponent<BoxCollider2D>();
            ((BoxCollider2D)_collider).size = Vector2.Scale(screenUnits * 2, Pow(screenUnits * 2, -Abs(direction)));
            _collider.offset = Vector2.Scale(direction, screenUnits + 0.5f * Vector2.one);

            if (gameOverOnTouch)
            {
                BallCollision = _collider.OnCollisionEnter2DAsObservable()
                    .Where(x => x.gameObject.CompareTag(_ballTag))
                    .AsUnitObservable();
            }
            else
            {
                SubscribeOnBallCollision(ballVelocity);
            }
        }

        private Vector2 Abs(Vector2 vec) => new Vector2(Mathf.Abs(vec.x), Mathf.Abs(vec.y));

        private Vector2 Pow(Vector2 vec, Vector2 pow) => new Vector2(Mathf.Pow(vec.x, pow.x), Mathf.Pow(vec.y, pow.y));
    }
}
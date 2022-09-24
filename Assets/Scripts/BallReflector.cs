using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace ArkanoidTest
{
    public abstract class BallReflector
    {
        protected const string _ballTag = "Ball";
        
        protected Collider2D _collider;

        protected void SubscribeOnBallCollision(IReactiveProperty<Vector2> ballVelocity)
        {
            if (_collider == null)
            {
                return;
            }
            
            _collider.OnCollisionEnter2DAsObservable()
                .Where(x => x.gameObject.CompareTag(_ballTag))
                .Subscribe(x =>
                {
                    var reflectorVelocity = _collider.attachedRigidbody != null
                        ? _collider.attachedRigidbody.velocity
                        : Vector2.zero;
                    x.rigidbody.velocity = Vector3.Reflect(ballVelocity.Value, (x.contacts[0].normal + reflectorVelocity));
                    ballVelocity.Value = x.rigidbody.velocity;
                });
        }
    }
}
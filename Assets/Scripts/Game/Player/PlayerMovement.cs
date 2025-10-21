using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float _speed;  //units/sec
    private Rigidbody2D _rigidbody;
    private Vector2 _movementInput;         //WASD or left-stick

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        //Apply velocity directly (rigidbody2D is in 2D world units/sec)
        _rigidbody.velocity = _movementInput * _speed;
    }

    // Called by Input System (Send Message) for "Move"
    private void OnMove(InputValue inputValue)
    {
        _movementInput = inputValue.Get<Vector2>();
    }
}

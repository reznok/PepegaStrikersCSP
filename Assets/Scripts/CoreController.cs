using FishNet.Object;
using UnityEngine;

public class CoreController : NetworkBehaviour
{
    [SerializeField] Transform visualObject;
    Rigidbody2D _rigidbody;

    [SerializeField] Color _color;
    [SerializeField] Color _flashColor;
    [SerializeField] float _flashTimerMax;
    float _flashTimer;


    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    public void ProcessHit(bool replaying = false)
    {
        // Hit core Up and to the right
        // Just an example of a hit that will always be the same for testing

        _rigidbody.velocity = new Vector2();
        _rigidbody.AddForce(new Vector2(100, 100));

        // Flash Red
        if (!replaying)
        {
            _flashTimer = _flashTimerMax;
            visualObject.GetComponent<SpriteRenderer>().color = _flashColor;
        }
    }

    void Update()
    {
        if (_flashTimer <= 0)
        {
            visualObject.GetComponent<SpriteRenderer>().color = _color;         
        }
        _flashTimer -= Time.deltaTime;
    }
}

using UnityEngine;
using UnityEngine.Serialization;

public class CubeController : MonoBehaviour
{
    [SerializeField] private float weight;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float rotationSpeed;
    private float _velocity;

    void Update() {
        CalcGravity();
        CalcRotation();
        CalcMove();
    }

    private void CalcGravity() {
        Vector3 pos = transform.position;
        if (pos.y <= 0f) {
            _velocity = 5f;
        }
        float dt = Time.deltaTime;
        _velocity -= weight * dt;
        pos.y += _velocity * dt;
        transform.position = pos;
    }

    private void CalcRotation() {
        transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f);
    }

    private void CalcMove() {
        transform.position += moveSpeed * Time.deltaTime * transform.forward;
    }
    
    public void SetupSettings(float Weight, float MoveSpeed, float RotationSpeed) {
        this.weight = Weight;
        this.moveSpeed = MoveSpeed;
        this.rotationSpeed = RotationSpeed;
    }
}

using Unity.Entities;
using Unity.Transforms;

public partial struct GravitySystem : ISystem {
    public void OnUpdate(ref SystemState state) {
        float dt = SystemAPI.Time.DeltaTime;
        foreach (var (transform, gravity) in SystemAPI
                     .Query<RefRW<LocalTransform>, RefRW<GravityComponent>>()) {
            if (transform.ValueRO.Position.y <= 0f) {
                gravity.ValueRW.Velocity = 5f;
            }
            
            gravity.ValueRW.Velocity -= gravity.ValueRO.Weight * dt;
            transform.ValueRW.Position.y += gravity.ValueRO.Velocity * dt;
        }
    }
}

public partial struct MoveSystem : ISystem {
    public void OnUpdate(ref SystemState state) {
        foreach (var (transform, moveSpeed) in SystemAPI
                     .Query<RefRW<LocalTransform>, RefRO<MoveSpeedComponent>>()) {
            transform.ValueRW.Position +=
                transform.ValueRO.Forward() * moveSpeed.ValueRO.Value * SystemAPI.Time.DeltaTime;
        }
    }
}

public partial struct RotationSystem : ISystem {
    public void OnUpdate(ref SystemState state) {
        foreach (var (transform, rotationSpeed) in SystemAPI
                     .Query<RefRW<LocalTransform>, RefRO<RotationSpeedComponent>>()) {
            transform.ValueRW = transform.ValueRO.RotateY(rotationSpeed.ValueRO.Value * SystemAPI.Time.DeltaTime);
        }
    }
}
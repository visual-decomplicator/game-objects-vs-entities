using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

[BurstCompile]
public partial struct GravitySystem : ISystem {
    [BurstCompile]
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

[BurstCompile]
public partial struct MoveSystem : ISystem {
    
    [BurstCompile]
    public void OnUpdate(ref SystemState state) {
        foreach (var (transform, moveSpeed) in SystemAPI
                     .Query<RefRW<LocalTransform>, RefRO<MoveSpeedComponent>>()) {
            transform.ValueRW.Position +=
                transform.ValueRO.Forward() * moveSpeed.ValueRO.Value * SystemAPI.Time.DeltaTime;
        }
    }
}

[BurstCompile]
public partial struct RotationSystem : ISystem {
    
    [BurstCompile]
    public void OnUpdate(ref SystemState state) {
        foreach (var (transform, rotationSpeed) in SystemAPI
                     .Query<RefRW<LocalTransform>, RefRO<RotationSpeedComponent>>()) {
            transform.ValueRW = transform.ValueRO.RotateY(rotationSpeed.ValueRO.Value * SystemAPI.Time.DeltaTime);
        }
    }
}
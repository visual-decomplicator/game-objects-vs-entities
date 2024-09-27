using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

[BurstCompile]
public partial struct GravitySystem : ISystem {
    [BurstCompile]
    public void OnUpdate(ref SystemState state) {
        new GravityJob() {
            Dt = SystemAPI.Time.DeltaTime
        }.ScheduleParallel();
    }
}

[BurstCompile]
public partial struct GravityJob : IJobEntity {
    public float Dt;
    public void Execute(
        ref LocalTransform transform,
        ref GravityComponent gravity
        ) {
        if (transform.Position.y <= 0f) {
            gravity.Velocity = 5f;
        }
            
        gravity.Velocity -= gravity.Weight * Dt;
        transform.Position.y += gravity.Velocity * Dt;
    }
}

[BurstCompile]
public partial struct MoveSystem : ISystem {
    
    [BurstCompile]
    public void OnUpdate(ref SystemState state) {
        new MoveJob() {
            Dt = SystemAPI.Time.DeltaTime
        }.ScheduleParallel();
    }
}

[BurstCompile]
public partial struct MoveJob : IJobEntity {
    public float Dt;
    public void Execute(ref LocalTransform transform, in MoveSpeedComponent moveSpeed) {
        transform.Position += transform.Forward() * moveSpeed.Value * Dt;
    }
}

[BurstCompile]
public partial struct RotationSystem : ISystem {
    
    [BurstCompile]
    public void OnUpdate(ref SystemState state) {
        new RotationJob() { Dt = SystemAPI.Time.DeltaTime }.ScheduleParallel();
    }
}

[BurstCompile]
public partial struct RotationJob : IJobEntity {
    public float Dt;
    public void Execute(ref LocalTransform transform, in RotationSpeedComponent rotationSpeed) {
        transform = transform.RotateY(rotationSpeed.Value * Dt);
    }
}
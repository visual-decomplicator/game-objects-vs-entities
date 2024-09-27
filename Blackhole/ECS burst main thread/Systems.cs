using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
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
public partial struct GravityOrbitSetupSystem : ISystem {

    [BurstCompile]
    public void OnCreate(ref SystemState state) {
        state.RequireForUpdate<NeedInitCalcComponent>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state) {
        float3 gravityCenterPosition = new float3();
        bool gravityPresented = false;
        
        foreach (var (transform, gravityCenter) in SystemAPI.Query<LocalTransform, GravityCenterComponent>()) {
            gravityCenterPosition = transform.Position;
            gravityPresented = true;
        }

        if (!gravityPresented) {
            return;
        }

        var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged);
        foreach (var (transform, gravityOrbit, entity) in SystemAPI
                     .Query<RefRO<LocalTransform>, RefRW<OrbitGravityComponent>>()
                     .WithAll<NeedInitCalcComponent>()
                     .WithEntityAccess()
                 ) {
            gravityOrbit.ValueRW.Angle = math.atan2(transform.ValueRO.Position.z - gravityCenterPosition.z,
                transform.ValueRO.Position.x - gravityCenterPosition.x);
            gravityOrbit.ValueRW.Radius = math.distance(gravityCenterPosition, transform.ValueRO.Position);
            ecb.RemoveComponent<NeedInitCalcComponent>(entity);
        }
    }
}

[BurstCompile]
public partial struct GravityOrbitSystem : ISystem {
    [BurstCompile]
    public void OnCreate(ref SystemState state) {
        state.RequireForUpdate<GravityCenterComponent>();
    }
    
    [BurstCompile]
    public void OnUpdate(ref SystemState state) {
        float3 gravityCenterPosition = new float3();
        foreach (var (transform, gravityCenter) in SystemAPI.Query<LocalTransform, GravityCenterComponent>()) {
            gravityCenterPosition = transform.Position;
        }

        foreach (var (transform, gravityOrbit, gravity, entity) in SystemAPI
                     .Query<RefRW<LocalTransform>, RefRW<OrbitGravityComponent>, RefRO<GravityComponent>>()
                     .WithNone<NeedInitCalcComponent>()
                     .WithEntityAccess()
                 ) {
            gravityOrbit.ValueRW.Angle += gravityOrbit.ValueRO.Speed * SystemAPI.Time.DeltaTime;;
            float noiseValue = noise.cnoise(new float2((float)SystemAPI.Time.ElapsedTime * gravityOrbit.ValueRO.NoiseFrequency, (float)entity.Index));
            gravityOrbit.ValueRW.Radius += noiseValue;
            float newX = gravityCenterPosition.x + gravityOrbit.ValueRO.Radius * math.cos(gravityOrbit.ValueRO.Angle);
            float newZ = gravityCenterPosition.z + gravityOrbit.ValueRO.Radius * math.sin(gravityOrbit.ValueRO.Angle);
            transform.ValueRW.Position = new float3(newX, transform.ValueRO.Position.y, newZ);
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
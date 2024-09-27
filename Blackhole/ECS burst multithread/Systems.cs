using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
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
public partial struct GravityOrbitSetupSystem : ISystem {

    [BurstCompile]
    public void OnCreate(ref SystemState state) {
        state.RequireForUpdate<NeedInitCalcComponent>();
        state.RequireForUpdate<GravityCenterComponent>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state) {
        var gravityCenterNative = new NativeReference<float3>(state.WorldUpdateAllocator);
        var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();
        new GetGravityCenterJob() {
            GravityCenterNative = gravityCenterNative
        }.Schedule();
        new SetupOrbitalGravitySettingsJob() {
            Ecb = ecb,
            GravityCenterNative = gravityCenterNative
        }.ScheduleParallel();
    }
}

[BurstCompile]
[WithAll(typeof(NeedInitCalcComponent))]
public partial struct SetupOrbitalGravitySettingsJob : IJobEntity {
    public EntityCommandBuffer.ParallelWriter Ecb;
    [ReadOnly] public NativeReference<float3> GravityCenterNative;
    
    public void Execute(
        [ChunkIndexInQuery] int index,
        Entity entity,
        ref OrbitGravityComponent gravity,
        in LocalTransform transform
        ) {
        gravity.Angle = math.atan2(transform.Position.z - GravityCenterNative.Value.z,
            transform.Position.x - GravityCenterNative.Value.x);
        gravity.Radius = math.distance(GravityCenterNative.Value, transform.Position);
        Ecb.RemoveComponent<NeedInitCalcComponent>(index,entity);
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
        var gravityCenterNative = new NativeReference<float3>(state.WorldUpdateAllocator);
        new GetGravityCenterJob() {
            GravityCenterNative = gravityCenterNative
        }.Schedule();
        new CalcGravityOrbitStepJob() {
            Dt = SystemAPI.Time.DeltaTime,
            ElapsedTime = SystemAPI.Time.ElapsedTime,
            GravityCenterNative = gravityCenterNative
        }.ScheduleParallel();
    }
}

[BurstCompile]
[WithAll(typeof(GravityCenterComponent))]
public partial struct GetGravityCenterJob : IJobEntity {
    public NativeReference<float3> GravityCenterNative;
    public void Execute(in LocalTransform transform) {
        GravityCenterNative.Value = transform.Position;
    }
}

[BurstCompile]
[WithNone(typeof(NeedInitCalcComponent))]
public partial struct CalcGravityOrbitStepJob : IJobEntity {
    public float Dt;
    public double ElapsedTime;
    [ReadOnly] public NativeReference<float3> GravityCenterNative;
    
    public void Execute(
        Entity entity,
        ref LocalTransform transform,
        ref OrbitGravityComponent gravity
        ) {
        gravity.Angle += gravity.Speed * Dt;;
        float noiseValue = noise.cnoise(new float2((float)ElapsedTime * gravity.NoiseFrequency, (float)entity.Index));
        gravity.Radius += noiseValue;
        float newX = GravityCenterNative.Value.x + gravity.Radius * math.cos(gravity.Angle);
        float newZ = GravityCenterNative.Value.z + gravity.Radius * math.sin(gravity.Angle);
        transform.Position = new float3(newX, transform.Position.y, newZ);
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

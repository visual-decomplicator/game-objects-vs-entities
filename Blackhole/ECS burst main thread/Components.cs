using Unity.Entities;

public struct MoveSpeedComponent : IComponentData {
    public float Value;
}

public struct RotationSpeedComponent : IComponentData {
    public float Value;
}

public struct RandomComponent : IComponentData {
    public Unity.Mathematics.Random Random;
}

public struct GravityCenterComponent : IComponentData {}

public struct GravityComponent : IComponentData {
    public float Weight;
    public float Velocity;
}

public struct OrbitGravityComponent : IComponentData {
    public float Angle;
    public float Speed;
    public float Radius;
    public float NoiseFrequency;
}

public struct NeedInitCalcComponent : IComponentData {}
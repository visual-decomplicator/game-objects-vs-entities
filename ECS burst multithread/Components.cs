using Unity.Entities;

public struct GravityComponent : IComponentData {
    public float Weight;
    public float Velocity;
}

public struct MoveSpeedComponent : IComponentData {
    public float Value;
}

public struct RotationSpeedComponent : IComponentData {
    public float Value;
}
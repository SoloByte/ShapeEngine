using System.Numerics;
using Raylib_cs;
using ShapeEngine.Core.Collision;
using ShapeEngine.Core.Structs;

namespace ShapeEngine.Effects;

public struct SpawnInformation
{
    public Transform2D Transform;
    public Vector2 Velocity;
}

public interface ISpawnable
{
    public ISpawnable Create();
    public bool Spawn(SpawnInformation information);
    public bool Despawn();
    public bool IsFinished();
}

public class Particle2
{
    
}


public class ParticleSpawner<T> where T : ISpawnable
{
    public Transform2D Transform { get; set; }
    public Vector2 Velocity { get; set; } //does it move the particle spawner? or does it just use this for the spawn info?
    
    public float SpawnAngle { get; set; }
    public float SpawnAngleVariance { get; set; } = 0f;
    
    public float SpawnDistance { get; set; }
    public float SpawnDistanceVariance { get; set; } = 0f;
    
    public float SpawnRate { get; set; } //objects per second
    public float SpawnRateVariance { get; set; } = 0f;
    
    public float SpawnAmount { get; set; }
    public float SpawnAmountVariance { get; set; } = 0f;
    
    public int BurstCount { get; set; } = 0; //if bigger than 0, triggers every spawn, spawns spawn amount, repeats for count, with burst cooldown
    public float BurstCooldown { get; set; } = 0f;

    public bool RotateParticles { get; set; } = true;
    public bool ScaleParticles { get; set; } = true;

    
    public bool Active { get; private set; } = false;
    public readonly int MaxParticles;
    public readonly bool PoolParticles;
    
    private List<T>? pool = null;
    private T spawnable;

    public ParticleSpawner(T spawnable, int maxParticles = -1, bool poolParticles = true)
    {
        this.spawnable = spawnable;
        MaxParticles = maxParticles;
        PoolParticles = poolParticles;
        if (poolParticles)
        {
            pool = new List<T>(maxParticles > 0 ? maxParticles : 0);
        }
    }

    public bool Start()//start to trigger continuously, if spawn rate <= 0 calls burst
    {
        return true;
    }

    public bool Stop()
    {
        return true;
    }

    public bool Burst()//triggers once
    {
        return true;
    }

    public void Update(float dt)
    {
        
    }

    public void Close()
    {
        
    }

    public void DrawDebug()
    {
        
    }
}
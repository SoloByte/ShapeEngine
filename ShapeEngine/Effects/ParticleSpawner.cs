using System.Numerics;
using Raylib_cs;
using ShapeEngine.Core;
using ShapeEngine.Core.Collision;
using ShapeEngine.Core.Interfaces;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Core.Structs;

namespace ShapeEngine.Effects;

public struct SpawnInformation
{
    public Transform2D Transform;
    public Vector2 Velocity;
}

public interface ISpawnable
{
    public event Action<ISpawnable>? OnFinished;
    public bool Spawn(SpawnInformation information);
    public bool Despawn();
    
}

/*public class Particle2 : GameObject
{
    public override Rect GetBoundingBox()
    {
        throw new NotImplementedException();
    }

    public override void Update(GameTime time, ScreenInfo game, ScreenInfo gameUi, ScreenInfo ui)
    {
        throw new NotImplementedException();
    }

    public override void DrawGame(ScreenInfo game)
    {
        throw new NotImplementedException();
    }

    public override void DrawGameUI(ScreenInfo gameUi)
    {
        throw new NotImplementedException();
    }
}*/


public class ParticleSpawner<T> where T : ISpawnable
{
    public delegate T Create();
    
    
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
    
    private readonly Queue<T>? inUse = null;
    private readonly Queue<T>? available = null;
    private readonly Create creator;

    public ParticleSpawner(Create creator, int maxParticles = -1, bool poolParticles = true)
    {
        this.creator = creator;
        MaxParticles = maxParticles;
        PoolParticles = poolParticles;
        if (poolParticles)
        {
            inUse = new Queue<T>(maxParticles > 0 ? maxParticles : 0);
            available = new Queue<T>(maxParticles > 0 ? maxParticles : 0);
        }
    }
    
    
    public virtual void Spawn()
    {
        
    }

    protected T GetInstance()
    {
        if(available == null || inUse == null) return creator();

        if (available.Count <= 0)
        {
            var instance = creator();
            instance.OnFinished += InstanceOnOnFinished;
            inUse.Enqueue(instance);
            return instance;
        }
        else
        {
            var instance = available.Dequeue();
            inUse.Enqueue(instance);
            return instance;
        }
        
    }

    private void InstanceOnOnFinished(ISpawnable obj)
    {
        
    }

    public bool Start()//start to trigger continuously, if spawn rate <= 0 calls burst
    {
        return true;
    }

    public bool Stop()
    {
        return true;
    }

    public bool OnShot()//triggers once
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
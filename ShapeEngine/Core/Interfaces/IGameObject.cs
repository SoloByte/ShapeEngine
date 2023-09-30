using System.Numerics;
using ShapeEngine.Core.Collision;
using ShapeEngine.Core.Shapes;
namespace ShapeEngine.Core.Interfaces;

public interface IGameObject : ISpatial, IUpdateable, IDrawable, IKillable//, IBehaviorReceiver
{

    public bool DrawToGame(Rect gameArea);
    public bool DrawToUI(Rect screenArea);
        
    /// <summary>
    /// The area layer the object is stored in. Higher layers are draw on top of lower layers.
    /// </summary>
    public int Layer { get; set; }
    /// <summary>
    /// Is called by the area. Can be used to update the objects position based on the new parallax position.
    /// </summary>
    /// <param name="newParallaxPosition">The new parallax position from the layer the object is in.</param>
    public virtual void UpdateParallaxe(Vector2 newParallaxPosition) { }

    /// <summary>
    /// Check if the object is in a layer.
    /// </summary>
    /// <param name="layer"></param>
    /// <returns></returns>
    public sealed bool IsInLayer(int layer) { return this.Layer == layer; }

    /// <summary>
    /// Is called when gameobject is added to an area.
    /// </summary>
    public void AddedToHandler(GameObjectHandler gameObjectHandler);
    /// <summary>
    /// Is called by the area once a game object is dead.
    /// </summary>
    public void RemovedFromHandler(GameObjectHandler gameObjectHandler);

    /// <summary>
    /// Should this object be checked for leaving the bounds of the area?
    /// </summary>
    /// <returns></returns>
    public bool CheckHandlerBounds();
    /// <summary>
    /// Will be called if the object left the bounds of the area. The BoundingCircle is used for this check.
    /// </summary>
    /// <param name="safePosition">The closest position within the bounds.</param>
    /// <param name="collisionPoints">The points where the object left the bounds. Can be 1 or 2 max.</param>
    public void LeftHandlerBounds(Vector2 safePosition, CollisionPoints collisionPoints);
        
    ///// <summary>
    ///// Can be used to adjust the follow position of an attached camera.
    ///// </summary>
    ///// <param name="camPos"></param>
    ///// <returns></returns>
    //public Vector2 GetCameraFollowPosition(Vector2 camPos);

    /// <summary>
    /// Should the area add the collidables from this object to the collision system on area entry.
    /// </summary>
    /// <returns></returns>
    public virtual bool HasCollidables() { return false; }
    /// <summary>
    /// All the collidables that should be added to the collision system on area entry.
    /// </summary>
    /// <returns></returns>
    public virtual List<ICollidable> GetCollidables() { return new(); }


    /// <summary>
    ///  Is called right after update if a delta factor was applied to the objects dt.
    /// </summary>
    /// <param name="f">The factor that was applied.</param>
    public void DeltaFactorApplied(float f);
}
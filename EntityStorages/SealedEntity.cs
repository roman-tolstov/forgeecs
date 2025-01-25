using Unity.IL2CPP.CompilerServices;

namespace ForgeECS
{
  [Il2CppSetOption(Option.NullChecks, false)]
  [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
  [Il2CppSetOption(Option.DivideByZeroChecks, false)]
  public readonly struct SealedEntity
  {
    public Entity Unseal => _world.Entities[_id];
    private readonly World _world;
    private readonly int _id;
    private readonly int _generation;

    public SealedEntity(World world, int id, int generation)
    {
      _world = world;
      _id = id;
      _generation = generation;
    }

    public bool GetIfAlive(out Entity entity)
    {
      entity = _world.Entities[_id];
      
      return entity.IsAlive && entity.Generation == _generation;
    }
  }

  public static class Extensions
  {
    public static SealedEntity Seal(this Entity entity) =>
      new(entity.World, entity.Id, entity.Generation);
  }
}
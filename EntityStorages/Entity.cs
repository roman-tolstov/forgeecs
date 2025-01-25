using System;
using Unity.IL2CPP.CompilerServices;

namespace ForgeECS
{
  [Il2CppSetOption(Option.NullChecks, false)]
  [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
  [Il2CppSetOption(Option.DivideByZeroChecks, false)]
  public class Entity
  {
    public readonly int Id;
    public readonly World World;
    public int Generation;
    public bool IsAlive;
    public Archetype Archetype;
    public Archetype ArchetypeBeforeChanges;
    public int IndexInArchetype;

    public Entity(World world, int id)
    {
      World = world;
      Id = id;
      Archetype = null;
      Generation = 1;
      IsAlive = true;
    }

    public bool Has<TComponent>() where TComponent : struct, IEcsBaseComponent
    {
      return Archetype != null
             && Archetype.ComponentTypeIds.Contains(ComponentTypeInfo<TComponent>.Index);
    }

    public ref TComponent Get<TComponent>() where TComponent : struct, IValueComponent
    {
      return ref CacheFor<TComponent>().Get(Id);
    }

    public ref TComponent Add<TComponent>() where TComponent : struct, IValueComponent
    {
      World.NewTransaction<TComponent>(Id, true);

      var typeIndex = ComponentTypeInfo<TComponent>.Index;
      if (!World.ComponentCachesByTypeId.ContainsKey(typeIndex))
        World.ComponentCachesByTypeId.Add(typeIndex, new ComponentCache<TComponent>());

      return ref CacheFor<TComponent>().Add(Id);
    }

    public void Remove<TComponent>() where TComponent : struct, IEcsBaseComponent
    {
      World.ComponentCachesByTypeId[ComponentTypeInfo<TComponent>.Index].FreeId(Id);
      World.NewTransaction<TComponent>(Id, false);
    }

    public void Tag<TTag>() where TTag : struct, ITagComponent
    {
      World.NewTransaction<TTag>(Id, true);
    }
    
    public void Untag<TTag>() where TTag : struct, ITagComponent
    {
      World.NewTransaction<TTag>(Id, false);
    }
    
    public void RemoveFromArchetype()
    {
      ArchetypeBeforeChanges.Remove(this);
      Archetype = null;
      ArchetypeBeforeChanges = null;
    }

    public void Destroy()
    {
#if DEBUG
      if (!IsAlive)
        throw new Exception("Try to destroy an already dead entity");
#endif
  
      World.PrepareForRemove(this);
      IsAlive = false;
    }
    
    private ComponentCache<TComponent> CacheFor<TComponent>() where TComponent : struct, IValueComponent =>
      (ComponentCache<TComponent>) World.ComponentCachesByTypeId[ComponentTypeInfo<TComponent>.Index];
  }
}
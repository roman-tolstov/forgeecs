using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace ForgeECS
{
  [Il2CppSetOption(Option.NullChecks, false)]
  [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
  [Il2CppSetOption(Option.DivideByZeroChecks, false)]
  public class World
  {
    private static World _first;
    public static World First => _first ??= new World();

    private int _entitiesCount;
    public Entity[] Entities = new Entity[256];
    public readonly Dictionary<int, List<Archetype>> ArchetypesByHash = new();
    public readonly Dictionary<int, IComponentCache> ComponentCachesByTypeId = new();

    private readonly Stack<int> _freeEntityIds = new();
    private readonly List<Entity> _entitiesToBeRemoved = new();
    private readonly List<Entity> _entitiesToBeUpdated = new();
    private readonly HashSet<int> _entitiesToBeUpdatedIds = new();
    private readonly List<Action<Archetype>> _filterAssigners = new();
    private readonly Dictionary<Type, IUniqueComponent> _uniqueComponents = new();

    public World()
    {
      _first = this;
    }

    public Entity CreateEntity()
    {
      if (_freeEntityIds.Count != 0)
      {
        var freedId = _freeEntityIds.Pop();
        Entities[freedId].Generation += 1;
        Entities[freedId].IsAlive = true;

        return Entities[freedId];
      }

      if (_entitiesCount >= Entities.Length)
        Array.Resize(ref Entities, Entities.Length * 2);

      var entity = new Entity(this, _entitiesCount);
      Entities[_entitiesCount++] = entity;

      return entity;
    }

    public void DelayedEntitiesRemove()
    {
      foreach (var entity in _entitiesToBeRemoved)
      {
        var componentTypeIds = entity.Archetype.ComponentTypeIds;
        for (var i = 0; i < componentTypeIds.Count; i++)
        {
          // no cache for tag components
          if (ComponentCachesByTypeId.TryGetValue(componentTypeIds[i], out var cache))
            cache.FreeId(entity.Id);
        }

        entity.RemoveFromArchetype();
        _freeEntityIds.Push(entity.Id);
      }

      _entitiesToBeRemoved.Clear();
    }

    public void PrepareForRemove(Entity entity)
    {
      _entitiesToBeRemoved.Add(entity);
    }

    public void DelayedEntitiesUpdate()
    {
      foreach (var entity in _entitiesToBeUpdated)
      {
#if DEBUG
        if (!entity.IsAlive)
        {
          Debug.LogError("Dead at delayed update");
          continue;
        }
#endif

        if (entity.ArchetypeBeforeChanges != null)
          entity.ArchetypeBeforeChanges.Remove(entity);
        
        entity.Archetype.Add(entity);
        
        entity.ArchetypeBeforeChanges = entity.Archetype;
      }
      
      _entitiesToBeUpdated.Clear();
      _entitiesToBeUpdatedIds.Clear();
    }

    public void NewTransaction<T>(int entityId, bool added) where T : struct, IEcsBaseComponent
    {
      var entity = Entities[entityId];
      
#if DEBUG
      if (!entity.IsAlive)
      {
        Debug.LogError("Entity is not alive during transaction");
        return;
      }
#endif
      
      var typeIndex = ComponentTypeInfo<T>.Index;

#if DEBUG
      if (entity.Archetype != null)
      {
        if (entity.Archetype.ComponentTypeIds.Contains(typeIndex) == added)
          throw new Exception("Invalid transaction 1");
      }
      else
      {
        if (!added)
          throw new Exception("Invalid transaction 2");
      }
#endif

      var cachedTransactions = added 
        ? entity.Archetype?.AddTransactions 
        : entity.Archetype?.RemoveTransactions;
      
      if (cachedTransactions == null || !cachedTransactions.TryGetValue(typeIndex, out var newArchetype))
      {
        var newHash = entity.Archetype != null 
          ? entity.Archetype.ComponentTypeIds.Sum() + (added ? 1 : -1) * typeIndex 
          : typeIndex;
        
        if (ArchetypesByHash.ContainsKey(newHash))
        {
          newArchetype = GetArchetypeFrom(ArchetypesByHash[newHash], entity.Archetype, typeIndex, added);
          
          if (newArchetype == null)
          {
            newArchetype = NewArchetype(entity.Archetype, typeIndex, added);
            ArchetypesByHash[newHash].Add(newArchetype);
          }
        }
        else
        {
          newArchetype = NewArchetype(entity.Archetype, typeIndex, added);
          var archetypeByHash = new List<Archetype>();
          archetypeByHash.Add(newArchetype);
          ArchetypesByHash[newHash] = archetypeByHash;
        }

        if (entity.Archetype != null)
          (added ? entity.Archetype.AddTransactions : entity.Archetype.RemoveTransactions).Add(typeIndex, newArchetype);
      }
      
      entity.Archetype = newArchetype;

      if (_entitiesToBeUpdatedIds.Contains(entity.Id))
        return;
      
      _entitiesToBeUpdated.Add(entity);
      _entitiesToBeUpdatedIds.Add(entity.Id);
    }

    private Archetype GetArchetypeFrom(List<Archetype> archetypes, Archetype oldArchetype, int index, bool added)
    {
      if (oldArchetype != null)
      {
        for (int i = 0; i < archetypes.Count; i++)
        {
          if (Found(archetypes[i].ComponentTypeIds, oldArchetype.ComponentTypeIds, index, added))
            return archetypes[i];
        }
      }
      else
      {
        for (int i = 0; i < archetypes.Count; i++)
        {
          if (Found(archetypes[i].ComponentTypeIds, index))
            return archetypes[i];
        }
      }

      return null;
    }

    private bool Found(List<int> newComponentTypeIds, int index)
    {
      if (newComponentTypeIds.Count != 1)
        return false;

      return newComponentTypeIds[0] == index;
    }

    private bool Found(List<int> newComponentTypeIds, List<int> oldComponentTypeIds, int index, bool added)
    {
      if (newComponentTypeIds.Contains(index) != added)
        return false;
      
      for (int i = 0; i < oldComponentTypeIds.Count; i++)
      {
        if (oldComponentTypeIds[i] == index)
          continue;

        if (!newComponentTypeIds.Contains(oldComponentTypeIds[i]))
          return false;
      }

      return true;
    }

    private Archetype NewArchetype(Archetype oldArchetype, int componentTypeIndex, bool added)
    {
      var componentTypeIds = oldArchetype != null
        ? new List<int>(oldArchetype.ComponentTypeIds)
        : new List<int>();


      if (added)
        componentTypeIds.Add(componentTypeIndex);
      else
        componentTypeIds.Remove(componentTypeIndex);
      
      var newArchetype = new Archetype();
      newArchetype.ComponentTypeIds = componentTypeIds;
      AddArchetypeToFilters(newArchetype);
      
      return newArchetype;
    }
    
    public object FilterForType(Type filterType, object[] ctor)
    {
#if DEBUG
      var args = filterType.GetGenericArguments();
      if (args.Length != args.Distinct().Count())
        throw new Exception($"{args.GroupBy(x => x).First(x => x.Count() > 1).Key.Name} appears more then once in {filterType}");

      if (filterType == null) 
        throw new Exception("FilterType is null");
#endif
      
      var createdFilter = Activator.CreateInstance(
        filterType, 
        BindingFlags.Public | BindingFlags.Instance, 
        null, 
        ctor, 
        CultureInfo.InvariantCulture);
      
      _filterAssigners.Add(archetype => ((IAddArchetypeIfFits) createdFilter).AddArchetypeIfFits(archetype));
      
      return createdFilter;
    }
  
    public void Clear()
    {
      for (var i = 0; i < Entities.Length; i++)
      {
        var entity = Entities[i];
        if (entity == null)
          break;

        if (!entity.IsAlive)
          continue;
        
        entity.Destroy();
      }
      
      DelayedEntitiesUpdate();
      DelayedEntitiesRemove();
    }

    public void AddArchetypeToFilters(Archetype archetype)
    {
      foreach (var filterAssigner in _filterAssigners) 
        filterAssigner(archetype);
    }
    
    public T Get<T>() where T : struct, IUniqueComponent
    {
#if DEBUG
      if (!_uniqueComponents.TryGetValue(typeof(T), out var value))
        throw new Exception($"{typeof(T)} can't be found");

      return (T)value;
#endif

      return (T)_uniqueComponents[typeof(T)];
    }

    public World Set<T>(T value) where T : struct, IUniqueComponent
    {
      _uniqueComponents[typeof(T)] = value;

      return this;
    }

    public void Remove<T>() where T : struct, IUniqueComponent
    {
#if DEBUG
      if (!Has<T>())
        throw new Exception($"{typeof(T)} can't be removed, as there is none");
#endif

      _uniqueComponents.Remove(typeof(T));
    }

    public bool Has<T>() where T : struct, IUniqueComponent => 
      _uniqueComponents.ContainsKey(typeof(T));
  }
}
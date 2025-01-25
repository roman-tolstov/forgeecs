using System;
using System.Collections.Generic;
using Unity.IL2CPP.CompilerServices;

namespace ForgeECS
{
  
  [Il2CppSetOption(Option.NullChecks, false)]
  [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
  [Il2CppSetOption(Option.DivideByZeroChecks, false)]
  public class Archetype
  {
    public int Count;
    public List<int> EntityIds = new();
    public List<int> ComponentTypeIds;
    public Dictionary<int, Archetype> AddTransactions = new();
    public Dictionary<int, Archetype> RemoveTransactions = new();
    
    public void Add(Entity entity)
    {
#if DEBUG
      if (EntityIds.Contains(entity.Id))
      {
        throw new Exception("Try add same entity to archetype");
      }
#endif
      
      if (Count < EntityIds.Count)
      {
        entity.IndexInArchetype = Count;
        EntityIds[Count] = entity.Id;
        Count += 1;
      }
      else
      {
        entity.IndexInArchetype = Count;
        EntityIds.Add(entity.Id);
        Count = EntityIds.Count;
      }
    }

    public void Remove(Entity entity)
    {
      var index = entity.IndexInArchetype;
      var lastEntity = entity.World.Entities[EntityIds[Count - 1]];
      EntityIds[index] = -1;
      
      if (entity.Id == lastEntity.Id)
      {
        Count = index;
        entity.IndexInArchetype = -1;
        return;
      }

      var lastEntityIndex = lastEntity.IndexInArchetype;
      EntityIds[lastEntityIndex] = -1;
      EntityIds[index] = lastEntity.Id;
      lastEntity.IndexInArchetype = index;
      Count = lastEntityIndex;
    }
  }
}
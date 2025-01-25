using System;
using System.Collections.Generic;
using Unity.IL2CPP.CompilerServices;

namespace ForgeECS
{
  [Il2CppSetOption(Option.NullChecks, false)]
  [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
  [Il2CppSetOption(Option.DivideByZeroChecks, false)]
  public sealed class ComponentCache<T> : IComponentCache
    where T : struct, IValueComponent
  {
    private int[] _valueIdByEntityId = new int[16];
    private T[] _values = new T[16];
    private readonly Stack<int> _freeIds = new();
    private int _count;

    public ref T Add(int entityId)
    {
      int componentId;
      if (_freeIds.Count == 0)
      {
        componentId = _count;
        EnsureCapacity(ref _values, index: _count);
        _values[_count++] = Activator.CreateInstance<T>();
      }
      else
      {
        var item = _freeIds.Pop();
        componentId = item;
      }

      EnsureCapacity(ref _valueIdByEntityId, index: entityId);
      _valueIdByEntityId[entityId] = componentId;
      return ref _values[componentId];
    }
    
    public ref T Get(int entityId)
    {
      return ref _values[_valueIdByEntityId[entityId]];
    }

    public void FreeId(int entityId)
    {
      _freeIds.Push(_valueIdByEntityId[entityId]);
    }

    public static ComponentCache<T> From(World world)
    {
      var caches = world.ComponentCachesByTypeId;
      if (!caches.ContainsKey(ComponentTypeInfo<T>.Index))
        caches[ComponentTypeInfo<T>.Index] = new ComponentCache<T>();
      
      return (ComponentCache<T>) caches[ComponentTypeInfo<T>.Index];
    }
    
    private static void EnsureCapacity<TArray>(ref TArray[] array, int index)
    {
      if (index >= array.Length)
      {
        var newLength = array.Length * 2;

        while (index >= newLength)
          newLength *= 2;

        Array.Resize(ref array, newLength);
      }
    }
  }
}
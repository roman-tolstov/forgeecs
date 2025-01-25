// ReSharper disable UnusedTypeParameter

using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace ForgeECS
{
  [Il2CppSetOption(Option.NullChecks, false)]
  [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
  [Il2CppSetOption(Option.DivideByZeroChecks, false)]
  public static class ComponentTypeInfo<TComponent> where TComponent : struct, IEcsBaseComponent
  {
    // ReSharper disable once StaticMemberInGenericType
    public static readonly int Index;

    static ComponentTypeInfo()
    {
      Index = ++ComponentTypeCounter.Count;
      
#if DEBUG
      Debug.Log($"{Index} {typeof(TComponent).Name}");
#endif
    }
  }
}
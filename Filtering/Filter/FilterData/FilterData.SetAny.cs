using Unity.IL2CPP.CompilerServices;

namespace ForgeECS
{
  [Il2CppSetOption(Option.NullChecks, false)]
  [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
  [Il2CppSetOption(Option.DivideByZeroChecks, false)]
  public readonly partial struct FilterData
  {
    public void SetAny<T1>() where T1 : struct, IEcsBaseComponent =>
      _anyComponents.Add(ComponentTypeInfo<T1>.Index);

    public void SetAny<T1, T2>()
      where T1 : struct, IEcsBaseComponent
      where T2 : struct, IEcsBaseComponent
    {
      SetAny<T2>();
      SetAny<T1>();
    }

    public void SetAny<T1, T2, T3>()
      where T1 : struct, IEcsBaseComponent
      where T2 : struct, IEcsBaseComponent
      where T3 : struct, IEcsBaseComponent
    {
      SetAny<T3>();
      SetAny<T1, T2>();
    }

    public void SetAny<T1, T2, T3, T4>()
      where T1 : struct, IEcsBaseComponent
      where T2 : struct, IEcsBaseComponent
      where T3 : struct, IEcsBaseComponent
      where T4 : struct, IEcsBaseComponent
    {
      SetAny<T4>();
      SetAny<T1, T2, T3>();
    }

    public void SetAny<T1, T2, T3, T4, T5>()
      where T1 : struct, IEcsBaseComponent
      where T2 : struct, IEcsBaseComponent
      where T3 : struct, IEcsBaseComponent
      where T4 : struct, IEcsBaseComponent
      where T5 : struct, IEcsBaseComponent
    {
      SetAny<T5>();
      SetAny<T1, T2, T3, T4>();
    }

    public void SetAny<T1, T2, T3, T4, T5, T6>()
      where T1 : struct, IEcsBaseComponent
      where T2 : struct, IEcsBaseComponent
      where T3 : struct, IEcsBaseComponent
      where T4 : struct, IEcsBaseComponent
      where T5 : struct, IEcsBaseComponent
      where T6 : struct, IEcsBaseComponent
    {
      SetAny<T6>();
      SetAny<T1, T2, T3, T4, T5>();
    }
  }
}
using Unity.IL2CPP.CompilerServices;

namespace ForgeECS
{
  [Il2CppSetOption(Option.NullChecks, false)]
  [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
  [Il2CppSetOption(Option.DivideByZeroChecks, false)]
  public readonly partial struct FilterData
  {
    public void SetWith<T1>() where T1 : struct, IEcsBaseComponent =>
      _withComponents.Add(ComponentTypeInfo<T1>.Index);

    public void SetWith<T1, T2>()
      where T1 : struct, IEcsBaseComponent
      where T2 : struct, IEcsBaseComponent
    {
      SetWith<T2>();
      SetWith<T1>();
    }

    public void SetWith<T1, T2, T3>()
      where T1 : struct, IEcsBaseComponent
      where T2 : struct, IEcsBaseComponent
      where T3 : struct, IEcsBaseComponent
    {
      SetWith<T3>();
      SetWith<T1, T2>();
    }

    public void SetWith<T1, T2, T3, T4>()
      where T1 : struct, IEcsBaseComponent
      where T2 : struct, IEcsBaseComponent
      where T3 : struct, IEcsBaseComponent
      where T4 : struct, IEcsBaseComponent
    {
      SetWith<T4>();
      SetWith<T1, T2, T3>();
    }

    public void SetWith<T1, T2, T3, T4, T5>()
      where T1 : struct, IEcsBaseComponent
      where T2 : struct, IEcsBaseComponent
      where T3 : struct, IEcsBaseComponent
      where T4 : struct, IEcsBaseComponent
      where T5 : struct, IEcsBaseComponent
    {
      SetWith<T5>();
      SetWith<T1, T2, T3, T4>();
    }

    public void SetWith<T1, T2, T3, T4, T5, T6>()
      where T1 : struct, IEcsBaseComponent
      where T2 : struct, IEcsBaseComponent
      where T3 : struct, IEcsBaseComponent
      where T4 : struct, IEcsBaseComponent
      where T5 : struct, IEcsBaseComponent
      where T6 : struct, IEcsBaseComponent
    {
      SetWith<T6>();
      SetWith<T1, T2, T3, T4, T5>();
    }
  }
}
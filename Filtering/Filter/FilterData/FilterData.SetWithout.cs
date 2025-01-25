using Unity.IL2CPP.CompilerServices;

namespace ForgeECS
{
  [Il2CppSetOption(Option.NullChecks, false)]
  [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
  [Il2CppSetOption(Option.DivideByZeroChecks, false)]
  public readonly partial struct FilterData
  {
    public void SetWithout<T1>() where T1 : struct, IEcsBaseComponent =>
      _withoutComponents.Add(ComponentTypeInfo<T1>.Index);

    public void SetWithout<T1, T2>()
      where T1 : struct, IEcsBaseComponent
      where T2 : struct, IEcsBaseComponent
    {
      SetWithout<T2>();
      SetWithout<T1>();
    }

    public void SetWithout<T1, T2, T3>()
      where T1 : struct, IEcsBaseComponent
      where T2 : struct, IEcsBaseComponent
      where T3 : struct, IEcsBaseComponent
    {
      SetWithout<T3>();
      SetWithout<T1, T2>();
    }

    public void SetWithout<T1, T2, T3, T4>()
      where T1 : struct, IEcsBaseComponent
      where T2 : struct, IEcsBaseComponent
      where T3 : struct, IEcsBaseComponent
      where T4 : struct, IEcsBaseComponent
    {
      SetWithout<T4>();
      SetWithout<T1, T2, T3>();
    }

    public void SetWithout<T1, T2, T3, T4, T5>()
      where T1 : struct, IEcsBaseComponent
      where T2 : struct, IEcsBaseComponent
      where T3 : struct, IEcsBaseComponent
      where T4 : struct, IEcsBaseComponent
      where T5 : struct, IEcsBaseComponent
    {
      SetWithout<T5>();
      SetWithout<T1, T2, T3, T4>();
    }

    public void SetWithout<T1, T2, T3, T4, T5, T6>()
      where T1 : struct, IEcsBaseComponent
      where T2 : struct, IEcsBaseComponent
      where T3 : struct, IEcsBaseComponent
      where T4 : struct, IEcsBaseComponent
      where T5 : struct, IEcsBaseComponent
      where T6 : struct, IEcsBaseComponent
    {
      SetWithout<T6>();
      SetWithout<T1, T2, T3, T4, T5>();
    }
  }
}
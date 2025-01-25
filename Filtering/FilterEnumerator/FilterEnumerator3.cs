using System.Collections.Generic;

namespace ForgeECS
{
  public struct FilterEnumerator<T1, T2, T3>
    where T1 : struct, IValueComponent
    where T2 : struct, IValueComponent
    where T3 : struct, IValueComponent
  {
    private FilterEnumeratorData _enumeratorData;
    private readonly ComponentCache<T1> _cache1;
    private readonly ComponentCache<T2> _cache2;
    private readonly ComponentCache<T3> _cache3;
    private readonly World _world;

    public FilterEnumerator(World world, List<Archetype> archetypes, ComponentCache<T1> cache1, ComponentCache<T2> cache2, ComponentCache<T3> cache3)
    {
      _enumeratorData = new FilterEnumeratorData(archetypes);
      _world = world;
      _cache1 = cache1;
      _cache2 = cache2;
      _cache3 = cache3;
    }

    public bool MoveNext() => _enumeratorData.MoveNext();
    public Iteration<T1, T2, T3> Current => new(_enumeratorData.EntityId(), _world, _cache1,_cache2, _cache3);
  }
}
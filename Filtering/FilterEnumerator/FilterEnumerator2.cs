using System.Collections.Generic;

namespace ForgeECS
{
  public struct FilterEnumerator<T1, T2>
    where T1 : struct, IValueComponent
    where T2 : struct, IValueComponent
  {
    private FilterEnumeratorData _enumeratorData;
    private readonly ComponentCache<T1> _cache1;
    private readonly ComponentCache<T2> _cache2;
    private readonly World _world;

    public FilterEnumerator(World world, List<Archetype> archetypes, ComponentCache<T1> cache1, ComponentCache<T2> cache2)
    {
      _enumeratorData = new FilterEnumeratorData(archetypes);
      _world = world;
      _cache1 = cache1;
      _cache2 = cache2;
    }

    public bool MoveNext() => _enumeratorData.MoveNext();
    public Iteration<T1, T2> Current => new(_enumeratorData.EntityId(), _world, _cache1,_cache2);
  }
}
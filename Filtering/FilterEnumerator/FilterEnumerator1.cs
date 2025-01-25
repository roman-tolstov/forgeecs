using System.Collections.Generic;

namespace ForgeECS
{
  public struct FilterEnumerator<T1>
    where T1 : struct, IValueComponent
  {
    private FilterEnumeratorData _enumeratorData;
    private readonly ComponentCache<T1> _cache1;
    private readonly World _world;

    public FilterEnumerator(World world, List<Archetype> archetypes, ComponentCache<T1> cache1)
    {
      _enumeratorData = new FilterEnumeratorData(archetypes);
      _world = world;
      _cache1 = cache1;
    }

    public bool MoveNext() => _enumeratorData.MoveNext();
    public Iteration<T1> Current => new(_enumeratorData.EntityId(), _world, _cache1);
  }
}
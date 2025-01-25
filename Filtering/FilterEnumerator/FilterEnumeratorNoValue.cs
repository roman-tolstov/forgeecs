using System.Collections.Generic;

namespace ForgeECS
{
  public struct FilterEnumeratorNoValue
  {
    private FilterEnumeratorData _enumeratorData;
    private readonly World _world;

    public FilterEnumeratorNoValue(World world, List<Archetype> archetypes)
    {
      _enumeratorData = new FilterEnumeratorData(archetypes);
      _world = world;
    }

    public bool MoveNext() => _enumeratorData.MoveNext();
    public IterationNoValue Current => new(_enumeratorData.EntityId(), _world);
  }
}
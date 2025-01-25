using System.Collections.Generic;

namespace ForgeECS
{
  public struct FilterEnumeratorData
  {
    private readonly List<Archetype> _archetypes;
    private int _indexInArchetype;
    private int _archetypeIndex;

    public FilterEnumeratorData(List<Archetype> archetypes)
    {
      _archetypeIndex = archetypes.Count - 1;
      _indexInArchetype = _archetypeIndex >= 0 ? archetypes[_archetypeIndex].Count : 0;
      _archetypes = archetypes;
    }

    public int EntityId() => 
      _archetypes[_archetypeIndex].EntityIds[_indexInArchetype];

    public bool MoveNext()
    {
      if (_archetypes.Count == 0)
        return false;

      if (--_indexInArchetype >= 0)
        return true;

      do
      {
        _archetypeIndex -= 1;
      } while (_archetypeIndex >= 0
               && _archetypes[_archetypeIndex].Count == 0);
      
      if (_archetypeIndex >= 0)
        _indexInArchetype = _archetypes[_archetypeIndex].Count - 1;
      
      return _archetypeIndex >= 0;
    }
  }
}
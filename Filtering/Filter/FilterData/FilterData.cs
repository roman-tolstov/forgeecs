using System.Collections.Generic;
using Unity.IL2CPP.CompilerServices;

namespace ForgeECS
{
  [Il2CppSetOption(Option.NullChecks, false)]
  [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
  [Il2CppSetOption(Option.DivideByZeroChecks, false)]
  public readonly partial struct FilterData : IAddArchetypeIfFits
  {
    public readonly World World;
    public readonly List<Archetype> Archetypes;
    private readonly List<int> _withoutComponents;
    private readonly List<int> _withComponents;
    private readonly List<int> _anyComponents;

    public FilterData(World world)
    {
      World = world;
      _anyComponents = new List<int>();
      _withComponents = new List<int>();
      _withoutComponents = new List<int>();
      Archetypes = new List<Archetype>();
    }
    
    public void AddArchetypeIfFits(Archetype archetype)
    {
      if (Fits(archetype))
        Archetypes.Add(archetype);
    }

    private bool Fits(Archetype archetype)
    {
      for (int i = 0; i < _withComponents.Count; i++)
      {
        if (!archetype.ComponentTypeIds.Contains(_withComponents[i]))
          return false;
      }

      for (int i = 0; i < _withoutComponents.Count; i++)
      {
        if (archetype.ComponentTypeIds.Contains(_withoutComponents[i]))
          return false;
      }
      
      for (int i = 0; i < _anyComponents.Count; i++)
      {
        if (archetype.ComponentTypeIds.Contains(_anyComponents[i]))
          return true;
      }

      return _anyComponents.Count == 0;;
    }
  }

  public interface IAddArchetypeIfFits
  {
    void AddArchetypeIfFits(Archetype archetype);
  }
}
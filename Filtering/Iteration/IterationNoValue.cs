namespace ForgeECS
{
  public readonly ref struct IterationNoValue
  {
    private readonly World _world;
    private readonly int _entityId;
    public Entity Entity => _world.Entities[_entityId];

    public IterationNoValue(int entityId, World world)
    {
      _entityId = entityId;
      _world = world;
    }
  
    public ref TComponent Add<TComponent>() where TComponent : struct, IValueComponent
    {
      _world.NewTransaction<TComponent>(_entityId, true);

      var typeIndex = ComponentTypeInfo<TComponent>.Index;
      if (!_world.ComponentCachesByTypeId.ContainsKey(typeIndex))
        _world.ComponentCachesByTypeId[typeIndex] = new ComponentCache<TComponent>();
      
      return ref ((ComponentCache<TComponent>) _world.ComponentCachesByTypeId[ComponentTypeInfo<TComponent>.Index])
        .Add(_entityId);
    }

    public void Remove<TComponent>() where TComponent : struct, IValueComponent
    {
      _world.ComponentCachesByTypeId[ComponentTypeInfo<TComponent>.Index].FreeId(_entityId);
      _world.NewTransaction<TComponent>(_entityId, false);
    }
    
    public void Tag<TTag>() where TTag : struct, ITagComponent
    {
      _world.NewTransaction<TTag>(_entityId, true);
    }
    
    public void Untag<TTag>() where TTag : struct, ITagComponent
    {
      _world.NewTransaction<TTag>(_entityId, false);
    }
  }
}
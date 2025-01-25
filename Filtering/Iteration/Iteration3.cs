namespace ForgeECS
{
  public readonly ref struct Iteration<T1, T2, T3>
    where T1 : struct, IValueComponent
    where T2 : struct, IValueComponent
    where T3 : struct, IValueComponent
  {
    private readonly World _world;
    private readonly int _entityId;
    private readonly ComponentCache<T1> _cache1;
    private readonly ComponentCache<T2> _cache2;
    private readonly ComponentCache<T3> _cache3;

    public ref T1 C1 => ref _cache1.Get(_entityId);
    public ref T2 C2 => ref _cache2.Get(_entityId);
    public ref T3 C3 => ref _cache3.Get(_entityId);
    public Entity Entity => _world.Entities[_entityId];

    public Iteration(int entityId, World world, ComponentCache<T1> cache1, ComponentCache<T2> cache2, ComponentCache<T3> cache3)
    {
      _entityId = entityId;
      _world = world;
      _cache1 = cache1;
      _cache2 = cache2;
      _cache3 = cache3;
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

    public void Remove<TComponent>() where TComponent : struct, IEcsBaseComponent
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
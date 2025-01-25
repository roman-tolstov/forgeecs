using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.IL2CPP.CompilerServices;

namespace ForgeECS
{
  [Il2CppSetOption(Option.NullChecks, false)]
  [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
  [Il2CppSetOption(Option.DivideByZeroChecks, false)]
  public class SystemRunner
  {
    public readonly World World;
    private readonly List<IUpdateSystem> _loopingSystems = new();
    private readonly Dictionary<Type, object> _injections = new(32);

    public static SystemRunner For(World world)
    {
      return new SystemRunner(world);
    }

    private SystemRunner(World world)
    {
      World = world;
    }

    public SystemRunner Add<T>() where T : class, IUpdateSystem
    {
      var system = Activator.CreateInstance<T>();
      _loopingSystems.Add(system);
      return this;
    }

    public SystemRunner Add<T>(T system) where T : class, IUpdateSystem
    {
      _loopingSystems.Add(system);
      return this;
    }
 
    public void Update()
    {
      foreach (var loopingSystem in _loopingSystems)
      {
        World.DelayedEntitiesUpdate();
        World.DelayedEntitiesRemove();
        loopingSystem.Update();
      }
      
      World.DelayedEntitiesUpdate();
      World.DelayedEntitiesRemove();
    }

    public SystemRunner Inject<T>(T value)
    {
#if DEBUG
      if (_injections.ContainsKey(typeof(T)))
        throw new Exception($"Previously injected {nameof(T)} is overriden");
#endif
      
      _injections[typeof(T)] = value;
      return this;
    }

    public SystemRunner Init()
    {
      ProceedInjections();

      foreach (var system in _loopingSystems)
        if (system is IAwakeSystem awakeSystem)
          awakeSystem.Awake();

      return this;
    }

    private void ProceedInjections()
    {
      var cachedCtor = new object[] { World };
      
      foreach (var system in _loopingSystems)
      {
        foreach (var field in FieldsFrom(system))
        {
          if (StaticOrHasIgnoreAttribute(field))
            continue;
          if (WorldInjected(field, system))
            continue;
          if (FilterInjected(field, system, cachedCtor))
            continue;
          
          TryInjectCustom(field, system);
        }
      }
    }

    private bool WorldInjected(FieldInfo field, IUpdateSystem system)
    {
      var check = field.FieldType.IsInstanceOfType(World);
      if (check)
        field.SetValue(system, World);

      return check;
    }
    
    private bool FilterInjected(FieldInfo field, IUpdateSystem system, object[] ctor)
    {
      var check = typeof(IAddArchetypeIfFits).IsAssignableFrom(field.FieldType);
      if (check)
        field.SetValue(system, World.FilterForType(field.FieldType, ctor));

      return check;
    }

    private static bool StaticOrHasIgnoreAttribute(FieldInfo field) =>
      field.IsStatic || Attribute.IsDefined(field, typeof(IgnoreSystemInjectionAttribute));

    private void TryInjectCustom(FieldInfo field, IUpdateSystem system)
    {
      if (!Attribute.IsDefined(field, typeof(InjectOnSystemAwakeAttribute)))
        return;
      
      foreach (var injection in _injections)
      {
        if (field.FieldType.IsAssignableFrom(injection.Key))
        {
          field.SetValue(system, injection.Value);
          break;
        }

#if DEBUG
        UnityEngine.Debug.LogError("No injection type instance was found for " + field.Name);
#endif
      }
    }

    private static IEnumerable<FieldInfo> FieldsFrom(IUpdateSystem system) =>
      system
        .GetType()
        .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
  }
}
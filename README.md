# ForgeECS

<b>Open-source Unity/.Net Entity Component System framework focused on game development</b> 🕹️
* No setup ✔️
* Easy to use ✔️
* Struct components ✔️ 
* Minimalistic syntax ✔️
* No boilerplate code ✔️
* No code generation  ✔️
* Foolproof component getter ✔️

## Motivation
The framework was created to fulfil the wishes for a no-setup solution, that would have a rather minimalistic look without code generation, while providing a reliable component getter and decent perfomance.
 
## Overview
### 🌍 World

is an ECS instance
```csharp
var world = new World();
```
### 🗃️ Entity
is a component storage, ECS's workhorse
```csharp
var entity = world.CreateEntity();

entity.Add<CHealth>().Health = 10;
entity.Add<CSpeed>().Speed = 32f;
entity.Tag<CIsFlying>();
...
entity.Get<CHealth>.Health -= 2;
entity.Remove<CSpeed>();
entity.Untag<CIsFlying>();
```

### 📦 SealedEntity

Since an entity might be reused with a new index after being "destroyed", it's better to use sealed entity version as a long-term variable in non-system context (e.g. Monobehaviour scripts) and get entity itself by GetIfAlive() before any operations
```csharp
var sealedEntity = entity.Seal();
...
if (sealedEntity.GetIfAlive(out var entity))
{
  entity.Untag<CIsFlying>();
  entity.Tag<CIsWalking>();
  ...
```
or just ```sealedEntity.Unseal```, which is slighlty less safe



### 🛅 Component
is a unit of data, defines entity state
```csharp
struct CIsFlying : IEcsTagComponent { }

struct CHealth : IEcsValueComponent
{
  public int Health;
}
```

### 🎛️ Filter
is an entity queue, that presents entities choosen by several possible component criterias, which can look like this
```csharp
Entities.With<CValueComponent1, CValueComponent2, CValueComponent3> // prefix C is not necessary
  .Where<CTagComponent1, CTagComponent2>
  .WithAny<CAnyComponent1, CAnyComponent2, CAnyComponent3, CAnyComponent4>
  .Without<CAnyComponent1> _entities;
```
There's no need to initialize those by hand, since it's initialized at system initialization phase by injection.
Filter is the main way to present entities to systems.

### ⚙️ System
is a logic container with 2 basic interfaces - IAwakeSystem and IUpdateSystem

A simple system may be quite compact
```csharp
public class MoveForwardSystem : IUpdateSystem
{
  Entities.With<CSpeed, CTransform>.Where<CIsMovingForward> _units;  // initialization is handled by injection before system Awake call

  public void Update()
  {
    foreach (var unit in _units)
      unit.C2.Transform.Translate(Vector3.Forward * unit.C1.Speed * Time.deltaTime);  // C1 and C2 come from the components mentioned in With block, while also preventing possible ref break due to a storage displacement
  } 
}
```
or
```csharp
public class MoveForwardSystem : IUpdateSystem
{
  Entities.With<CSpeed, CTransform>.Where<CIsMovingForward> _units;  // initialization is handled by injection before system Awake call

  public void Update()
  {
    foreach (var unit in _units)
    {
      ref var transform = ref var unit.C2.Transform; 
      transform.Translate(Vector3.Forward * unit.C1.Speed * Time.deltaTime);  // low chance of breaking the ref if same type component is added to other entity between ref's assigment and usage
    }
  } 
}
```

While more complex systems might look like this. Though this is more like a feature demonstation, systems are rarely that bloated with attributes and stuff
```csharp
public class CollidableMoveForwardSystem : IAwakeSystem, IUpdateSystem
{
  private Entities.With<CPosition>.Where<CIsStone, CIsOnWay> _stones;
  private Entities.With<CSpeed, CTransform, CFuel>
    .Where<CIsActive, CIsAlive>
    .WithAny<CIsFlyingLow, CIsDriving>
    .Without<CGhostTimer, CIsHeavy> _units;

  [IgnoreSystemInjection]
  private World _uiWorld; // world-type field is injected by default if without the attribute

  [InjectBySystem]
  private CollisionSettings _collisionSettings;
  [InjectBySystem]
  private UiSettings _uiSettings;

  public void Awake()
  {
    _uiWorld = _uiSettings.UiWorld;
  }

  public void Update()
  {
    foreach (var unit in _units)
    {
      if (unit.C3.Fuel <= 0)
        continue;

      foreach (var stone in _stones)
      {
        if (_collisionSettings.HitHappened(unit.C2.Transform.position, stone.C1.Position))
        {
          _uiWorld.CreateEntity().Add<CShowMessage>().ShowMessage = "Hit a stone";
          unit.C3.Speed *= 0.5;
        }
      }
      
      movingVehicle.C4.Transform.Translate(Vector3.forward * movingVehicle.C3.Speed * Time.deltaTime);
    }
  }
}
```
### 🛵 SystemRunner
is a cycling system updater, that also handles system startup injections
```csharp
SystemRunner systemRunner = SystemRunner
        .For(new World())
        .Add(new SpawnUnitSystem()) //both are valid
        .Add<GoToPositionSystem>()  //both are valid
        .Inject<SettingsTypeExample>(settings)
        .Init();
```
After setup, its Update() method should be called continuously. For Unity engine it may be done from any Update/FixedUpdate/LateUpdate

### 🦟 System data injection
is performed by reflection at system creation/startup

* by default for entity queue such as Entities.With and also World, unless the field has IgnoreSystemInjectionAttribute

* for custom type fields with InjectBySystem attribute

## Demo
[Simple tank game](https://github.com/roman-tolstov/ForgeEcsDemo)
# Contact info

✉️ tolstovgamedev@gmail.com

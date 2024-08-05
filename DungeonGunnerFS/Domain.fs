namespace DungeonGunnerFS

open System.Collections
open UnityEngine
open UnityEngine.UI
open System.Collections.Generic

[<AllowNullLiteral>]
type IFSBehavior =
    abstract GameObject: GameObject with get
    abstract Transform: Transform with get

[<AllowNullLiteral>]
type FSBehavior() = 
    inherit MonoBehaviour()
    // since the C# == operator override to compare if a gameObject is null
    // is the way to check if the object is destroyed or null, as it can
    // be destroyed but not yet be null, this == operator override doesn't
    // work in F# for some reason, and even if you make a C# library that
    // all you have it do is you pass an object to a C# function and have it
    // check if it is null it still didn't work (perhaps it needs a certain library
    // to be in the program for it to bring in the == operator overload in the C#
    // project I'm not sure, possibly something along those lines maybe). But
    // as a workaround, I just added a 'isDestroyed' private bool that is false
    // at first, but then when OnDestroy is called it then is set to true, and
    // then the this.IsDestroyed getter only property (since another object shouldn't
    // be able to set the internal 'isDestroyed' field directly itself) will say
    // if the object is destroyed (if this object had been made null, then this property
    // will cause a null reference error, hence the Utilities.fs function where you
    // pass it an FSBehavior and it then does the null check first, and if not null
    // then also checks the 'IsDestroyed' property to tell you if it is destroyed or not
    let mutable isDestroyed : bool = false
    member this.IsDestroyed
        with get() = isDestroyed        
    member this.OnDestroy() =
        isDestroyed <- true        
    interface IFSBehavior with
        member this.GameObject
            with get() = this.gameObject
        member this.Transform 
            with get() = this.transform
            
type ISideEffects =
    abstract member ColliderGetComponent<'T when 'T:null> : Collider2D -> 'T option
    abstract member GameObjectInstantiate : GameObject -> Vector3 -> Quaternion -> Object
    abstract member GameObjectDestroy : GameObject -> unit
    abstract member GameObjectSetActive : GameObject -> bool -> unit
        
    
type SideEffects() =
    interface ISideEffects with
        member this.ColliderGetComponent<'T when 'T:null> collider =
            let collidedComponent = collider.GetComponent<'T>()
            match collidedComponent with
            | null -> None
            | comp -> Some(comp)
        member this.GameObjectInstantiate (gameObject : GameObject) (position : Vector3) (rotation : Quaternion) =
            GameObject.Instantiate(gameObject, position, rotation)
        member this.GameObjectDestroy (gameObject : GameObject) =
            GameObject.Destroy(gameObject)
        member this.GameObjectSetActive (gameObject : GameObject) isActive =
            gameObject.SetActive(isActive)

module Tags =
    let Player = "Player"
    let PlayerBullet = "PlayerBullet"
    let Enemy = "Enemy"

[<System.Serializable>]
[<AbstractClass>]
[<AllowNullLiteral>]
type GunType() =
    inherit FSBehavior()
    abstract GunUI : Sprite with get
    abstract WeaponName : string with get
    abstract ItemCost : int with get
    abstract GunShopSprite : Sprite with get
    
[<System.Serializable>]
[<AbstractClass>]
type GunPickupType() =
    inherit FSBehavior()
    
[<AllowNullLiteral>]
type ICharacterTracker =
    inherit IFSBehavior
    abstract CurrentHealth : int with get, set
    abstract MaxHealth : int with get, set
    abstract CurrentCoins : int with get, set
    
[<AllowNullLiteral>]
type IEnemy =
    inherit IFSBehavior
    abstract member TakeDamage : int -> unit
    
[<AllowNullLiteral>]
type IPlayerHealth =
    inherit IFSBehavior
    abstract member TakeDamage : unit -> unit
    abstract member HealPlayer : int -> unit
    abstract member IncreaseMaxHealth : int -> unit
    abstract member MaxHealth : int with get
    abstract member CurrentHealth : int with get

[<AllowNullLiteral>]
type IPlayer =
    inherit IFSBehavior
    abstract DashCounter : float32 with get
    abstract member GetBodySRColor : unit -> Color
    abstract member SetBodySRColor : Color -> unit
    abstract CanMove : bool with get, set
    abstract PickupGun : GunType -> bool
    abstract member CurrentGun : GunType with get
    
[<AllowNullLiteral>]
type IUI =
    inherit IFSBehavior
    abstract HealthSlider: Slider with get
    abstract HealthText : Text with get
    abstract CoinText : Text with get
    abstract DeathScreen : GameObject with get
    abstract PauseScreen : GameObject with get
    abstract MapDisplay : GameObject with get
    abstract BigMapText : GameObject with get
    abstract CurrentGun : Image with get
    abstract GunText : Text with get
    abstract FadeToBlack : unit -> unit

type SfxEnum = BoxBreaking = 0 | EnemyDeath = 1 | EnemyHurt = 2 | Explosion = 3 | Impact = 4 | PickupCoin = 5 |
                PickupGun = 6 | PickupHealth = 7 | PlayerDash = 8 | PlayerDeath = 9 | PlayerDie = 10 |
                PlayerHurt = 11 | Shoot1 = 12 | Shoot2 = 13 | Shoot3 = 14 | Shoot4 = 15 | Shoot5 = 16 | Shoot6 = 17 |
                ShopBuy = 18 | ShopNotEnough = 19 | WarpOut = 20
     
[<AllowNullLiteral>]
type IAudioManager =
    inherit IFSBehavior
    abstract PlayGameOver : unit -> unit
    abstract PlayLevelWin : unit -> unit
    abstract PlaySfx : SfxEnum -> unit
    
[<AllowNullLiteral>]
type ICamera =
    inherit IFSBehavior    
    abstract ChangeTarget : Transform -> unit
    abstract MainCamera : Camera with get
    
[<AllowNullLiteral>]
type ILevelManager =
    inherit IFSBehavior
    abstract LevelEnd : unit -> IEnumerator
    abstract PauseUnpause : unit -> unit
    abstract GetCoins : int -> unit
    abstract SpendCoins : int -> unit
    abstract IsPaused : bool with get
    abstract CurrentCoins : int with get

[<AllowNullLiteral>]    
type IRoom =
    inherit IFSBehavior
    abstract RoomActive : bool with get, set
    abstract CloseWhenEntered : bool with get, set
    abstract OpenDoors : unit -> unit
    
[<AbstractClass; Sealed>]     
type SingletonAccessor private () =
    static member val public ICharacterTracker : ICharacterTracker = null with get, set
    static member val public IPlayer : IPlayer = null with get, set
    static member val public IEnemy : IEnemy = null with get, set
    static member val public IPlayerHealth : IPlayerHealth = null with get, set
    static member val public IUI : IUI = null with get, set
    static member val public IAudioManager : IAudioManager = null with get, set
    static member val public ICamera : ICamera = null with get, set
    static member val public ILevelManager : ILevelManager = null with get, set
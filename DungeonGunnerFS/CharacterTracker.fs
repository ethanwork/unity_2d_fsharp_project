namespace DungeonGunnerFS

open UnityEngine

type CharacterTracker() =
    inherit FSBehavior()
    
    [<DefaultValue>] val mutable currentHealth : int
    [<DefaultValue>] val mutable maxHealth : int
    [<DefaultValue>] val mutable currentCoins : int
    
    member this.Awake() =
        SingletonAccessor.ICharacterTracker <- this
        
    interface ICharacterTracker with
        member this.CurrentHealth
            with get() = this.currentHealth
            and set(value) = this.currentHealth <- value
        member this.MaxHealth
            with get() = this.maxHealth
            and set(value) = this.maxHealth <- value
        member this.CurrentCoins
            with get() = this.currentCoins
            and set(value) = this.currentCoins <- value
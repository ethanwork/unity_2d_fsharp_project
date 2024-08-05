namespace DungeonGunnerFS

open UnityEngine
open DungeonGunnerFS


type DamagePlayerData = {
    IPlayerHealth : IPlayerHealth
}

module DamagePlayerModule =
    let Initialize playerHealth =
        { IPlayerHealth = playerHealth }
        
    let OnTrigger2D data (other : Collider2D) =
        if other.tag = Tags.Player then
            data.IPlayerHealth.TakeDamage()
    let OnCollision2D data (other : Collision2D) =
        if other.gameObject.tag = Tags.Player then
            data.IPlayerHealth.TakeDamage()
    

open DamagePlayerModule
type DamagePlayerFs() =
    inherit FSBehavior()
    
    [<DefaultValue>] val mutable private data : DamagePlayerData
    
    member this.Start() =
        this.data <- Initialize SingletonAccessor.IPlayerHealth     

    member this.OnTriggerEnter2D(other : Collider2D) =
        OnTrigger2D this.data other
        
    member this.OnTriggerStay2D(other : Collider2D) =
        OnTrigger2D this.data other
        
    member this.OnCollisionEnter2D(other : Collision2D) =
        OnCollision2D this.data other
        
    member this.OnCollisionStay2D(other : Collision2D) =
        OnCollision2D this.data other
namespace DungeonGunnerFS

open UnityEngine
open DungeonGunnerFS

type HealthPickupFs() =
    inherit FSBehavior()    
    [<DefaultValue>] val mutable healAmount : int
    let mutable timeTillPickup = 0.5f
    
    member this.Update() =
        timeTillPickup <-
            if timeTillPickup > 0.0f then
                timeTillPickup - Time.deltaTime
            else
                timeTillPickup
    
    member this.OnTriggerEnter2D(other : Collider2D) =
        if other.tag = Tags.Player && timeTillPickup <= 0.0f then
            SingletonAccessor.IPlayerHealth.HealPlayer this.healAmount
            SingletonAccessor.IAudioManager.PlaySfx SfxEnum.PickupHealth
            GameObject.Destroy this.gameObject
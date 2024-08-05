namespace DungeonGunnerFS

open UnityEngine

type GunPickup() =
    inherit GunPickupType()        
    [<DefaultValue>] val mutable gun : GunType
    let mutable timeTillPickup = 0.5f
    
    member this.Update() =
        timeTillPickup <-
            if timeTillPickup > 0.0f then
                timeTillPickup - Time.deltaTime
            else
                timeTillPickup
    
    member this.OnTriggerEnter2D(other : Collider2D) =
        if other.tag = Tags.Player && timeTillPickup <= 0.0f then
            let gunWasPickedUp = SingletonAccessor.IPlayer.PickupGun this.gun
            if gunWasPickedUp then                
                GameObject.Destroy this.gameObject
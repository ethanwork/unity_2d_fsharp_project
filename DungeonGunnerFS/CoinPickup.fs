namespace DungeonGunnerFS

open UnityEngine

[<AllowNullLiteral>]
type CoinPickup() =
    inherit FSBehavior()
    
    //member val coinValue = 1 with get,set
    [<DefaultValue>] val mutable coinValue : int
    [<DefaultValue>] val mutable timeTillPickup : float32
    
    member this.Update() =
        this.timeTillPickup <-
            if this.timeTillPickup > 0.0f then
                this.timeTillPickup - Time.deltaTime
            else
                this.timeTillPickup
    
    member this.OnTriggerEnter2D(other : Collider2D) =
        if other.tag = Tags.Player && this.timeTillPickup <= 0.0f then
            SingletonAccessor.ILevelManager.GetCoins this.coinValue
            SingletonAccessor.IAudioManager.PlaySfx SfxEnum.PickupCoin
            GameObject.Destroy this.gameObject
        
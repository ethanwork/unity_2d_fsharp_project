namespace DungeonGunnerFS

open UnityEngine

[<System.Serializable>]
type Gun() =
    inherit GunType()
    [<DefaultValue>] val mutable bulletToFire : GameObject
    [<DefaultValue>] val mutable firePoint : Transform
    [<DefaultValue>] val mutable timeBetweenShots : float32
    [<DefaultValue>] val mutable weaponName : string
    [<DefaultValue>] val mutable gunUI : Sprite
    [<DefaultValue>] val mutable itemCost : int
    [<DefaultValue>] val mutable gunShopSprite : Sprite
    let mutable bulletCountdownTimer : float32 = 0.0f
    
    member this.Start() =
        bulletCountdownTimer <- this.timeBetweenShots
    
    member this.Update() =
        let fireBullet() =
            SingletonAccessor.IAudioManager.PlaySfx SfxEnum.Shoot1
            GameObject.Instantiate(this.bulletToFire, this.firePoint.position, this.firePoint.rotation) |> ignore
        let isFireKeyPressed = Input.GetKeyDown(KeyCode.Space)
        let isFireKeyHeldDown = Input.GetKey(KeyCode.Space)
        if SingletonAccessor.IPlayer.CanMove && not SingletonAccessor.ILevelManager.IsPaused then   
            bulletCountdownTimer <-
                if bulletCountdownTimer > 0.0f then
                    bulletCountdownTimer - Time.deltaTime
                elif isFireKeyPressed || isFireKeyHeldDown then
                        fireBullet()
                        this.timeBetweenShots
                else
                    bulletCountdownTimer
                    
    override this.GunUI
        with get() = this.gunUI
    override this.WeaponName
        with get() = this.weaponName
    override this.ItemCost
        with get() = this.itemCost
    override this.GunShopSprite
        with get() = this.gunShopSprite    
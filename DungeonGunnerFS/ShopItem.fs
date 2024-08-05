namespace DungeonGunnerFS

open UnityEngine
open UnityEngine.UI

type ShopItem() =
    inherit FSBehavior()
    
    [<DefaultValue>] val mutable buyMessage : GameObject
    [<DefaultValue>] val mutable isHealthRestore : bool
    [<DefaultValue>] val mutable isHealthUpgrade : bool
    [<DefaultValue>] val mutable healthUpgradeAmount : int
    [<DefaultValue>] val mutable isWeapon : bool
    [<DefaultValue>] val mutable itemCost : int
    [<DefaultValue>] val mutable potentialGuns : GunType[]
    [<DefaultValue>] val mutable gunSprite : SpriteRenderer
    [<DefaultValue>] val mutable gunText : Text
    let mutable theGun : GunType = null
    let mutable inBuyZone : bool = false
    
    member this.Start() =
        if this.isWeapon then
            let selectedGun = Random.Range(0, this.potentialGuns.Length)
            theGun <- this.potentialGuns.[selectedGun]
            this.gunSprite.sprite <- theGun.GunShopSprite
            this.gunText.text <- sprintf "%s\n- %i Gold -" theGun.WeaponName theGun.ItemCost
            this.itemCost <- theGun.ItemCost
    
    member this.Update() =
        if inBuyZone then
            if Input.GetKeyDown(KeyCode.U) then
                if SingletonAccessor.ILevelManager.CurrentCoins >= this.itemCost then
                    SingletonAccessor.ILevelManager.SpendCoins this.itemCost
                    if this.isHealthRestore then
                        SingletonAccessor.IPlayerHealth.HealPlayer SingletonAccessor.IPlayerHealth.MaxHealth
                    if this.isHealthUpgrade then
                        SingletonAccessor.IPlayerHealth.IncreaseMaxHealth this.healthUpgradeAmount
                    if this.isWeapon then
                        SingletonAccessor.IPlayer.PickupGun theGun |> ignore
                    
                    this.gameObject.SetActive(false)
                    inBuyZone <- false
                    SingletonAccessor.IAudioManager.PlaySfx SfxEnum.ShopBuy
                else
                    SingletonAccessor.IAudioManager.PlaySfx SfxEnum.ShopNotEnough
                
    member this.OnTriggerEnter2D(other : Collider2D) =
        if other.tag = Tags.Player then
            this.buyMessage.SetActive(true)
            inBuyZone <- true
            
    member this.OnTriggerExit2D(other : Collider2D) =
        if other.tag = Tags.Player then
            this.buyMessage.SetActive(false)
            inBuyZone <- false
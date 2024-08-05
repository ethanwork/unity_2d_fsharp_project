namespace DungeonGunnerFS

open System
open UnityEngine
open DungeonGunnerFS

type PlayerHealthData = {
    SE : ISideEffects
    CurrentHealth : int
    MaxHealth : int
    InvincibleCounter : float32
    MaxInvincibleTime : float32
    IPlayer : IPlayer
    IUI : IUI
}
module PlayerHealthControllerModule =
    let Initialize se iPlayer (iUI : IUI) =
        let maxHealth = SingletonAccessor.ICharacterTracker.MaxHealth
        let currentHealth = SingletonAccessor.ICharacterTracker.CurrentHealth
        iUI.HealthSlider.maxValue <- (float32 maxHealth)
        iUI.HealthSlider.value <- (float32 currentHealth)
        iUI.HealthText.text <- sprintf "%i / %i" currentHealth maxHealth
        { SE = se; CurrentHealth = currentHealth; MaxHealth = maxHealth; MaxInvincibleTime = 1.0f
          InvincibleCounter = 1.0f; IPlayer = iPlayer; IUI = iUI; }
        
    let Update data deltaTime =
        let updatedInvincibleCounter =
            if data.InvincibleCounter > 0.0f then
                if (data.InvincibleCounter - deltaTime <= 0.0f) then
                    let bodyColor = data.IPlayer.GetBodySRColor()
                    data.IPlayer.SetBodySRColor
                        (new Color(bodyColor.r, bodyColor.g, bodyColor.b, 1.0f))
                data.InvincibleCounter - deltaTime                
            else
                data.InvincibleCounter
        { data with InvincibleCounter = updatedInvincibleCounter }
        
    let UpdateUI data =
        data.IUI.HealthSlider.maxValue <- float32 data.MaxHealth
        data.IUI.HealthSlider.value <- (float32 data.CurrentHealth)
        data.IUI.HealthText.text <- sprintf "%i / %i" data.CurrentHealth data.MaxHealth
        
    let TakeDamage (data : PlayerHealthData) =
        if data.InvincibleCounter > 0.0f || data.IPlayer.DashCounter > 0.0f then
            data
        else
            let bodyColor = data.IPlayer.GetBodySRColor()
            data.IPlayer.SetBodySRColor(new Color(bodyColor.r, bodyColor.g, bodyColor.b, 0.5f))             
            let currentHealth = data.CurrentHealth - 1        
            if currentHealth <= 0 then
                data.SE.GameObjectSetActive data.IPlayer.GameObject false
                data.SE.GameObjectSetActive data.IUI.DeathScreen true
                SingletonAccessor.IAudioManager.PlaySfx SfxEnum.PlayerDeath
                SingletonAccessor.IAudioManager.PlayGameOver()
            else
                SingletonAccessor.IAudioManager.PlaySfx SfxEnum.PlayerHurt
            let updatedData = { data with CurrentHealth = currentHealth; InvincibleCounter = data.MaxInvincibleTime }
            UpdateUI updatedData
            updatedData
            
    let HealPlayer data healAmount =
        let health =
            if data.CurrentHealth + healAmount <= data.MaxHealth then
                data.CurrentHealth + healAmount
            else
                data.MaxHealth
        let updatedData = { data with CurrentHealth = health }
        UpdateUI updatedData
        updatedData
        
    let IncreaseMaxHealth data amount =
        let newMaxHealth = data.MaxHealth + amount
        let newData = { data with MaxHealth = newMaxHealth; CurrentHealth = newMaxHealth }
        UpdateUI newData
        newData
        
open PlayerHealthControllerModule

[<AllowNullLiteral>]
type PlayerHealthControllerFs() =
    inherit FSBehavior()
    
    [<DefaultValue>] val mutable maxHealth : int
    [<DefaultValue>] val mutable private data : PlayerHealthData
    
    member this.Awake() =
        SingletonAccessor.IPlayerHealth <- this 
    member this.Start() =
        this.data <- Initialize (SideEffects()) SingletonAccessor.IPlayer SingletonAccessor.IUI
    member this.Update() =
        this.data <- Update this.data Time.deltaTime
        
    interface IPlayerHealth with
        member this.TakeDamage() =
            this.data <- TakeDamage this.data
        member this.HealPlayer healAmount =
            this.data <- HealPlayer this.data healAmount
        member this.MaxHealth
            with get() = this.data.MaxHealth
        member this.CurrentHealth
            with get() = this.data.CurrentHealth
        member this.IncreaseMaxHealth amount =
            this.data <- IncreaseMaxHealth this.data amount
            
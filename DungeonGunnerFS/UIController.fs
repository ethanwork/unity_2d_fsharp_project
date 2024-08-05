namespace DungeonGunnerFS

open UnityEngine
open UnityEngine.SceneManagement
open UnityEngine.SceneManagement
open UnityEngine.UI

[<AllowNullLiteral>]
type UIControllerFs() =
    inherit FSBehavior()
    
    [<DefaultValue>] val mutable healthSlider : Slider
    [<DefaultValue>] val mutable healthText : Text
    [<DefaultValue>] val mutable coinText : Text
    [<DefaultValue>] val mutable deathScreen : GameObject
    [<DefaultValue>] val mutable fadeScreen : Image
    [<DefaultValue>] val mutable pauseScreen : GameObject
    [<DefaultValue>] val mutable fadeSpeed : float32
    [<DefaultValue>] val mutable mainMenuScreen : string
    [<DefaultValue>] val mutable newGameScreen : string
    [<DefaultValue>] val mutable mapDisplay : GameObject
    [<DefaultValue>] val mutable bigMapText : GameObject
    [<DefaultValue>] val mutable currentGun : Image
    [<DefaultValue>] val mutable gunText : Text
    let mutable fadeToBlack : bool = false
    let mutable areFading : bool = true
    
    member this.Awake() =
        SingletonAccessor.IUI <- this
        
    member this.Start() =
        this.deathScreen.gameObject.SetActive(false)        
        this.currentGun.sprite <- SingletonAccessor.IPlayer.CurrentGun.GunUI
        this.gunText.text <- SingletonAccessor.IPlayer.CurrentGun.WeaponName
        
    member this.Update() =
        if (areFading) then
            let color = this.fadeScreen.color
            let alphaToFadeTo = if fadeToBlack then 1.0f else 0.0f
            this.fadeScreen.color <- Color(color.r, color.g, color.b,
                                           Mathf.MoveTowards(color.a, alphaToFadeTo, this.fadeSpeed * Time.deltaTime))
            areFading <- this.fadeScreen.color.a <> alphaToFadeTo
            fadeToBlack <- if areFading then fadeToBlack else not fadeToBlack
    member this.NewGame() =
        Time.timeScale <- 1.0f
        SceneManager.LoadScene(this.newGameScreen)
    member this.ReturnToMainMenu() =
        Time.timeScale <- 1.0f
        SceneManager.LoadScene(this.mainMenuScreen)
    member this.Resume() =
        SingletonAccessor.ILevelManager.PauseUnpause()
    
    interface IUI with
        member this.MapDisplay
            with get() = this.mapDisplay
        member this.BigMapText
            with get() = this.bigMapText
        member this.HealthSlider
            with get() = this.healthSlider
        member this.HealthText
            with get() = this.healthText
        member this.CoinText
            with get() = this.coinText
        member this.DeathScreen
            with get() = this.deathScreen
        member this.PauseScreen
            with get() = this.pauseScreen
        member this.CurrentGun
            with get() = this.currentGun
        member this.GunText
            with get() = this.gunText
        member this.FadeToBlack() =
            areFading <- true
            
                
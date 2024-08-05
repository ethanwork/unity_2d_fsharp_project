namespace DungeonGunnerFS

open System.Collections
open System.Collections.Generic
open UnityEngine
open DungeonGunnerFS
open UnityEngine

type LevelManagerFs() =
    inherit FSBehavior()
    
    [<DefaultValue>] val mutable waitToLoad : float32
    [<DefaultValue>] val mutable nextLevel : string
    [<DefaultValue>] val mutable isPaused : bool
    [<DefaultValue>] val mutable currentCoins : int
    [<DefaultValue>] val mutable startPoint : Transform
    
    member this.Awake() =
        SingletonAccessor.ILevelManager <- this
        this.waitToLoad <- 4.0f
    member this.Start() =        
        SingletonAccessor.IPlayer.Transform.position <- this.startPoint.position
        SingletonAccessor.IPlayer.CanMove <- true
        this.currentCoins <- SingletonAccessor.ICharacterTracker.CurrentCoins
        Time.timeScale <- 1.0f
        SingletonAccessor.IUI.CoinText.text <- sprintf "%i" this.currentCoins
    member this.Update() =
        if Input.GetKeyDown(KeyCode.P) then
            (this :> ILevelManager).PauseUnpause()            
            
    interface ILevelManager with
        member this.IsPaused
            with get() = this.isPaused
        member this.CurrentCoins
            with get() = this.currentCoins
        member this.PauseUnpause() =
            this.isPaused <- not this.isPaused
            SingletonAccessor.IUI.PauseScreen.SetActive(this.isPaused)
            Time.timeScale <- if this.isPaused then 0.0f else 1.0f
        member this.LevelEnd() : IEnumerator =            
            seq {
                // store the player stats to transfer over to the next level
                SingletonAccessor.ICharacterTracker.CurrentCoins <- this.currentCoins
                SingletonAccessor.ICharacterTracker.CurrentHealth <-
                    SingletonAccessor.IPlayerHealth.CurrentHealth
                SingletonAccessor.ICharacterTracker.MaxHealth <-
                    SingletonAccessor.IPlayerHealth.MaxHealth
                    
                SingletonAccessor.IAudioManager.PlayLevelWin()
                SingletonAccessor.IUI.FadeToBlack()
                yield WaitForSeconds(this.waitToLoad)
                SceneManagement.SceneManager.LoadScene(this.nextLevel)
            } :?> IEnumerator
        member this.GetCoins amount =
            this.currentCoins <- this.currentCoins + amount
            SingletonAccessor.IUI.CoinText.text <- sprintf "%i" this.currentCoins
        member this.SpendCoins amount =
            this.currentCoins <-
                if this.currentCoins - amount >= 0 then
                    this.currentCoins - amount
                else 0
            SingletonAccessor.IUI.CoinText.text <- sprintf "%i" this.currentCoins
            
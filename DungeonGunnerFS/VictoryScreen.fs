namespace DungeonGunnerFS

open UnityEngine
open UnityEngine.SceneManagement

type VictoryScreen() =
    inherit FSBehavior()
    
    [<DefaultValue>] val mutable anyKeyText : GameObject
    [<DefaultValue>] val mutable mainMenuScreen : string
    let mutable waitForAnyKey : float32 = 2.0f
    
    member this.Start() =
        Time.timeScale <- 1.0f
    member this.Update() =
        if waitForAnyKey > 0.0f then
            waitForAnyKey <- waitForAnyKey - Time.deltaTime
            if waitForAnyKey <= 0.0f then
                this.anyKeyText.SetActive(true)
        else
            if Input.anyKeyDown then
                SceneManager.LoadScene(this.mainMenuScreen)
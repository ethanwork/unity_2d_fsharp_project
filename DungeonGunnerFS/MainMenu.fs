namespace DungeonGunnerFS

open UnityEngine
open UnityEngine.SceneManagement

type MainMenu() =
    inherit FSBehavior()
    
    [<DefaultValue>] val mutable levelToLoad : string
    
    member this.StartGame() =
        SceneManager.LoadScene(this.levelToLoad)        
    member this.ExitGame() =
        // Application.Quit() doesn't do anything when you run the game from the editor
        Application.Quit();
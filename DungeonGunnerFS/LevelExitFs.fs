namespace DungeonGunnerFS

open UnityEngine

[<AllowNullLiteral>]
type LevelExitFs() =
    inherit FSBehavior()
    
    [<DefaultValue>] val mutable levelToLoad : string
    
    member this.OnTriggerEnter2D(other : Collider2D) =
        if other.tag = Tags.Player then
            SingletonAccessor.IPlayer.CanMove <- false
            this.StartCoroutine(SingletonAccessor.ILevelManager.LevelEnd()) |> ignore
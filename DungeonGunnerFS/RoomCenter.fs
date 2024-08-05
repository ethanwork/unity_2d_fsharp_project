namespace DungeonGunnerFS

open UnityEngine
open Utilities

type RoomCenter() =
    inherit FSBehavior()
    
    [<DefaultValue>] val mutable openWhenEnemiesCleared : bool 
    [<DefaultValue>] val mutable enemies : FSBehavior[]
    [<DefaultValue>] val mutable theRoom : IRoom
    
    member this.Start() =
        if this.openWhenEnemiesCleared then
            this.theRoom.CloseWhenEntered <- true
    member this.Update() =
        if this.theRoom.RoomActive = true then
            if this.openWhenEnemiesCleared && this.enemies.Length > 0 then
                this.enemies <- this.enemies |> Array.filter (fun x -> (IsGameObjectDestroyed x) = false)
                if this.enemies.Length = 0 then
                    this.theRoom.OpenDoors()
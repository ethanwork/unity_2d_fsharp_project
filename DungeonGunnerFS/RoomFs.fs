namespace DungeonGunnerFS

open UnityEngine
open DungeonGunnerFS
open Utilities

type RoomFs() =
    inherit FSBehavior()
    [<HideInInspector>][<DefaultValue>] val mutable roomActive : bool
    [<DefaultValue>] val mutable closeWhenEntered : bool
    [<DefaultValue>] val mutable doors : GameObject[]
    [<DefaultValue>] val mutable mapHider : GameObject
    
    member this.Awake() =
        this.roomActive <- false   
    member this.OnTriggerEnter2D(other : Collider2D) =
        if other.tag = Tags.Player then
            SingletonAccessor.ICamera.ChangeTarget this.transform
            this.roomActive <- true
            this.mapHider.SetActive(false)
            if this.closeWhenEntered then
                this.doors |> Array.iter (fun x -> x.SetActive(true))    
    member this.OnTriggerExit2D(other : Collider2D) =
        if (other.tag = Tags.Player) then
            this.roomActive <- false                
        
    interface IRoom with
        member this.RoomActive
            with get() = this.roomActive
            and set(value) = this.roomActive <- value
        member this.CloseWhenEntered
            with get() = this.closeWhenEntered
            and set(value) = this.closeWhenEntered <- value
        member this.OpenDoors() =
            this.doors |> Array.iter (fun x -> x.SetActive(false))
            this.closeWhenEntered <- false        
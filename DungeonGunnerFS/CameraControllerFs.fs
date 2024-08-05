namespace DungeonGunnerFS

open UnityEngine
open DungeonGunnerFS
open UnityEngine

type CameraControllerFs() =
    inherit FSBehavior()
    
    [<DefaultValue>] val mutable moveSpeed : float32
    [<DefaultValue>] val mutable target : Transform
    [<DefaultValue>] val mutable mainCamera : Camera
    [<DefaultValue>] val mutable bigMapCamera : Camera
    
    let mutable bigMapActive = false
    
    member this.Awake() =
        SingletonAccessor.ICamera <- this
    
    member this.Update() =
        if (this.target <> null) then
            let pos = this.transform.position
            let tarPos = this.target.position
            // make sure for the target vector we're moving to, only move towards the target's x/y,
            // but keep the source's z position, as we don't want the camera's z position to change in this 2d game
            this.transform.position <-
                Vector3.MoveTowards(pos, new Vector3(tarPos.x, tarPos.y, pos.z), this.moveSpeed * Time.deltaTime)
        if (Input.GetKeyDown(KeyCode.H)) then
            if (bigMapActive = false) then
                this.ActivateBigMap()
            else
                this.DeactivateBigMap()
            
                
    member this.ActivateBigMap() =
        if SingletonAccessor.ILevelManager.IsPaused = false then
            bigMapActive <- true
            this.bigMapCamera.enabled <- true
            this.mainCamera.enabled <- false
            SingletonAccessor.IPlayer.CanMove <- false
            Time.timeScale <- 0.0f
            SingletonAccessor.IUI.MapDisplay.SetActive false
            SingletonAccessor.IUI.BigMapText.SetActive true
    member this.DeactivateBigMap() =
        if SingletonAccessor.ILevelManager.IsPaused = false then
            bigMapActive <- false
            this.bigMapCamera.enabled <- false
            this.mainCamera.enabled <- true
            SingletonAccessor.IPlayer.CanMove <- true
            Time.timeScale <- 1.0f
            SingletonAccessor.IUI.MapDisplay.SetActive true
            SingletonAccessor.IUI.BigMapText.SetActive false
                
    interface ICamera with
        member this.ChangeTarget target =
            this.target <- target
        member this.MainCamera
            with get() = this.mainCamera
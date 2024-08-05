namespace DungeonGunnerFS

open DungeonGunnerFS
open UnityEngine
open UnityEngine
open UnityEngine
open UnityEngine
open Utilities

type DashConfig = {
    DashSpeed : float32
    DashLength : float32
    DashCooldown : float32
    DashInvincibility : float32
    DashCounter : float32
    DashCooldownCounter : float32
}

type PlayerControllerData = {
    AnimIsMoving : bool
    AnimSetTrigger : string -> unit
    GunArmRotation : Quaternion
    GunArmLocalScale : Vector3
    GunArm : Transform
    MoveSpeed : float32
    ActiveMoveSpeed : float32
    MoveInput : Vector2
    TheRBVelocity : Vector2
    TransformLocalScale : Vector3
    BodySRColor : Color    
    Dash : DashConfig
    CanMove : bool
    AvailableGuns : GunType[]
    CurrentGun : int
}

module PlayerControllerModule =    
    let SetBodySRColor data color =
        { data with BodySRColor = color }
        
    let getMovementInfo () =
        let (inputX, inputY) = GetInputAxisXYTuple()
        let moveInput = (Vec2Create inputX inputY).normalized
        let animIsMoving = moveInput <> Vector2.zero
        {| MoveInput = moveInput; AnimIsMoving = animIsMoving |}
        
    let getGunAndPlayerScaling (theCam : Camera) (theTransform : Transform) =
        let screenPoint = theCam.WorldToScreenPoint(theTransform.localPosition)
        let _transformLocalScale, _gunArmLocalScale = 
            if Input.mousePosition.x < screenPoint.x then (Vector3(-1.0f, 1.0f, 1.0f), Vector3(-1.0f, -1.0f, 1.0f))
            else (Vector3.one, Vector3.one)            
        let offset = Input.mousePosition - screenPoint
        let mouseAngle = Mathf.Atan2(offset.y, offset.x) * Mathf.Rad2Deg
        let _gunArmRotation = Quaternion.Euler(Vector3(0.0f, 0.0f, mouseAngle))
        {| TransformLocalScale = _transformLocalScale; GunArmLocalScale = _gunArmLocalScale
           GunArmRotation = _gunArmRotation |}
                       
    let getDashAndVelocityInfo dash (moveInput : Vector2) moveSpeed animSetTrigger =
        let isDashKeyPressed = (Input.GetKeyDown(KeyCode.M)) || (Input.GetKeyDown(KeyCode.Y))
        let dashJustActivated =
            if isDashKeyPressed && dash.DashCounter <= 0.0f && dash.DashCooldownCounter <= 0.0f then
                animSetTrigger "Dash"
                SingletonAccessor.IAudioManager.PlaySfx SfxEnum.PlayerDash
                true
            else false
        let _dashCounter =
            if dashJustActivated then dash.DashLength
            elif dash.DashCounter <= 0.0f then dash.DashCounter
            else dash.DashCounter - Time.deltaTime
        let _dashCooldownCounter =
            if dashJustActivated then dash.DashCooldown
            elif dash.DashCooldownCounter <= 0.0f then dash.DashCooldownCounter
            else dash.DashCooldownCounter - Time.deltaTime
        let _activeMoveSpeed =
            if _dashCounter > 0.0f then dash.DashSpeed
            else moveSpeed
        let _theRBVelocity = moveInput * _activeMoveSpeed
        {| DashCounter = _dashCounter; DashCooldownCounter = _dashCooldownCounter
           ActiveMoveSpeed = _activeMoveSpeed; TheRBVelocity = _theRBVelocity |}
    
    let getCurrentGun (availableGuns : GunType[]) currentGun = 
        if Input.GetKeyDown KeyCode.Period && availableGuns.Length > 0 then
            (currentGun + 1) % availableGuns.Length
        else
            currentGun
            
    let updateGunUI previousGun currentGun (availableGuns : GunType[]) =
        if currentGun <> previousGun then
            availableGuns |> Array.iter (fun x -> x.gameObject.SetActive false)
            availableGuns.[currentGun].gameObject.SetActive true
            SingletonAccessor.IUI.CurrentGun.sprite <- availableGuns.[currentGun].GunUI
            SingletonAccessor.IUI.GunText.text <- availableGuns.[currentGun].WeaponName
            
    let Update data (theCam : Camera) (theTransform: Transform) =
        if data.CanMove && not SingletonAccessor.ILevelManager.IsPaused then
            // get movement info
            let mi = getMovementInfo()
            
            // get gun and player scaling/rotation and facing direction            
            let playerScaling = getGunAndPlayerScaling theCam theTransform 
 
            // get rigid body velocity and dashing info
            let dash = getDashAndVelocityInfo data.Dash mi.MoveInput data.MoveSpeed data.AnimSetTrigger
            
            // get current gun
            let _currentGunNew = getCurrentGun data.AvailableGuns data.CurrentGun
            // update current gun UI
            updateGunUI data.CurrentGun _currentGunNew data.AvailableGuns
            
            { data with
                MoveInput = mi.MoveInput; AnimIsMoving = mi.AnimIsMoving; GunArmRotation = playerScaling.GunArmRotation
                GunArmLocalScale = playerScaling.GunArmLocalScale; TransformLocalScale = playerScaling.TransformLocalScale
                TheRBVelocity = dash.TheRBVelocity; ActiveMoveSpeed = dash.ActiveMoveSpeed; CurrentGun = _currentGunNew
                Dash = { data.Dash with DashCooldownCounter = dash.DashCooldownCounter; DashCounter = dash.DashCounter }
            }
        else
            { data with TheRBVelocity = Vector2.zero; AnimIsMoving = false }
            
    let pickupGun data (gun : GunType) =
        let hasGunAlready = data.AvailableGuns |> Array.exists (fun x -> x.WeaponName = gun.WeaponName)        
        let _availableGuns =
            if hasGunAlready then
                data.AvailableGuns
            else
                SingletonAccessor.IAudioManager.PlaySfx SfxEnum.PickupGun
                let gunClone = MonoBehaviour.Instantiate gun :?> GunType
                gunClone.transform.parent <- data.GunArm
                gunClone.transform.position <- data.GunArm.transform.position
                gunClone.transform.localRotation <- Quaternion.Euler Vector3.zero
                gunClone.transform.localScale <- Vector3.one
                Array.append data.AvailableGuns [| gunClone |]
        let _currentGunNew =
            if hasGunAlready then
                data.CurrentGun
            else
                _availableGuns.Length - 1
        updateGunUI data.CurrentGun _currentGunNew _availableGuns
        {| Data = { data with AvailableGuns = _availableGuns; CurrentGun = _currentGunNew }
           GunWasPickedUp = not hasGunAlready |}
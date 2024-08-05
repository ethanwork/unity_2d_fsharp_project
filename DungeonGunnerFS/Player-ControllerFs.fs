namespace DungeonGunnerFS

open UnityEngine
open Utilities
  
open PlayerControllerModule

[<AllowNullLiteral>]
type PlayerControllerFs() =
    inherit FSBehavior()
    
    member this.Awake() =
        (*
        IMPORTANT NOTE!!!!!!!
        I was getting a really hard to figure out bug, but in the LevelManagerFs.fs file in its
        Start() method, it would throw an error when doing these lines
            SingletonAccessor.IPlayer.Transform.position <- this.startPoint.position
            SingletonAccessor.IPlayer.CanMove <- true
        The first line would run fine, then the second line would blow up with a null reference
        error when trying to update the CanMove property, I was confused as to how the previous
        line could work, but the second wouldn't. And If we look at the bottom of the code here
        we see that the IPlayer interface's CanMove property references not a primitive type,
        but references our data type PlayerControllerData and when LevelManager's Start() method
        ran, this data record had not yet been initialized, as it was initialized originally
        in this class's Start() method which hadn't run yet.
        So what I did was I moved the initialization code for this PlayerControllerData 'data'
        field up into the Awake() method, and did it before I assigned 'this' to the
        SingletonAccess.IPlayer interface, so that I know if something can access SingletonAccess.IPlayer
        and it doesn't get a null reference on the IPlayer itself (possibly because that code was called
        before this class's Awake() method had run, that then if IPlayer was populated, it was guaranteed
        that the 'data' field also would be that the 'CanMove' property uses.
        So from a guess, it looks like the Awake() methods run before the Start() methods do, and that
        possibly you can't rely on the Start() methods for various classes to run in a specific order.
        *)
        let animSetTrigger (triggerName : string) =
            this.anim.SetTrigger(triggerName)            
        let moveSpeed = 5.0f
        if this.availableGuns.Length > 0 then
            this.availableGuns |> Array.iter (fun x -> x.gameObject.SetActive false)
            this.availableGuns.[0].gameObject.SetActive(true)        
        let dash = { DashSpeed = 8.0f; DashLength = 0.5f; DashCooldown = 1.0f; DashInvincibility = 0.5f;
                     DashCounter = 0.0f; DashCooldownCounter = 0.0f }
        let currentGun = 0
        this.data <-
          { MoveSpeed = moveSpeed; MoveInput = Vector2(); TheRBVelocity = Vector2(); GunArmRotation = Quaternion();
            GunArmLocalScale = Vector3(); TransformLocalScale = Vector3(); AnimIsMoving = false;
            BodySRColor = this.bodySR.color; Dash = dash; ActiveMoveSpeed = moveSpeed; AnimSetTrigger = animSetTrigger
            CanMove = true; AvailableGuns = this.availableGuns; CurrentGun = currentGun; GunArm = this.gunArm }
          
        SingletonAccessor.IPlayer <- this
        MonoBehaviour.DontDestroyOnLoad(this.gameObject)
        
    
    member this.Start() =
        // Why is not startup code happening here, see the Awake function above for an important
        // explanation why
        ()
        
    member this.Update() =
        this.data <- Update this.data SingletonAccessor.ICamera.MainCamera this.transform
        this.MutateState()           
    
    member this.MutateState() =
        this.gunArm.localScale <- this.data.GunArmLocalScale
        this.gunArm.rotation <- this.data.GunArmRotation
        this.theRB.velocity <- this.data.TheRBVelocity
        this.transform.localScale <- this.data.TransformLocalScale
        this.anim.SetBool("IsMoving", this.data.AnimIsMoving)
        this.bodySR.color <- this.data.BodySRColor
        this.availableGuns <- this.data.AvailableGuns

    interface IPlayer with
        member self.DashCounter
            with get() = self.data.Dash.DashCounter
        member this.CanMove
            with get() = this.data.CanMove
            and set(value) = this.data <- { this.data with CanMove = value }                    
        member this.CurrentGun
            with get() = this.data.AvailableGuns.[this.data.CurrentGun]
        member this.SetBodySRColor color =
            this.data <- SetBodySRColor this.data color
            this.MutateState()        
        member this.GetBodySRColor() =
            this.data.BodySRColor
        override this.PickupGun gun =
            let result = pickupGun this.data gun  
            this.data <- result.Data
            this.MutateState()
            result.GunWasPickedUp
        
            
    [<DefaultValue>] val mutable anim : Animator
    [<DefaultValue>] val mutable theRB : Rigidbody2D
    [<DefaultValue>] val mutable gunArm : Transform
    [<DefaultValue>] val mutable bodySR : SpriteRenderer
    [<DefaultValue>] val mutable availableGuns : GunType[]
    [<DefaultValue>] val mutable private data : PlayerControllerData
    [<DefaultValue>] val mutable private activeMoveSpeed : float32
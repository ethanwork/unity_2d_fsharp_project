namespace DungeonGunnerFS

open UnityEngine
open DungeonGunnerFS

type BrokenPiecesData = {
    Self : IFSBehavior
    SE : ISideEffects
    MoveDirection : Vector3
    TransformPosition : Vector3
    Lifetime : float32
    FadeSpeed : float32
    TheSRColor : Color
    Deceleration : float32    
}

module BrokenPiecesModule =
    let Initialize (self : IFSBehavior) se (moveSpeed : float32) theSRColor =
        let moveX = Random.Range(-moveSpeed, moveSpeed)
        let moveY = Random.Range(-moveSpeed, moveSpeed)
        { Self = self; MoveDirection = Vector3(moveX, moveY); TransformPosition = self.Transform.position
          Deceleration = 5.0f; Lifetime = 3.0f; FadeSpeed = 2.5f; TheSRColor = theSRColor; SE = se }
        
    let Update data deltaTime =        
        let transformPos = data.TransformPosition + (data.MoveDirection * deltaTime)
        let moveDir = Vector3.Lerp(data.MoveDirection, Vector3.zero, data.Deceleration * deltaTime)
        let lifetime = data.Lifetime - deltaTime
        let srColor =
            if lifetime > 0.0f then
                data.TheSRColor
            else
                let color = data.TheSRColor
                Color(color.r, color.g, color.b, Mathf.MoveTowards(color.a, 0.0f, data.FadeSpeed * deltaTime))
        if srColor.a <= 0.0f then
            data.SE.GameObjectDestroy data.Self.GameObject
        { data with
            TransformPosition = transformPos; MoveDirection = moveDir; Lifetime = lifetime; TheSRColor = srColor }

open BrokenPiecesModule

type BrokenPiecesFs() =
    inherit FSBehavior()
    
    [<DefaultValue>] val mutable moveSpeed : float32
    [<DefaultValue>] val mutable theSR : SpriteRenderer
    
    [<DefaultValue>] val mutable private data : BrokenPiecesData
    
    member this.Start() =
        this.data <- Initialize this (SideEffects()) this.moveSpeed this.theSR.color    
        
    member this.Update() =
        this.data <- Update this.data Time.deltaTime
        this.Mutate()
        
    member this.Mutate() =
        this.transform.position <- this.data.TransformPosition
        this.theSR.color <- this.data.TheSRColor
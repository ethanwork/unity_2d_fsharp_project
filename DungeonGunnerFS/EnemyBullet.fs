namespace DungeonGunnerFS

open DungeonGunnerFS
open UnityEngine

type EnemyBulletData = {
    Speed : float32
    Direction : Vector3
    TransformPosition : Vector3
    SE : ISideEffects
    Self : IFSBehavior
    IPlayerHealth : IPlayerHealth
}

module EnemyBulletModule =
    let Initialize self se speed bulletPosition playerPosition playerHealth =
        let direction : Vector3 = playerPosition - bulletPosition
        { Speed = speed; Direction = direction.normalized; TransformPosition = bulletPosition
          SE = se; Self = self; IPlayerHealth = playerHealth }

    let Update data deltaTime : EnemyBulletData =
        { data with TransformPosition = data.TransformPosition + data.Direction * data.Speed * deltaTime }
        
    let DestroySelf data =
        data.SE.GameObjectDestroy data.Self.GameObject
    let OnTriggerEnter2D data (other : Collider2D) =
        if other.tag = Tags.Player then
            data.IPlayerHealth.TakeDamage()
        data.SE.GameObjectDestroy data.Self.GameObject

open EnemyBulletModule

[<AllowNullLiteral>]
type EnemyBullet() =
    inherit FSBehavior()
    // public
    [<DefaultValue>] val mutable speed : float32
    //private
    [<DefaultValue>] val mutable private data : EnemyBulletData 
    
    member this.Start() =
        this.data <- Initialize this (SideEffects()) this.speed this.transform.position
                         SingletonAccessor.IPlayer.Transform.position SingletonAccessor.IPlayerHealth
        this.MutateState()
    member this.Update() =
        this.data <- Update this.data Time.deltaTime 
        this.MutateState()
    member this.MutateState() =
        this.transform.position <- this.data.TransformPosition        
    member this.OnTriggerEnter2D (other : Collider2D) =
        OnTriggerEnter2D this.data other
    member this.OnBecameInvisible() =
        DestroySelf this.data
namespace DungeonGunnerFS

open UnityEngine
open DungeonGunnerFS

type PlayerBulletData = {
    Speed : float32
    TheRBVelocity : Vector2
    BulletDamage : int
    ImpactEffect : GameObject
    Self : IFSBehavior
    SE : ISideEffects
}

module PlayerBulletModule =
    let Update data : PlayerBulletData =
        let transformRight = data.Self.Transform.right
        { data with TheRBVelocity = Vector2(transformRight.x, transformRight.y) * data.Speed }    
    let DealDamage (otherCollider : Collider2D) bulletDamage (se : ISideEffects) =
        if otherCollider.tag = Tags.Enemy then
            let enemyController = se.ColliderGetComponent<IEnemy>(otherCollider)
            match enemyController with
            | Some enemy -> enemy.TakeDamage(bulletDamage)
            | None -> ()
    let CreateImpactEffect impactEffect position rotation (se : ISideEffects) =
        se.GameObjectInstantiate
            impactEffect position rotation |> ignore        
    let DestroySelf gameObject (se : ISideEffects) =
        se.GameObjectDestroy gameObject
    let OnTriggerEnter2D (otherCollider : Collider2D) (data : PlayerBulletData) =
            SingletonAccessor.IAudioManager.PlaySfx SfxEnum.Impact
            DealDamage otherCollider data.BulletDamage data.SE
            CreateImpactEffect data.ImpactEffect data.Self.Transform.position data.Self.Transform.rotation data.SE
            DestroySelf data.Self.GameObject data.SE
open PlayerBulletModule

type PlayerBulletFs() =
    inherit FSBehavior()
    // public fields
    [<DefaultValue>] val mutable speed : float32
    [<DefaultValue>] val mutable theRB : Rigidbody2D
    [<DefaultValue>] val mutable impactEffect : GameObject
    [<DefaultValue>] val mutable bulletDamage : int
    // private fields
    [<DefaultValue>] val mutable private data : PlayerBulletData
    
    member this.Start() =
        this.speed <- 7.8f
        this.data <- { Speed = this.speed; TheRBVelocity = Vector2.zero; BulletDamage = this.bulletDamage
                       ImpactEffect = this.impactEffect; Self = this; SE = SideEffects() }        
    member this.Update() =
        this.data <- Update this.data
        this.MutateState this.data
    member this.OnTriggerEnter2D (other : Collider2D) =
        OnTriggerEnter2D other this.data
    member this.OnBecameInvisible() =
        DestroySelf this.data.Self.GameObject
    member this.MutateState data =
        this.theRB.velocity <- data.TheRBVelocity

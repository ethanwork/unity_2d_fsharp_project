namespace DungeonGunnerFS

open DungeonGunnerFS
open UnityEngine

open EnemyControllerModule

[<AllowNullLiteral>]
type EnemyController() =
    inherit FSBehavior()
    
    member this.Start() =
        this.data <-
            { Self = this; MoveSpeed = this.moveSpeed; TheRBVelocity = this.theRB.velocity
              RangeToChasePlayer = this.rangeToChasePlayer; AnimIsMoving = false; Health = 150
              DeathSplatters = this.deathSplatters; SE = (SideEffects()); HitEffect = this.hitEffect
              ShouldShoot = this.shouldShoot; FireRate = this.fireRate; FireCounter = this.fireRate
              Bullet = this.bullet; FirePoint = this.firePoint; TheBodySprite = this.theBodySprite
              ShootRange = this.shootRange; IPlayer = SingletonAccessor.IPlayer
              ShouldChasePlayer = this.shouldChasePlayer; ShouldRunAway = this.shouldRunAway
              RunawayRange = this.runawayRange; ShouldWander = this.shouldWander
              WanderLength = this.wanderLength; WanderPauseLength = this.wanderPauseLength; WanderCounter = 0.0f
              WanderDirection = Vector3.zero
              WanderPauseCounter = Random.Range(this.wanderPauseLength * 0.75f, this.wanderPauseLength * 1.25f)
              ShouldPatrol = this.shouldPatrol; PatrolPoints = this.patrolPoints; CurrentPatrolPoint = 0
              ShouldDropItem = this.shouldDropItem; ItemsToDrop = this.itemsToDrop; DropChance = this.dropChance
               }      
    member this.Update() =
        this.data <- update this.data Time.deltaTime
        this.MutateState()        
    member this.MutateState() =
        this.theRB.velocity <- this.data.TheRBVelocity
        this.anim.SetBool("IsMoving", this.data.AnimIsMoving)
        
    interface IEnemy with
        member this.TakeDamage damage =
            this.data <- takeDamage this.data damage
    
    // public
    [<DefaultValue>] val mutable moveSpeed : float32
    [<Header("Chase Player")>]
    [<DefaultValue>] val mutable shouldChasePlayer : bool
    [<DefaultValue>] val mutable rangeToChasePlayer : float32
    [<Header("Shooting")>]
    [<DefaultValue>] val mutable shouldShoot : bool
    [<DefaultValue>] val mutable shootRange : float32
    [<DefaultValue>] val mutable fireRate : float32   
    [<DefaultValue>] val mutable bullet : GameObject
    [<DefaultValue>] val mutable firePoint : Transform
    [<Header("Run Away")>]
    [<DefaultValue>] val mutable shouldRunAway : bool
    [<DefaultValue>] val mutable runawayRange : float32
    [<Header("Wandering")>]
    [<DefaultValue>] val mutable shouldWander : bool
    [<DefaultValue>] val mutable wanderLength : float32
    [<DefaultValue>] val mutable wanderPauseLength : float32
    [<Header("Patrolling")>]
    [<DefaultValue>] val mutable shouldPatrol : bool
    [<DefaultValue>] val mutable patrolPoints : Transform[]
    [<Header("Item Drops")>]
    [<DefaultValue>] val mutable shouldDropItem : bool
    [<DefaultValue>] val mutable itemsToDrop : GameObject[]
    [<DefaultValue>] val mutable dropChance : float32
    [<Header("Variables")>]
    [<DefaultValue>] val mutable theRB : Rigidbody2D    
    [<DefaultValue>] val mutable anim : Animator
    [<DefaultValue>] val mutable deathSplatters : GameObject[]
    [<DefaultValue>] val mutable hitEffect : GameObject
    [<DefaultValue>] val mutable theBodySprite : SpriteRenderer    
    //private
    [<DefaultValue>] val mutable private data : EnemyControllerData
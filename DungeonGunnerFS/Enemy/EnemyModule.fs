namespace DungeonGunnerFS

open DungeonGunnerFS
open UnityEngine
open Utilities

type EnemyControllerData = {
    Self : IFSBehavior; SE : ISideEffects; MoveSpeed : float32; TheRBVelocity : Vector2; AnimIsMoving : bool
    Health : int; DeathSplatters : GameObject[]; HitEffect : GameObject; FireRate : float32; FireCounter : float32
    Bullet : GameObject; FirePoint : Transform; TheBodySprite : SpriteRenderer; IPlayer : IPlayer; ShouldShoot : bool
    ShouldChasePlayer : bool; ShouldWander : bool; ShouldRunAway : bool; ShootRange : float32; RunawayRange : float32
    RangeToChasePlayer : float32; WanderLength : float32; WanderPauseLength : float32; WanderCounter : float32
    WanderPauseCounter : float32; WanderDirection : Vector3; ShouldPatrol : bool; PatrolPoints : Transform[]
    CurrentPatrolPoint : int; ShouldDropItem : bool; ItemsToDrop : GameObject[]; DropChance : float32
}   

module EnemyControllerModule =
    let getWanderInfo data deltaTime =
        if data.ShouldWander then
            let wanderCounter =
                if data.WanderCounter > 0.0f then
                    data.WanderCounter - deltaTime
                elif data.WanderPauseCounter <= 0.0f then
                    Random.Range(data.WanderLength * 0.75f, data.WanderLength * 1.25f)
                else data.WanderCounter
            let wanderPauseCounter =
                if data.WanderPauseCounter > 0.0f then
                    data.WanderPauseCounter - deltaTime
                elif data.WanderCounter > 0.0f && wanderCounter <= 0.0f then
                    Random.Range(data.WanderPauseLength * 0.75f, data.WanderPauseLength * 1.25f)                
                else data.WanderPauseCounter
            let wanderDirection =
                if wanderPauseCounter > 0.0f then
                    Vector3.zero
                elif data.WanderPauseCounter > 0.0f && wanderPauseCounter <= 0.0f then
                    Vector3(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f), 0.0f)                
                else
                    vec2ToVec3 data.TheRBVelocity
            (wanderDirection, wanderCounter, wanderPauseCounter)
        else
            (Vector3.zero, 0.0f, 0.0f)
    
    let getPatrolInfo data (enemyPosition : Vector3) (playerPosition : Vector3) =
        if data.ShouldPatrol = false ||
           data.ShouldChasePlayer && Vector3.Distance(enemyPosition, playerPosition) < data.RangeToChasePlayer then
            (Vector3.zero, 0)
        else
            let direction =
                data.PatrolPoints.[data.CurrentPatrolPoint].position - data.Self.Transform.position
            let currentPatrolPoint =
                if Vector3.Distance(data.Self.Transform.position,
                                     data.PatrolPoints.[data.CurrentPatrolPoint].position) < 0.2f then
                    (data.CurrentPatrolPoint + 1) % data.PatrolPoints.Length
                else
                    data.CurrentPatrolPoint
            (direction, currentPatrolPoint)
            
    let updateMovement (enemyPosition : Vector3) (playerPosition : Vector3) data wanderDirection patrolDirection =
        let direction =
            if data.ShouldChasePlayer && Vector3.Distance(enemyPosition, playerPosition) < data.RangeToChasePlayer then
                playerPosition - enemyPosition
            elif data.ShouldWander then
                wanderDirection
            elif data.ShouldRunAway && Vector3.Distance(enemyPosition, playerPosition) < data.RunawayRange then
                enemyPosition - playerPosition
            elif data.ShouldPatrol then
                patrolDirection
            else
                Vector3.zero
        Vec3ToVec2 (direction.normalized * data.MoveSpeed)

    let isTimeForNextShot deltaTime fireCounter fireRate shouldShoot  =
        if shouldShoot = false then
            (false, fireRate)
        elif fireCounter - deltaTime <= 0.0f then
            (true, fireRate)
        else
            (false, fireCounter - deltaTime)            
    
    let withinFiringRange shouldShoot playerPosition enemyPosition shootRange =
        if shouldShoot = true && Vector3.Distance(enemyPosition, playerPosition) < shootRange then
            true
        else
            false
    
    let update data deltaTime =
        if data.TheBodySprite.isVisible = false || data.IPlayer.GameObject.activeInHierarchy = false then
            { data with TheRBVelocity = Vector2.zero }
        else
            let playerPosition = data.IPlayer.Transform.position
            let (wanderDirection, wanderCounter, wanderPauseCounter) = getWanderInfo data deltaTime
            let (patrolDirection, currentPatrolPoint) = getPatrolInfo data data.Self.Transform.position playerPosition
            let velocity =
                updateMovement data.Self.Transform.position playerPosition data wanderDirection patrolDirection 
            let (shotFired, fireCounter) =
                withinFiringRange data.ShouldShoot playerPosition data.Self.Transform.position data.ShootRange
                |> isTimeForNextShot deltaTime data.FireCounter data.FireRate
            if shotFired then
                SingletonAccessor.IAudioManager.PlaySfx SfxEnum.Shoot6
                data.SE.GameObjectInstantiate
                    data.Bullet data.FirePoint.position data.FirePoint.rotation |> ignore
            { data with
                TheRBVelocity = velocity; AnimIsMoving = Vector2.zero <> velocity; FireCounter = fireCounter
                WanderCounter = wanderCounter; WanderPauseCounter = wanderPauseCounter
                CurrentPatrolPoint = currentPatrolPoint }
        
    let createSplatter (deathSplatters : GameObject[]) position instantiate =
        let splatterNumber = int (Random.Range(0, deathSplatters.Length))
        let rotationAmount = (float32 (int (Random.Range(0, 4)))) * 270.0f
        instantiate deathSplatters.[splatterNumber] position (Quaternion.Euler(0.0f, 0.0f, rotationAmount)) |> ignore
    
    let dropItem data =
        if data.ShouldDropItem then
            let dropChance = Random.Range(0.0f, 100.0f)
            if dropChance <= data.DropChance then
                let itemToDrop = Random.Range(0, data.ItemsToDrop.Length)
                data.SE.GameObjectInstantiate data.ItemsToDrop.[itemToDrop] data.Self.Transform.position
                    data.Self.Transform.rotation |> ignore
                    
    let takeDamage data damage =
        data.SE.GameObjectInstantiate
            data.HitEffect data.Self.Transform.position data.Self.Transform.rotation |> ignore
        if data.Health <= 0 then
            SingletonAccessor.IAudioManager.PlaySfx SfxEnum.EnemyDeath
            GameObject.Destroy data.Self.GameObject            
            createSplatter data.DeathSplatters data.Self.Transform.position data.SE.GameObjectInstantiate
            dropItem data
        else
            SingletonAccessor.IAudioManager.PlaySfx SfxEnum.EnemyHurt
        { data with Health = data.Health - damage }
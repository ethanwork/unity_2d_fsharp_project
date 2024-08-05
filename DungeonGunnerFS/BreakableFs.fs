namespace DungeonGunnerFS

open UnityEngine
open DungeonGunnerFS

type BreakableData = {    
    Self : IFSBehavior
    SE : ISideEffects
    IPlayer : IPlayer
    BrokenPieces : GameObject[]
    MaxPieces : int
    ShouldDropItem : bool
    ItemsToDrop : GameObject[]
    DropChance : float32
}

module BreakableModule =
    let Initialize self se iPlayer brokenPieces shouldDropItem itemsToDrop dropChance =
        { Self = self; SE = se; IPlayer = iPlayer; BrokenPieces = brokenPieces; MaxPieces = 5
          ShouldDropItem = shouldDropItem; ItemsToDrop = itemsToDrop; DropChance = dropChance; }
    let Smash data =
        let createPiece _ =
                let pieceIndex = Random.Range(0, data.BrokenPieces.Length)
                data.SE.GameObjectInstantiate data.BrokenPieces.[pieceIndex] data.Self.Transform.position
                    data.Self.Transform.rotation |> ignore
                    
        data.SE.GameObjectDestroy data.Self.GameObject
        [1 .. Random.Range(1, data.MaxPieces)] |> List.iter createPiece
        SingletonAccessor.IAudioManager.PlaySfx SfxEnum.BoxBreaking
        
        if data.ShouldDropItem then
            let dropChance = Random.Range(0.0f, 100.0f)
            if dropChance <= data.DropChance then
                let itemToDrop = Random.Range(0, data.ItemsToDrop.Length)
                data.SE.GameObjectInstantiate data.ItemsToDrop.[itemToDrop] data.Self.Transform.position
                    data.Self.Transform.rotation |> ignore
                  
    let OnTriggerEnter2D data (other : Collider2D) =
        if (other.tag = Tags.Player && data.IPlayer.DashCounter > 0.0f) ||
           other.tag = Tags.PlayerBullet then
            Smash data
                                   
open BreakableModule

type BreakableFs() =
    inherit FSBehavior()
    
    [<DefaultValue>] val mutable brokenPieces : GameObject[]
    [<DefaultValue>] val mutable shouldDropItem : bool
    [<DefaultValue>] val mutable itemsToDrop : GameObject[]
    [<DefaultValue>] val mutable dropChance : float32
    [<DefaultValue>] val mutable private data : BreakableData    
    
    member this.Start() =
        this.data <- Initialize this (SideEffects()) SingletonAccessor.IPlayer this.brokenPieces
            this.shouldDropItem this.itemsToDrop this.dropChance
    
    member this.OnTriggerEnter2D (other : Collider2D) =
        OnTriggerEnter2D this.data other
namespace DungeonGunnerFS

open UnityEngine
open DungeonGunnerFS

type SpriteSortOrderFs() =
    inherit FSBehavior()
    
    [<DefaultValue>] val mutable private theSR : SpriteRenderer
    
    member this.Start() =
        this.theSR <- this.GetComponent<SpriteRenderer>()
        this.theSR.sortingOrder <- Mathf.RoundToInt(this.transform.position.y * -10.0f)
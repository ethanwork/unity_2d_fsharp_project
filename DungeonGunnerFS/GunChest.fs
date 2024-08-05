namespace DungeonGunnerFS

open UnityEngine
open UnityEngine
open UnityEngine
open UnityEngine
open UnityEngine
open UnityEngine
open UnityEngine
open UnityEngine

type GunChest() =
    inherit FSBehavior()
    
    [<DefaultValue>] val mutable potentialGuns : GunPickupType[]
    [<DefaultValue>] val mutable theSR : SpriteRenderer
    [<DefaultValue>] val mutable chestOpen : Sprite
    [<DefaultValue>] val mutable notification : GameObject
    [<DefaultValue>] val mutable spawnPoint : Transform
    [<DefaultValue>] val mutable scaleSpeed : float32
    let mutable canOpen = false
    let mutable isOpen = false
    
    member this.Update () =
        if canOpen && isOpen = false then
            if Input.GetKeyDown KeyCode.U then
                isOpen <- true
                let gunSelect = Random.Range(0, this.potentialGuns.Length)
                let gun = MonoBehaviour.Instantiate(this.potentialGuns.[gunSelect],
                                                    this.spawnPoint.position,
                                                    this.spawnPoint.rotation)
                this.theSR.sprite <- this.chestOpen
                this.transform.localScale <- Vector3(1.2f, 1.2f, 1.2f)
        if isOpen then
            this.transform.localScale <-
                Vector3.MoveTowards(this.transform.localScale, Vector3.one, Time.deltaTime * this.scaleSpeed)
                
    member this.OnTriggerEnter2D (other : Collider2D) =
        if other.tag = Tags.Player then
            this.notification.SetActive true
            canOpen <- true
        
    member this.OnTriggerExit2D (other : Collider2D) =
        if other.tag = Tags.Player then
            this.notification.SetActive false
            canOpen <- false            
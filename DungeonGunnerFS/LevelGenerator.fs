namespace DungeonGunnerFS

open System
open UnityEngine
open UnityEngine.SceneManagement
open Utilities

type Direction = Up = 0 | Right = 1 | Down = 2 | Left = 3

[<System.Serializable>]
type RoomPrefabs() =
    [<DefaultValue>] val mutable R : GameObject
    [<DefaultValue>] val mutable U : GameObject
    [<DefaultValue>] val mutable L : GameObject
    [<DefaultValue>] val mutable D : GameObject
    [<DefaultValue>] val mutable LR : GameObject
    [<DefaultValue>] val mutable UD : GameObject
    [<DefaultValue>] val mutable UR : GameObject
    [<DefaultValue>] val mutable RD : GameObject
    [<DefaultValue>] val mutable LD : GameObject
    [<DefaultValue>] val mutable LU : GameObject
    [<DefaultValue>] val mutable URD : GameObject
    [<DefaultValue>] val mutable LRD : GameObject
    [<DefaultValue>] val mutable LUD : GameObject
    [<DefaultValue>] val mutable LUR : GameObject
    [<DefaultValue>] val mutable LURD : GameObject
 
type LevelGenerator() as m =
    inherit FSBehavior()
        
    [<DefaultValue>] val mutable includeShop : bool
    [<DefaultValue>] val mutable minDistanceToShop : int
    [<DefaultValue>] val mutable maxDistanceToShop : int
    [<DefaultValue>] val mutable includeGunRoom : bool
    [<DefaultValue>] val mutable minDistanceToGunRoom : int
    [<DefaultValue>] val mutable maxDistanceToGunRoom : int
    [<DefaultValue>] val mutable layoutRoom : GameObject
    [<DefaultValue>] val mutable startColor : Color
    [<DefaultValue>] val mutable endColor : Color
    [<DefaultValue>] val mutable shopColor : Color
    [<DefaultValue>] val mutable gunRoomColor : Color
    [<DefaultValue>] val mutable generatorPoint : Transform
    [<DefaultValue>] val mutable selectedDirection : Direction
    [<DefaultValue>] val mutable distanceToEnd : int
    [<DefaultValue>] val mutable whatIsRoom : LayerMask
    [<DefaultValue>] val mutable roomPrefabs : RoomPrefabs
    [<DefaultValue>] val mutable centerStart : RoomCenter
    [<DefaultValue>] val mutable centerEnd : RoomCenter
    [<DefaultValue>] val mutable centerShop : RoomCenter
    [<DefaultValue>] val mutable centerGunRoom : RoomCenter
    [<DefaultValue>] val mutable potentialCenters : RoomCenter[]
    
    let mutable startRoom : GameObject = null
    let mutable endRoom : GameObject = null
    let mutable shopRoom : GameObject = null
    let mutable gunRoom : GameObject = null
    let mutable layoutRooms : GameObject list = []
    let mutable generatedOutlines : GameObject list = []
    
    let xOffset = 18.0f
    let yOffset = 10.0f
    let getNewGenerationPoint currentPos direction =
        match direction with
        | Direction.Up -> currentPos + Vector3(0.0f, yOffset, 0.0f)
        | Direction.Down -> currentPos + Vector3(0.0f, -yOffset, 0.0f)
        | Direction.Right -> currentPos + Vector3(xOffset, 0.0f, 0.0f)
        | Direction.Left -> currentPos + Vector3(-xOffset, 0.0f, 0.0f)
        | _ -> currentPos
        
    let getNextRoomPosition currentRoomPosition roomLayer =
        // calculate the new direction
        let direction = (enum<Direction>(Random.Range(0, 4)))
        // calculate the next position
        let mutable nextPosition = (getNewGenerationPoint currentRoomPosition direction)
        // keep moving one more spot over in the given direction until we don't collide with an existing room
        while hasCircleCollision nextPosition 0.2f roomLayer do
            nextPosition <- (getNewGenerationPoint nextPosition direction)
        nextPosition
                
    member this.CreateRoomOutline position =
        let roomLayer = (int m.whatIsRoom)
        let roomLeft = if hasCircleCollision (position + Vector3(-xOffset, 0.0f, 0.0f)) 0.2f roomLayer then "L" else ""
        let roomUp = if hasCircleCollision (position + Vector3(0.0f, yOffset, 0.0f)) 0.2f roomLayer then "U" else ""
        let roomRight = if hasCircleCollision (position + Vector3(xOffset, 0.0f, 0.0f)) 0.2f roomLayer then "R" else ""
        let roomDown = if hasCircleCollision (position + Vector3(0.0f, -yOffset, 0.0f)) 0.2f roomLayer then "D" else ""
        let roomTypeName =
            sprintf "%s%s%s%s" roomLeft roomUp roomRight roomDown
        let prefabRoom = (typedefof<RoomPrefabs>).GetField(roomTypeName).GetValue(this.roomPrefabs) :?> GameObject
        InstantiateGO prefabRoom position m.generatorPoint.rotation
    
    member this.Start() =
        let roomObjects =
            [1 .. (m.distanceToEnd - 1)]
            |> List.fold (fun (rooms : GameObject list) _ ->
                let nextPosition = getNextRoomPosition rooms.Head.gameObject.transform.position (int this.whatIsRoom)
                let newRoom = InstantiateGO m.layoutRoom nextPosition m.generatorPoint.rotation
                newRoom :: rooms)
                // start off the list of rooms at the original position
                [InstantiateGO m.layoutRoom m.generatorPoint.position m.generatorPoint.rotation]
            |> List.rev
            
        startRoom <- roomObjects.Head        
        endRoom <- (last roomObjects)
        
        generatedOutlines <- roomObjects |> List.map (fun x -> m.CreateRoomOutline x.transform.position)
        
        // if the level generator is supplied with bad values for the min/max distance to the shop
        // and gun room then throw an exception otherwise an infinite loop will be entered a little farther down
        if this.includeShop && this.includeGunRoom && this.minDistanceToShop = this.maxDistanceToShop &&
           this.minDistanceToGunRoom = this.maxDistanceToGunRoom then
               failwith "Invalid Min/Max Distance to Shop and GunRoom values"
        // pick which rooms we'll make the shop room and gun room in the level
        let shopNumber =
            if this.includeShop then Random.Range(this.minDistanceToShop, this.maxDistanceToShop + 1)
            else -1
        let gunRoomNumber =
            if this.includeGunRoom then
                let mutable gunRoomNumber = Random.Range(this.minDistanceToGunRoom, this.maxDistanceToGunRoom + 1)
                while gunRoomNumber = shopNumber do
                    gunRoomNumber <- Random.Range(this.minDistanceToGunRoom, this.maxDistanceToGunRoom + 1)
                gunRoomNumber
            else -1
        let mutable currentCenterCounter = -1
        let generatedCenters =
            generatedOutlines
            |> List.map (fun x ->
                currentCenterCounter <- currentCenterCounter + 1
                let centerPrefab =
                    if x.transform.position = endRoom.transform.position then
                        this.centerEnd
                    elif x.transform.position = startRoom.transform.position then
                        this.centerStart
                    elif currentCenterCounter = shopNumber then
                        this.centerShop
                    elif currentCenterCounter = gunRoomNumber then
                        this.centerGunRoom
                    else
                        this.potentialCenters.[Random.Range(0, this.potentialCenters.Length)]
                let center = InstantiateMB centerPrefab x.transform.position x.transform.rotation
                (center :?> RoomCenter).theRoom <- x.GetComponent<RoomFs>()
                center)
            
        // color the start/end rooms        
        startRoom.GetComponent<SpriteRenderer>().color <- m.startColor
        endRoom.GetComponent<SpriteRenderer>().color <- m.endColor
        if this.includeShop then
            shopRoom <- roomObjects.[shopNumber]
            shopRoom.GetComponent<SpriteRenderer>().color <- m.shopColor
        if this.includeGunRoom then
            gunRoom <- roomObjects.[gunRoomNumber]
            gunRoom.GetComponent<SpriteRenderer>().color <- m.gunRoomColor
        
        // get a list of the rooms excluding the start and end rooms
        layoutRooms <- roomObjects.[1..(roomObjects.Length - 2)]
        
    member this.Update() =
        // what's weird, is if I say !UNITY_EDITOR, as in Not UNITY_EDITOR
        // it then allows this code to run in the Unity Editor.
        // But if I do UNITY_EDITOR, then it doesn't allow this code to run in
        // the Unity Editor, the opposite of what I would expect. But it does
        // seem to enable/disable behavior based on if the editor is being used
        // which is good, even if it does seem to be the opposite of expected
        // boolean wise for the 'pound if' preproccesor directive
        #if !UNITY_EDITOR        
        if Input.GetKey(KeyCode.R) then
            SceneManager.LoadScene(SceneManager.GetActiveScene().name)
        #endif
        ()            
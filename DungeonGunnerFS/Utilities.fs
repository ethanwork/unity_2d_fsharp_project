namespace DungeonGunnerFS

open UnityEngine

module Utilities =    
    let rec last = function
    | hd :: [] -> hd
    | hd :: tl -> last tl
    | _ -> failwith "Empty list."
    
    let Vec2Create x y =
        new Vector2(x, y)
    let Vec3ToVec2 (v : Vector3) =
        Vector2(v.x, v.y)
    let vec2ToVec3 (v : Vector2) =
        Vector3(v.x, v.y, 0.0f)

    let GetInputAxisXYTuple () =
        (Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"))

    let IsGameObjectDestroyed (enemy : FSBehavior) =
        if enemy = null || enemy.IsDestroyed then true else false
    let InstantiateMB (go : MonoBehaviour) (pos : Vector3) (rot : Quaternion) =
        (GameObject.Instantiate(go, pos, rot) :?> MonoBehaviour)
    let InstantiateGO (go : GameObject) (pos : Vector3) (rot : Quaternion) =
        (GameObject.Instantiate(go, pos, rot) :?> GameObject)    
    let hasCircleCollision position radius layer =
        Physics2D.OverlapCircle((Vec3ToVec2 position), radius, layer) <> null
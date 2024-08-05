# Unity Engine FSharp Game Development Project
This project was to gain more experience with F#, and even though it is a functionally oriented language I was curious to see how it would do in a game development context.

F# worked surprisingly well, substituting in for C# in Unity without too much work. This being a game development project, there would be lots of mutations since immutability is not usually ideal for performance critical situations like game development (even though this simple of a 2D game could run well even with using immutable types more often).

The simplest, but most valuable thing I took from this experience, was how much it can simplify code process flow if you only define variables one time, and then don't mutate them afterwards. If/else expressions help with this greatly, as you can initialize a variable that may have somewhat involved initialization logic, but still do it all in one assignment since you can use multi-line code blocks (unline ternary expressions in C# which don't allow multi-line expressions).

The benefit then becomes, instead of having the code create variables which you then mutate or change them in various places later in the code, you can often times just define the variables in sequence, each depending upon the previous one. So we can set 'isDashKeyPressed', then use that to initialize 'dashJustActivated', which then helps set '_dashCounter' etc. And if there's ever any issue with some value not appearing to be set right in the code, you can go directly to its assignment and know it isn't changed elsewhere in the code so it helps narrow when debugging a problem issue. 

This style of coding flow can be done in pretty much any language, but the if/else block expressions were of help to simplify and streamline this kind of flow.

```fsharp
    let getDashAndVelocityInfo dash (moveInput : Vector2) moveSpeed animSetTrigger =
        let isDashKeyPressed = (Input.GetKeyDown(KeyCode.M)) || (Input.GetKeyDown(KeyCode.Y))
        let dashJustActivated =
            if isDashKeyPressed && dash.DashCounter <= 0.0f && dash.DashCooldownCounter <= 0.0f then
                animSetTrigger "Dash"
                SingletonAccessor.IAudioManager.PlaySfx SfxEnum.PlayerDash
                true
            else false
        let _dashCounter =
            if dashJustActivated then dash.DashLength
            elif dash.DashCounter <= 0.0f then dash.DashCounter
            else dash.DashCounter - Time.deltaTime
        let _dashCooldownCounter =
            if dashJustActivated then dash.DashCooldown
            elif dash.DashCooldownCounter <= 0.0f then dash.DashCooldownCounter
            else dash.DashCooldownCounter - Time.deltaTime
        let _activeMoveSpeed =
            if _dashCounter > 0.0f then dash.DashSpeed
            else moveSpeed
        let _theRBVelocity = moveInput * _activeMoveSpeed
        {| DashCounter = _dashCounter; DashCooldownCounter = _dashCooldownCounter
           ActiveMoveSpeed = _activeMoveSpeed; TheRBVelocity = _theRBVelocity |}
```
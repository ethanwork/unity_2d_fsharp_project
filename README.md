
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
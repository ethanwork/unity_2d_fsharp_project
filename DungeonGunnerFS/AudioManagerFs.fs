namespace DungeonGunnerFS

open UnityEngine
open DungeonGunnerFS

type AudioManagerFs() =
    inherit FSBehavior()
    
    [<DefaultValue>] val mutable levelMusic : AudioSource
    [<DefaultValue>] val mutable gameOverMusic : AudioSource
    [<DefaultValue>] val mutable winMusic : AudioSource
    [<DefaultValue>] val mutable sfx : AudioSource[]
    
    member this.Awake() =
        SingletonAccessor.IAudioManager <- this
    
    interface IAudioManager with
        member this.PlayGameOver() =
            this.levelMusic.Stop()
            this.gameOverMusic.Play()
        member this.PlayLevelWin() =
            this.levelMusic.Stop()
            this.winMusic.Play()
        member this.PlaySfx sfxEnum =
            let index = int sfxEnum
            if index >= 0 && index < this.sfx.Length then
                this.sfx.[index].Stop()
                this.sfx.[index].Play()
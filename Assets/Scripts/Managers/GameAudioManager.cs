using JinkeGroup.Logic;
using JinkeGroup.Audio;
using JinkeGroup.Util;
using JinkeGroup.TalkBack;
using JinkeGroup.Tom.Common;
using UnityEngine;
using UnityEngine.Audio;

namespace JinkeGroup.Tom.Gameplay.Audio
{

    public class GameAudioManager : Manager<GameAudioManager>
    {
        public MusicStateMachine MusicStateMachine = null;
        public AudioMixer Mixer = null;
        public TalkBackHandler TalkBackHandler = null;
        public PerCharacterSizeTalkbackSettings PerCharacterSizeTalkbackSettings = null;

        public AnimationCurve AnimalZonePanVolume = new AnimationCurve();
        public AnimationCurve AnimalZonePanStereo = new AnimationCurve();

        const string Tag = "GameAudioManager";
        const string UserMuteSFXName = "Jinke.GameAudio.UserSFX.Mute";
        const string UserMuteMusicName = "Jinke.GameAudio.UserMusic.Mute";

        const string MixerNameUserSFXVolume = "UserSFX.Volume";
        const string MixerNameUserMusicVolume = "UserMusic.Volume";

        const string MixerNameSFXVolume = "SFX.Volume";
        const string MixerNameMusicVolume = "Music.Volume";
        const string MixerNameSFXAmbientVolume = "SFX.Ambient.Volume";
        const string MixerNameSFXAnimationsVolume = "SFX.Animations.Volume";
        const string MixerNameSFXAnimalsVolume = "SFX.Animals.Volume";
        const string MixerNameSFXToysVolume = "SFX.Toys.Volume";

        const string MixerNameSFXPitch = "SFX.Pitch";

        // interal values
        private bool MuteUserMusicInternal = false;
        private bool MuteUserSfxInternal = false;

        private bool MixerMuteAnimations = false;
        private bool MixerMuteAmbient = false;
        private bool MixerMuteAnimals = false;
        private bool MixerMuteToys = false;
        private int FixSoundIssues = 3;

        const float MixerBlendTime = 0.0f;

        public bool UserSFXMute
        {
            get { return MuteUserSfxInternal; }
            set
            {
                MuteUserSfxInternal = value;
                Mixer.Mute(MixerNameUserSFXVolume, value, MixerBlendTime);

            }
        }

        public bool UserMusicMute
        {
            get { return MuteUserMusicInternal; }
            set
            {
                MuteUserMusicInternal = value;
                Mixer.Mute(MixerNameUserMusicVolume, value, MixerBlendTime);
            }
        }
        public bool GameMuteSfx
        {
            set { Mixer.Mute(MixerNameSFXVolume, value, MixerBlendTime); }
        }
        public bool GameMuteSfxAmbient
        {
            set { Mixer.Mute(MixerNameSFXAmbientVolume, value, MixerBlendTime); }
        }
        public bool GameMuteSfxAnimations
        {
            set { Mixer.Mute(MixerNameSFXAnimationsVolume, value, MixerBlendTime); }
        }
        public bool GameMuteSfxAnimals
        {
            set
            { Mixer.Mute(MixerNameSFXAnimalsVolume, value, MixerBlendTime); }
        }
        public bool GameMuteSfxToys
        {
            set
            {
                JinkeGroup.Util.Logger.DebugT(Tag, "GameMuteSfxToys: {0}", value);
                Mixer.Mute(MixerNameSFXToysVolume, value, MixerBlendTime);
            }
        }

        public bool GameMuteMusic
        {
            set
            {
                JinkeGroup.Util.Logger.DebugT(Tag, "GameMuteMusic: {0}", value);
                Mixer.Mute(MixerNameMusicVolume, value, MixerBlendTime);
            }
        }

        public float GameSFXPitch
        {
            set
            {
                float val = Mathf.Max(value, 0.0f);
                Mixer.SetFloat(MixerNameSFXPitch, val);
            }
            get
            {
                float val = 1.0f;
                Mixer.GetFloat(MixerNameSFXPitch, out val);
                return val;
            }
        }

        public void SaveUserPrefs()
        {
            UserPrefs.SetBool(UserMuteSFXName, MuteUserSfxInternal);
            UserPrefs.SetBool(UserMuteMusicName, MuteUserMusicInternal);
            UserPrefs.Save();
        }

        public override bool OnInitialize()
        {
            MuteUserSfxInternal = UserPrefs.GetBool(UserMuteSFXName, MuteUserSfxInternal);
            MuteUserMusicInternal = UserPrefs.GetBool(UserMuteMusicName, MuteUserMusicInternal);
            Mixer.Mute(MixerNameUserSFXVolume, MuteUserSfxInternal, 0.0f);
            Mixer.Mute(MixerNameUserMusicVolume, MuteUserMusicInternal, 0.0f);

            AudioPlugin.ForceToSpeaker();
            AudioEventManager.Instance.Set2DPanning(CameraManager.Instance.MainCamera, AnimalZonePanStereo, AnimalZonePanVolume);

            return base.OnInitialize();
        }

        public override void OnResume()
        {
            base.OnResume();
            FixSoundIssues = 3;
        }

        public override void OnPreUpdate(float deltaTime)
        {
            base.OnPreUpdate(deltaTime);
            if (FixSoundIssues > 0)
            {
                FixSoundIssues--;
                Main.Instance.FixSoundIssuesOnIphone();
            }
            if (TalkBackHandler != null && TalkBackHandler.TalkBackSettings != null
                && PerCharacterSizeTalkbackSettings != null)
            {
                int index = (int)Main.Instance.CharacterState.CurrentSize;
                if (index < PerCharacterSizeTalkbackSettings.PerSizeSettings.Length)
                {
                    PerCharacterSizeTalkbackSettings pss = PerCharacterSizeTalkbackSettings.PerSizeSettings[index];
                    if (Main.Instance.MeterGameLogic.MustSleep)
                    {
                        TalkBackHandler.TalkBackSettings.SoundTouchPitch = pss.SleepyPitch;
                        TalkBackHandler.TalkBackSettings.SoundTouchRate = pss.SleepyRate;
                        TalkBackHandler.TalkBackSettings.SoundTouchTempo = pss.SleepyTempo;
                    }
                    else
                    {
                        TalkBackHandler.TalkBackSettings.SoundTouchPitch = pss.Pitch;
                        TalkBackHandler.TalkBackSettings.SoundTouchRate = pss.Rate;
                        TalkBackHandler.TalkBackSettings.SoundTouchTempo = pss.Tempo;
                    }
                }
            }
            //mute animations if an dialog opens
            bool mixerMuteAnimations = false;
            bool mixerMuteAnimals = false;
            bool mixerMuteToys = false;
            bool mixerMuteAmbient = false;
            for (int a = 0; a < SceneStateManager.Instance.SceneSM.Layers.Length; a++)
            {
                var layer = SceneStateManager.Instance.SceneSM.Layers[a];
                if (layer.CurrentState != null)
                {
                    if ((layer.CurrentState.AttributeMask & SceneSM.Attributes.MixerMuteAnimations) == SceneSM.Attributes.MixerMuteAnimations)
                    {
                        mixerMuteAnimations = true;
                    }
                    if ((layer.CurrentState.AttributeMask & SceneSM.Attributes.MixerMuteAnimals) == SceneSM.Attributes.MixerMuteAnimals)
                    {
                        mixerMuteAnimals = true;
                    }
                    if ((layer.CurrentState.AttributeMask & SceneSM.Attributes.MixerMuteToys) == SceneSM.Attributes.MixerMuteToys)
                    {
                        mixerMuteToys = true;
                    }
                    if ((layer.CurrentState.AttributeMask & SceneSM.Attributes.MixerMuteAmbient) == SceneSM.Attributes.MixerMuteAmbient)
                    {
                        mixerMuteAmbient = true;
                    }
                }
                if (MixerMuteAnimations != mixerMuteAnimations)
                {
                    MixerMuteAnimations = mixerMuteAnimations;
                    GameMuteSfxAnimations = MixerMuteAnimations;
                }
                if (MixerMuteAnimals != mixerMuteAnimals)
                {
                    MixerMuteAnimals = mixerMuteAnimals;
                    GameMuteSfxAnimals = MixerMuteAnimals;
                }
                if (MixerMuteToys != mixerMuteToys)
                {
                    MixerMuteToys = mixerMuteToys;
                    GameMuteSfxToys = MixerMuteToys;
                }
                if (MixerMuteAmbient != mixerMuteAmbient)
                {
                    MixerMuteAmbient = mixerMuteAmbient;
                    GameMuteSfxAmbient = MixerMuteAmbient;
                }
            }
        }
    }
}
        
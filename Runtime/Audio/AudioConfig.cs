using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Audio;

namespace BinhoGames.Core
{
    public class AudioConfig : ScriptableObject
    {
        [SerializeField] private AssetReference audioClip;
        public AssetReference AudioClip => audioClip;
        
        [SerializeField] private AudioMixerGroup mixerGroup;
        public AudioMixerGroup MixerGroup => mixerGroup;

        [SerializeField] private bool isLoop;
        public bool IsLoop => isLoop;

        [SerializeField] private int priority;
        public int Priority => priority;
    }
}

using Cysharp.Threading.Tasks;
using UnityEngine;

namespace BinhoGames.Core
{
    public class AudioService
    {
        private readonly AssetService _assetService;
        private readonly PoolService _poolService;
        private readonly Logger _logger;
        
        public AudioService() : this(
            Locator.Get<AssetService>(),
            Locator.Get<PoolService>(),
            new Logger(nameof(AudioService)))
        {
        }
        
        public AudioService(AssetService assetService, PoolService poolService, Logger logger)
        {
            _assetService = assetService;
            _poolService = poolService;
            _logger = logger;
        }

        public async UniTask PlayAsync(AudioConfig config)
        {
            var audioClip = await _assetService.GetAsync<AudioClip>(config.AudioClip);
            _poolService.Get()
            var audioSource = new AudioSource();
            audioSource.clip = audioClip;
            audioSource.priority = config.Priority;
            audioSource.loop = config.IsLoop;
            audioSource.outputAudioMixerGroup = config.MixerGroup;
            audioSource.Play();
        }
    }
}

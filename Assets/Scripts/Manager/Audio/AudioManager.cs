
using UnityEngine;

namespace ESS.Manager.Audio
{
	public class AudioManager : MonoBehaviour
	{
		public static AudioManager Instance { get; private set; }

		[Header("Sound Effects")]
		[SerializeField] private AudioClip hoverSound;
		[SerializeField] private AudioClip clickSound;
		[SerializeField] private AudioClip clickWoodSound;
		[SerializeField] private AudioClip shotSound;
		[SerializeField] private AudioClip flapSound;

		[Header("Settings")]
		[SerializeField, Range(0f, 1f)]
		private float masterVolume = 1f;

		private AudioSource audioSource;

		private void Awake()
		{
			if (Instance != null && Instance != this)
			{
				Destroy(gameObject);
				return;
			}

			Instance = this;
			DontDestroyOnLoad(gameObject);

			audioSource = gameObject.AddComponent<AudioSource>();
			audioSource.playOnAwake = false;
			audioSource.loop = false;
		}

		public void PlayHover()
		{
			PlaySFX(hoverSound, 0.5f);
		}

		public void PlayClick()
		{
			PlaySFX(clickSound);
		}

		public void PlayClickWood()
		{
			PlaySFX(clickWoodSound);
		}

		public void PlayShot()
		{
			PlaySFX(shotSound);
		}

		public void PlayFlap()
		{
			PlaySFX(flapSound);
		}

		public void PlayClip(AudioClip clip, float volume = 1f)
		{
			PlaySFX(clip, volume);
		}

		private void PlaySFX(AudioClip clip, float volume = 1f)
		{
			if (clip == null) return;

			audioSource.PlayOneShot(
				clip,
				masterVolume * volume
			);
		}
	}
}
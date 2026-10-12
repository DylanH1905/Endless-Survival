
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ESS.Manager
{
	public class MusicManager : MonoBehaviour
	{
		public static MusicManager Instance { get; private set; }

		[Serializable]
		private class SceneMusic
		{
			public string sceneName;
			public AudioClip music;
			[Range(0f, 1f)] public float volume = 0.3f;
		}

		[SerializeField] private SceneMusic[] sceneMusics;
		[SerializeField] private float fadeDuration = 1f;

		private AudioSource audioSource;
		private Coroutine fadeCoroutine;

		private void Awake()
		{
			if (Instance != null && Instance != this)
			{
				Destroy(gameObject);
				return;
			}

			Instance = this;
			DontDestroyOnLoad(gameObject);

			audioSource = GetComponent<AudioSource>();

			if (audioSource == null)
				audioSource = gameObject.AddComponent<AudioSource>();

			audioSource.loop = true;
			audioSource.playOnAwake = false;
			audioSource.volume = sceneMusics.Length > 0 ? sceneMusics[0].volume : 0.3f;

			SceneManager.sceneLoaded += OnSceneLoaded;

			PlaySceneMusic(SceneManager.GetActiveScene().name, false);
		}

		private void OnDestroy()
		{
			if (Instance != this)
				return;

			SceneManager.sceneLoaded -= OnSceneLoaded;
			Instance = null;
		}

		private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
		{
			PlaySceneMusic(scene.name, true);
		}

		private void PlaySceneMusic(string sceneName, bool useFade)
		{
			AudioClip clip = null;

			foreach (var item in sceneMusics)
			{
				if (item.sceneName == sceneName)
				{
					clip = item.music;
					break;
				}
			}

			if (audioSource.clip == clip && audioSource.isPlaying)
				return;

			if (fadeCoroutine != null)
				StopCoroutine(fadeCoroutine);

			fadeCoroutine = StartCoroutine(
				ChangeMusic(clip, useFade)
			);
		}

		private System.Collections.IEnumerator ChangeMusic(
			AudioClip newClip,
			bool useFade)
		{
			if (useFade && audioSource.isPlaying)
			{
				float startVolume = audioSource.volume;
				float elapsed = 0f;

				while (elapsed < fadeDuration)
				{
					elapsed += Time.unscaledDeltaTime;
					audioSource.volume = Mathf.Lerp(
						startVolume,
						0f,
						elapsed / fadeDuration
					);
					yield return null;
				}
			}

			audioSource.Stop();
			audioSource.clip = newClip;
			audioSource.volume = newClip != null ? Array.Find(sceneMusics, item => item.music == newClip)?.volume ?? 0.3f : 0.3f;

			if (newClip != null)
				audioSource.Play();

			fadeCoroutine = null;
		}
	}
}
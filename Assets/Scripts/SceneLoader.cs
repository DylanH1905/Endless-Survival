
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ESS
{
	public class SceneLoader : SystemMonoBehaviour
	{
		public void PlayGame()
		{
			Info("Load GamePlay Scene");
			SceneManager.LoadScene("GamePlay", LoadSceneMode.Single);
		}
	}
}
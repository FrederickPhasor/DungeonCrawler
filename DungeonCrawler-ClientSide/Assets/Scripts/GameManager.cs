using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
	public static GameManager sceneManager;
	[SerializeField] GameObject waitingScreenGameObject;
	[SerializeField] TextMeshProUGUI textDisplayer;
	private void Awake()
	{
		if (sceneManager != null)
			GameObject.Destroy(sceneManager);
		else
			sceneManager = this;
		DontDestroyOnLoad(this);
	}
	private void OnEnable()
	{
		ServerController.GameStartEvent += StartingGameScene;
	}
	private void OnDisable()
	{
		ServerController.GameStartEvent -= StartingGameScene;
	}
	List<AsyncOperation> scenesLoading = new List<AsyncOperation>();
	bool gameStarted = false;
	int gameSeed;
	void StartingGameScene(int seed)
	{
		gameStarted = true;
		gameSeed = seed;
	}
	private void Update()
	{
		if (gameStarted)
		{
			Debug.Log("The seed is : " + gameSeed);
			waitingScreenGameObject.SetActive(true);
			scenesLoading.Add(SceneManager.UnloadSceneAsync(1));
			scenesLoading.Add(SceneManager.LoadSceneAsync(2, LoadSceneMode.Additive));
			StartCoroutine(GetSceneLoadProgress());
		}
	}
	void EndOfGame()
	{
		waitingScreenGameObject.SetActive(true);
		scenesLoading.Add(SceneManager.UnloadSceneAsync(2));
		scenesLoading.Add(SceneManager.LoadSceneAsync(1, LoadSceneMode.Additive));
		StartCoroutine(GetSceneLoadProgress());
	}
	private void Start()
	{
		SceneManager.LoadSceneAsync(1, LoadSceneMode.Additive);
	}

	float totalSceneProgress;
	public IEnumerator GetSceneLoadProgress()
	{
		for(int i = 0; i < scenesLoading.Count; i++)
		{
			while (!scenesLoading[i].isDone)
			{
				totalSceneProgress = 0;
				foreach(AsyncOperation operation in scenesLoading)
				{
					totalSceneProgress += operation.progress;
				}
				totalSceneProgress = (totalSceneProgress / scenesLoading.Count) * 100f;
				textDisplayer.text = $"CARGANDO : {totalSceneProgress}";
				yield return null;
			}
		}
		scenesLoading.Clear();
	}
	
}

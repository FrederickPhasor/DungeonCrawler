using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
	public static GameManager sceneManager;
	//public string[] groups = new string [5];
	public List<string> groups = new List<string>();
	public bool groupsUpdated = false;
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
		ServerController.EnemyListUpdateEvent += SetGroupsInGame;
	}

	private void OnDisable()
	{
		ServerController.GameStartEvent -= StartingGameScene;
	}

	List<AsyncOperation> scenesLoading = new List<AsyncOperation>();
	bool gameStarted = false;
	bool startingGameSceneStartedAlready = true;
	public static int gameSeed;

	void StartingGameScene(int seed)
	{
		if (startingGameSceneStartedAlready)
		{
			gameStarted = true;
			startingGameSceneStartedAlready = false;
		}
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
			waitingScreenGameObject.SetActive(false);
			gameStarted = false;
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

	public void SetGroupsInGame(string enemyList)
    {
		string cleanedString = enemyList.Split(new[] { '\0' }, 2)[0];
		string[] groupss = cleanedString.Split('_');
        for (int i = 0; i < groupss.Length-1; i++)
        {
			groups.Add(groupss[i]);
		}
		groupsUpdated = true;
	}
	
}

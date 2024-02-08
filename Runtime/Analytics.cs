using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Serialization;
using UnityEngine.SceneManagement;


public class Analytics : MonoBehaviour
{
	[Tooltip("Any unique identifier, required only if multiple Analytics components are in use.")]
	[SerializeField] private string _tableID;
	[Tooltip("Move the root GameObject to DontDestroyOnLoad to persist scene loads?  Doing so ensures against uncompleted uploads.")]
	[SerializeField] private bool _persistent = true;
	[SerializeField] private bool _cancelOnSceneLoad = true;
	
	[Tooltip("File will be saved to My Documents folder, as determined by the operating system.")]
	[SerializeField] private string _localFilename = "Analytics.txt";
	[SerializeField] private Separator _separator = Separator.Tab; 
	
	[SerializeField, TextArea] private string _googleFormURL;
	[SerializeField] private float _retryDelay = 5f;
	
	[FormerlySerializedAs("_gameData"),SerializeField] private AnalyticsMetric[] _metrics;
	

	static private List<Analytics> _instance;
	
	private DateTime _gameStartTime;
	private bool _gameInProgress;
	private Coroutine _sending;

	private enum Separator : byte
	{
		Tab = (byte)'\t',
		Pipe = (byte)'|',
		Comma = (byte)',',
		Semicolon = (byte)';'
	}
	

	private void Awake()
	{
		_instance ??= new List<Analytics>();
		_instance.Add(this);
		
		var root = GetComponent<Transform>();
		while (root.parent) root = root.parent;
		if (_persistent) DontDestroyOnLoad(root.gameObject);
		if (_cancelOnSceneLoad) SceneManager.sceneUnloaded += CancelGame;
	}

	private void CancelGame(Scene scene)
	{
		_gameInProgress = false;
	}

	private void OnDestroy()
	{
		_instance.Remove(this);
	}


	static private Analytics FindInstance(string tableID)
	{
		if (_instance.Count == 1) Debug.Log("[ANALYTICS] If only one Analytics table is in use, there is no need to pass the tableID");
		foreach (var inst in _instance)
		{
			var equal = string.Compare(inst._tableID, tableID, CultureInfo.InvariantCulture, CompareOptions.IgnoreCase | CompareOptions.IgnoreSymbols) == 0;
			if (equal) return inst;
		}
		throw new Exception($"[ANALYTICS]  Could not find Analytics component with tableID \"{tableID}\"");
	}

	static private Analytics GetInstance()
	{
		if (_instance == null || _instance.Count == 0) throw new Exception("[ANALYTICS]  There are no active Analytics instances, or it has not yet initialized");
		if (_instance.Count > 1) throw new Exception("[ANALYTICS]  Analytics tableID must be specified when multiple Analytics components are in use!");
		return _instance[0];
	}

	
	/// <summary>
	/// Call this at the start of each game, before setting any Analytics metrics.  All metrics are reset to their default values.
	/// </summary>
	/// <param name="timestamp">(Optional) The name of a metric to record the date/time the game started.</param>
	static public void GameStarted(string timestamp = null)
	{
		GetInstance().SetGameStarted(timestamp);
	}

	/// <summary>
	/// Call this at the start of each game, before setting any Analytics metrics.  All metrics are reset to their default values.
	/// </summary>
	/// <param name="tableID">Required only if there are multiple Analytics tables.</param>
	/// <param name="timestamp">The name of a metric to record the date/time the game started.</param>
	static public void GameStarted(string tableID, string timestamp)
	{
		FindInstance(tableID).SetGameStarted(timestamp);
	}

	private void SetGameStarted(string timestamp)
	{
		foreach (var metric in _metrics)
		{
			metric.Reset();
		}
		if (_gameInProgress)
		{
			Debug.LogError("[ANALYTICS]  Game restarted without being ended!");
			GameEnded();
		}
		_gameInProgress = true;
		_gameStartTime = DateTime.Now;
		if (!string.IsNullOrEmpty(timestamp))
		{
			var metric = FindMetric(timestamp);
			if (metric.type != AnalyticsMetric.ValueType._dateTime) Debug.LogError($"[ANALYTICS]  Timestamp metric \"{timestamp}\" must have type Timestamp.");
			else metric.DateTimeValue = DateTime.Now;
		}
	}
	
	
	/// <summary>
	/// Call this at the end of each game, before beginning a new game, to record current metric values.
	/// </summary>
	/// <param name="duration">(Optional) The name of a metric to record the duration of the game.</param>
	static public void GameEnded(string duration = null)
	{
		GetInstance().SetGameEnded(duration);
	}
	
	/// <summary>
	/// Call this at the end of each game, before beginning a new game, to record current metric values.
	/// </summary>
	/// <param name="tableID">Required only if there are multiple Analytics tables.  Case, white-space and symbols are ignored.</param>
	/// <param name="duration">The name of a metric to record the duration of the game.</param>
	static public void GameEnded(string tableID, string duration)
	{
		FindInstance(tableID).SetGameEnded(duration);
	}
	
	/// <summary>
	/// Call this at the end of each game, before beginning a new game, to record current metric values.
	/// </summary>
	/// <param name="duration">(Optional) The name of a metric to record the duration of the game.</param>
	private void SetGameEnded(string duration)
	{
		if (!_gameInProgress)
		{
			Debug.LogError("[ANALYTICS]  Game can not be ended without having started!");
			return;
		}
		_gameInProgress = false;
		if (!string.IsNullOrEmpty(duration))
		{
			var metric = FindMetric(duration);
			if (metric.type != AnalyticsMetric.ValueType._timeSpan) Debug.LogError($"[ANALYTICS]  Duration metric \"{duration}\" must have type Timespan.");
			else metric.TimeSpanValue = DateTime.Now - _gameStartTime;
		}
		WriteToDataFile();
		SendDataToOnlineForm();
	}
	

	/// <summary>
	/// Returns true if a game is in progress, ie. GameStarted() has been called, but GameEnded() has not.
	/// </summary>
	static public bool GameInProgress() => GetInstance()._gameInProgress;
	
	/// <summary>
	/// Returns true if a game is in progress, ie. GameStarted() has been called, but GameEnded() has not.
	/// </summary>
	/// <param name="tableID">Required only if there are multiple Analytics tables.  Case, white-space and symbols are ignored.</param>
	/// <returns></returns>
	static public bool GameInProgress(string tableID) => FindInstance(tableID)._gameInProgress;
	

	/// <summary>
	/// Record an entry.  Use this instead of calling GameStarted and GameEnded, for tables not encompassing the full duration of a game.  All metrics are reset to their defaults.
	/// </summary>
	static public void RecordEntry()
	{
		var inst = GetInstance();
		if (inst._gameInProgress) Debug.LogError("[ANALYTICS] RecordEntry() and GameStarted() should not both be called for the same table.");
		inst._gameInProgress = true;
		inst.SetGameEnded(null);
		
		foreach (var metric in inst._metrics)
		{
			metric.Reset();
		}
	}

	/// <summary>
	/// Record an entry.  Use this instead of calling GameStarted and GameEnded, for tables not encompassing the full duration of a game.  All metrics are reset to their defaults.
	/// </summary>
	static public void RecordEntry(string tableID)
	{
		var inst = FindInstance(tableID);
		if (inst._gameInProgress) Debug.LogError("[ANALYTICS] RecordEntry() and GameStarted() should not both be called for the same table.");
		inst._gameInProgress = true;
		inst.SetGameEnded(null);
		
		foreach (var metric in inst._metrics)
		{
			metric.Reset();
		}
	}

	
	/// <summary>
	/// Call this to end the game without recording data.  Otherwise, the next time GameStarted() is called, an incomplete game will automatically be recorded.
	/// </summary>
	static public void AbortGame()
	{
		GetInstance().AbortCurrentGame();
	}
	
	/// <summary>
	/// Call this to end the game without recording data.  Otherwise, the next time GameStarted() is called, an incomplete game will automatically be recorded.
	/// </summary>
	/// <param name="tableID">Required only if there are multiple Analytics tables.  Case, white-space and symbols are ignored.</param>
	static public void AbortGame(string tableID)
	{
		FindInstance(tableID).AbortCurrentGame();
	}
	
	private void AbortCurrentGame()
	{
		_gameInProgress = false;
	}



	/// <summary>
	/// Get a specified Analytics Metric.
	/// </summary>
	/// <param name="metricName">The name of the metric.  Case, white-space and symbols are ignored.</param>
	static public AnalyticsMetric Metric(string metricName)
	{
		var metric = GetInstance().FindMetric(metricName);
		if (metric != null) return metric;
		throw new Exception($"[ANALYTICS]  Metric not found: \"{metricName}\"");
	}

	/// <summary>
	/// Get a specified Analytics Metric.
	/// </summary>
	/// <param name="tableID">Required only if there are multiple Analytics tables.  Case, white-space and symbols are ignored.</param>
	/// <param name="metricName">The name of the metric.  Case, white-space and symbols are ignored.</param>
	static public AnalyticsMetric Metric(string tableID, string metricName)
	{
		return FindInstance(tableID).FindMetric(metricName);
	}
	
	private AnalyticsMetric FindMetric(string metricName)
	{
		foreach (var metric in _metrics)
		{
			if (!metric.IsNamed(metricName)) continue;
			return metric;
		}
		
		if (string.IsNullOrEmpty(_tableID))
			throw new Exception($"[ANALYTICS]  Metric not found: \"{metricName}\"");
		throw new Exception($"[ANALYTICS]  Metric not found: \"{metricName}\" in table \"{_tableID}\"");
	}
	
	
	//TODO  Add GameDuration(tableID) and GameInProgress(tableID)
	

	private void WriteToDataFile()
	{
		try
		{
			var path = DataFilePath;
			var exists = File.Exists(path);
			using (StreamWriter file = File.AppendText(path))
			{
				var len = _metrics.Length;
				var data = new StringBuilder();
				if (!exists)
				{
					for (var i = 0; i < len; i++)
					{
						var metric = _metrics[i];
						data.Append(metric.name);
						if (i < len - 1) data.Append((char)_separator);
					}
					file.WriteLine(data.ToString());
					data.Clear();
				}
				for (var i = 0; i < len; i++)
				{
					var metric = _metrics[i];
					data.Append(metric);
					if (i < len - 1) data.Append((char)_separator);
				}
				file.WriteLine(data.ToString());
			}
		}
		catch (Exception e)
		{
			Debug.LogException(e);
		}
	}
	
	
	public string DataFilePath
	{
		get
		{
			var folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
			var path = Path.Combine(folder, _localFilename);
			return path;
		}
	}


	private void SendDataToOnlineForm()
	{
		if (string.IsNullOrEmpty(_googleFormURL))
		{
			Debug.Log("[ANALYTICS]  Google Form URL not set.  Skipping online submission.");
			return;
		}
		if (_sending != null)
		{
			Debug.LogError("[ANALYTICS]  Could not send analytics!");
			StopCoroutine(_sending);
		}
		_sending = StartCoroutine(SubmitUntilSent());
	}


	private IEnumerator SubmitUntilSent()
	{
		do
		{
			StartCoroutine(SubmitGameData());
			if (_sending != null) yield return new WaitForSeconds(_retryDelay);
		} while (_sending != null);
	}


	private IEnumerator SubmitGameData()
	{
		UnityWebRequest www = null;
		try
		{
			var form = new WWWForm();
			foreach (var metric in _metrics)
			{
				form.AddField(metric.formID, metric.ToString());
			}
			www = UnityWebRequest.Post(new Uri(_googleFormURL), form);
		}
		catch (Exception e)
		{
			Debug.LogException(e);
		}

		if (www == null) yield break;
		using (www)
		{
			yield return www.SendWebRequest();
			if (www.result == UnityWebRequest.Result.Success)
			{
				_sending = null;
				Debug.Log($"[ANALYTICS]  Game data sent!");
			}
			else
			{
				Debug.Log($"[ANALYTICS]  Failed to send!  {www.error}");
			}
		}
	}

}
using UnityEngine;
using Barliesque.Analytics;


public class AnalyticsTest : MonoBehaviour
{
	[SerializeField] private string _analyticsTableID;
	[SerializeField, Range(0, 1)] private float _progress;

	public void StartGame()
	{
		Analytics.GameStarted(_analyticsTableID, "Start Time");
		Analytics.Metric(_analyticsTableID, "Completed").StringValue = "Incomplete";
	}

	public void AbandonGame()
	{
		Analytics.GameEnded(_analyticsTableID, "Game Duration");
		// Alternatively, call AbortGame() to avoid recording an incomplete game
		//Analytics.AbortGame();
	}

	private void OnValidate()
	{
		try
		{
			var metric = Analytics.Metric(_analyticsTableID, "Progress");
			if (metric != null) metric.FloatValue = _progress;
		}
		catch
		{
			// OnValidate only runs in the editor and is used here for ease of demonstration only.
			// The Editor invokes it when the game starts, but before all components have initialized,
			// causing an error in Analytics.Metric()
		}
	}

	public void IncrementGoals()
	{
		Analytics.Metric(_analyticsTableID, "Goals").IntValue++;
	}

	public void IncrementPenalties()
	{
		Analytics.Metric(_analyticsTableID, "Penalties").IntValue++;
	}

	public void WinGame()
	{
		Analytics.Metric(_analyticsTableID, "Won").BoolValue = true;
		Analytics.Metric(_analyticsTableID, "Progress").FloatValue = _progress;
		Analytics.Metric(_analyticsTableID, "Completed").StringValue = "Complete";
		Analytics.GameEnded(_analyticsTableID, "Game Duration");
	}

	public void LoseGame()
	{
		Analytics.Metric(_analyticsTableID, "Won").BoolValue = false;
		Analytics.Metric(_analyticsTableID, "Progress").FloatValue = _progress;
		Analytics.Metric(_analyticsTableID, "Completed").StringValue = "Complete";
		Analytics.GameEnded(_analyticsTableID, "Game Duration");
	}
}
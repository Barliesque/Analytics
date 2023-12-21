using Barliesque.InspectorTools.Editor;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AnalyticsTest))]
public class AnalyticsTestEditor : EditorBase<AnalyticsTest>
{
	override protected void CustomInspector(AnalyticsTest inst)
	{
		PropertyField("_analyticsTableID");
		PropertyField("_progress");
		GUI.enabled = Application.isPlaying;
		if (GUILayout.Button("Start Game")) inst.StartGame();
		if (GUILayout.Button("Increment Goals")) inst.IncrementGoals();
		if (GUILayout.Button("Increment Penalties")) inst.IncrementPenalties();
		if (GUILayout.Button("Abandon Game")) inst.AbandonGame();
		if (GUILayout.Button("Win Game")) inst.WinGame();
		if (GUILayout.Button("Lose Game")) inst.LoseGame();
	}
}

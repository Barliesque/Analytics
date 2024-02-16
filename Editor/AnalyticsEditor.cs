using Barliesque.InspectorTools.Editor;
using Barliesque.Utils;
using UnityEditor;
using UnityEngine;

namespace Barliesque.Analytics.Editor
{
	[CustomEditor(typeof(Analytics))]
	public class AnalyticsEditor : EditorBase<Analytics>
	{
		private ListEditor _metrics;
		private bool _showGoogleHelp;
		private bool _showFormatHelp;

		override protected bool ShowScriptField => false;

		override protected void OnEnabled(Analytics inst)
		{
			_metrics = new ListEditor(serializedObject, "_metrics", "Metrics", 2);
			_metrics.OnElementAdded += InitNewElement;
		}

		private void InitNewElement(SerializedProperty obj)
		{
			var format = obj.FindPropertyRelative("_format");
			format.stringValue = null;
			var formatTrue = obj.FindPropertyRelative("_formatTrue");
			formatTrue.stringValue = "Yes";
			var formatFalse = obj.FindPropertyRelative("_formatFalse");
			formatFalse.stringValue = "No";
			var name = obj.FindPropertyRelative("_name");
			name.stringValue = null;
			var formID = obj.FindPropertyRelative("_formID");
			formID.stringValue = null;
		}

		override protected void CustomInspector(Analytics inst)
		{
			PropertyField("_persistent");
			PropertyField("_tableID");
			EditorGUILayout.Space();

			var saveContext = ContextSelect("_saveToLocalFile");
			if (saveContext != 0)
			{
				EditorGUILayout.BeginHorizontal();
				PropertyField("_localFilename");
				if (GUILayout.Button("Open", GUILayout.Width(60f)))
				{
					Application.OpenURL(inst.DataFilePath);
				}

				if (GUILayout.Button("Find", GUILayout.Width(60f)))
				{
					EditorUtility.RevealInFinder(inst.DataFilePath);
				}

				EditorGUILayout.EndHorizontal();
				PropertyField("_separator", "Data Separator");
				EditorGUILayout.Space();
			}

			var sendContext = ContextSelect("_sendToGoogle");
			if (sendContext != 0)
			{
				PropertyField("_googleFormURL");
				PropertyField("_retryDelay");
				EditorGUILayout.Space();
			}

			EditorGUILayout.Space();

			GUI.enabled = !Application.isPlaying;
			_metrics.DoLayoutList();
			GUI.enabled = true;

			if (saveContext == 0 && sendContext == 0) return;

			EditorGUILayout.Space();
			if (EditorTools.BeginFoldout("Help with formatting", ref _showFormatHelp))
			{
				EditorTools.HelpBox("Leaving the Format field empty uses a default format which works well in most cases.  " +
				                    "Here are some examples of custom formats that may come in handy.\n\n" +
				                    "Fixed-point\tF2\t\t123.45  67.00  -4.32\n\n" +
				                    "Percentage\t0%\t\t7%  32%  100%\n" +
				                    "\t\t0.0%\t\t6.9%  32.1%  100.0%\n" +
				                    "\t\t0.##%\t\t6.89%  32.1%  100%\n\n" +
				                    "Currency\t\t$0.00\t\t$5.99  $500.10  $3600.00\n" +
				                    "\t\tC\t\t$5.99  \u00a3500.10  \u00a53600  <i>(local currency assumed)</i>\n\n" +
				                    "Time\t\th\\:mm\\:ss\t3:09:26\n" +
				                    "\t\tm\\:ss.f\t\t9:26.3\n" +
				                    "\t\ts.fff\t\t26.317");

				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.Space();
				if (GUILayout.Button("More help on www.microsoft.com"))
				{
					Application.OpenURL("https://learn.microsoft.com/en-us/dotnet/standard/base-types/formatting-types");
				}

				EditorGUILayout.Space();
				EditorGUILayout.EndHorizontal();
			}

			EditorTools.EndFoldout(_showFormatHelp);

			if (sendContext == 0) return;

			if (EditorTools.BeginFoldout("Help setting up Google Form", ref _showGoogleHelp))
			{
				EditorTools.HelpBox(@"- All data will be sent as strings, so use the type ""Short Answer"" for each of the form questions.
- When completed, open a preview of the form and view the page source html
    - Search for ""<form action="" to locate the submission URL.
    - Copy and paste that into the Google Form URL field above.
- Click the three-dots menu button and ""Get pre-filled link""
    - Right-click on each answer field and Inspect.
    - Find the name property of an <input> element, e.g.:  name=""entry.1772648449""
    - Copy and paste the numeric part into the corresponding ID field above.
- In the form settings, under ""Responses"" deselect all options requiring sign in.
- To link form results to a spreadsheet, click on the Responses tab and ""Link to Sheets""
");

				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.Space();
				if (GUILayout.Button("More help on YouTube"))
				{
					Application.OpenURL("https://www.youtube.com/watch?v=z9b5aRfrz7M");
				}

				EditorGUILayout.Space();
				EditorGUILayout.EndHorizontal();
			}

			EditorTools.EndFoldout(_showGoogleHelp);
		}

		private int ContextSelect(string field)
		{
			var prop = GetProperty(field);
			var inEditor = (prop.intValue & (int)Analytics.Context.Editor) > 0;
			var atRuntime = (prop.intValue & (int)Analytics.Context.Runtime) > 0;
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel(field[1..].SplitCamelCase().ToTitleCase());
			EditorGUILayout.LabelField("Editor", GUILayout.Width(40f));
			inEditor = EditorGUILayout.Toggle(inEditor, GUILayout.Width(20f));
			EditorGUILayout.Space(10f, false);
			EditorGUILayout.LabelField("Runtime", GUILayout.Width(60f));
			atRuntime = EditorGUILayout.Toggle(atRuntime, GUILayout.Width(20f));
			EditorGUILayout.EndHorizontal();
			if (!EditorGUI.EndChangeCheck()) return prop.intValue;
			prop.intValue = (inEditor ? (int)Analytics.Context.Editor : 0) | (atRuntime ? (int)Analytics.Context.Runtime : 0);
			return prop.intValue;
		}
	}
}
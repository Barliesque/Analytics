using Barliesque.InspectorTools.Editor;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(AnalyticsMetric))]
public class AnalyticsMetricDrawer : PropertyDrawerHelper
{

	override protected int LinesPerElement => 2;

	override public void CustomDrawer()
	{
		Space(5f);
		var totalWidth = _position.width;
		
		Field(160f, "_name");
		var type = (AnalyticsMetric.ValueType)Field(100f, "_type").intValue;
		Field(20f, "ID", -1, "_formID");

		NextLine();
		Space(5f);

		var fieldWidth = totalWidth - 86f - 26f - 48f - 80f - 8f;
		if (Application.isPlaying)
		{
			Field(86f, "Current Value", fieldWidth, "_stringValue");
		}
		else
		{
			Space(fieldWidth - 16f);
		}

		if (type == AnalyticsMetric.ValueType._bool)
		{
			Field(32f, "True", 80f, "_formatTrue", "Optional custom string for True state");
			Field(38f, "False", 80f, "_formatFalse", "Optional custom string for False state");
		}
		else if (type != AnalyticsMetric.ValueType._string)
		{
			Field(48f, "Format", -1, "_format", "Optional string format");
		}
	}
	
}

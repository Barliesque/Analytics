using System;
using System.Globalization;
using Barliesque.Utils;
using UnityEngine;


[Serializable]
public class AnalyticsMetric
{

	[SerializeField] private string _name;
	[SerializeField] private ValueType _type;
	[Tooltip("Entry ID found in the Google Form page source (numeric part only.)")]
	[SerializeField] private string _formID;

	[SerializeField] private bool _boolValue;
	[SerializeField] private int _intValue;
	[SerializeField] private float _floatValue;
	[SerializeField] private string _stringValue;
	private DateTime _dateTimeValue; // String value copied into _stringValue for inspection
	private TimeSpan _timeSpanValue; // String value copied into _stringValue for inspection

	[SerializeField] private string _format;
	[SerializeField] private string _formatTrue;
	[SerializeField] private string _formatFalse;
	

	public enum ValueType
	{
		_int, _float, _bool, _string, _dateTime, _timeSpan
	}

	public ValueType type => _type;
	public string name => _name;
	public string formID => $"entry.{_formID}";

	/// <summary>
	/// Compares the name of this metric, ignoring case, whitespace and symbols
	/// </summary>
	public bool IsNamed(string value)
	{
		return string.Compare(_name, value, CultureInfo.InvariantCulture, CompareOptions.IgnoreCase | CompareOptions.IgnoreSymbols) == 0;
	}
	
	
	//TODO  Add isAssigned to AnalyticsMetric


	public bool BoolValue
	{
		get => _boolValue;
		set
		{
			if (_type != ValueType._bool)
			{
				Debug.LogError($"Analytics cannot set {_name} to a bool value, because it's type is: {_type.ToString()[1..].ToTitleCase()}");
				return;
			}
			_boolValue = value;
			_stringValue = ToString();  // Formatted string value copied into _stringValue for inspection
		}
	}

	public int IntValue
	{
		get => _intValue;
		set
		{
			if (_type != ValueType._int)
			{
				Debug.LogError($"Analytics cannot set {_name} to an int value, because it's type is: {_type.ToString()[1..].ToTitleCase()}");
				return;
			}
			_intValue = value;
			_stringValue = ToString();  // Formatted string value copied into _stringValue for inspection
		}
	}
	
	public float FloatValue
	{
		get => _floatValue;
		set
		{
			if (_type != ValueType._float)
			{
				Debug.LogError($"Analytics cannot set {_name} to a float value, because it's type is: {_type.ToString()[1..].ToTitleCase()}");
				return;
			}
			_floatValue = value;
			_stringValue = ToString();  // Formatted string value copied into _stringValue for inspection
		}
	}
	
	public string StringValue
	{
		get => _stringValue;
		set
		{
			if (_type != ValueType._string)
			{
				Debug.LogError($"Analytics cannot set {_name} to a string value, because it's type is: {_type.ToString()[1..].ToTitleCase()}");
				return;
			}
			_stringValue = value;
		}
	}
	
	public DateTime DateTimeValue
	{
		get => _dateTimeValue;
		set
		{
			if (_type != ValueType._dateTime)
			{
				Debug.LogError($"Analytics cannot set {_name} to a DateTime value, because it's type is: {_type.ToString()[1..].ToTitleCase()}");
				return;
			}
			_dateTimeValue = value;
			_stringValue = ToString();  // Formatted string value copied into _stringValue for inspection
		}
	}
	
	public TimeSpan TimeSpanValue
	{
		get => _timeSpanValue;
		set
		{
			if (_type != ValueType._timeSpan)
			{
				Debug.LogError($"Analytics cannot set {_name} to a TimeSpan value, because it's type is: {_type.ToString()[1..].ToTitleCase()}");
				return;
			}
			_timeSpanValue = value;
			_stringValue = ToString();  // Formatted string value copied into _stringValue for inspection
		}
	}


	public void Reset()
	{
		_boolValue = false;
		_intValue = 0;
		_floatValue = 0f;
		_stringValue = null;
		_dateTimeValue = DateTime.MinValue;
		_timeSpanValue = TimeSpan.Zero;
	}


	override public string ToString()
	{
		return _type switch
		{
			ValueType._bool => string.IsNullOrEmpty(_formatTrue) ? _boolValue.ToString() : (_boolValue ? _formatTrue : _formatFalse),
			ValueType._int => _intValue.ToString(_format),
			ValueType._float => _floatValue.ToString(string.IsNullOrEmpty(_format) ? "0.00" : _format, CultureInfo.InvariantCulture),
			ValueType._string => string.IsNullOrEmpty(_stringValue) ? "" : _stringValue,
			ValueType._dateTime => _dateTimeValue.ToString(string.IsNullOrEmpty(_format) ? "G" : _format),
			ValueType._timeSpan => _timeSpanValue.ToString(string.IsNullOrEmpty(_format) ? @"h\:mm\:ss" : _format),
			_ => throw new Exception($"Unknown AnalyticsMetric type: {_type}")
		};
	}

}
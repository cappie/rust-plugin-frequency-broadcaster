using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;

namespace Oxide.Plugins
{
	[Info("rust-plugin-frequency-broadcaster", "cappie", "1.0.0")]
	[Description("Broadcast a frequency on a specific event.")]
	internal class FrequencyBroadcaster : RustPlugin
	{
		private PluginConfigData _configData;
		private List<IRFObject> _nightFrequencyListeners;

		/// <summary>
		///     Load default configuration.
		/// </summary>
		protected override void LoadDefaultConfig()
		{
			Config.Settings.DefaultValueHandling = DefaultValueHandling.Populate;
			Config.WriteObject(new PluginConfigData());

			Puts("Default configuration created.");
		}

		protected override void LoadConfig()
		{
			base.LoadConfig();

			Config.Settings.DefaultValueHandling = DefaultValueHandling.Populate;
			_configData = Config.ReadObject<PluginConfigData>();

			Config.WriteObject(_configData);
		}

		/// <summary>
		///     Event fired upon loading the plugin.
		/// </summary>
		private void Init()
		{
			_configData = Config.ReadObject<PluginConfigData>();
		}

		private void OnServerInitialized()
		{
			_nightFrequencyListeners = RFManager.GetListenList(_configData.NightFrequency);
		}

		private void OnRfListenerAdded()
		{
			OnServerInitialized();
		}

		private void OnRfListenerRemoved()
		{
			OnServerInitialized();
		}

		private void OnTimeSunset()
		{
			foreach (var listener in _nightFrequencyListeners) listener.RFSignalUpdate(true);
			Puts($"Started broadcasting at frequency {_configData.NightFrequency} because of sunset.");
		}

		private void OnTimeSunrise()
		{
			foreach (var listener in _nightFrequencyListeners) listener.RFSignalUpdate(false);
			Puts($"Stopped broadcasting at frequency {_configData.NightFrequency} because of sunrise.");
		}

		/// <summary>
		///     Configuration object for the plugin.
		/// </summary>
		private class PluginConfigData
		{
			[DefaultValue(4760)]
			[JsonProperty(PropertyName = "nightFrequency")]
			public int NightFrequency { get; set; }
		}
	}
}

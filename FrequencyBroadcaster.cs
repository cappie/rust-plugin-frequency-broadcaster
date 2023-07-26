using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;

namespace Oxide.Plugins
{
	[Info("Frequency Broadcaster", "cappie", "1.1.0")]
	[Description("Broadcasts frequencies for specific events.")]
	public class FrequencyBroadcaster : RustPlugin
	{
		#region Variables
		//[PluginReference] Plugin TimeOfDay;
		private PluginConfigData _configData;

		//private bool Active;
		private bool isDayTime = false;

		private List<IRFObject> _dayFrequencyListeners;
		private List<IRFObject> _nightFrequencyListeners;

		#endregion

		/// <summary>
		///     Load default configuration.
		/// </summary>
		#region Data
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
			//VerifyDependency();
			SetupListeners();
		}

		#endregion

		#region Functions
		/*
        private void VerifyDependency()
        {
            if (TimeOfDay)
                Active = true;
            else
            {
                PrintWarning(GetMsg("noTOD"));
                Active = false;
            }
        }
		*/

		private void SetupListeners() {
			_dayFrequencyListeners = RFManager.GetListenList(_configData.DayFrequency);
			_nightFrequencyListeners = RFManager.GetListenList(_configData.NightFrequency);
		}

		private void OnRfListenerAdded(IRFObject obj, int frequency)
		{
			SetupListeners();

			// if the listener has tuned into the day frequency...
			if (frequency == _configData.DayFrequency) {
				if (isDayTime) {
					foreach (var listener in _dayFrequencyListeners) {
						listener.RFSignalUpdate(true);
					}
				}
				Puts($"A listener for the day frequency was added.");
			}
			
			// if the listener has tuned into the night frequency...
			if (frequency == _configData.NightFrequency) {
				if (!isDayTime) {
					foreach (var listener in _nightFrequencyListeners) {
						listener.RFSignalUpdate(true);
					}
				}
				Puts($"A listener for the night frequency was added.");
			}
			OnServerInitialized();
		}

		private void OnRfListenerRemove(IRFObject obj, int frequency)
		{
			if (frequency == _configData.DayFrequency) {
				Puts($"A listener for the day frequency was removed.");
			}
			if (frequency == _configData.NightFrequency) {
				Puts($"A listener for the night frequency was removed.");
			}
			OnServerInitialized();
		}

		private void OnTimeSunset()
		{
			isDayTime = false;
			// timeComponent.DayLengthInMinutes = dayLength * (24.0f / (TOD_Sky.Instance.SunsetTime - TOD_Sky.Instance.SunriseTime));
			foreach (var listener in _dayFrequencyListeners) {
				listener.RFSignalUpdate(false);
			}
			foreach (var listener in _nightFrequencyListeners) {
				listener.RFSignalUpdate(true);
			}
			Puts($"Stopped broadcasting at frequency {_configData.DayFrequency} because of sunset.");
			Puts($"Started broadcasting at frequency {_configData.NightFrequency} because of sunset.");
		}

		private void OnTimeSunrise()
		{
			isDayTime = true;
			
			// timeComponent.DayLengthInMinutes = dayLength * (24.0f / (TOD_Sky.Instance.SunsetTime - TOD_Sky.Instance.SunriseTime));
			foreach (var listener in _dayFrequencyListeners) {
				listener.RFSignalUpdate(true);
			}
			foreach (var listener in _nightFrequencyListeners) {
				listener.RFSignalUpdate(false);
			}
			Puts($"Stopped broadcasting at frequency {_configData.NightFrequency} because of sunrise.");
			Puts($"Started broadcasting at frequency {_configData.DayFrequency} because of sunrise.");
		}

		/// <summary>
		///     Configuration object for the plugin.
		/// </summary>
		private class PluginConfigData
		{
			[DefaultValue(4760)]
			[JsonProperty(PropertyName = "nightFrequency")]
			public int NightFrequency { get; set; }

			[DefaultValue(4761)]
			[JsonProperty(PropertyName = "dayFrequency")]
			public int DayFrequency { get; set; }
		}

		#endregion

        #region Messaging
        private void SendMsg(BasePlayer player, string message, string keyword) => SendReply(player, $"<color=orange>{keyword}</color><color=#939393>{message}</color>");
        private string GetMsg(string key) => lang.GetMessage(key, this);
        protected override void LoadDefaultMessages()
        {
            lang.RegisterMessages(Messages, this);
        }
        Dictionary<string, string> Messages = new Dictionary<string, string>
        {
            {"noTOD", "Unable to find TimeOfDay, unable to proceeed"},
			{"started", "Started broadcasting at frequency {0} because of {1}."},
			{"stopped", "Stopped broadcasting at frequency {0} because of {1}."},
			{"sunrise", "sunrise"},
			{"sunset", "sunset"}
        };

        #endregion
	}
}

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using IPA.Config.Stores;
using IPA.Config.Stores.Attributes;
using IPA.Config.Stores.Converters;

[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]
namespace CustomAlerts.Configuration
{
	internal class Config
	{
		[NonNullable]
		public virtual Tokens Tokens { get; set; } = new Tokens();

		[NonNullable]
		public virtual Twitch Twitch { get; set; } = new Twitch();

		[NonNullable, UseConverter(typeof(ListConverter<AlertValue>))]
		public virtual List<AlertValue> Alerts { get; set; } = new List<AlertValue>();

		public virtual float AlertDelay { get; set; } = 2f;
	}
}
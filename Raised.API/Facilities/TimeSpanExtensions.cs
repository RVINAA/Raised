using Raised.API.Facilities;

namespace System
{
	internal static class TimeSpanExtensions
	{
		public static TimeSpan Mean(this ICollection<TimeSpan> source)
		{
			Guard.IsNotNull(source, nameof(source));

			var mean = 0L;
			var remainder = 0L;
			var n = source.Count;

			foreach (var item in source)
			{
				var ticks = item.Ticks;
				mean += ticks / n;
				remainder += ticks % n;
				mean += remainder / n;
				remainder %= n;
			}

			return TimeSpan.FromTicks(mean);
		}

		public static string Prettify(this TimeSpan @this)
			=> string.Format("{0:D2}h:{1:D2}m:{2:D2}s:{3:D3}ms", @this.Hours, @this.Minutes, @this.Seconds, @this.Milliseconds);
	}
}

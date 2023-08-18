namespace Raised.API.Facilities
{
	internal static class Guard
	{
		public static void IsNotNull(object obj, string message)
		{
			if (obj == null)
				throw new ArgumentException(message);
		}

		public static void IsNotNullOrWhiteSpace(string str, string message)
		{
			if (string.IsNullOrWhiteSpace(str))
				throw new ArgumentException(message);
		}

		public static void IsNotDefault<T>(T obj, string message)
		{
			if (EqualityComparer<T>.Default.Equals(obj, default))
				throw new ArgumentException(message);
		}

		public static void Against<TException>(bool assertion, string message, params object[] args)
			where TException : Exception
		{
			if (assertion)
			{
				message = args != null ? string.Format(message, args) : message;
				throw (TException)Activator.CreateInstance(typeof(TException), message);
			}
		}

		public static T ThrowIfNull<T>(this T @this, string message = null)
			=> @this ?? throw new ArgumentException(message);

		public static string ThrowIfNullOrWhiteSpace(this string str, string message = null)
		{
			if (string.IsNullOrWhiteSpace(str))
				throw new ArgumentException(message);

			return str;
		}
	}
}

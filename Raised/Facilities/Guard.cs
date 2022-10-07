namespace Raised.Facilities
{
	internal static class Guard
	{
		public static void IsNotNull(object obj, string message)
		{
			if (obj == null)
				throw new ArgumentNullException(message);
		}

		public static void IsNotNullOrWhiteSpace(string obj, string message)
		{
			if (string.IsNullOrWhiteSpace(obj))
				throw new ArgumentNullException(message);
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

		public static T ThrowIfNull<T>(this T obj, string message = null)
		{
			if (obj == null)
				throw new ArgumentNullException(message);

			return obj;
		}

		public static string ThrowIfNullOrWhiteSpace(this string obj, string message = null)
		{
			if (string.IsNullOrWhiteSpace(obj))
				throw new ArgumentNullException(message);

			return obj;
		}
	}
}

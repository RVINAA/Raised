namespace Raised.Facilities
{
	internal static class SwallowExtensions
	{
		public static void Execute(Action operation)
		{
			try
			{
				operation?.Invoke();
			}
			catch { }
		}
	}
}

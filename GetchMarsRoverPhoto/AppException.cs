using System;

namespace GetchMarsRoverPhoto
{
	// generic application exception
	class AppException : Exception
	{
		public AppException (string Message, Exception InnerException = null)
			: base (Message, InnerException)
		{
		}
	}

	// NASA API related exception
	class NasaApiException : AppException
	{
		public NasaApiException (string Message, Exception InnerException = null)
			: base (Message, InnerException)
		{
		}
	}

	// NASA API access problem
	class NasaApiForbiddenException : NasaApiException
	{
		public NasaApiForbiddenException (Exception InnerException = null)
			: base ("NASA API returns 'Forbidden' status. Must be wrong API key.", InnerException)
		{
		}
	}

	// NASA API photo downloading problem
	class NasaApiImageException : NasaApiException
	{
		public NasaApiImageException (string ImageUrl, Exception InnerException = null)
			: base ($"Failed to download photo at {ImageUrl}", InnerException)
		{
		}
	}
}

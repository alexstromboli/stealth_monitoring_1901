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

	// wrong/inconsistent command line arguments
	class ArgumentsException : AppException
	{
		public ArgumentsException (string Message, Exception InnerException = null)
			: base (Message, InnerException)
		{
		}
	}

	// missing command line arguments
	class NoArgumentsException : AppException
	{
		public NoArgumentsException (Exception InnerException = null)
			: base ("Missing commmand line arguments.", InnerException)
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

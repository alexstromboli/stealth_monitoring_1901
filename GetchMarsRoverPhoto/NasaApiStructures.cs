using System;
using System.Linq;
using System.Collections.Generic;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GetchMarsRoverPhoto.NasaApi
{
	class DateFormatConverter : IsoDateTimeConverter
	{
		public DateFormatConverter(string Format)
		{
			DateTimeFormat = Format;
		}
	}

	class Rover
	{
		[JsonProperty("id")]
		public int Id;

		[JsonProperty("name")]
		public string Name;
	}

	class Camera
	{
		[JsonProperty("id")]
		public int Id;

		[JsonProperty("name")]
		public string Name;
	}

	class Photo
	{
		[JsonProperty("id")]
		public int Id;

		[JsonProperty("earth_date")]
		[JsonConverter(typeof(DateFormatConverter), "yyyy-MM-dd")]
		public DateTime EarthDate;

		[JsonProperty("img_src")]
		public string ImageUrl;

		[JsonProperty("camera")]
		public Camera Camera;

		[JsonProperty("rover")]
		public Rover Rover;
	}

	class DaySummary
	{
		[JsonProperty("photos")]
		public Photo[] Photos;
	}
}

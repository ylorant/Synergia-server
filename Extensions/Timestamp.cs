using System;

namespace MMORPGServer
{
	public struct Timestamp
	{
		public static double Convert(DateTime value)
		{
		    //create Timespan by subtracting the value provided from
		    //the Unix Epoch
		    TimeSpan span = (value - new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime());
		
		    //return the total seconds (which is a UNIX timestamp)
		    return (double)span.TotalSeconds;
		}
	}
}


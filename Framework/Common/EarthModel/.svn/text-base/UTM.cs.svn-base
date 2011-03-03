

using System;
namespace Magic.Common.EarthModel
{
	public static class UTM
	{

		const long UTM_NO_ERROR = 0x0000;
		const long UTM_LAT_ERROR = 0x0001;
		const long UTM_LON_ERROR = 0x0002;
		const long UTM_EASTING_ERROR = 0x0004;
		const long UTM_NORTHING_ERROR = 0x0008;
		const long UTM_ZONE_ERROR = 0x0010;
		const long UTM_HEMISPHERE_ERROR = 0x0020;
		const long UTM_ZONE_OVERRIDE_ERROR = 0x0040;
		const long UTM_A_ERROR = 0x0080;
		const long UTM_INV_F_ERROR = 0x0100;

		const double MIN_LAT = ((-80.5 * Math.PI) / 180.0); /* -80.5 degrees in radians    */
		const double MAX_LAT = ((84.5 * Math.PI) / 180.0);  /* 84.5 degrees in radians     */
		const double MIN_EASTING = 100000;
		const double MAX_EASTING = 900000;
		const double MIN_NORTHING = 0;
		const double MAX_NORTHING = 10000000;
		static double UTM_a = 6378137.0;         /* Semi-major axis of ellipsoid in meters  */
		static double UTM_f = 1 / 298.257223563; /* Flattening of ellipsoid                 */
		static long UTM_Override = 0;          /* Zone override flag                      */


		public static void LatLon2UTM(double lat, double lon, out double utmX, out double utmY)
		{
			// Constants - WGS-84(?)	
			double sa = 6378137.000000;
			double sb = 6356752.314245;

			double e2 = (Math.Sqrt((sa * sa) - (sb * sb))) / sb;
			double e2cuadrada = e2 * e2;
			double c = (sa * sa) / sb;

			lat = lat * (Math.PI / 180.0);
			lon = lat * (Math.PI / 180.0);

			int Huso = (int)((lon / 6.0) + 31.0);
			double S = (((double)Huso * 6.0) - 183.0);
			double deltaS = lon - (S * (Math.PI / 180.0));
			double a = Math.Cos(lat) * Math.Sin(deltaS);
			double epsilon = 0.5 * Math.Log((1.0 + a) / (1.0 - a));
			double nu = Math.Atan(Math.Tan(lat) / Math.Cos(deltaS)) - lat;
			double v = (c / Math.Sqrt((1.0 + (e2cuadrada * (Math.Cos(lat) * Math.Cos(lat)))))) * 0.9996;
			double ta = (e2cuadrada / 2.0) * (epsilon * epsilon) * (Math.Cos(lat) * Math.Cos(lat));
			double a1 = Math.Sin(2.0 * lat);
			double a2 = a1 * (Math.Cos(lat) * Math.Cos(lat));
			double j2 = lat + (a1 / 2.0);
			double j4 = ((3.0 * j2) + a2) / 4.0;
			double j6 = ((5.0 * j4) + (a2 * (Math.Cos(lat) * Math.Cos(lat)))) / 3.0;
			double alfa = (3.0 / 4.0) * e2cuadrada;
			double beta = (5.0 / 3.0) * (alfa * alfa);
			double gama = (35.0 / 27.0) * (alfa * alfa * alfa);
			double Bm = 0.9996 * c * (lat - alfa * j2 + beta * j4 - gama * j6);
			utmX = epsilon * v * (1.0 + (ta / 3.0)) + 500000.0;
			utmY = nu * v * (1.0 + ta) + Bm;

		}

		static long Set_UTM_Parameters(double a, double f, long paramOverride)
		{
			/*
			 * The function Set_UTM_Parameters receives the ellipsoid parameters and
			 * UTM zone override parameter as inputs, and sets the corresponding state
			 * variables.  If any errors occur, the error code(s) are returned by the 
			 * function, otherwise UTM_NO_ERROR is returned.
			 *
			 *    a                 : Semi-major axis of ellipsoid, in meters       (input)
			 *    f                 : Flattening of ellipsoid						            (input)
			 *    override          : UTM override zone, zero indicates no override (input)
			 */

			double inv_f = 1 / f;
			long Error_Code = UTM_NO_ERROR;

			if (a <= 0.0)
			{ /* Semi-major axis must be greater than zero */
				Error_Code |= UTM_A_ERROR;
			}
			if ((inv_f < 250) || (inv_f > 350))
			{ /* Inverse flattening must be between 250 and 350 */
				Error_Code |= UTM_INV_F_ERROR;
			}
			if ((paramOverride < 0) || (paramOverride > 60))
			{
				Error_Code |= UTM_ZONE_OVERRIDE_ERROR;
			}
			if (Error_Code==0)
			{ /* no errors */
				UTM_a = a;
				UTM_f = f;
				UTM_Override = paramOverride;
			}
			return (Error_Code);
		} /* END OF Set_UTM_Parameters */


		static void Get_UTM_Parameters(out double a, out  double f, out long paramOverride)
		{
			/*
			 * The function Get_UTM_Parameters returns the current ellipsoid
			 * parameters and UTM zone override parameter.
			 *
			 *    a                 : Semi-major axis of ellipsoid, in meters       (output)
			 *    f                 : Flattening of ellipsoid						            (output)
			 *    override          : UTM override zone, zero indicates no override (output)
			 */

			a = UTM_a;
			f = UTM_f;
			paramOverride = UTM_Override;
		} /* END OF Get_UTM_Parameters */


		public static long Convert_Geodetic_To_UTM(double Latitude,
																	double Longitude,
																	out long Zone,
																	out char Hemisphere,
																	out double Easting,
																	out double Northing)
		{
			/*
			 * The function Convert_Geodetic_To_UTM converts geodetic (latitude and
			 * longitude) coordinates to UTM projection (zone, hemisphere, easting and
			 * northing) coordinates according to the current ellipsoid and UTM zone
			 * override parameters.  If any errors occur, the error code(s) are returned
			 * by the function, otherwise UTM_NO_ERROR is returned.
			 *
			 *    Latitude          : Latitude in radians                 (input)
			 *    Longitude         : Longitude in radians                (input)
			 *    Zone              : UTM zone                            (output)
			 *    Hemisphere        : North or South hemisphere           (output)
			 *    Easting           : Easting (X) in meters               (output)
			 *    Northing          : Northing (Y) in meters              (output)
			 */

			long Lat_Degrees;
			long Long_Degrees;
			long temp_zone;
			long Error_Code = UTM_NO_ERROR;
			double Origin_Latitude = 0;
			double Central_Meridian = 0;
			double False_Easting = 500000;
			double False_Northing = 0;
			double Scale = 0.9996;
			Zone = -1;
			Hemisphere = '-';
			Easting = 0;
			Northing = 0;
			if ((Latitude < MIN_LAT) || (Latitude > MAX_LAT))
			{ /* Latitude out of range */
				Error_Code |= UTM_LAT_ERROR;
			}
			if ((Longitude < -Math.PI) || (Longitude > (2 * Math.PI)))
			{ /* Longitude out of range */
				Error_Code |= UTM_LON_ERROR;
			}
			if (Error_Code==0)
			{ /* no errors */
				if ((Latitude > -1.0e-9) && (Latitude < 0))
					Latitude = 0.0;
				if (Longitude < 0)
					Longitude += (2 * Math.PI) + 1.0e-10;

				Lat_Degrees = (long)(Latitude * 180.0 / Math.PI);
				Long_Degrees = (long)(Longitude * 180.0 / Math.PI);

				if (Longitude < Math.PI)
					temp_zone = (long)(31 + ((Longitude * 180.0 / Math.PI) / 6.0));
				else
					temp_zone = (long)(((Longitude * 180.0 / Math.PI) / 6.0) - 29);

				if (temp_zone > 60)
					temp_zone = 1;
				/* UTM special cases */
				if ((Lat_Degrees > 55) && (Lat_Degrees < 64) && (Long_Degrees > -1)
						&& (Long_Degrees < 3))
					temp_zone = 31;
				if ((Lat_Degrees > 55) && (Lat_Degrees < 64) && (Long_Degrees > 2)
						&& (Long_Degrees < 12))
					temp_zone = 32;
				if ((Lat_Degrees > 71) && (Long_Degrees > -1) && (Long_Degrees < 9))
					temp_zone = 31;
				if ((Lat_Degrees > 71) && (Long_Degrees > 8) && (Long_Degrees < 21))
					temp_zone = 33;
				if ((Lat_Degrees > 71) && (Long_Degrees > 20) && (Long_Degrees < 33))
					temp_zone = 35;
				if ((Lat_Degrees > 71) && (Long_Degrees > 32) && (Long_Degrees < 42))
					temp_zone = 37;

				if (UTM_Override!=0)
				{
					if ((temp_zone == 1) && (UTM_Override == 60))
						temp_zone = UTM_Override;
					else if ((temp_zone == 60) && (UTM_Override == 1))
						temp_zone = UTM_Override;
					else if ((Lat_Degrees > 71) && (Long_Degrees > -1) && (Long_Degrees < 42))
					{
						if (((temp_zone - 2) <= UTM_Override) && (UTM_Override <= (temp_zone + 2)))
							temp_zone = UTM_Override;
						else
							Error_Code = UTM_ZONE_OVERRIDE_ERROR;
					}
					else if (((temp_zone - 1) <= UTM_Override) && (UTM_Override <= (temp_zone + 1)))
						temp_zone = UTM_Override;
					else
						Error_Code = UTM_ZONE_OVERRIDE_ERROR;
				}
				if (Error_Code==0)
				{
					if (temp_zone >= 31)
						Central_Meridian = (6 * temp_zone - 183) * Math.PI / 180.0;
					else
						Central_Meridian = (6 * temp_zone + 177) * Math.PI / 180.0;
					Zone = temp_zone;
					if (Latitude < 0)
					{
						False_Northing = 10000000;
						Hemisphere = 'S';
					}
					else
						Hemisphere = 'N';
					TransverseMercator.Set_Transverse_Mercator_Parameters(UTM_a, UTM_f, Origin_Latitude, Central_Meridian, False_Easting, False_Northing, Scale);
					TransverseMercator.Convert_Geodetic_To_Transverse_Mercator(Latitude, Longitude, out Easting, out Northing);
					if ((Easting < MIN_EASTING) || (Easting > MAX_EASTING))
						Error_Code = UTM_EASTING_ERROR;
					if ((Northing < MIN_NORTHING) || (Northing > MAX_NORTHING))
						Error_Code |= UTM_NORTHING_ERROR;
				}
			} /* END OF if (!Error_Code) */
			return (Error_Code);
		} /* END OF Convert_Geodetic_To_UTM */


		public static long Convert_UTM_To_Geodetic(long Zone,
																 char Hemisphere,
																 double Easting,
																 double Northing,
																 out double Latitude,
																 out double Longitude)
		{
			/*
			 * The function Convert_UTM_To_Geodetic converts UTM projection (zone, 
			 * hemisphere, easting and northing) coordinates to geodetic(latitude
			 * and  longitude) coordinates, according to the current ellipsoid
			 * parameters.  If any errors occur, the error code(s) are returned
			 * by the function, otherwise UTM_NO_ERROR is returned.
			 *
			 *    Zone              : UTM zone                               (input)
			 *    Hemisphere        : North or South hemisphere              (input)
			 *    Easting           : Easting (X) in meters                  (input)
			 *    Northing          : Northing (Y) in meters                 (input)
			 *    Latitude          : Latitude in radians                    (output)
			 *    Longitude         : Longitude in radians                   (output)
			 */
			long Error_Code = UTM_NO_ERROR;
			long tm_error_code = UTM_NO_ERROR;
			double Origin_Latitude = 0;
			double Central_Meridian = 0;
			double False_Easting = 500000;
			double False_Northing = 0;
			double Scale = 0.9996;
			Latitude = 0;
			Longitude = 0;
			if ((Zone < 1) || (Zone > 60))
				Error_Code |= UTM_ZONE_ERROR;
			if ((Hemisphere != 'S') && (Hemisphere != 'N'))
				Error_Code |= UTM_HEMISPHERE_ERROR;
			if ((Easting < MIN_EASTING) || (Easting > MAX_EASTING))
				Error_Code |= UTM_EASTING_ERROR;
			if ((Northing < MIN_NORTHING) || (Northing > MAX_NORTHING))
				Error_Code |= UTM_NORTHING_ERROR;
			if (Error_Code==0)
			{ /* no errors */
				if (Zone >= 31)
					Central_Meridian = ((6 * Zone - 183) * Math.PI / 180.0 /*+ 0.00000005*/);
				else
					Central_Meridian = ((6 * Zone + 177) * Math.PI / 180.0 /*+ 0.00000005*/);
				if (Hemisphere == 'S')
					False_Northing = 10000000;
				TransverseMercator.Set_Transverse_Mercator_Parameters(UTM_a, UTM_f, Origin_Latitude,
																					 Central_Meridian, False_Easting, False_Northing, Scale);

				tm_error_code = TransverseMercator.Convert_Transverse_Mercator_To_Geodetic(Easting, Northing, out Latitude, out Longitude);
				if (tm_error_code!=0)
				{
					if ((tm_error_code & TransverseMercator.TRANMERC_EASTING_ERROR) != 0)
						Error_Code |= UTM_EASTING_ERROR;
					if ((tm_error_code & TransverseMercator.TRANMERC_NORTHING_ERROR) != 0)
						Error_Code |= UTM_NORTHING_ERROR;
				}

				if ((Latitude < MIN_LAT) || (Latitude > MAX_LAT))
				{ /* Latitude out of range */
					Error_Code |= UTM_NORTHING_ERROR;
				}
			}
			return (Error_Code);
		} /* END OF Convert_UTM_To_Geodetic */


	}
}

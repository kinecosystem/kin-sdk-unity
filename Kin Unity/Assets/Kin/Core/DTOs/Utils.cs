using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


namespace Kin
{
	/// <summary>
	/// helper utilities
	/// </summary>
	public static class Utils
	{
		/// <summary>
		/// returns a random string
		/// </summary>
		/// <returns>The string.</returns>
		public static string RandomString()
		{
			return Path.GetRandomFileName().Replace( ".", "" );
		}


		/// <summary>
		/// adds obj to the list keyed on key only if it is not present
		/// </summary>
		/// <param name="Dictionary<string"></param>
		/// <param name="dict"></param>
		/// <param name="key"></param>
		/// <param name="value"></param>
		/// <typeparam name="T"></typeparam>
		public static void AddIfNotPresent<T>( this Dictionary<string, List<T>> dict, string key, T value )
		{
			if( !dict.ContainsKey( key ) )
				dict[key] = new List<T>();

			if( !dict[key].Contains( value ) )
				dict[key].Add( value );
		}


		/// <summary>
		/// removes obj from the list keyed on key only if it is present
		/// </summary>
		/// <param name="Dictionary<string"></param>
		/// <param name="dict"></param>
		/// <param name="key"></param>
		/// <param name="obj"></param>
		/// <typeparam name="T"></typeparam>
		public static void RemoveIfPresent<T>( this Dictionary<string, List<T>> dict, string key, T value )
		{
			if( !dict.ContainsKey( key ) )
				return;

			if( dict[key].Contains( value ) )
				dict[key].Remove( value );
		}


		public static void FireActionInDict<T,U>( this Dictionary<string,Action<T,U>> dict, string key, T param1, U param2 )
		{
			if( key == null )
			{
				Debug.LogWarning( "FireActionInDict received a null key!" );
				return;
			}

			if( dict.ContainsKey( key ) )
			{
				dict[key]( param1, param2 );
				dict.Remove( key );
			}
		}

        public static void FireActionInDict<T,U,X>(this Dictionary<string, Action<T,U,X>> dict,
         string key, T param, U param2, X param3)
        {
            if (key == null)
            {
                Debug.LogWarning("FireActionInDict received a null key!");
                return;
            }

            if (dict.ContainsKey(key))
            {
                dict[key](param, param2, param3);
                dict.Remove(key);
            }
        }

    }
}
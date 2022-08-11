using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Newtonsoft.Json;

namespace OpenAuditLog
{
    /// <summary>
    /// Commonly-used static methods.
    /// </summary>
    public static class Common
    {
        #region Input

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static bool InputBoolean(string question, bool yesDefault)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            Console.Write(question);

            if (yesDefault) Console.Write(" [Y/n]? ");
            else Console.Write(" [y/N]? ");

            string userInput = Console.ReadLine();

            if (String.IsNullOrEmpty(userInput))
            {
                if (yesDefault) return true;
                return false;
            }

            userInput = userInput.ToLower();

            if (yesDefault)
            {
                if (
                    (String.Compare(userInput, "n") == 0)
                    || (String.Compare(userInput, "no") == 0)
                   )
                {
                    return false;
                }

                return true;
            }
            else
            {
                if (
                    (String.Compare(userInput, "y") == 0)
                    || (String.Compare(userInput, "yes") == 0)
                   )
                {
                    return true;
                }

                return false;
            }
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static string InputString(string question, string defaultAnswer, bool allowNull)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            while (true)
            {
                Console.Write(question);

                if (!String.IsNullOrEmpty(defaultAnswer))
                {
                    Console.Write(" [" + defaultAnswer + "]");
                }

                Console.Write(" ");

                string userInput = Console.ReadLine();

                if (String.IsNullOrEmpty(userInput))
                {
                    if (!String.IsNullOrEmpty(defaultAnswer)) return defaultAnswer;
                    if (allowNull) return null;
                    else continue;
                }

                return userInput;
            }
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static int InputInteger(string question, int defaultAnswer, bool positiveOnly, bool allowZero)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            while (true)
            {
                Console.Write(question);
                Console.Write(" [" + defaultAnswer + "] ");

                string userInput = Console.ReadLine();

                if (String.IsNullOrEmpty(userInput))
                {
                    return defaultAnswer;
                }

                int ret = 0;
                if (!Int32.TryParse(userInput, out ret))
                {
                    Console.WriteLine("Please enter a valid integer.");
                    continue;
                }

                if (ret == 0)
                {
                    if (allowZero)
                    {
                        return 0;
                    }
                }

                if (ret < 0)
                {
                    if (positiveOnly)
                    {
                        Console.WriteLine("Please enter a value greater than zero.");
                        continue;
                    }
                }

                return ret;
            }
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static List<string> InputStringList(string question, bool allowEmpty)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            List<string> ret = new List<string>();

            while (true)
            {
                Console.Write(question);

                Console.Write(" ");

                string userInput = Console.ReadLine();

                if (String.IsNullOrEmpty(userInput))
                {
                    if (ret.Count < 1 && !allowEmpty) continue;
                    return ret;
                }

                ret.Add(userInput);
            }
        }

        #endregion

        #region Serialization

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static string SerializeJson(object obj, bool pretty)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            if (obj == null) return null;
            string json;

            if (pretty)
            {
                json = JsonConvert.SerializeObject(
                  obj,
                  Newtonsoft.Json.Formatting.Indented,
                  new JsonSerializerSettings
                  {
                      NullValueHandling = NullValueHandling.Ignore,
                      DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                  });
            }
            else
            {
                json = JsonConvert.SerializeObject(obj,
                  new JsonSerializerSettings
                  {
                      NullValueHandling = NullValueHandling.Ignore,
                      DateTimeZoneHandling = DateTimeZoneHandling.Utc
                  });
            }

            return json;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static T DeserializeJson<T>(string json)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            if (String.IsNullOrEmpty(json)) throw new ArgumentNullException(nameof(json));
            return JsonConvert.DeserializeObject<T>(json);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static T DeserializeJson<T>(byte[] data)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            if (data == null || data.Length < 1) throw new ArgumentNullException(nameof(data));
            return DeserializeJson<T>(Encoding.UTF8.GetString(data));
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static T CopyObject<T>(object o)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            if (o == null) return default(T);
            string json = SerializeJson(o, false);
            T ret = DeserializeJson<T>(json);
            return ret;
        }

        #endregion
    }
}
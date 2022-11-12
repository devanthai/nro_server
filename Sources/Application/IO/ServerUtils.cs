using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;
using NRO_Server.Application.Helper;
using NRO_Server.Application.Threading;
using NRO_Server.DatabaseManager;

namespace NRO_Server.Application.IO
{
    public class ServerUtils
    {
        public static string ProjectDir(string path)
        {
            return $"{Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\.."))}/{path}";
        }
        private static readonly DateTime Jan1st1970 = new DateTime(1970, 1, 1, 0, 0, 00, DateTimeKind.Utc);

        public static byte[] ConvertArraySByteToByte(sbyte[] data)
        {
            return Array.ConvertAll(data, b => unchecked((byte) b));
        }
        
        public static sbyte[] ConvertArrayByteToSByte(byte[] data)
        {
            return Array.ConvertAll(data, b => unchecked((sbyte)b));
        }

        public static DateTime TimeNow()
        {
            return (new DateTime(1970, 1, 1)).AddMilliseconds(double.Parse(ServerUtils.CurrentTimeMillis().ToString()));
        }

        public static long CurrentTimeMillis()
        {
            return (long) (DateTime.Now - Jan1st1970).TotalMilliseconds;
        }   

        public static int CurrentTimeSecond()
        {
            return (int) (DateTime.Now - Jan1st1970).TotalSeconds;
        }

        public static string GetDate(int second)
        {
            var num = (long)second * 1000L;
            var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).Add(new TimeSpan(num * 10000)).ToUniversalTime();
            var hour = dateTime.Hour;
            var minute = dateTime.Minute;
            var day = dateTime.Day;
            var month = dateTime.Month;
            var year = dateTime.Year;
            return day + "/" + month + "/" + year + " " + hour + "h";
        }


        public static byte ConvertSbyteToByte(sbyte var)
        {
            if (var > 0)
            {
                return (byte)var;
            }
            return (byte)(var + 256);
        }

        public static byte[] ConvertSbyteToByte(sbyte[] var)
        {
            var array = new byte[var.Length];
            for (var i = 0; i < var.Length; i++)
            {
                if (var[i] > 0)
                {
                    array[i] = (byte)var[i];
                }
                else
                {
                    array[i] = (byte)(var[i] + 256);
                }
            }
            return array;
        }

        public static byte[] ReadFileBytes(string path)
        {
            try
            {
                return File.ReadAllBytes(path);
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error Read File Bytes in ServerUtils.cs: {e.Message}\n{e.StackTrace}");
                return null;
            }
        }

        public static int[] ConvertStringToInt(string text)
        {
            try
            {
                return Array.ConvertAll(text.Replace("[", "").Replace("]", "").Trim().Split(','), int.Parse).ToArray();
            }
            catch (Exception)
            {
                return null;
            }                                                                                                                                                 
        }

        public static int RandomNumber(int max)
        {
            var random = new MyRandom();
            return random.NextInt(max);
        }

        public static int RandomNumber(int min, int max)
        {
            var random = new MyRandom();
            if (min <= max) return random.NextInt(min, max);
            (min, max) = (max, min);
            return random.NextInt(min, max);
        }

        public static double RandomNumber(double minimum, double maximum)
        { 
            Random random = new Random();
            return random.NextDouble() * (maximum - minimum) + minimum;
        }

        public static int UShort(short s)
        {
            return s & 0xFFFF;
        }
        
        public static string GetPower(long m)
        {
            const string text = "";
            switch (m)
            {
                case < 10000:
                    return m + text;
                case < 1000000:
                    return m/1000 + " K";
                case < 1000000000:
                {
                    var num2 = m/1000000;
                    var num3 = m - num2*1000000;
                    return num2 + "," + num3/100000 + " Tr";
                }
                default:
                {
                    var num2 = m/1000000000;
                    var num3 = m - num2*1000000000;
                    return num2 + "," + num3/100000000 + " Tỷ";
                }
            }
        }

        public static string GetMoney(int m)
        {
            if (m < 1000)
            {
                return m + "";
            }
            if (m < 1000000)
            {
                return $"{m / 1000} K";
            }
            if (m < 1000000000)
            {
                return $"{m / 1000000} Tr";
            }
            return $"{m / 1000000000} Tỷ";
        }

        public static string GetMoney(long m)
        {
            if (m < 1000)
            {
                return m + "";
            }
            if (m < 1000000)
            {
                return $"{m / 1000} K";
            }
            if (m < 1000000000)
            {
                return $"{m / 1000000} Tr";
            }
            return $"{m / 1000000000} Tỷ";
        }

        public static string GetTime(long time) {
            var seconds = time / 1000;
            var minutes = seconds / 60;
            var hours = minutes / 60;
            var days = hours / 24;

            if(seconds <= 0) {
                seconds = 0;
            }
            return hours <= 0 
                ? string.Format($"{minutes% 60} phút {seconds% 60} giây") 
                : string.Format(days <= 0 
                    ? $"{hours% 24} giờ {minutes% 60} phút" 
                    : $"{days} ngày {hours% 24} giờ");
        }

        public static string ConvertMilisecond(long time)
        {
            var t = TimeSpan.FromMilliseconds(time);
            if (t.Days > 0)
            {
                return t.Hours < 60 ? $"{t.Days:D2}d {t.Hours:D2}h" : $"{t.Days:D2}d";
            }
            if (t.Hours > 0)
            {
                return t.Minutes < 60 ? $"{t.Hours:D2}h {t.Minutes:D2}ph" : $"{t.Hours:D2}h";
            }
            if (t.Minutes > 0)
            {
                return t.Seconds < 60 ? $"{t.Minutes:D2}ph {t.Seconds:D2}s" : $"{t.Minutes:D2}ph";
            }
            return $"{t.Seconds:D2}s";
        }

        public static string ConvertMilisecondToMinute(long time)
        {
            var t = TimeSpan.FromMilliseconds(time);
            return $"{t.Minutes:D2} phút";
        }

        public static string ConvertMilisecondToHour(long time)
        {
            var t = TimeSpan.FromMilliseconds(time);
            return $"{t.Hours:D2} giờ";
        }

        public static string ConvertMilisecondToDay(long time)
        {
            var t = TimeSpan.FromMilliseconds(time);
            return $"{t.Days:D2} ngày";
        }

        public static int ConvertSecondToDay(int time)
        {
            var hours = time/3600;
            if (hours >= 24)
            {
                return (hours/24);
            }
            else 
            {
                return 0;
            }
        }

        public static string GetTime2(long time) {
            var seconds = time / 1000;
            var minutes = seconds / 60;
            var hours = minutes / 60;
            var days = hours / 24;
            var text = "";
            if (days > 0)
            {
                text += $"{days}d";
                if (hours < 24)
                {
                    text += $" {hours}h";
                }
                return text;
            }
            if (hours > 0)
            {
                text += $"{hours}h";
                if (minutes < 60)
                {
                    text += $" {minutes}ph";
                }
                return text;
            }
            if (minutes > 0)
            {
                text += $"{minutes}ph";
                if (seconds < 60)
                {
                    text += $" {seconds}s";
                }
                return text;
            }

            return "9999d";
        }

        public static string GetTimeAgo(int timeRemainS)
        {
            var num = 0;
            if (timeRemainS > 60)
            {
                num = timeRemainS / 60;
                timeRemainS %= 60;
            }
            var num2 = 0;
            if (num > 60)
            {
                num2 = num / 60;
                num %= 60;
            }
            var num3 = 0;
            if (num2 > 24)
            {
                num3 = num2 / 24;
                num2 %= 24;
            }
            var empty = string.Empty;
            if (num3 > 0)
            {
                empty += num3;
                empty += "d";
                return empty + num2 + "h";
            }
            if (num2 > 0)
            {
                empty += num2;
                empty += "h";
                return empty + num + "'";
            }
            if (num == 0)
            {
                num = 1;
            }
            empty += num;
            return empty + "ph";
        }

        public static string Color(string color)
        {
            return color switch
            {
                "brown" => "\b|0|",
                "green" => "\b|1|",
                "blue" => "\b|2|",
                "light-red" => "\b|3|",
                "light-green" => "\b|4|",
                "light-blue" => "\b|5|",
                "red" => "\b|7|",
                _ => ""
            };
        }

        public static string GetMoneys(long m)
        {
            var text = string.Empty;
            var num = m / 1000 + 1;
            for (var i = 0; i < num; i++)
            {
                if (m >= 1000)
                {
                    var num2 = m % 1000;
                    text = ((num2 != 0) ? ((num2 >= 10) ? ((num2 >= 100) ? ("." + num2 + text) : (".0" + num2 + text)) : (".00" + num2 + text)) : (".000" + text));
                    m /= 1000;
                    continue;
                }
                text = m + text;
                break;
            }
            return text;
        }

        public static string ToKMB(long num)
        {
            if (num > 999999999 || num < -999999999 )
            {
                return num.ToString("0,,,.###B", CultureInfo.InvariantCulture);
            }
            else
            if (num > 999999 || num < -999999 )
            {
                return num.ToString("0,,.##M", CultureInfo.InvariantCulture);
            }
            else
            if (num > 999 || num < -999)
            {
                return num.ToString("0,.#K", CultureInfo.InvariantCulture);
            }
            else
            {
                return num.ToString(CultureInfo.InvariantCulture);
            }
        }

        public static List<int> GetTimeAmulet(long time) {
            var seconds = time / 1000;
            var minutes = seconds / 60;
            var hours = minutes / 60;
            var days = hours / 24;

            if(hours <= 0) {
                return new List<int>{65, (int)minutes};
            }
            return days <= 0 ? new List<int>{64, (int)hours} : new List<int>{63, (int)days};
        }

        public static string FilterWords(string text)
        {
            var check = text.ToLower();
            check = Regex.Replace(check, @"\s+", " ");

            Cache.Gi().RegexText.ForEach(t =>
            {
                check = check.Replace(t, "***");
            });
            return check;
        }

        public static string RemoveSpaceText(string text)
        {
            text = text.Trim(); // Xóa đầu cuối
            var trimmer = new Regex(@"\s\s+"); // Xóa khoảng trắng thừa trong chuỗi
            return ConvertToUnSign3(trimmer.Replace(text, " "));
        }

        public static string ConvertToUnSign3(string s)
        {
            var regex = new Regex("\\p{IsCombiningDiacriticalMarks}+");
            var temp = s.Normalize(NormalizationForm.FormD);
            return regex.Replace(temp, string.Empty).Replace('\u0111', 'd').Replace('\u0110', 'D');
        } 


        public static void WriteTraceLog(string filename, string message)
        {
            try
            {
                var path = $"tracelogs/{DatabaseManager.Manager.gI().ServerPort}/{DateTime.Now.ToString("MM-dd-yyyy")}/{filename}.txt";
                new FileInfo(path).Directory.Create();

                if (File.Exists(path))
                {
                    File.AppendAllText(path, message + " at " + DateTime.Now.ToString("MM/dd/yyyy h:mm tt") + Environment.NewLine);
                }
                else
                {
                    File.WriteAllText(path, message + " at " + DateTime.Now.ToString("MM/dd/yyyy h:mm tt") + Environment.NewLine);
                }
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error WriteTraceLog File Bytes in ServerUtils.cs: {e.Message}\n{e.StackTrace}");
            }
        }

        public static void WriteLogBug(string charname, int userid)
        {
            try
            {
                var path = $"svlogs/logbugs.txt";
                new FileInfo(path).Directory.Create();

                if (File.Exists(path))
                {
                    File.AppendAllText(path, charname + "_uid:" + userid + " at " + DateTime.Now.ToString("MM/dd/yyyy h:mm tt") + Environment.NewLine);
                }
                else
                {
                    File.WriteAllText(path, charname + "_uid:" + userid + " at " + DateTime.Now.ToString("MM/dd/yyyy h:mm tt") + Environment.NewLine);
                }
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error WriteLogBug File Bytes in ServerUtils.cs: {e.Message}\n{e.StackTrace}");
            }
        }

        public static void WriteLog(string filename, string message)
        {
            try
            {
                var path = $"svlogs/{DatabaseManager.Manager.gI().ServerPort}/{DateTime.Now.ToString("MM-dd-yyyy")}/{filename}.txt";
                new FileInfo(path).Directory.Create();

                if (File.Exists(path))
                {
                    File.AppendAllText(path, message + " at " + DateTime.Now.ToString("MM/dd/yyyy h:mm tt") + Environment.NewLine);
                }
                else
                {
                    File.WriteAllText(path, message + " at " + DateTime.Now.ToString("MM/dd/yyyy h:mm tt") + Environment.NewLine);
                }
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error WriteLog File Bytes in ServerUtils.cs: {e.Message}\n{e.StackTrace}");
            }
        }

        public static void WriteTradeLog(string message, int tongThoiVangGiaoDich)
        {
            try
            {
                var path = $"tradelogs/{DatabaseManager.Manager.gI().ServerPort}/{DateTime.Now.ToString("MM-dd-yyyy")}/trade.txt";
                if (tongThoiVangGiaoDich > 300)
                {
                    path = $"tradelogs/{DatabaseManager.Manager.gI().ServerPort}/{DateTime.Now.ToString("MM-dd-yyyy")}/tradethoivang.txt";
                }
                new FileInfo(path).Directory.Create();

                if (File.Exists(path))
                {
                    File.AppendAllText(path, message + " at " + DateTime.Now.ToString("MM/dd/yyyy h:mm tt") + Environment.NewLine);
                }
                else
                {
                    File.WriteAllText(path, message + " at " + DateTime.Now.ToString("MM/dd/yyyy h:mm tt") + Environment.NewLine);
                }
            }
            catch (Exception e)
            {
                Server.Gi().Logger.Error($"Error WriteTradeLog File Bytes in ServerUtils.cs: {e.Message}\n{e.StackTrace}");
            }
            
        }

        public static string MD5Hash(string input)
        {
            StringBuilder hash = new StringBuilder();
            MD5CryptoServiceProvider md5provider = new MD5CryptoServiceProvider();
            byte[] bytes = md5provider.ComputeHash(new UTF8Encoding().GetBytes(input));

            for (int i = 0; i < bytes.Length; i++)
            {
                hash.Append(bytes[i].ToString("x2"));
            }
            return hash.ToString();
        }
    }
}
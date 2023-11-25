using System;
using System.Text;
using System.Text.RegularExpressions;

namespace Core.Database.SQLHelper
{
    public class SecureUtil
    {

        public const string mscSQLInjectionMessage = "SQL Injection detected. Please re-check parameter clase";
        private static readonly Regex regSystemThreats= new Regex("\\s?;\\s?|\\s?drop \\s|\\s?grant \\s|^'|\\s?--|\\s?union \\s|\\s?delete \\s|\\s?update \\s|\\s?truncate \\s|\\s?sysobjects\\s?|\\s?xp_.*?|\\s?syslogins\\s?|\\s?sysremote\\s?|\\s?sysusers\\s?|\\s?sysxlogins\\s?|\\s?sysdatabases\\s?|\\s?aspnet_.*?|\\s?exec \\s?|\\s?execute \\s? ", RegexOptions.IgnoreCase| RegexOptions.Compiled);

        /// <summary>
        /// kiểm tra chuỗi có sql injection hay không
        /// </summary>
        /// <param name="inputSql"></param>
        /// <returns></returns>
        public static bool DetectSqlInjection(string inputSql)
        {
            return !string.IsNullOrWhiteSpace(inputSql) && SecureUtil.regSystemThreats.IsMatch(inputSql);
        }
        
        /// <summary>
        /// replace ' thanh '' tranh sql injection for string
        /// </summary>
        /// <param name="inputSql"></param>
        /// <returns></returns>
        public static string SafeSqlLiternalForStringValue(string inputSql)
        {
            if (string.IsNullOrEmpty(inputSql))
            {
                return inputSql;
            }

            return inputSql.Replace("`", "``");
        }
        /// <summary>
        /// replace ' thanh '' tranh sql injection
        /// </summary>
        /// <param name="tableViewColumnName"></param>
        /// <returns></returns>
        public static string SafeSqlLiternalForObjectName(string tableViewColumnName)
        {
            if (string.IsNullOrEmpty(tableViewColumnName))
            {
                return tableViewColumnName;
            }

            return tableViewColumnName.Replace("`", "``");
        }

        public static string SafeSqlLiteralForColumnsName(string columns)
        {
            StringBuilder stringBuilder= new StringBuilder();
            if (!string.IsNullOrEmpty((columns)))
            {
                string[] array = columns.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                if (array != null && array.Length > 0)
                {
                    for (int i = 0; i < array.Length; i++)
                    {
                        string text = array[i].Trim();
                        if (text.CompareTo("*") != 0 && (!text.StartsWith("`") || !text.EndsWith("`")))
                        {
                            text = "`" + SafeSqlLiternalForObjectName(text) + "`";
                        }

                        if (!string.IsNullOrEmpty(text) && stringBuilder.Length > 0)
                        {
                            stringBuilder.Append(",");
                        }

                        stringBuilder.Append(text);
                    }
                }
            }
            return stringBuilder.ToString();
        }
        
        
    }
}
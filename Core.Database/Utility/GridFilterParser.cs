using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Core.Model;
using Core.Ultitily;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Core.Database.Utility
{
    /// <summary>
    /// Parse chuỗi filter từ grid devexpress client truyền lên
    /// </summary>
    public class GridFilterParser
    {
        public static WhereParameter Parse(string input, string input2 = "")
        {
            var inputWhere = ParseFilterWhere(input);
            var input2Where = ParseFilterWhere(input2);
            if (inputWhere != null)
            {
                inputWhere.AddWhere(input2Where);
            }
            else
            {
                inputWhere = input2Where;
            }

            return inputWhere;
        }

        /// <summary>
        /// Parse chuỗi filter thành whereparameter để lấy dữ liệu paging
        /// </summary>
        /// <param name="input"></param>
        /// <param name="suffixParamName"></param>
        /// <returns></returns>
        private static WhereParameter ParseFilterWhere(string input, string suffixParamName ="WP")
        {
            if (string.IsNullOrEmpty(input))
            {
                return null;
            }
            else
            {
                StringBuilder builder=new StringBuilder();
                Dictionary<string,object> parameters=new Dictionary<string, object>();
                JArray obj = JsonConvert.DeserializeObject<JArray>(input, Converter.GetJsonSerializeSetting());
                ConvertArray(obj, builder, parameters, suffixParamName);
                WhereParameter whereParameter = new WhereParameter(builder.ToString(), parameters);
                return whereParameter;
            }
        }

        /// <summary>
        /// Xử lí đối tượng JArray thành các thành phần của  WhereParameter
        /// </summary>
        /// <param name="arr"></param>
        /// <param name="builder"></param>
        /// <param name="parameters"></param>
        /// <param name="suffixParamName"></param>
        private static void ConvertArray(JArray arr, StringBuilder builder, Dictionary<string, object> parameters, string suffixParamName="WP")
        {
            if (arr.First != null && arr.First.Type == JTokenType.Array)
            {
                foreach (JToken item in arr)
                {
                    if (item.Type == JTokenType.Array)
                    {
                        builder.Append(" (");
                        ConvertArray((JArray)item, builder,parameters,suffixParamName);
                        builder.Append(") ");
                    }
                    else
                    {
                        if (string.Compare(item.Value<string>(), "and", true) == 0)
                        {
                            builder.Append(" AND ");
                        }
                        else if (string.Compare(item.Value<string>(), "or", true) == 0)
                        {
                            builder.Append(" OR ");
                        }
                    }
                    
                }
            }
            else
            {
                builder.Append(" (");
                builder.Append(ConvertFilterItem((JArray) arr, parameters, suffixParamName));
                builder.Append(") ");
            }

            string res = builder.ToString();
        }

        /// <summary>
        /// Convert 1 bộ filer item sang chuỗi sql
        /// </summary>
        /// <param name="item"></param>
        /// <param name="parameters"></param>
        /// <param name="suffixParamName"></param>
        /// <returns></returns>
        private static string ConvertFilterItem(JArray item, Dictionary<string, object> parameters, string suffixParamName="WP")
        {
            string propertyName = item.First.Value<string>();
            string operatorValue = item.First.Next.Value<string>();
            object paramValue = null;
            string paramName = $@"p{(parameters.Count + 1).ToString()}{suffixParamName}";
            string paramValueAlias = paramName;
            string operatorAlias = operatorValue;
            string pattern = " {0} {1} {2}";
            // Lấy value
            switch (item.Last.Type)
            {
                case JTokenType.TimeSpan:
                    paramValue = item.Last.Value<TimeSpan>();
                    break;
                case JTokenType.Date:
                    paramValue = item.Last.Value<DateTime>();
                    break;
                case JTokenType.Integer:
                    paramValue = item.Last.Value<int>();
                    break;
                case JTokenType.Float:
                    paramValue = item.Last.Value<decimal>();
                    break;
               default:
                   paramValue = item.Last.Value<object>().ToString();
                   //Kiểm tra kiểu dữ liệu khác như guid, bool
                    Guid guid= Guid.Empty;
                   bool boolValue = false;
                   if (Guid.TryParse(paramValue.ToString(), out guid))
                   {
                       paramValue = guid;
                   }
                   else if (bool.TryParse(paramValue.ToString(), out boolValue))
                   {
                       paramValue = boolValue;
                   }
                   break;
            }
            var patternParams=new List<object>();
            switch (operatorValue.ToLower())
            {
                case "contains":
                    operatorAlias = " LIKE ";
                    paramValueAlias = $" CONCAT('%',{paramName},'%')";
                    break;
                case "notcontains":
                    operatorAlias = " NOT LIKE ";
                    paramValueAlias = $" CONCAT('%',{paramName},'%')";
                    break;
                case "startswith":
                    operatorAlias = " LIKE ";
                    paramValueAlias = $" CONCAT({paramName},'%')";
                    break;
                case "endswith":
                    operatorAlias = " LIKE ";
                    paramValueAlias = $" CONCAT('%',{paramName})";
                    break;
                case "in":
                    operatorAlias = " IN ";
                    paramValueAlias = $" ('({paramName})";
                    break;
                case "isnullorempty":
                    pattern = " ( {0} IS NULL OR {0} = '' ) ";
                    patternParams.Add(pattern);
                    break;
                case "isnull":
                    pattern = " {0} IS NULL ";
                    patternParams.Add(pattern);
                    break;
                case "notnull":
                    pattern = " {0} IS NOT NULL ";
                    patternParams.Add(pattern);
                    break;
                case "hasvalue":
                    pattern = " {0} IS NOT NULL AND {0} <> '' ";
                    patternParams.Add(pattern);
                    break;
                default:
                    break;

            }
            parameters.Add(paramName,paramValue);
            if (patternParams.Count == 0)
            {
                patternParams.AddRange(new object[]{propertyName,operatorAlias,paramValueAlias});
            }

            string res = string.Format(pattern, patternParams.ToArray());
            return res;
        }
    }
}
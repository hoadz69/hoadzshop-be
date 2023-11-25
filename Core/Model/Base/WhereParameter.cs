using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Model
{
    public class WhereParameter
    {
        private string _whereClause;
        private Dictionary<string,object> _whereValues=new Dictionary<string, object>();

        public string WhereClause
        {
            get => _whereClause;
            // set => _whereClause = value;
        }

        public Dictionary<string, object> WhereValues
        {
            get => _whereValues;
            // set => _whereValues = value;
        }

        public WhereParameter(string whereClause, Dictionary<string,object> whereValues)
        {
            this._whereClause = whereClause;
            this._whereValues = whereValues;
        }

        public void AddWhere(string whereClause, Dictionary<string,object> whereValues)
        {
            StringBuilder stringBuilder= new StringBuilder(whereClause);
            _whereClause = $"{_whereClause} AND ({stringBuilder.ToString()})";
            if (whereValues != null && whereValues.Count > 0)
            {
                foreach (KeyValuePair<string,object> current in whereValues)
                {
                    string key = current.Key;
                    if (_whereValues.ContainsKey(key))
                    {
                        _whereValues[key] = current.Value;
                    }
                    else
                    {
                        _whereValues.Add(key,current.Value);
                    }
                }
            }
        }

        public void AddWhere(WhereParameter whereParameter)
        {
            if (whereParameter != null)
            {
                AddWhere(whereParameter.WhereClause,whereParameter.WhereValues);
            }
        }

        public static string Compile(WhereParameter whereParameter)
        {
            if (whereParameter != null && !string.IsNullOrEmpty(whereParameter.WhereClause))
            {
                StringBuilder stringBuilder= new StringBuilder(whereParameter.WhereClause);
                var keys = new List<string>();
                if (whereParameter.WhereValues.Count > 0)
                {
                    foreach (var key in whereParameter.WhereValues.Keys)
                    {
                        keys.Add(key);
                    }
                    keys.Sort();
                }

                for (int i = keys.Count-1; i >=0; i--)
                {
                    object paramValue = whereParameter.WhereValues[keys[i]];
                    if (paramValue != null)
                    {
                        var type = paramValue.GetType();
                        if (type == typeof(string))
                        {
                            paramValue = $"'{paramValue.ToString()}'";
                        }
                        else if (type == typeof(TimeSpan))
                        {
                            paramValue = $"'{((TimeSpan) paramValue).ToString("HH:mm:ss")}'";
                        }
                        else if (type == typeof(DateTime))
                        {
                            paramValue = $"'{((DateTime) paramValue).ToString("yyyy-MM-dd HH:mm:ss")}'";
                        }
                        else if (type == typeof(int) || type==typeof(double)||type==typeof(decimal))
                        {
                            paramValue = paramValue.ToString();
                        }
                        else if (type == typeof(bool))
                        {
                            paramValue = ((bool) paramValue) ? "1" : "0";
                        }
                        else if (type == typeof(Guid))
                        {
                            paramValue = $"'{((Guid) paramValue).ToString()}'";
                        }

                        stringBuilder.Replace(keys[i], paramValue.ToString());
                    }
                }

                return stringBuilder.ToString();
            }
            return  String.Empty;
        }
    }
}
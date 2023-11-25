namespace Core.Ultitily
{
    public class MergeDataEmail
    {
        /// <summary>
        /// merge data email với dấu ##
        /// </summary>
        /// <param name="content"></param>
        /// <param name="dataMerge"></param>
        /// <returns></returns>
        public static string MergeMailContent(string content, object dataMerge)
        {
            string contentResult = "";

            if (!string.IsNullOrEmpty(content) && dataMerge != null)
            {
                contentResult = content;
                var listProper = dataMerge.GetType().GetProperties();
                foreach (var property in listProper)
                {
                    if (!string.IsNullOrEmpty(property.Name) && content.Contains(property.Name))
                    {
                        string keyMerge = $"##{property.Name}##";
                        string valueMerge = property.GetValue(dataMerge)?.ToString() ?? string.Empty;
                        contentResult = contentResult.Replace(keyMerge, valueMerge);
                    }
                }
            }
            
            return contentResult;
        }
    }
}
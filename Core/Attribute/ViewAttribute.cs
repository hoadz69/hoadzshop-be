namespace Core.Attribute
{
    /// <summary>
    /// attribute chỉ định tên view để lấy full thông tin của table
    /// </summary>
    public class ViewAttribute: System.Attribute
    {
        //Tên của SQL view dùng để select đầy đủ thông tin table
        public string ViewName { get; set; }

        public ViewAttribute(string viewName)
        {
            ViewName = viewName;
        }
    }
}
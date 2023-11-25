namespace Core.Model
{
    /// <summary>
    /// Trạng thái của model
    /// </summary>
    public enum ModelState:int
    {
        None=0,
        Insert=1,
        Update=2,
        Delete=3,
        Duplicate=4
    }
}
namespace Demo.Base_Components
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class ParserInput : Attribute 
    {
        public string SerializedName { get; private init; }
        public ParserInput(string serializedName = "") => SerializedName = serializedName;
    }
}

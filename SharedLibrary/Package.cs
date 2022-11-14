namespace SharedLibrary;

public class Package
{
    public string Action { get; set; }
    public string Message { get; set; }
    
    public Package()
    {
        Action = "None";
        Message = "None";
    }
    public Package(string action, string message)
    {
        Action = action;
        Message = message;
    }
}
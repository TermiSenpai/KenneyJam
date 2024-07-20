using System.Collections.Generic;

public class Method : IMethod
{
    public string AccessModifier { get; set; } = "public";
    public string ReturnType { get; set; } = "void";
    public string Name { get; set; } = "Method";
    public List<IParameter> Parameters { get; set; } = new List<IParameter>();
}

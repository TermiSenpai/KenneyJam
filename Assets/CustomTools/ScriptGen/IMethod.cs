using System.Collections.Generic;

public interface IMethod
{
    string AccessModifier { get; set; }
    string ReturnType { get; set; }
    string Name { get; set; }
    List<IParameter> Parameters { get; set; }
}


using System.Collections.Generic;

public interface IScriptGenerator
{
    void GenerateScript(string name, string path, List<IVariable> variables, List<IMethod> methods, List<IMethod> unityMethods);
}


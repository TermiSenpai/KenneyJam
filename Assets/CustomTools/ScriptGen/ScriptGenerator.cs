using System.Collections.Generic;
using System.IO;
using UnityEditor;

public class ScriptGenerator : IScriptGenerator
{
    public void GenerateScript(string name, string path, List<IVariable> variables, List<IMethod> methods, List<IMethod> unityMethods)
    {
        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(path))
        {
            EditorUtility.DisplayDialog("Error", "Please provide both name and path.", "OK");
            return;
        }

        string scriptContent = "using UnityEngine;\n\n" +
                               "public class " + name + " : MonoBehaviour\n{\n";

        foreach (var variable in variables)
        {
            scriptContent += "    " + variable.AccessModifier + " " + variable.Type + " " + variable.Name + ";\n";
        }

        scriptContent += "\n";

        foreach (var method in methods)
        {
            string parameters = string.Join(", ", method.Parameters.ConvertAll(p => p.Type + " " + p.Name).ToArray());
            scriptContent += "    " + method.AccessModifier + " " + method.ReturnType + " " + method.Name + "(" + parameters + ")\n    {\n";

            if (method.ReturnType != "void")
            {
                scriptContent += "        // Return a value of type " + method.ReturnType + "\n";
                scriptContent += "        throw new System.NotImplementedException();\n";
            }
            else
            {
                scriptContent += "        // Method body\n";
            }

            scriptContent += "    }\n\n";
        }

        // Add Unity methods
        foreach (var method in unityMethods)
        {
            scriptContent += "    " + method.AccessModifier + " " + method.ReturnType + " " + method.Name + "()\n    {\n";
            scriptContent += "        // Unity method body\n";
            scriptContent += "    }\n\n";
        }

        scriptContent += "\n}";

        string fullPath = Path.Combine(path, name + ".cs");

        if (File.Exists(fullPath))
        {
            EditorUtility.DisplayDialog("Error", "A script with this name already exists at the specified path.", "OK");
            return;
        }

        File.WriteAllText(fullPath, scriptContent);
        AssetDatabase.Refresh();
    }
}

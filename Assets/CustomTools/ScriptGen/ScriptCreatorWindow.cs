using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ScriptCreatorWindow : EditorWindow
{
    private string scriptName = "NewScript";
    private string scriptPath = "Assets/";
    private List<IVariable> variables = new List<IVariable>();
    private List<IMethod> methods = new List<IMethod>();
    private List<IMethod> unityMethods = new List<IMethod>();
    private int nextVariableIndex = 1;
    private bool[] methodParameterFoldouts;
    private Vector2 scrollPosition;
    private int selectedUnityMethodIndex = 0;

    private IScriptGenerator scriptGenerator = new ScriptGenerator();

    private readonly string[] accessModifiers = new string[] { "public", "private", "protected" };
    private readonly string[] variableTypes = new string[] { "int", "float", "string", "bool", "Vector3", "GameObject", "Transform" };
    private readonly string[] returnTypes = new string[] { "void", "int", "float", "string", "bool", "Vector3", "GameObject", "Transform" };
    private readonly string[] unityMethodNames = new string[] { "Start", "Update", "Awake", "OnDestroy" };

    [MenuItem("Tools/Script Creator")]
    public static void ShowWindow()
    {
        GetWindow<ScriptCreatorWindow>("Script Creator");
    }

    private void OnEnable()
    {
        methodParameterFoldouts = new bool[methods.Count];
        InitializeUnityMethods();
    }

    private void OnGUI()
    {
        GUILayout.Label("Create New Script", EditorStyles.boldLabel);

        scriptName = EditorGUILayout.TextField("Script Name", scriptName);
        scriptPath = EditorGUILayout.TextField("Script Path", scriptPath);

        EditorGUILayout.Space();

        // Scroll view for content
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Width(position.width), GUILayout.Height(position.height - 150));

        // Variables Section
        DrawVariablesSection();

        EditorGUILayout.Space();

        // Methods Section
        DrawMethodsSection();

        EditorGUILayout.Space();

        // Unity Methods Section
        DrawUnityMethodsSection();

        EditorGUILayout.EndScrollView(); // End scroll view

        EditorGUILayout.Space();

        // Button to generate the script
        if (GUILayout.Button("Create Script", GUILayout.Width(200)))
        {
            // Call CreateScript method instead of directly calling scriptGenerator
            CreateScript(scriptName, scriptPath, variables, methods, unityMethods);
        }
    }

    private void DrawVariablesSection()
    {
        GUILayout.Label("Variables", EditorStyles.boldLabel);
        List<IVariable> variablesToRemove = null;

        for (int i = 0; i < variables.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();

            // Access Modifier Dropdown
            GUILayout.Label("Access", GUILayout.Width(60));
            int accessIndex = System.Array.IndexOf(accessModifiers, variables[i].AccessModifier);
            if (accessIndex == -1) accessIndex = 0; // Default to private if not found
            accessIndex = EditorGUILayout.Popup(accessIndex, accessModifiers, GUILayout.Width(100));
            variables[i].AccessModifier = accessModifiers[accessIndex];

            // Variable Type Dropdown
            GUILayout.Label("Type", GUILayout.Width(50));
            int typeIndex = System.Array.IndexOf(variableTypes, variables[i].Type);
            if (typeIndex == -1) typeIndex = 0; // Default to int if not found
            typeIndex = EditorGUILayout.Popup(typeIndex, variableTypes, GUILayout.Width(100));
            variables[i].Type = variableTypes[typeIndex];

            // Variable Name Field
            GUILayout.Label("Name", GUILayout.Width(50));
            string newName = EditorGUILayout.TextField(variables[i].Name, GUILayout.Width(100));
            if (string.IsNullOrEmpty(newName))
            {
                newName = "var" + nextVariableIndex;
            }
            variables[i].Name = newName;

            // Remove Button
            if (GUILayout.Button("Remove", GUILayout.Width(60)))
            {
                if (variablesToRemove == null)
                    variablesToRemove = new List<IVariable>();

                variablesToRemove.Add(variables[i]);
            }

            EditorGUILayout.EndHorizontal();
        }

        if (variablesToRemove != null)
        {
            foreach (var variable in variablesToRemove)
            {
                variables.Remove(variable);
            }
        }

        // Button to add a new variable
        if (GUILayout.Button("Add Variable"))
        {
            variables.Add(new Variable());
            nextVariableIndex++;
        }
    }

    private void DrawMethodsSection()
    {
        GUILayout.Label("Methods", EditorStyles.boldLabel);
        List<IMethod> methodsToRemove = null;

        // Update foldouts to match the number of methods
        if (methodParameterFoldouts.Length != methods.Count)
        {
            methodParameterFoldouts = new bool[methods.Count];
        }

        for (int i = 0; i < methods.Count; i++)
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal();

            // Access Modifier Dropdown
            GUILayout.Label("Access", GUILayout.Width(60));
            int accessIndex = System.Array.IndexOf(accessModifiers, methods[i].AccessModifier);
            if (accessIndex == -1) accessIndex = 0; // Default to public if not found
            accessIndex = EditorGUILayout.Popup(accessIndex, accessModifiers, GUILayout.Width(100));
            methods[i].AccessModifier = accessModifiers[accessIndex];

            // Return Type Dropdown
            GUILayout.Label("Return Type", GUILayout.Width(80));
            int returnTypeIndex = System.Array.IndexOf(returnTypes, methods[i].ReturnType);
            if (returnTypeIndex == -1) returnTypeIndex = 0; // Default to void if not found
            returnTypeIndex = EditorGUILayout.Popup(returnTypeIndex, returnTypes, GUILayout.Width(100));
            methods[i].ReturnType = returnTypes[returnTypeIndex];

            // Method Name Field
            GUILayout.Label("Name", GUILayout.Width(50));
            methods[i].Name = EditorGUILayout.TextField(methods[i].Name, GUILayout.Width(100));

            // Remove Method Button
            if (GUILayout.Button("Remove Method", GUILayout.Width(100)))
            {
                if (methodsToRemove == null)
                    methodsToRemove = new List<IMethod>();

                methodsToRemove.Add(methods[i]);
            }

            EditorGUILayout.EndHorizontal();

            // Expand/Collapse Toggle for Parameters
            methodParameterFoldouts[i] = EditorGUILayout.Foldout(methodParameterFoldouts[i], "Parameters");

            if (methodParameterFoldouts[i])
            {
                GUILayout.Label("Parameters", EditorStyles.boldLabel);
                List<IParameter> parametersToRemove = null;

                for (int j = 0; j < methods[i].Parameters.Count; j++)
                {
                    EditorGUILayout.BeginHorizontal();

                    // Parameter Type Dropdown
                    GUILayout.Label("Type", GUILayout.Width(50));
                    int paramTypeIndex = System.Array.IndexOf(variableTypes, methods[i].Parameters[j].Type);
                    if (paramTypeIndex == -1) paramTypeIndex = 0; // Default to int if not found
                    paramTypeIndex = EditorGUILayout.Popup(paramTypeIndex, variableTypes, GUILayout.Width(100));
                    methods[i].Parameters[j].Type = variableTypes[paramTypeIndex];

                    // Parameter Name Field
                    GUILayout.Label("Name", GUILayout.Width(50));
                    methods[i].Parameters[j].Name = EditorGUILayout.TextField(methods[i].Parameters[j].Name, GUILayout.Width(100));

                    // Remove Parameter Button
                    if (GUILayout.Button("Remove", GUILayout.Width(60)))
                    {
                        if (parametersToRemove == null)
                            parametersToRemove = new List<IParameter>();

                        parametersToRemove.Add(methods[i].Parameters[j]);
                    }

                    EditorGUILayout.EndHorizontal();
                }

                if (parametersToRemove != null)
                {
                    foreach (var param in parametersToRemove)
                    {
                        methods[i].Parameters.Remove(param);
                    }
                }

                if (GUILayout.Button("Add Parameter", GUILayout.Width(120)))
                {
                    methods[i].Parameters.Add(new Parameter());
                }
            }

            EditorGUILayout.EndVertical();
        }

        if (methodsToRemove != null)
        {
            foreach (var method in methodsToRemove)
            {
                methods.Remove(method);
            }
        }

        // Button to add a new method
        if (GUILayout.Button("Add Method"))
        {
            methods.Add(new Method());
            // Update foldouts array to match new method count
            System.Array.Resize(ref methodParameterFoldouts, methods.Count);
        }
    }

    private void DrawUnityMethodsSection()
    {
        GUILayout.Label("Unity Methods", EditorStyles.boldLabel);

        // Unity Method Selector Popup
        selectedUnityMethodIndex = EditorGUILayout.Popup("Select Unity Method", selectedUnityMethodIndex, unityMethodNames, GUILayout.Width(200));

        // Button to add the selected Unity method
        if (GUILayout.Button("Add Selected Unity Method", GUILayout.Width(200)))
        {
            if (selectedUnityMethodIndex >= 0 && selectedUnityMethodIndex < unityMethodNames.Length)
            {
                string selectedUnityMethodName = unityMethodNames[selectedUnityMethodIndex];
                IMethod existingMethod = unityMethods.Find(m => m.Name == selectedUnityMethodName);

                if (existingMethod == null)
                {
                    unityMethods.Add(new Method { Name = selectedUnityMethodName, ReturnType = "void", AccessModifier = "public" });
                }
                else
                {
                    EditorUtility.DisplayDialog("Error", "Method already added.", "OK");
                }
            }
        }

        EditorGUILayout.Space();

        // Display added Unity methods
        if (unityMethods.Count > 0)
        {
            GUILayout.Label("Added Unity Methods", EditorStyles.boldLabel);
            List<IMethod> unityMethodsToRemove = null;

            foreach (var method in unityMethods)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(method.Name, GUILayout.Width(150));

                // Remove Unity Method Button
                if (GUILayout.Button("Remove", GUILayout.Width(100)))
                {
                    if (unityMethodsToRemove == null)
                        unityMethodsToRemove = new List<IMethod>();

                    unityMethodsToRemove.Add(method);
                }

                EditorGUILayout.EndHorizontal();
            }

            if (unityMethodsToRemove != null)
            {
                foreach (var method in unityMethodsToRemove)
                {
                    unityMethods.Remove(method);
                }
            }
        }
    }

    private void InitializeUnityMethods()
    {
        // Prepopulate with common Unity methods
        unityMethods.Add(new Method { Name = "Start", ReturnType = "void", AccessModifier = "public" });
        unityMethods.Add(new Method { Name = "Update", ReturnType = "void", AccessModifier = "public" });
        unityMethods.Add(new Method { Name = "Awake", ReturnType = "void", AccessModifier = "public" });
        unityMethods.Add(new Method { Name = "OnDestroy", ReturnType = "void", AccessModifier = "public" });
    }

    private void CreateScript(string name, string path, List<IVariable> variables, List<IMethod> methods, List<IMethod> unityMethods)
    {
        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(path))
        {
            EditorUtility.DisplayDialog("Error", "Please provide both name and path.", "OK");
            return;
        }

        IScriptGenerator generator = new ScriptGenerator();
        generator.GenerateScript(name, path, variables, methods, unityMethods);
    }
}

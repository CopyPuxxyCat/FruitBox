using UnityEditor;
using UnityEditor.Callbacks;
using System.IO;

public class ScriptOrganizer
{
    private const string targetFolder = "Assets/Project/Scripts";

    [OnOpenAsset]
    public static bool MoveNewScript(int instanceID, int line)
    {
        var path = AssetDatabase.GetAssetPath(instanceID);
        if (path.EndsWith(".cs") && Path.GetDirectoryName(path) == "Assets")
        {
            string fileName = Path.GetFileName(path);
            string newPath = Path.Combine(targetFolder, fileName);
            AssetDatabase.MoveAsset(path, newPath.Replace("\\", "/"));
            return true;
        }
        return false;
    }
}


using System.IO;
using UnityEngine;
using UnityEditor;

public static class ExportPackage
{
	private const string ExportPackageDirectoryKey = "ImpossibleOdds_Export_Path";
	private const string ExportPackageNameKey = "ImpossibleOdds_Export_TacticalCamera_Filename";

	private const string PackageExtension = "unitypackage";

	[MenuItem("Assets/Impossible Odds/Export Tactical Camera")]
	private static void ExportToolkit()
	{
		string path = EditorPrefs.GetString(ExportPackageDirectoryKey, Application.dataPath);
		string name = EditorPrefs.GetString(ExportPackageNameKey, "Impossible Odds Tactical Camera");
		string fullPath = EditorUtility.SaveFilePanel("Export Impossible Odds Tactical Camera", path, name, PackageExtension);

		if (string.IsNullOrEmpty(fullPath))
		{
			return;
		}

		FileInfo fileInfo = new FileInfo(fullPath);
		path = fileInfo.DirectoryName;
		name = Path.GetFileNameWithoutExtension(fileInfo.Name);
		EditorPrefs.SetString(ExportPackageDirectoryKey, path);
		EditorPrefs.SetString(ExportPackageNameKey, name);

		AssetDatabase.ExportPackage("Assets/Impossible Odds", fullPath, ExportPackageOptions.Recurse);
	}
}

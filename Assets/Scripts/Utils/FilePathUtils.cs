using UnityEngine;
using System.IO;

public class FilePathUtils {
    public static bool IsPathValid(string path) {
        return path.IndexOf(Path.GetFullPath("./")) == 0;
    }

    public static string FullPathToLocalPath(string path) {
        string currentDirectory = Path.GetFullPath("./");
        int index = path.IndexOf(currentDirectory);
        if (index != 0) {
            return null;
        }

        return path.Substring(currentDirectory.Length);        
    }

    public static string LocalPathToFullPath(string path) {
        return Path.GetFullPath("./") + path;
    }
}
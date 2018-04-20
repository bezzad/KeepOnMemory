using System;
using System.IO;
using System.Text;

namespace CloudStorage
{
    public static class FileSystemInfoExtensions
    {
        public static bool IsDirectory(this string path)
        {
            // get the file attributes for file or directory
            FileAttributes attr = File.GetAttributes(path);

            //detect whether its a directory or file
            if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                return true;
            else
                return false;
        }
    }
}

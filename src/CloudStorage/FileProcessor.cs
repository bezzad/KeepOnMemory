using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using DevExpress.Xpf.Grid;
using DevExpress.Utils;
using xDialog;

namespace CloudStorage
{
    public class FileProcessor
    {
        
        private System.Windows.Window parent_Window;
        private GridControl parent_Grid;
        private TreeListView parent_TreeListView;
         
        public FileProcessor(System.Windows.Window mainWindow,
                             DevExpress.Xpf.Grid.GridControl mainGrid, 
                             DevExpress.Xpf.Grid.TreeListView mainTreeListView)
        {
            parent_Window = mainWindow;
            parent_Grid = mainGrid;
            parent_TreeListView = mainTreeListView;
        }

        
        
        
        #region File Load Analyzer Code
        public void fetchBytesOfFiles(string[] pathOfFiles)
        {
            try
            {
                parent_Grid.Dispatcher.Invoke(new Action(delegate { parent_Grid.BeginDataUpdate(); }), System.Windows.Threading.DispatcherPriority.DataBind);
                foreach (string filePath in pathOfFiles)
                {
                    bool isDirectory = filePath.IsDirectory();
                    string fileNameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(filePath);
                    string fileType = (isDirectory) ? @"<Folder>" : System.IO.Path.GetExtension(filePath);
                    long fileSizeInBytes = (isDirectory) ? GetDirectorySize(filePath) : GetFileSize(filePath);

                    TreeListNode node = new TreeListNode() { Content = new RowItem(fileNameWithoutExtension, fileType, fileSizeInBytes, filePath) };
                    node.IsExpandButtonVisible = (isDirectory && HasFiles(filePath)) ? DefaultBoolean.True : DefaultBoolean.False;

                    writeFileBytesOnTheRAM(node);

                    parent_TreeListView.Dispatcher.Invoke(new Action(delegate { parent_TreeListView.Nodes.Add(node); }), System.Windows.Threading.DispatcherPriority.DataBind);


                    if (node.IsExpandButtonVisible == DefaultBoolean.True)
                    {
                        InitOneFolder(node);
                    }
                    parent_TreeListView.Dispatcher.Invoke(new Action(delegate { parent_TreeListView.CollapseAllNodes(); }), System.Windows.Threading.DispatcherPriority.Render);
                }
            }
            catch (IOException io) { parent_Window.Dispatcher.Invoke(new Action(delegate { MsgBox.Show(io.Message, io.Source, parent_Window); }), System.Windows.Threading.DispatcherPriority.Render); }
            catch (Exception ex) { parent_Window.Dispatcher.Invoke(new Action(delegate { MsgBox.Show(ex.Message, ex.Source, parent_Window); }), System.Windows.Threading.DispatcherPriority.Render); }
            finally { parent_Grid.Dispatcher.Invoke(new Action(delegate { parent_Grid.EndDataUpdate(); }), System.Windows.Threading.DispatcherPriority.DataBind); }
        }
        private byte[] VirtualFAT32(string realFileAddress)
        {
            try
            {
                byte[] buffer;
                using (FileStream fs = new FileStream(realFileAddress, FileMode.Open, FileAccess.Read))
                {
                    buffer = new byte[fs.Length];
                    fs.Read(buffer, 0, Convert.ToInt32(fs.Length));
                }
                if (buffer.LongLength > 0)
                {
                    return buffer;
                }
            }
            catch (IOException ix) { parent_Window.Dispatcher.Invoke(new Action(delegate { MsgBox.Show(string.Format("Unable to add file {0}\r\n{1}", realFileAddress, ix.Message), ix.Source, parent_Window); }), System.Windows.Threading.DispatcherPriority.Render); }
            catch (Exception ex) { parent_Window.Dispatcher.Invoke(new Action(delegate { MsgBox.Show(string.Format("{0}\r\n\r\n{1}", ex.Message, ex.StackTrace), ex.Source, parent_Window); }), System.Windows.Threading.DispatcherPriority.Render); }
            return null;
        }
        private byte[] VirtualFAT64(string realFileAddress)
        {
            try
            {
                List<byte> buffer = new List<byte>();
                FileStream fs = new FileStream(realFileAddress, FileMode.Open, FileAccess.Read);
                BinaryReader br = new BinaryReader(fs);
                long numBytes = new FileInfo(realFileAddress).Length;
                //
                for (long i = 0; i < numBytes; i++)
                    buffer.Add(br.ReadByte());

                if (buffer.LongCount() > 0)
                {
                    return buffer.ToArray();
                }
            }
            catch (IOException ix) { parent_Window.Dispatcher.Invoke(new Action(delegate { MsgBox.Show(string.Format("Unable to add file {0}\r\n{1}", realFileAddress, ix.Message), ix.Source, parent_Window); }), System.Windows.Threading.DispatcherPriority.Render); }
            catch (Exception ex) { parent_Window.Dispatcher.Invoke(new Action(delegate { MsgBox.Show(string.Format("{0}\r\n\r\n{1}", ex.Message, ex.StackTrace), ex.Source, parent_Window); }), System.Windows.Threading.DispatcherPriority.Render); }
            return null;
        }
        public static long GetDirectorySize(string path)
        {
            long size = 0;

            foreach (string dir in Directory.GetDirectories(path))
            {
                size += GetDirectorySize(dir);
            }

            foreach (FileInfo file in new DirectoryInfo(path).GetFiles())
            {
                size += file.Length;
            }

            return size;
        }
        public static long GetFileSize(string path)
        {
            // Use FileInfo to get length of each file.
            FileInfo info = new FileInfo(path);
            return info.Length;
        }
        private bool HasFiles(string folderPath)
        {
            string[] root = Directory.GetFiles(folderPath);
            if (root.Length > 0) return true;
            root = Directory.GetDirectories(folderPath);
            if (root.Length > 0) return true;
            return false;
        }
        private void InitOneFolder(TreeListNode treeListNode)
        {
            InitFolders(treeListNode);
            InitFiles(treeListNode);
        }
        private void InitFiles(TreeListNode treeListNode)
        {
            RowItem item = treeListNode.Content as RowItem;
            if (item == null) return;
            try
            {
                string[] root = Directory.GetFiles(item.Address);
                foreach (string s in root)
                {
                    TreeListNode node = new TreeListNode()
                    {
                        Content = new RowItem(System.IO.Path.GetFileNameWithoutExtension(s),
                            System.IO.Path.GetExtension(s), GetFileSize(s), s)
                    };
                    node.IsExpandButtonVisible = DefaultBoolean.False;
                    writeFileBytesOnTheRAM(node);
                    parent_TreeListView.Dispatcher.Invoke(new Action(delegate { treeListNode.Nodes.Add(node); }), System.Windows.Threading.DispatcherPriority.DataBind);
                }
            }
            catch (IOException io) { parent_Window.Dispatcher.Invoke(new Action(delegate { MsgBox.Show(io.Message, io.Source, parent_Window); }), System.Windows.Threading.DispatcherPriority.Render); }
            catch (Exception ex) { parent_Window.Dispatcher.Invoke(new Action(delegate { MsgBox.Show(ex.Message, ex.Source, parent_Window); }), System.Windows.Threading.DispatcherPriority.Render); }
        }
        private void InitFolders(TreeListNode treeListNode)
        {
            RowItem item = treeListNode.Content as RowItem;
            if (item == null) return;

            try
            {
                string[] root = Directory.GetDirectories(item.Address);
                foreach (string s in root)
                {
                    try
                    {
                        TreeListNode node = new TreeListNode()
                        {
                            Content = new RowItem(System.IO.Path.GetFileNameWithoutExtension(s),
                                @"<Folder>", GetDirectorySize(s), s)
                        };
                        if (HasFiles(s))
                        {
                            node.IsExpandButtonVisible = DefaultBoolean.True;
                            parent_TreeListView.Dispatcher.Invoke(new Action(delegate { treeListNode.Nodes.Add(node); }), System.Windows.Threading.DispatcherPriority.DataBind);
                            InitOneFolder(node);
                        }
                        else
                        {
                            node.IsExpandButtonVisible = DefaultBoolean.False;
                            parent_TreeListView.Dispatcher.Invoke(new Action(delegate { treeListNode.Nodes.Add(node); }), System.Windows.Threading.DispatcherPriority.DataBind);
                        }
                    }
                    catch (Exception ex) { parent_Window.Dispatcher.Invoke(new Action(delegate { MsgBox.Show(ex.Message, ex.Source, parent_Window); }), System.Windows.Threading.DispatcherPriority.Render); }
                }
            }
            catch (IOException io) { parent_Window.Dispatcher.Invoke(new Action(delegate { MsgBox.Show(io.Message, io.Source, parent_Window); }), System.Windows.Threading.DispatcherPriority.Render); }
            catch (Exception ex) { parent_Window.Dispatcher.Invoke(new Action(delegate { MsgBox.Show(ex.Message, ex.Source, parent_Window); }), System.Windows.Threading.DispatcherPriority.Render); }
        }
        private void writeFileBytesOnTheRAM(TreeListNode node)
        {
            RowItem item = node.Content as RowItem;
            if (item.Extension == @"<Folder>") return;
            //
            // Check File Size to select Fat32 or Fat64 Reader (Larger than 2GB)
            // and Read all bytes of file or folder
            if (item.longSize / 1024 / 1024 / 1024 >= 2) // FAT64
            {
                item.Data = VirtualFAT64(item.Address);
            }
            else // FAT32
            {
                item.Data = VirtualFAT32(item.Address);
            }
        }
        public double getFilesTotalSizeFromPaths_Bytes(string[] nextFilesPath)
        {
            double totalSize = 0;

            foreach (string filePath in nextFilesPath)
            {
                totalSize += (filePath.IsDirectory()) ? GetDirectorySize(filePath) : GetFileSize(filePath);
            }

            return totalSize;
        }

        #endregion

        #region File Save Analyzer Code
        public void saveFILE_Folder(string TargetPath)
        {
            try
            {
                foreach (TreeListNode node in parent_TreeListView.Nodes)
                {
                    //
                    // if node checked then delete that...
                    if ((node.Content as RowItem).Selection == true)
                    {
                        writeFileNodesOntheTargetPath(node, TargetPath);
                    }
                    //
                    // First check node childrens for delete
                    if (node.Nodes.Count() > 0)
                        // Add Folder Name in forward of TargetPath and read subFulders , subFiles ...
                        readFolderNodesFromTheRAM(node, string.Format("{0}\\{1}", TargetPath, (node.Content as RowItem).Name));
                }
            }
            catch (IOException io) { parent_Window.Dispatcher.Invoke(new Action(delegate { MsgBox.Show(io.Message, io.Source, parent_Window); }), System.Windows.Threading.DispatcherPriority.Render); }
            catch (Exception ex) { parent_Window.Dispatcher.Invoke(new Action(delegate { MsgBox.Show(ex.Message, ex.Source, parent_Window); }), System.Windows.Threading.DispatcherPriority.Render); }
            finally
            {
                parent_Window.Dispatcher.Invoke(new Action(delegate
                {
                    MsgBox.Show("Data saved in the '" + TargetPath + "' path.",
                        "Save Completed", MsgBox.Buttons.OK, MsgBox.Icons.Info, MsgBox.AnimateStyle.FadeIn, parent_Window);
                }), System.Windows.Threading.DispatcherPriority.Render);
            }
        }
        private void writeFileNodesOntheTargetPath(TreeListNode node, string TargetPath)
        {
            RowItem dataRow = node.Content as RowItem;
            //
            // If node is a Folder then create that directory on the target path
            if (dataRow.Data == null && dataRow.Extension == "<Folder>")
            {
                if (!Directory.Exists(TargetPath))
                    Directory.CreateDirectory(TargetPath);
            }
            //
            // Check File Size to select Fat32 or Fat64 Writer (Larger than 2GB)
            // and Read all bytes of file
            else if (dataRow.longSize / 1024 / 1024 / 1024 >= 2) // FAT64 ------------------------------------------
            {
                if (!Directory.Exists(TargetPath))
                    Directory.CreateDirectory(TargetPath);

                using (var file = File.Create(string.Format("{0}\\{1}{2}", TargetPath, dataRow.Name, dataRow.Extension)))
                {
                    long length = dataRow.Data.LongCount();
                    for (long i = 0; i < length; i++)
                    {
                        file.WriteByte(dataRow.Data[i]);
                    }
                }
            }
            else // FAT32 ----------------------------------------------------------------------------------------------
            {
                if (!Directory.Exists(TargetPath))
                    Directory.CreateDirectory(TargetPath);

                using (var file = File.Create(string.Format("{0}\\{1}.{2}", TargetPath, dataRow.Name, dataRow.Extension)))
                {
                    file.Write(dataRow.Data, 0, dataRow.Data.Length);
                }
            }
        }
        private void readFolderNodesFromTheRAM(TreeListNode folderNode, string TargetPath)
        {
            foreach (TreeListNode node in folderNode.Nodes)
            {
                //
                // if node checked then delete that...
                if ((node.Content as RowItem).Selection == true)
                {
                    writeFileNodesOntheTargetPath(node, TargetPath);
                }
                //
                // First check node childrens for delete
                if (node.Nodes.Count() > 0)
                    // Add Folder Name in forward of TargetPath and read subFulders , subFiles ...
                    readFolderNodesFromTheRAM(node, string.Format("{0}\\{1}", TargetPath, (node.Content as RowItem).Name));
            }
        }
        #endregion
    }
}

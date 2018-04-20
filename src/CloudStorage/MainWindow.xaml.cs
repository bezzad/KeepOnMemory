using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using DevExpress.Xpf.Grid;
using DevExpress.Utils;
using System.Windows.Media.Animation;
using xDialog;


namespace CloudStorage
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static double TotalPhysicalMemoryMB;
        public PerformanceCounter ramAvailableCounterMB;
        private System.Windows.Threading.DispatcherTimer ramCounterTimer = new System.Windows.Threading.DispatcherTimer();
        public FileProcessor IOP;

        public MainWindow()
        {
            InitializeComponent();

            IOP = new FileProcessor(this, grid, treeListView1);

            btnSelectAll.Click += btnSelectAll_Click; 
            btnUnSelectAll.Click += btnUnSelectAll_Click;
            btnExpandAll.Click += btnExpandAll_Click;
            btnCollapseAll.Click += btnCollapseAll_Click;

            imgButtonAbout.MouseLeftButtonDown += imgButtonAbout_MouseLeftButtonDown;
            recAbout.MouseLeftButtonDown += recAbout_MouseLeftButtonDown;

            treeListView1.CellValueChanging += treeListView1_CellValueChanging; // For check box column changed event
            treeListView1.KeyDown += treeListView1_KeyDown;
            treeListView1.PreviewKeyDown += treeListView1_PreviewKeyDown;
            treeListView1.AllowScrollToFocusedRow = true;
            treeListView1.AllowHorizontalScrollingVirtualization = true;
            treeListView1.AllowPerPixelScrolling = true;
            treeListView1.ShowAutoFilterRow = true;
            treeListView1.RowAnimationKind = RowAnimationKind.Opacity;
            treeListView1.CustomScrollAnimation += treeListView1_CustomScrollAnimation;
            treeListView1.AllowScrollHeaders = true;
            treeListView1.AllowCascadeUpdate = true;
            
            

            ramAvailableCounterMB = new PerformanceCounter("Memory", "Available MBytes", true);
            TotalPhysicalMemoryMB = (double)(new Microsoft.VisualBasic.Devices.ComputerInfo().TotalPhysicalMemory) / 1024 / 1024; // convert "byte in ulong prototype" to "MegaByte in double prototype"
            proTotalRAM.Maximum = TotalPhysicalMemoryMB;

            ramCounterTimer.Tick += new EventHandler(ramCounterTimer_Tick);
            ramCounterTimer.Interval = new TimeSpan(0, 0, 1);
            ramCounterTimer.Start();
        }

       
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (treeListView1.Nodes.Count() > 0)
            {
                if (MsgBox.Show("Data within the program is not yet clear, however, you want to close this program?",
                    "Cloud Storage Shutting Down", MsgBox.Buttons.OKCancel,
                    MsgBox.Icons.Warning, MsgBox.AnimateStyle.SlideDown, this) == System.Windows.Forms.DialogResult.Cancel)
                    e.Cancel = true;
                else
                {
                    GarbrageCollectionClear();
                }
            }
        }
        private void treeListView1_CustomScrollAnimation(object sender, CustomScrollAnimationEventArgs e)
        {
            e.Storyboard = new Storyboard();
            DoubleAnimation animation = new DoubleAnimation();
            animation.From = e.OldOffset;
            animation.To = e.NewOffset;
            animation.Duration = new Duration(TimeSpan.FromMilliseconds(600));
            animation.EasingFunction = new ExponentialEase() { Exponent = 0 };
            e.Storyboard.Children.Add(animation);
        }
        private void treeListView1_CellValueChanging(object sender, DevExpress.Xpf.Grid.TreeList.TreeListCellValueChangedEventArgs e)
        {
            if (e.Column.Name == "colSelection")
            {
                grid.BeginDataUpdate();
                //
                foreach (TreeListNode childNode in e.Node.Nodes)
                {
                    //
                    // if node checked then delete that...
                    (childNode.Content as RowItem).Selection = (e.Value as Nullable<bool>);

                    //
                    // First check node childrens for delete
                    if (childNode.Nodes.Count() > 0)
                        setChildNodesSelection(childNode, (e.Value as Nullable<bool>));
                }
                //
                grid.EndDataUpdate();
                grid.RefreshData();
            }
        }
        
        private void ramCounterTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                var AvailableMemoryMB = ramAvailableCounterMB.NextValue();
                var inUsedMemoryMB = TotalPhysicalMemoryMB - AvailableMemoryMB;
                txtState.Text = String.Format("Total Physical Memory is: {0:0.##} MBytes               " +
                    "       Available Memory is: {1:0.##} MBytes",
                    Math.Round(TotalPhysicalMemoryMB), AvailableMemoryMB);
                proTotalRAM.Value = inUsedMemoryMB;
            }
            catch (Exception ex)
            {
                // remote computer might have become unavailable; 
                // show exception and close this application
                ramCounterTimer.Stop();
                MsgBox.Show(ex.Message, ex.Source, this);
            }
        }
        public string getAvailableRAM()
        {
            return ramAvailableCounterMB.NextValue() + " MB";
        }

        #region Control Buttons
        private void btnClose_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.Close();
        }
        private void btnRestoreSize_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (this.WindowState == System.Windows.WindowState.Normal)
            {
                this.WindowState = System.Windows.WindowState.Maximized;
            }
            else
            {
                this.WindowState = System.Windows.WindowState.Normal;
            }
        }
        private void btnMinimize_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            try
            {
                base.OnMouseLeftButtonDown(e);

                // Begin dragging the window
                // if mouse down on LayoutRoot
                if ((e.GetPosition(this).Y < 90) &&
                    (e.GetPosition(this).X < LayoutRoot.ActualWidth))
                {
                    if (e.ClickCount > 1)
                    {
                        if (this.WindowState == System.Windows.WindowState.Normal)
                        {
                            this.WindowState = System.Windows.WindowState.Maximized;
                        }
                        else
                        {
                            this.WindowState = System.Windows.WindowState.Normal;
                        }
                    }
                    else
                    {
                        this.DragMove();
                    }
                }
            }
            catch { }
        }
        #endregion
        
        #region Drop Events
        private void TextBlock_Drop(object sender, System.Windows.DragEventArgs e)
        {
            Drag(sender, e);
        }
        private void Image_Drop(object sender, System.Windows.DragEventArgs e)
        {
            Drag(sender, e);
        }
        private void Border_Drop(object sender, DragEventArgs e)
        {
            Drag(sender, e);
        }
        private void grid_Drop(object sender, DragEventArgs e)
        {
            Drag(sender, e);
        }
        private void TreeListDragDropManager_Drop(object sender, DevExpress.Xpf.Grid.DragDrop.TreeListDropEventArgs e)
        {
            if (e.TargetNode != null)
            {
                if ((e.TargetNode.Content as RowItem).Extension != @"<Folder>") // Parent Node is a File's !!!
                {
                    e.Handled = true;
                    MsgBox.Show(string.Format("Your target node: '{0}' is a file, but you act like a folder.", (e.TargetNode.Content as RowItem).Name),
                        "Incorrect Target", MsgBox.Buttons.OK, MsgBox.Icons.Info, MsgBox.AnimateStyle.ZoomIn, this);
                    return;
                }
                else // Change parent node size's
                {
                    foreach (TreeListNode draggedNode in e.DraggedRows)
                        (e.TargetNode.Content as RowItem).longSize += (draggedNode.Content as RowItem).longSize;
                }
            }
        }
        private void Drag(object sender, System.Windows.DragEventArgs e)
        {
            string[] droppedFilePaths = e.Data.GetData(DataFormats.FileDrop, true) as string[];

            #region Check the capacity of the RAM space for files (Async) ----------------------------------------------------
            Task taskA = Task.Factory.StartNew(() => AsyncDoWork_UpLoadData(droppedFilePaths)).ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    MsgBox.Show(task.Exception.Message, task.Exception.Source, this);
                    return;
                }

                MsgBox.Show(task.Result, "Cloud Storage", MsgBox.Buttons.OK, MsgBox.Icons.Application, MsgBox.AnimateStyle.FadeIn, this);

            }, TaskScheduler.FromCurrentSynchronizationContext());
            #endregion -------------------- (By Task for Parallel Processing) --------------------------
        }
        #endregion

        private string AsyncDoWork_UpLoadData(string[] paths)
        {
            try
            {
                double freeRam = ramAvailableCounterMB.NextValue() * 1024 * 1024; // convert MegaBytes to bytes
                double allFilesSize = IOP.getFilesTotalSizeFromPaths_Bytes(paths);
                //
                // Check the capacity of the RAM space for 2*files + 100 MB free space.
                // Any data on RAM used space equal to twice the size of the disk.
                double additionalSizeOfRam = (freeRam - (allFilesSize * 2) - (100 * 1024 * 1024));
                bool HasEnoughSpaceOnTheRAM = (additionalSizeOfRam > 0) ? true : false;
                additionalSizeOfRam = Math.Abs(additionalSizeOfRam); // Get into absolute positive number.

                if (HasEnoughSpaceOnTheRAM)
                    IOP.fetchBytesOfFiles(paths);
                else
                {
                    this.Dispatcher.Invoke(new Action(delegate
                    {
                        MsgBox.Show(string.Format("There is not enough space on RAM. You need an additional {0} to copy these files." +
                            "\nAttention:  Used space for any data on the RAM equal to twice the data size on the disk." +
                            "\n          RAM:" +
                            "\n          Space free: {1}" +
                            "\n          Tota Size: {2}" +
                            "\n\n          Your files size on disk: {3}" +
                            "\n          Your files size on RAM: {4}",
                            RowItem.GetBreakingSize(additionalSizeOfRam),
                            RowItem.GetBreakingSize(freeRam),
                            RowItem.GetBreakingSize(TotalPhysicalMemoryMB * 1024 * 1024),
                            RowItem.GetBreakingSize(allFilesSize),
                            RowItem.GetBreakingSize(allFilesSize * 2)),
                            "Cloud Storage files", MsgBox.Buttons.OK,
                            MsgBox.Icons.Warning, MsgBox.AnimateStyle.SlideDown, this);
                    }), DispatcherPriority.Render);
                    return @"Sorry, did not read the files !";
                }

                return @"Files loading complete successfully.";
            }
            catch (Exception ex) { this.Dispatcher.Invoke(new Action(delegate { MsgBox.Show(ex.Message, ex.Source, this); }), DispatcherPriority.Render); }
            return @"Files loading complete successfully.";
        }

        #region Event Buttons

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            GarbrageCollectionClear();
        }
        public void GarbrageCollectionClear()
        {
            try
            {
                grid.BeginDataUpdate();
                treeListView1.Nodes.Clear();
                grid.EndDataUpdate();

                GC.Collect(GC.MaxGeneration);
                GC.WaitForPendingFinalizers();
                txtState.Text = @"Garbrage Collection Completed!";
            }
            catch (Exception ex) { MsgBox.Show(ex.Message, ex.Source, this); }
        }


        void btnCollapseAll_Click(object sender, RoutedEventArgs e)
        {
            treeListView1.CollapseAllNodes();
        }
        void btnExpandAll_Click(object sender, RoutedEventArgs e)
        {
            treeListView1.ExpandAllNodes();
        }
        void btnUnSelectAll_Click(object sender, RoutedEventArgs e)
        {
            grid.BeginDataUpdate();
            //
            foreach (TreeListNode node in treeListView1.Nodes)
            {
                //
                // if node checked then delete that...
                (node.Content as RowItem).Selection = false;

                //
                // First check node childrens for delete
                if (node.Nodes.Count() > 0)
                    setChildNodesSelection(node, false);
            }
            //
            grid.EndDataUpdate();
            grid.RefreshData();
        }
        void btnSelectAll_Click(object sender, RoutedEventArgs e)
        {
            grid.BeginDataUpdate();
            //
            foreach (TreeListNode node in treeListView1.Nodes)
            {
                //
                // if node checked then delete that...
                (node.Content as RowItem).Selection = true;
                
                //
                // First check node childrens for delete
                if (node.Nodes.Count() > 0)
                   setChildNodesSelection(node, true);
            }
            //
            grid.EndDataUpdate();
        }
        /// <summary>
        /// Select All the Selection Column of Node
        /// </summary>
        /// <param name="parentNode">Special Node For Select</param>
        /// <param name="selected">Booolean type for Select or UnSelect</param>
        private void setChildNodesSelection(TreeListNode parentNode, bool? selected)
        {
            foreach (TreeListNode node in parentNode.Nodes)
            {
                //
                // if node checked then delete that...
                (node.Content as RowItem).Selection = selected;

                //
                // First check node childrens for delete
                if (node.Nodes.Count() > 0)
                    setChildNodesSelection(node, selected);
            }
        }

        
        private void imgButtonSave_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            treeListView1.Focus();
            grid.UnselectAll();
            // Create SaveFileDialog
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            //
            // Set property of savefile dialog
            dialog.ShowNewFolderButton = true;
            dialog.Description = @"Select the desired folder to store the selected data.";
            //
            // Open Dialog:
            if(dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Task taskC = Task.Factory.StartNew(() => IOP.saveFILE_Folder(dialog.SelectedPath));
            }
        }
        private void imgButtonOpen_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Create OpenFileDialog
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            
            // Set filter for file extension and default file extension
            dlg.DefaultExt = "*.*";
            dlg.Filter = @"All files|*.*";
            dlg.CheckPathExists = true;
            dlg.Multiselect = true;
            dlg.Title = @"Browse the files that you want to copy in RAM.";
            

            // Display OpenFileDialog by calling ShowDialog method
            Nullable<bool> result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox
            if (result == true)
            {
                #region Check the capacity of the RAM space for files ----------------------------------------------------
                Task taskB = Task.Factory.StartNew(() => AsyncDoWork_UpLoadData(dlg.FileNames)).ContinueWith(task =>
                {
                    if (task.IsFaulted)
                    {
                        MsgBox.Show(task.Exception.Message, task.Exception.Source, this);
                        return;
                    }

                    MsgBox.Show(task.Result, "Cloud Storage", MsgBox.Buttons.OK, MsgBox.Icons.Application, MsgBox.AnimateStyle.FadeIn, this);

                }, TaskScheduler.FromCurrentSynchronizationContext());
                #endregion
            }
        }        
        private void imgButtonAbout_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Storyboard aboutAnimate = this.FindResource("Storyboard_CloudStorage_Lable") as Storyboard;
            aboutAnimate.Begin();
        }
        void recAbout_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Storyboard aboutAnimate = this.FindResource("Storyboard_CloudStorage_Lable") as Storyboard;
            aboutAnimate.Stop();
        }
      

		/// <summary>
		/// Delete Key Down then remove selected rows
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
        private void treeListView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                try
                {
                    treeListView1.Focus();
                    grid.UnselectAll();
                    grid.BeginDataUpdate();
                    //
                    foreach (TreeListNode node in treeListView1.Nodes)
                    {
                        //
                        // if node checked then delete that...
                        if ((node.Content as RowItem).Selection == true)
                        {
                            treeListView1.DeleteNode(node, true);
                        }
                        //
                        // First check node childrens for delete
                        else if (node.Nodes.Count() > 0)
                            checkSelectedChildNodes_Remover(node);
                    }

                }
                catch 
                {
                    treeListView1_KeyDown(sender, e);
                }
                finally
                {
                    grid.EndDataUpdate();
                    GC.Collect(GC.MaxGeneration);
                    GC.WaitForFullGCComplete();
                    GC.WaitForPendingFinalizers();
                }
            }
        }
        private void checkSelectedChildNodes_Remover(TreeListNode parentNode)
        {
            foreach (TreeListNode node in parentNode.Nodes)
            {
                //
                // if node checked then delete that...
                if ((node.Content as RowItem).Selection == true)
                {
                    parentNode.Nodes.Remove(node);
                }
                //
                // First check node childrens for delete
                else if (node.Nodes.Count() > 0)
                    checkSelectedChildNodes_Remover(node);
            }
        }
        private void treeListView1_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            treeListView1_KeyDown(sender, e);
        }
        #endregion

    }
}

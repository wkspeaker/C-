using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LabelFilesReplace
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        LabelFileReplace labelFileReplace = new LabelFileReplace();
        public MainWindow()
        {
            InitializeComponent();

            Binding bindingSourceFile = new Binding();
            bindingSourceFile.Source = labelFileReplace;
            bindingSourceFile.Path = new PropertyPath("SourcePath");
            BindingOperations.SetBinding(this.txtSourceFolder, TextBox.TextProperty, bindingSourceFile);

            Binding bindingTargetFile = new Binding();
            bindingTargetFile.Source = labelFileReplace;
            bindingTargetFile.Path = new PropertyPath("TargetPath");
            BindingOperations.SetBinding(this.txtTargetFolder, TextBox.TextProperty, bindingTargetFile);

            Binding bindingNeedBackup = new Binding();
            bindingNeedBackup.Source = labelFileReplace;
            bindingNeedBackup.Path = new PropertyPath("NeedBackup");
            BindingOperations.SetBinding(this.cbNeedBackup, CheckBox.IsCheckedProperty, bindingNeedBackup);

            Binding bindingProcessStatus = new Binding();
            bindingProcessStatus.Source = labelFileReplace;
            bindingProcessStatus.Path = new PropertyPath("Status");
            BindingOperations.SetBinding(this.tbStatus, TextBlock.TextProperty, bindingProcessStatus);

            Binding bindingHelpText = new Binding();
            bindingHelpText.Source = labelFileReplace;
            bindingHelpText.Path = new PropertyPath("HelpText");
            BindingOperations.SetBinding(this.tbHelp, TextBlock.TextProperty, bindingHelpText);

        }

        private Microsoft.Win32.OpenFileDialog GetFilePath()
        {
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Filter = "Label File|*.ald";
            if (dialog.ShowDialog()==true)
            {
                return dialog;
            }
            return null;
        }

        private void btnSourceFolder_Click(object sender, RoutedEventArgs e)
        {
            labelFileReplace.SourceFile = GetFilePath();
            getFileList();
        }

        private void btnTargetFolder_Click(object sender, RoutedEventArgs e)
        {
            labelFileReplace.TargetFile = GetFilePath();
            getFileList();
        }

        private void getFileList()
        {
            Thread thread = new Thread(new ThreadStart(labelFileReplace.RefreshFileList));
            thread.Start();
            //labelFileReplace.RefreshFileList();
            while(thread.ThreadState.ToString() == "Running")
            {
                Thread.Sleep(1);
            }
            this.lvLabelFiles.ItemsSource = labelFileReplace.labelFiles;
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            labelFileReplace.NeedBackup = true;
        }

        private void cbNeedBackup_Click(object sender, RoutedEventArgs e)
        {
            labelFileReplace.CheckBackupStatus();
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            getFileList();
        }

        private void btnProceed_Click(object sender, RoutedEventArgs e)
        {
            Thread thread = new Thread(new ThreadStart(labelFileReplace.ProceedActions));
            thread.Start();
        }
    }
}

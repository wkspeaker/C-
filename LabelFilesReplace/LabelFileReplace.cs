using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows;
using System.IO;

namespace LabelFilesReplace
{
    class LabelFileReplace : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string sourcePath;
        public string SourcePath
        {
            get
            {
                return sourcePath;                
            }
            set
            {
                sourcePath = System.IO.Path.GetDirectoryName(value);

                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("SourcePath"));
                }
            }
        }
        private string targetPath;
        public string TargetPath
        {
            get
            {
                return targetPath;
            }
            set
            {
                targetPath = System.IO.Path.GetDirectoryName(value);

                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("TargetPath"));
                }
            }
        }

        private Microsoft.Win32.OpenFileDialog sourceFile;
        public Microsoft.Win32.OpenFileDialog SourceFile
        {
            set
            {
                sourceFile = value;
                if (sourceFile != null)
                {
                    this.SourcePath = sourceFile.FileName;
                }
            }
        }

        private Microsoft.Win32.OpenFileDialog targetFile;
        public Microsoft.Win32.OpenFileDialog TargetFile
        {
            set
            {
                targetFile = value;
                if (targetFile != null)
                {
                    this.TargetPath = targetFile.FileName;
                }
            }
        }

        private bool needBackup;
        public bool NeedBackup
        {
            get
            {
                return needBackup;
            }
            set
            {
                needBackup = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("NeedBackup"));
                }
            }
        }

        private string status;
        public string Status
        {
            get
            {
                return status;
            }
            set
            {
                status = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("Status"));
                }
            }
        }

        private string helpText;
        public string HelpText
        {
            get
            {
                return helpText;
            }
            set
            {
                helpText = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("HelpText"));
                }
            }
        }

        public List<LabelFiles> labelFiles;

        public bool ProcessCompleted = false;
        
        public void RefreshFileList()
        {
            if(sourceFile == null)
            {
                //MessageBox.Show("Please specify the source folder includes the labels you are going to copy.");
                return;
            }
            if(targetFile == null)
            {
                //MessageBox.Show("Please specify the target folder includes all label related files to be replaced.");
                return;
            }

            labelFiles = new List<LabelFiles>();

            getSourceFileList();

            this.Status = "File list refreshed.";
        }

        private void getSourceFileList()
        {
            DirectoryInfo dirc = new DirectoryInfo(this.SourcePath);
            foreach(FileInfo file in dirc.GetFiles())
            {
                if(file.Extension == ".ald")
                {
                    labelFiles.Add(new LabelFilesReplace.LabelFiles(Path.GetFileNameWithoutExtension(file.FullName)));
                }
                
            }

            getFileStatus();
        }

        private void getFileStatus()
        {
            /*
             If the file doesn't exist, use the symbal  ○;
             If the file exists, use the symbal ●;
             If the file was existed and now removed, use the symbal ✘;
             For backup, if the option is no, then use "N/A", if the option is yes, before the backup, use symbal ◎, after the backup use the simbal √;
             For status of the file, before the completion, use the symbal ◎, after that use ✔;
             */

            foreach (LabelFiles lbFile in labelFiles)
            {
                if(File.Exists(this.TargetPath + "\\" + lbFile.LabelID + ".ald"))
                {
                    lbFile.ald = "●";
                }
                else
                {
                    lbFile.ald = "○";
                }
                    
                if (File.Exists(this.TargetPath + "\\" + lbFile.LabelID + ".alc"))
                {
                    lbFile.alc = "●";
                }
                else
                {
                    lbFile.alc = "○";
                }
                    
                if (File.Exists(this.TargetPath + "\\" + lbFile.LabelID + ".ali"))
                {
                    lbFile.ali = "●";
                }
                else
                {
                    lbFile.ali = "○";
                }

                if(this.NeedBackup)
                {
                    lbFile.Backup = "◎";
                }
                else
                {
                    lbFile.Backup = "N/A";
                }

                lbFile.Status = "◎";

                
            }
        }

        public void CheckBackupStatus()
        {
            if (labelFiles != null)
            {
                if (NeedBackup)
                {
                    if (ProcessCompleted)
                    {
                        updateBackupStatus("√");
                    }
                    else
                    {
                        updateBackupStatus("◎");
                    }
                }
                else
                {
                    updateBackupStatus("N/A");
                }
            }
        }

        private void updateBackupStatus(string status)
        {
            foreach(LabelFiles lbFile in labelFiles)
            {
                lbFile.Backup = status;
            }
        }

        public void ProceedActions()
        {
            this.Status = "Start the process.";
            if (this.NeedBackup)
            {
                backupFiles();
            }

            //Start to delete files.
            this.Status = "Start to delete label files from destination.";
            foreach(LabelFiles lbFile in labelFiles)
            {
                lbFile.ald = deleteFiles(lbFile, "ald", this.TargetPath);
                lbFile.alc = deleteFiles(lbFile, "alc", this.TargetPath);
                lbFile.ali = deleteFiles(lbFile, "ali", this.TargetPath);
            }
            this.Status = "Related files are deleted from destination.";

            //Start copy files.
            this.Status = "Start to copy label files into destination.";
            foreach(LabelFiles lbFile in labelFiles)
            {
                lbFile.ald = copyFiles(lbFile, "ald", this.SourcePath, this.TargetPath, "CopyNewLabels");
                lbFile.Status = "✔";
            }
            this.Status = "Label files are copied into destination.";
        }

        private void backupFiles()
        {
            this.Status = "Start backup.";
            string currentDateTime = DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString() + DateTime.Now.Day.ToString() + " " + DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString();
            string backupFolder = this.SourcePath + @"\BackupLabels " + currentDateTime ;
            Directory.CreateDirectory(backupFolder);

            foreach(LabelFiles lbFile in labelFiles)
            {
                lbFile.ald = copyFiles(lbFile, "ald", this.TargetPath, backupFolder);
                lbFile.alc = copyFiles(lbFile, "alc", this.TargetPath, backupFolder);
                lbFile.ali = copyFiles(lbFile, "ali", this.TargetPath, backupFolder);
            }
            this.Status = "Backup completed.";
        }

        private string deleteFiles(LabelFiles lbFile, string postFix, string deleteFrom)
        {
            string fileToDelete = deleteFrom + "\\" + lbFile.LabelID + "." + postFix;
            if (File.Exists(fileToDelete))
            {
                File.Delete(fileToDelete);
                return "✘";
            }
            else
            {
                return "○";
            }
        }

        private string copyFiles(LabelFiles lbFile, string postFix, string copyFrom, string copyTo, string copyType = "Backup")
        {
            string copyFromFile = copyFrom + "\\" + lbFile.LabelID + "." + postFix;
            string copyToFile = copyTo + "\\" + lbFile.LabelID + "." + postFix;
            if (File.Exists(copyFromFile))
            {
                File.Copy(copyFromFile, copyToFile);
                if (copyType == "Backup")
                {
                    return "√";
                }
                else
                {
                    return "✔";
                }
                
            }
            else
            {
                return "○";
            }
        }

        public LabelFileReplace()
        {
            this.Status = "Process not started.";
            this.HelpText = @"    If the file doesn't exist, use the symbal  ○;
    If the file exists, use the symbal ●;
    If the file was existed and now removed, use the symbal ✘;
    For backup, if the option is no, then use N/A, if the option is yes, before the backup, use symbal ◎, after the backup use the simbal √;
    For status of the file, before the completion, use the symbal ◎, after that use ✔;";
        }

    }

    class LabelFiles : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string labelID;
        public string LabelID
        {
            get
            {
                return labelID;
            }
            set
            {
                labelID = value;
                if(this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("LabelID"));
                }
            }
        }

        private string _ald;
        public string ald
        {
            get
            {
                return _ald;
            }
            set
            {
                _ald = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("ald"));
                }
            }
        }

        private string _ali;
        public string ali
        {
            get
            {
                return _ali;
            }
            set
            {
                _ali = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("ali"));
                }
            }
        }

        private string _alc;
        public string alc
        {
            get
            {
                return _alc;
            }
            set
            {
                _alc = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("alc"));
                }
            }
        }

        private string backup;
        public string Backup
        {
            get
            {
                return backup;
            }
            set
            {
                backup = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("Backup"));
                }
            }
        }

        private string status;
        public string Status
        {
            get
            {
                return status;
            }
            set
            {
                status = value;
                if(this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("Status"));
                }
            }
        }

        public LabelFiles(string ID,string hasALD = "", string hasALI = "",string hasALC = "",string hasBackup = "", string hasStatus = "")
        {
            this.LabelID = ID;
            this.ald = hasALD;
            this.ali = hasALI;
            this.alc = hasALC;
            this.Backup = hasBackup;
            this.Status = hasStatus;
        }
    }
}

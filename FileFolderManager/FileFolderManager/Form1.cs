using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace FileFolderManager
{
    public partial class Form1 : Form
    {
        private List<string> filePaths = new List<string>();
        private List<string> folderPaths = new List<string>();
        private string currentDirectory;
        private System.Windows.Forms.Timer timerDriveInfo;

        public Form1()
        {
            InitializeComponent();
            SetupForm();
        }

        private void SetupForm()
        {
            this.Text = "Файловый менеджер";
            buttonProperties.Click += FileProperties;
            buttonCopy.Click += ButtonCopy_Click;
            buttonRename.Click += ButtonRename_Click;
            buttonRemove.Click += ButtonMove_Click;
            buttonDelete.Click += ButtonDelete_Click;
            buttonBrowse.Click += buttonBrowse_Click;
            comboBoxSortBy.SelectedIndexChanged += SortFiles;

            comboBoxDisks.SelectedIndexChanged += ComboBoxDisks_SelectedIndexChanged;
            timerDriveInfo = new System.Windows.Forms.Timer { Interval = 5000 };
            timerDriveInfo.Tick += TimerDriveInfo_Tick;
            timerDriveInfo.Start();

            RefreshDriveList();
        }

        private static class NativeMethods
        {
            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
            public struct SHFILEOPSTRUCT
            {
                public IntPtr hwnd;
                public uint wFunc;
                public string pFrom;
                public string pTo;
                public ushort fFlags;
                public bool fAnyOperationsAborted;
                public IntPtr hNameMappings;
                public string lpszProgressTitle;
            }

            public const uint FO_COPY = 0x0002;
            public const uint FO_DELETE = 0x0003;
            public const uint FO_MOVE = 0x0001;
            public const uint FO_RENAME = 0x0004;

            public const ushort FOF_ALLOWUNDO = 0x0040;
            public const ushort FOF_WANTNUKEWARNING = 0x4000;

            [DllImport("shell32.dll", CharSet = CharSet.Auto)]
            public static extern int SHFileOperation(ref SHFILEOPSTRUCT FileOp);

            [DllImport("kernel32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool GetDiskFreeSpaceEx(
                string lpDirectoryName,
                out ulong lpFreeBytesAvailable,
                out ulong lpTotalNumberOfBytes,
                out ulong lpTotalNumberOfFreeBytes);

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern uint GetLogicalDrives();

            public struct MEMORYSTATUSEX
            {
                public uint dwLength;
                public uint dwMemoryLoad;
                public ulong ullTotalPhys;
                public ulong ullAvailPhys;
                public ulong ullTotalPageFile;
                public ulong ullAvailPageFile;
                public ulong ullTotalVirtual;
                public ulong ullAvailVirtual;
                public ulong ullAvailExtendedVirtual;
            }

            [DllImport("kernel32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool GlobalMemoryStatusEx(ref MEMORYSTATUSEX lpBuffer);

            [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            public static extern bool GetVolumeInformation(
                string lpRootPathName,
                StringBuilder lpVolumeNameBuffer,
                int nVolumeNameSize,
                out uint lpVolumeSerialNumber,
                out uint lpMaximumComponentLength,
                out uint lpFileSystemFlags,
                StringBuilder lpFileSystemNameBuffer,
                int nFileSystemNameSize);
        }

        private bool PerformFileOperation(uint operation, string source, string destination = null)
        {
            var fileOp = new NativeMethods.SHFILEOPSTRUCT
            {
                hwnd = this.Handle,
                wFunc = operation,
                pFrom = source + "\0\0",
                pTo = destination != null ? destination + "\0\0" : null,
                fFlags = NativeMethods.FOF_ALLOWUNDO | NativeMethods.FOF_WANTNUKEWARNING
            };

            int result = NativeMethods.SHFileOperation(ref fileOp);
            if (result != 0)
            {
                MessageBox.Show($"Ошибка при выполнении операции. Код: {result}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return true;
        }

        private void TimerDriveInfo_Tick(object sender, EventArgs e)
        {
            UpdateDriveInfo();
            UpdateRamInfo();
        }

        private void RefreshDriveList()
        {
            comboBoxDisks.Items.Clear();
            uint drives = NativeMethods.GetLogicalDrives();
            for (int i = 0; i < 26; i++)
            {
                if ((drives & (1 << i)) != 0)
                {
                    string driveLetter = $"{(char)('A' + i)}:\\";
                    comboBoxDisks.Items.Add(driveLetter);
                }
            }
            if (comboBoxDisks.Items.Count > 0)
                comboBoxDisks.SelectedIndex = 0;
        }

        private void UpdateRamInfo()
        {
            var memStatus = new NativeMethods.MEMORYSTATUSEX();
            memStatus.dwLength = (uint)Marshal.SizeOf(typeof(NativeMethods.MEMORYSTATUSEX));
            if (NativeMethods.GlobalMemoryStatusEx(ref memStatus))
            {
                ulong totalRAM = memStatus.ullTotalPhys;
                ulong availRAM = memStatus.ullAvailPhys;
                ulong usedRAM = totalRAM - availRAM;
                label2.Text = $"ОЗУ: всего {FormatBytes(totalRAM)}, свободно {FormatBytes(availRAM)} (используется {FormatBytes(usedRAM)})";
            }
            else
            {
                label2.Text = "Не удалось получить информацию о памяти";
            }
        }

        private void ComboBoxDisks_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateDriveInfo();
        }

        private void UpdateDriveInfo()
        {
            if (comboBoxDisks.SelectedItem == null) return;
            string drive = comboBoxDisks.SelectedItem.ToString();
            try
            {
                if (NativeMethods.GetDiskFreeSpaceEx(drive, out ulong freeBytes, out ulong totalBytes, out ulong _))
                {
                    labelDriveInfo.Text = $"Диск {drive}\r\nСвободно: {FormatBytes(freeBytes)} из {FormatBytes(totalBytes)}";
                }
                else
                {
                    labelDriveInfo.Text = "Не удалось получить информацию о диске";
                }
            }
            catch (Exception ex)
            {
                labelDriveInfo.Text = $"Ошибка: {ex.Message}";
            }
        }

        private string FormatBytes(ulong bytes)
        {
            string[] sizes = { "Б", "КБ", "МБ", "ГБ", "ТБ" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len /= 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        private void buttonBrowse_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
            {
                folderDialog.Description = "Выберите папку с файлами";
                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    LoadFiles(folderDialog.SelectedPath);
                }
            }
        }

        private void SortFiles(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(currentDirectory))
                LoadFiles(currentDirectory);
        }

        private void LoadFiles(string directoryPath)
        {
            try
            {
                if (!Directory.Exists(directoryPath))
                {
                    MessageBox.Show("Указанная папка не существует.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                currentDirectory = directoryPath;
                filePaths.Clear();
                folderPaths.Clear();
                listBoxFiles.Items.Clear();

                // Папки (всегда по имени)
                string[] directories = Directory.GetDirectories(directoryPath);
                Array.Sort(directories);
                foreach (string dir in directories)
                {
                    folderPaths.Add(dir);
                    listBoxFiles.Items.Add($"{Path.GetFileName(dir)}\\");
                }


                string[] files = Directory.GetFiles(directoryPath);
                var fileInfos = files.Select(f => new FileInfo(f)).ToArray();

                string sortBy = comboBoxSortBy.SelectedItem?.ToString() ?? "По имени";
                switch (sortBy)
                {
                    case "По размеру":
                        fileInfos = fileInfos.OrderBy(f => f.Length).ToArray();
                        break;
                    case "По дате изменения":
                        fileInfos = fileInfos.OrderBy(f => f.LastWriteTime).ToArray();
                        break;
                    case "По типу":
                        fileInfos = fileInfos.OrderBy(f => f.Extension).ThenBy(f => f.Name).ToArray();
                        break;
                    default:
                        fileInfos = fileInfos.OrderBy(f => f.Name).ToArray();
                        break;
                }

                foreach (var fi in fileInfos)
                {
                    filePaths.Add(fi.FullName);
                    listBoxFiles.Items.Add(fi.Name);
                }

                if (directories.Length == 0 && fileInfos.Length == 0)
                    listBoxFiles.Items.Add("Папка пуста");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FileProperties(object sender, EventArgs e)
        {
            int selectedIndex = listBoxFiles.SelectedIndex;
            labelName.Text = labelSize.Text = labelPath.Text = "";
            if (selectedIndex < 0)
            {
                MessageBox.Show("Выберите элемент.", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (selectedIndex < folderPaths.Count)
            {
                string folderPath = folderPaths[selectedIndex];
                DirectoryInfo dirInfo = new DirectoryInfo(folderPath);
                labelName.Text = dirInfo.Name;
                labelSize.Text = "<папка>";
                labelPath.Text = folderPath;
                return;
            }

            int fileIndex = selectedIndex - folderPaths.Count;
            if (fileIndex < filePaths.Count)
            {
                string filePath = filePaths[fileIndex];
                FileInfo fileInfo = new FileInfo(filePath);
                labelName.Text = fileInfo.Name;
                labelSize.Text = FormatBytes((ulong)fileInfo.Length);
                labelPath.Text = filePath;
            }
        }

        private void ButtonRename_Click(object sender, EventArgs e)
        {
            int selectedIndex = listBoxFiles.SelectedIndex;
            if (selectedIndex < 0)
            {
                MessageBox.Show("Выберите элемент.", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string newName = textBoxNewName.Text.Trim();
            if (string.IsNullOrEmpty(newName))
            {
                MessageBox.Show("Введите новое имя.", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string sourcePath = selectedIndex < folderPaths.Count ? folderPaths[selectedIndex] : filePaths[selectedIndex - folderPaths.Count];
            string destPath = Path.Combine(Path.GetDirectoryName(sourcePath), newName);

            // Для файла добавляем расширение, если нужно
            if (selectedIndex >= folderPaths.Count && !string.IsNullOrEmpty(Path.GetExtension(sourcePath)) && !newName.Contains("."))
                destPath += Path.GetExtension(sourcePath);

            if (PerformFileOperation(NativeMethods.FO_RENAME, sourcePath, destPath))
            {
                MessageBox.Show("Элемент переименован.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadFiles(currentDirectory);
            }
        }

        private void ButtonCopy_Click(object sender, EventArgs e)
        {
            int selectedIndex = listBoxFiles.SelectedIndex;
            if (selectedIndex < 0)
            {
                MessageBox.Show("Выберите элемент.", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string destDir = textBoxDestination.Text.Trim();
            if (string.IsNullOrEmpty(destDir) || !Directory.Exists(destDir))
            {
                MessageBox.Show("Укажите существующую папку назначения.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string sourcePath = selectedIndex < folderPaths.Count ? folderPaths[selectedIndex] : filePaths[selectedIndex - folderPaths.Count];
            string destPath = Path.Combine(destDir, Path.GetFileName(sourcePath));

            if (PerformFileOperation(NativeMethods.FO_COPY, sourcePath, destPath))
            {
                MessageBox.Show("Элемент скопирован.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                if (destDir == currentDirectory)
                    LoadFiles(currentDirectory);
            }
        }

        private void ButtonMove_Click(object sender, EventArgs e)
        {
            int selectedIndex = listBoxFiles.SelectedIndex;
            if (selectedIndex < 0)
            {
                MessageBox.Show("Выберите элемент.", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string destDir = textBoxDestination.Text.Trim();
            if (string.IsNullOrEmpty(destDir) || !Directory.Exists(destDir))
            {
                MessageBox.Show("Укажите существующую папку назначения.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string sourcePath = selectedIndex < folderPaths.Count ? folderPaths[selectedIndex] : filePaths[selectedIndex - folderPaths.Count];
            string destPath = Path.Combine(destDir, Path.GetFileName(sourcePath));

            if (PerformFileOperation(NativeMethods.FO_MOVE, sourcePath, destPath))
            {
                MessageBox.Show("Элемент перемещён.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadFiles(currentDirectory);
            }
        }

        private void ButtonDelete_Click(object sender, EventArgs e)
        {
            int selectedIndex = listBoxFiles.SelectedIndex;
            if (selectedIndex < 0)
            {
                MessageBox.Show("Выберите элемент.", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string sourcePath = selectedIndex < folderPaths.Count ? folderPaths[selectedIndex] : filePaths[selectedIndex - folderPaths.Count];
            string itemType = selectedIndex < folderPaths.Count ? "папку" : "файл";
            DialogResult result = MessageBox.Show($"Удалить {itemType} \"{Path.GetFileName(sourcePath)}\"?", "Подтверждение",
                                                   MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result != DialogResult.Yes) return;

            if (PerformFileOperation(NativeMethods.FO_DELETE, sourcePath))
            {
                MessageBox.Show("Элемент удалён.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadFiles(currentDirectory);
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Windows.Forms;
using System.IO.Compression;

namespace files
{
    public partial class Form1 : Form
    {
        private List<string> filePaths = new List<string>();
        private List<string> folderPaths = new List<string>();
        private string currentDirectory;

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
            buttonCreateZip.Click += ButtonCreateZip_Click;
            comboBoxSortBy.SelectedIndexChanged += SortFiles;
        }

        private void buttonBrowse_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
            {
                folderDialog.Description = "Выберите папку с файлами";
                folderDialog.ShowNewFolderButton = false;

                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    LoadFilesSorted(folderDialog.SelectedPath);
                }
            }
        }

        // Сортировка при изменении выбора в комбобоксе
        private void SortFiles(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(currentDirectory))
                LoadFilesSorted(currentDirectory);
        }

        // Загрузка с сортировкой
        private void LoadFilesSorted(string directoryPath)
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

                // Файлы
                string[] files = Directory.GetFiles(directoryPath);
                var fileInfos = files.Select(f => new FileInfo(f)).ToArray();

                // Сортировка файлов
                string sortBy = comboBoxSortBy.SelectedItem?.ToString() ?? "По имени";
                switch (sortBy)
                {
                    case "По имени":
                        fileInfos = fileInfos.OrderBy(f => f.Name).ToArray();
                        break;
                    case "По размеру":
                        fileInfos = fileInfos.OrderBy(f => f.Length).ToArray();
                        break;
                    case "По дате изменения":
                        fileInfos = fileInfos.OrderBy(f => f.LastWriteTime).ToArray();
                        break;
                    case "По типу":
                        fileInfos = fileInfos.OrderBy(f => f.Extension).ThenBy(f => f.Name).ToArray();
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
            labelName.Text = labelSize.Text = labelOwner.Text = labelData.Text = labelPath.Text = "";
            if (selectedIndex < 0) return;

            if (selectedIndex < folderPaths.Count) return; // папки не обрабатываем

            int fileIndex = selectedIndex - folderPaths.Count;
            if (fileIndex < filePaths.Count)
            {
                string filePath = filePaths[fileIndex];
                FileInfo fileInfo = new FileInfo(filePath);
                labelName.Text = fileInfo.Name;
                labelSize.Text = FormatFileSize(fileInfo.Length);
                labelOwner.Text = GetFileOwner(filePath);
                labelData.Text = fileInfo.LastWriteTime.ToString("dd.MM.yyyy HH:mm:ss");
                labelPath.Text = filePath;
            }
        }

        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "Б", "КБ", "МБ", "ГБ" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len /= 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        private string GetFileOwner(string filePath)
        {
            try
            {
                var fileInfo = new FileInfo(filePath);
                var fileSecurity = fileInfo.GetAccessControl();
                IdentityReference identity = fileSecurity.GetOwner(typeof(NTAccount));
                return identity?.Value ?? "Неизвестно";
            }
            catch
            {
                return "Нет доступа";
            }
        }

        private void ButtonRename_Click(object sender, EventArgs e)
        {
            int selectedIndex = listBoxFiles.SelectedIndex;
            if (selectedIndex < 0 || selectedIndex < folderPaths.Count)
            {
                MessageBox.Show("Выберите файл.", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string newName = textBoxNewName.Text.Trim();
            if (string.IsNullOrEmpty(newName))
            {
                MessageBox.Show("Введите новое имя файла.", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int fileIndex = selectedIndex - folderPaths.Count;
            string selectedFilePath = filePaths[fileIndex];
            string extension = Path.GetExtension(selectedFilePath);
            if (!string.IsNullOrEmpty(extension) && !newName.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
                newName += extension;

            string directory = Path.GetDirectoryName(selectedFilePath);
            string newPath = Path.Combine(directory, newName);

            try
            {
                if (File.Exists(newPath))
                {
                    DialogResult result = MessageBox.Show("Файл с таким именем уже существует. Заменить?", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result != DialogResult.Yes)
                        return;
                    File.Delete(newPath);
                }
                File.Move(selectedFilePath, newPath);
                MessageBox.Show("Файл успешно переименован.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadFilesSorted(currentDirectory);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Копирование через потоки
        private void ButtonCopy_Click(object sender, EventArgs e)
        {
            int selectedIndex = listBoxFiles.SelectedIndex;
            if (selectedIndex < 0 || selectedIndex < folderPaths.Count)
            {
                MessageBox.Show("Выберите файл.", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string destDir = textBoxDestination.Text.Trim();
            if (string.IsNullOrEmpty(destDir) || !Directory.Exists(destDir))
            {
                MessageBox.Show("Укажите существующую папку назначения.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int fileIndex = selectedIndex - folderPaths.Count;
            string sourcePath = filePaths[fileIndex];
            string destPath = Path.Combine(destDir, Path.GetFileName(sourcePath));

            try
            {
                if (File.Exists(destPath))
                {
                    DialogResult result = MessageBox.Show("Файл уже существует. Заменить?", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result != DialogResult.Yes)
                        return;
                    File.Delete(destPath);
                }

                // Используем потоковое копирование
                CopyFileWithStream(sourcePath, destPath);
                MessageBox.Show("Файл скопирован.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);

                if (destDir == currentDirectory)
                    LoadFilesSorted(currentDirectory);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Перемещение через потоки (копирование + удаление)
        private void ButtonMove_Click(object sender, EventArgs e)
        {
            int selectedIndex = listBoxFiles.SelectedIndex;
            if (selectedIndex < 0 || selectedIndex < folderPaths.Count)
            {
                MessageBox.Show("Выберите файл.", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string destDir = textBoxDestination.Text.Trim();
            if (string.IsNullOrEmpty(destDir) || !Directory.Exists(destDir))
            {
                MessageBox.Show("Укажите существующую папку назначения.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int fileIndex = selectedIndex - folderPaths.Count;
            string sourcePath = filePaths[fileIndex];
            string destPath = Path.Combine(destDir, Path.GetFileName(sourcePath));

            try
            {
                if (File.Exists(destPath))
                {
                    DialogResult result = MessageBox.Show("Файл уже существует. Заменить?", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result != DialogResult.Yes)
                        return;
                    File.Delete(destPath);
                }

                // Копируем потоком, затем удаляем исходный
                CopyFileWithStream(sourcePath, destPath);
                File.Delete(sourcePath);
                MessageBox.Show("Файл перемещён.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadFilesSorted(currentDirectory);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Удаление (без изменений)
        private void ButtonDelete_Click(object sender, EventArgs e)
        {
            int selectedIndex = listBoxFiles.SelectedIndex;
            if (selectedIndex < 0 || selectedIndex < folderPaths.Count)
            {
                MessageBox.Show("Выберите файл.", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int fileIndex = selectedIndex - folderPaths.Count;
            string filePath = filePaths[fileIndex];
            DialogResult result = MessageBox.Show($"Удалить файл \"{Path.GetFileName(filePath)}\"?", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result != DialogResult.Yes) return;

            try
            {
                File.Delete(filePath);
                MessageBox.Show("Файл удалён.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadFilesSorted(currentDirectory);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Потоковое копирование
        private void CopyFileWithStream(string sourcePath, string destPath)
        {
            const int bufferSize = 81920; // 80 КБ
            using (FileStream sourceStream = new FileStream(sourcePath, FileMode.Open, FileAccess.Read))
            using (FileStream destStream = new FileStream(destPath, FileMode.Create, FileAccess.Write))
            {
                byte[] buffer = new byte[bufferSize];
                int bytesRead;
                while ((bytesRead = sourceStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    destStream.Write(buffer, 0, bytesRead);
                }
            }
        }

        // Создание ZIP-архива
        private void ButtonCreateZip_Click(object sender, EventArgs e)
        {
            var selectedIndices = listBoxFiles.SelectedIndices;
            if (selectedIndices.Count == 0)
            {
                MessageBox.Show("Выберите файлы для архивации.", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            List<string> filesToZip = new List<string>();
            foreach (int index in selectedIndices)
            {
                if (index < folderPaths.Count)
                {
                    MessageBox.Show("Архивация папок пока не поддерживается.", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    continue;
                }
                int fileIndex = index - folderPaths.Count;
                if (fileIndex < filePaths.Count)
                    filesToZip.Add(filePaths[fileIndex]);
            }

            if (filesToZip.Count == 0) return;

            using (SaveFileDialog saveDialog = new SaveFileDialog())
            {
                saveDialog.Filter = "ZIP-архив (*.zip)|*.zip";
                saveDialog.DefaultExt = "zip";
                saveDialog.FileName = "Archive.zip";
                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    string zipPath = saveDialog.FileName;
                    try
                    {
                        using (FileStream zipStream = new FileStream(zipPath, FileMode.Create))
                        using (ZipArchive archive = new ZipArchive(zipStream, ZipArchiveMode.Create))
                        {
                            foreach (string filePath in filesToZip)
                            {
                                // Добавляем файл в архив (CreateEntryFromFile сама открывает поток)
                                archive.CreateEntryFromFile(filePath, Path.GetFileName(filePath));
                            }
                        }
                        MessageBox.Show("Архив успешно создан.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при создании архива: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
    }
}
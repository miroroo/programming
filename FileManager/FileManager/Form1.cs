using System;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Windows.Forms;

namespace FileManager
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
            comboBoxSortBy.SelectedIndexChanged += SortFiles;
        }

        // Выбор папки через диалог
        private void buttonBrowse_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
            {
                folderDialog.Description = "Выберите папку с файлами";
                folderDialog.ShowNewFolderButton = false;

                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    LoadFiles(folderDialog.SelectedPath);
                }
            }
        }

        // Загрузка списка папок и файлов
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

                // Папки
                string[] directories = Directory.GetDirectories(directoryPath);
                foreach (string dir in directories)
                {
                    folderPaths.Add(dir);
                    listBoxFiles.Items.Add($"{Path.GetFileName(dir)}\\");
                }

                // Файлы
                string[] files = Directory.GetFiles(directoryPath);
                foreach (string file in files)
                {
                    filePaths.Add(file);
                    listBoxFiles.Items.Add(Path.GetFileName(file));
                }

                // Если ничего нет
                if (directories.Length == 0 && files.Length == 0)
                {
                    listBoxFiles.Items.Add("Папка пуста");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Отображение свойств выбранного файла
        private void FileProperties(object sender, EventArgs e)
        {
            int selectedIndex = listBoxFiles.SelectedIndex;

            // Сброс меток
            labelName.Text = labelSize.Text = labelOwner.Text = labelData.Text = labelPath.Text = "";

            if (selectedIndex < 0) return;

            string selectedItem = listBoxFiles.Items[selectedIndex].ToString();

            if (selectedIndex < folderPaths.Count)
            {
                return;
            }

            // Иначе это файл
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
            if (selectedIndex < 0)
            {
                MessageBox.Show("Выберите элемент из списка.", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string selectedItem = listBoxFiles.Items[selectedIndex].ToString();

            if (selectedIndex < folderPaths.Count)
            {
                MessageBox.Show("Переименование папок пока не поддерживается.", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
            {
                newName += extension;
            }

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
                LoadFiles(currentDirectory);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при переименовании: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Копирование файла в указанную папку
        private void ButtonCopy_Click(object sender, EventArgs e)
        {
            int selectedIndex = listBoxFiles.SelectedIndex;
            if (selectedIndex < 0)
            {
                MessageBox.Show("Выберите элемент из списка.", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (selectedIndex < folderPaths.Count)
            {
                MessageBox.Show("Копирование папок пока не поддерживается.", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string destinationDirectory = textBoxDestination.Text.Trim();
            if (string.IsNullOrEmpty(destinationDirectory))
            {
                MessageBox.Show("Введите путь к папке назначения.", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!Directory.Exists(destinationDirectory))
            {
                MessageBox.Show("Указанная папка назначения не существует.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int fileIndex = selectedIndex - folderPaths.Count;
            string selectedFilePath = filePaths[fileIndex];
            string fileName = Path.GetFileName(selectedFilePath);
            string destPath = Path.Combine(destinationDirectory, fileName);

            try
            {
                if (File.Exists(destPath))
                {
                    DialogResult result = MessageBox.Show("Файл в папке назначения уже существует. Заменить?", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result != DialogResult.Yes)
                        return;
                    File.Delete(destPath);
                }

                File.Copy(selectedFilePath, destPath);
                MessageBox.Show("Файл успешно скопирован.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);

                if (destinationDirectory == currentDirectory)
                {
                    LoadFiles(currentDirectory);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при копировании: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Перемещение файла
        private void ButtonMove_Click(object sender, EventArgs e)
        {
            int selectedIndex = listBoxFiles.SelectedIndex;
            if (selectedIndex < 0)
            {
                MessageBox.Show("Выберите элемент из списка.", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (selectedIndex < folderPaths.Count)
            {
                MessageBox.Show("Перемещение папок пока не поддерживается.", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string destDir = textBoxDestination.Text.Trim();
            if (string.IsNullOrEmpty(destDir))
            {
                MessageBox.Show("Введите путь к папке назначения.", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!Directory.Exists(destDir))
            {
                MessageBox.Show("Указанная папка назначения не существует.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int fileIndex = selectedIndex - folderPaths.Count;
            string selectedFilePath = filePaths[fileIndex];
            string fileName = Path.GetFileName(selectedFilePath);
            string destPath = Path.Combine(destDir, fileName);

            try
            {
                if (File.Exists(destPath))
                {
                    DialogResult result = MessageBox.Show("Файл в папке назначения уже существует. Заменить?", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result != DialogResult.Yes)
                        return;
                    File.Delete(destPath);
                }

                File.Move(selectedFilePath, destPath);
                MessageBox.Show("Файл успешно перемещён.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadFiles(currentDirectory);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при перемещении: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Удаление файла
        private void ButtonDelete_Click(object sender, EventArgs e)
        {
            int selectedIndex = listBoxFiles.SelectedIndex;
            if (selectedIndex < 0)
            {
                MessageBox.Show("Выберите элемент из списка.", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (selectedIndex < folderPaths.Count)
            {
                MessageBox.Show("Удаление папок пока не поддерживается.", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int fileIndex = selectedIndex - folderPaths.Count;
            string filePath = filePaths[fileIndex];
            DialogResult result = MessageBox.Show($"Удалить файл \"{Path.GetFileName(filePath)}\"?",
                                                  "Подтверждение удаления",
                                                  MessageBoxButtons.YesNo,
                                                  MessageBoxIcon.Question);
            if (result != DialogResult.Yes)
                return;

            try
            {
                File.Delete(filePath);
                MessageBox.Show("Файл удалён.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadFiles(currentDirectory);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SortFiles(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(currentDirectory))
                LoadFilesSorted(currentDirectory);
        }

        private void LoadFilesSorted(string directoryPath)
        {
            try
            {
                if (!Directory.Exists(directoryPath))
                    return;

                currentDirectory = directoryPath;
                listBoxFiles.Items.Clear();

                // Получаем папки (всегда сортируем по имени)
                string[] directories = Directory.GetDirectories(directoryPath);
                Array.Sort(directories); // по алфавиту
                foreach (string dir in directories)
                {
                    listBoxFiles.Items.Add($"{Path.GetFileName(dir)}\\");
                }

                // Получаем файлы
                string[] files = Directory.GetFiles(directoryPath);
                var fileInfos = files.Select(f => new FileInfo(f)).ToArray();

                // Сортируем в зависимости от выбранного критерия
                string sortBy = comboBoxSortBy.SelectedItem.ToString();
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

                foreach (var fileInfo in fileInfos)
                {
                    listBoxFiles.Items.Add(fileInfo.Name);
                }

                if (directories.Length == 0 && fileInfos.Length == 0)
                    listBoxFiles.Items.Add("Папка пуста");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
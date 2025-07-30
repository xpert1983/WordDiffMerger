using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Diagnostics;

namespace WordDiffMerger
{
    public partial class MainForm : Form
    {
        public string OriginalFilePath { get; set; }
        public string ChangedFilesFolder { get; set; }
        public List<string> ChangedFiles { get; set; }
        public List<ChangeSet> AllChanges { get; set; }

        public MainForm()
        {
            InitializeComponent();
            ChangedFiles = new List<string>();
            AllChanges = new List<ChangeSet>();
            
            // Настройка формы
            this.Text = "WordDiffMerger - Объединение изменений в документах Word";
            this.StartPosition = FormStartPosition.CenterScreen;
            
            // Инициализация состояния кнопок
            UpdateButtonStates();
        }

        private void btnSelectOriginal_Click(object sender, EventArgs e)
        {
            try
            {
                using (var ofd = new OpenFileDialog())
                {
                    ofd.Filter = "Word Documents|*.doc;*.docx|All Files|*.*";
                    ofd.Title = "Выберите файл-оригинал";
                    ofd.CheckFileExists = true;

                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        OriginalFilePath = ofd.FileName;
                        txtOriginalFile.Text = OriginalFilePath;
                        
                        // Обновляем список измененных файлов если папка уже выбрана
                        if (!string.IsNullOrEmpty(ChangedFilesFolder))
                        {
                            UpdateChangedFilesList();
                        }
                        
                        UpdateButtonStates();
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError("Ошибка при выборе файла", ex);
            }
        }

        private void btnSelectFolder_Click(object sender, EventArgs e)
        {
            try
            {
                using (var fbd = new FolderBrowserDialog())
                {
                    fbd.Description = "Выберите папку с измененными файлами";
                    fbd.ShowNewFolderButton = false;

                    if (fbd.ShowDialog() == DialogResult.OK)
                    {
                        ChangedFilesFolder = fbd.SelectedPath;
                        txtChangedFolder.Text = ChangedFilesFolder;
                        
                        UpdateChangedFilesList();
                        UpdateButtonStates();
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError("Ошибка при выборе папки", ex);
            }
        }

        private void UpdateChangedFilesList()
        {
            try
            {
                ChangedFiles.Clear();
                
                if (string.IsNullOrEmpty(ChangedFilesFolder) || !Directory.Exists(ChangedFilesFolder))
                    return;

                var wordFiles = Directory.GetFiles(ChangedFilesFolder, "*.doc*", SearchOption.TopDirectoryOnly);
                
                foreach (var file in wordFiles)
                {
                    // Исключаем файл-оригинал и временные файлы
                    if (!string.Equals(file, OriginalFilePath, StringComparison.OrdinalIgnoreCase) &&
                        !Path.GetFileName(file).StartsWith("~$"))
                    {
                        ChangedFiles.Add(file);
                    }
                }

                // Обновляем отображение
                lstChangedFiles.DataSource = null;
                lstChangedFiles.DataSource = ChangedFiles.Select(Path.GetFileName).ToList();
                lstChangedFiles.ClearSelected();
            }
            catch (Exception ex)
            {
                ShowError("Ошибка при обновлении списка файлов", ex);
            }
        }

        private void btnAnalyze_Click(object sender, EventArgs e)
        {
            if (!ValidateInputs())
                return;

            try
            {
                // Показываем прогресс
                using (var progressForm = new ProgressForm("Анализ изменений", ChangedFiles.Count))
                {
                    progressForm.Show();
                    Application.DoEvents();

                    AllChanges.Clear();
                    int processedFiles = 0;

                    foreach (var changedFile in ChangedFiles)
                    {
                        try
                        {
                            progressForm.UpdateProgress(processedFiles, $"Обработка: {Path.GetFileName(changedFile)}");
                            Application.DoEvents();

                            var changeSet = WordComparer.CompareDocuments(OriginalFilePath, changedFile);
                            changeSet.ChangedFileName = Path.GetFileName(changedFile);
                            AllChanges.Add(changeSet);

                            processedFiles++;
                        }
                        catch (Exception ex)
                        {
                            var result = MessageBox.Show(
                                $"Ошибка при обработке файла {Path.GetFileName(changedFile)}:\n{ex.Message}\n\nПродолжить с другими файлами?",
                                "Ошибка обработки файла",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Warning);

                            if (result == DialogResult.No)
                                break;
                        }
                    }

                    progressForm.Close();
                }

                // Показываем результаты анализа
                ShowAnalysisResults();

                // Открываем форму разрешения конфликтов
                using (var reviewForm = new ChangeReviewForm(AllChanges))
                {
                    reviewForm.ShowDialog();
                }

                UpdateButtonStates();
            }
            catch (Exception ex)
            {
                ShowError("Ошибка при анализе изменений", ex);
            }
        }

        private void btnMerge_Click(object sender, EventArgs e)
        {
            if (!ValidateForMerge())
                return;

            try
            {
                // Запрашиваем путь для сохранения
                string mergedFilePath;
                using (var sfd = new SaveFileDialog())
                {
                    sfd.Filter = "Word Documents|*.docx|Word 97-2003|*.doc";
                    sfd.Title = "Сохранить объединенный документ";
                    sfd.FileName = Path.GetFileNameWithoutExtension(OriginalFilePath) + "_merged" + Path.GetExtension(OriginalFilePath);
                    sfd.InitialDirectory = Path.GetDirectoryName(OriginalFilePath);

                    if (sfd.ShowDialog() != DialogResult.OK)
                        return;

                    mergedFilePath = sfd.FileName;
                }

                // Применяем изменения
                using (var progressForm = new ProgressForm("Применение изменений", 1))
                {
                    progressForm.Show();
                    progressForm.UpdateProgress(0, "Создание объединенного документа...");
                    Application.DoEvents();

                    string resultPath = WordMerger.ApplyChanges(OriginalFilePath, AllChanges, mergedFilePath);

                    // Сохраняем лог
                    string logPath = Path.ChangeExtension(mergedFilePath, ".json");
                    ChangeLogger.SaveLog(AllChanges, logPath);

                    progressForm.Close();

                    // Показываем результат
                    var result = MessageBox.Show(
                        $"Объединение завершено!\n\nФайл: {resultPath}\nЛог: {logPath}\n\nОткрыть папку с результатом?",
                        "Готово",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Information);

                    if (result == DialogResult.Yes)
                    {
                        Process.Start("explorer.exe", $"/select,\"{resultPath}\"");
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError("Ошибка при объединении изменений", ex);
            }
        }

        private bool ValidateInputs()
        {
            if (string.IsNullOrEmpty(OriginalFilePath))
            {
                MessageBox.Show("Выберите файл-оригинал.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (!File.Exists(OriginalFilePath))
            {
                MessageBox.Show("Файл-оригинал не найден.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (ChangedFiles.Count == 0)
            {
                MessageBox.Show("Выберите папку с измененными файлами.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        private bool ValidateForMerge()
        {
            if (AllChanges.Count == 0)
            {
                MessageBox.Show("Сначала выполните анализ изменений.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        private void ShowAnalysisResults()
        {
            int totalChanges = AllChanges.Sum(cs => cs.Changes.Count);
            int conflictChanges = AllChanges.Sum(cs => cs.Changes.Count(c => c.IsConflict));

            string message = $"Анализ завершен!\n\n" +
                           $"Обработано файлов: {AllChanges.Count}\n" +
                           $"Найдено изменений: {totalChanges}\n" +
                           $"Конфликтов: {conflictChanges}";

            MessageBox.Show(message, "Результаты анализа", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void UpdateButtonStates()
        {
            btnAnalyze.Enabled = !string.IsNullOrEmpty(OriginalFilePath) && ChangedFiles.Count > 0;
            btnMerge.Enabled = AllChanges.Count > 0;
        }

        private void ShowError(string title, Exception ex)
        {
            string message = $"{ex.Message}\n\nПодробности:\n{ex}";
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
            
            // Логируем ошибку
            Trace.WriteLine($"[ERROR] {title}: {ex}");
        }
    }

    // Простая форма прогресса
    public class ProgressForm : Form
    {
        private ProgressBar progressBar;
        private Label statusLabel;
        private int maxValue;

        public ProgressForm(string title, int maxValue)
        {
            this.maxValue = maxValue;
            InitializeComponents(title);
        }

        private void InitializeComponents(string title)
        {
            this.Text = title;
            this.Size = new System.Drawing.Size(400, 120);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;

            statusLabel = new Label
            {
                Location = new System.Drawing.Point(12, 15),
                Size = new System.Drawing.Size(360, 20),
                Text = "Инициализация..."
            };

            progressBar = new ProgressBar
            {
                Location = new System.Drawing.Point(12, 40),
                Size = new System.Drawing.Size(360, 25),
                Maximum = maxValue,
                Value = 0
            };

            this.Controls.Add(statusLabel);
            this.Controls.Add(progressBar);
        }

        public void UpdateProgress(int value, string status)
        {
            progressBar.Value = Math.Min(value, maxValue);
            statusLabel.Text = status;
            this.Refresh();
        }
    }
}
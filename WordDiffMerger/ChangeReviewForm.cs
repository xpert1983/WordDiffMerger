using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace WordDiffMerger
{
    public partial class ChangeReviewForm : Form
    {
        private List<ChangeSet> allChanges;
        private FlowLayoutPanel flowPanel;
        private Button btnAcceptAll;
        private Button btnRejectAll;
        private Button btnClose;
        private Label lblStatus;

        public ChangeReviewForm(List<ChangeSet> changes)
        {
            InitializeComponent();
            allChanges = changes ?? new List<ChangeSet>();
            InitializeCustomComponents();
            InitializeReview();
            UpdateStatus();
        }

        private void InitializeCustomComponents()
        {
            this.Text = "Разрешение конфликтов изменений";
            this.Size = new Size(900, 700);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Панель кнопок
            var buttonPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                BackColor = SystemColors.Control
            };

            btnAcceptAll = new Button
            {
                Text = "Принять все",
                Location = new Point(10, 10),
                Size = new Size(100, 30)
            };
            btnAcceptAll.Click += BtnAcceptAll_Click;

            btnRejectAll = new Button
            {
                Text = "Отклонить все",
                Location = new Point(120, 10),
                Size = new Size(100, 30)
            };
            btnRejectAll.Click += BtnRejectAll_Click;

            btnClose = new Button
            {
                Text = "Закрыть",
                Location = new Point(780, 10),
                Size = new Size(100, 30),
                DialogResult = DialogResult.OK
            };

            lblStatus = new Label
            {
                Location = new Point(240, 15),
                Size = new Size(300, 20),
                Text = "Готово к применению"
            };

            buttonPanel.Controls.AddRange(new Control[] { btnAcceptAll, btnRejectAll, lblStatus, btnClose });

            // Основная панель прокрутки
            flowPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding = new Padding(10)
            };

            this.Controls.Add(flowPanel);
            this.Controls.Add(buttonPanel);
        }

        private void InitializeReview()
        {
            flowPanel.Controls.Clear();

            if (allChanges.Count == 0)
            {
                var noChangesLabel = new Label
                {
                    Text = "Изменения не найдены",
                    Font = new Font(SystemFonts.DefaultFont, FontStyle.Italic),
                    ForeColor = Color.Gray,
                    AutoSize = true,
                    Margin = new Padding(10)
                };
                flowPanel.Controls.Add(noChangesLabel);
                return;
            }

            foreach (var changeSet in allChanges)
            {
                CreateChangeSetPanel(changeSet);
            }
        }

        private void CreateChangeSetPanel(ChangeSet changeSet)
        {
            // Заголовок набора изменений
            var fileLabel = new Label
            {
                Text = $"Файл: {changeSet.ChangedFileName}",
                Font = new Font(SystemFonts.DefaultFont, FontStyle.Bold),
                BackColor = Color.LightBlue,
                Padding = new Padding(5),
                Width = 850,
                Height = 25
            };
            flowPanel.Controls.Add(fileLabel);

            var conflictChanges = changeSet.Changes.Where(c => c.IsConflict && c.Variants.Count > 1).ToList();
            var nonConflictChanges = changeSet.Changes.Where(c => !c.IsConflict || c.Variants.Count <= 1).ToList();

            // Конфликтующие изменения
            foreach (var change in conflictChanges)
            {
                CreateChangePanel(change, true);
            }

            // Неконфликтующие изменения (автоматически принимаются)
            if (nonConflictChanges.Count > 0)
            {
                foreach (var change in nonConflictChanges)
                {
                    CreateChangePanel(change, false);
                    // Автоматически выбираем первый вариант
                    if (change.Variants.Count > 0)
                        change.SelectedVariantIndex = 0;
                }
            }
        }

        private void CreateChangePanel(WordChange change, bool showSelection)
        {
            int panelHeight = showSelection ? (60 + change.Variants.Count * 50) : 80;
            var panel = new Panel
            {
                Width = 850,
                Height = panelHeight,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(5),
                BackColor = showSelection ? Color.LightYellow : Color.LightGreen
            };

            // Заголовок изменения
            var changeHeader = new Label
            {
                Text = $"Тип: {GetChangeTypeText(change.Type)} | Позиция: {change.StartPos}-{change.EndPos}",
                Font = new Font(SystemFonts.DefaultFont, FontStyle.Bold),
                Location = new Point(10, 5),
                Size = new Size(600, 15)
            };
            panel.Controls.Add(changeHeader);

            // Оригинальный текст
            var originalLabel = new Label
            {
                Text = $"Оригинал: \"{TruncateText(change.OriginalText, 80)}\"",
                Location = new Point(10, 25),
                Size = new Size(800, 20),
                ForeColor = Color.DarkBlue
            };
            panel.Controls.Add(originalLabel);

            if (showSelection && change.Variants.Count > 1)
            {
                // Варианты для выбора
                for (int i = 0; i < change.Variants.Count; i++)
                {
                    CreateVariantControls(panel, change, i, 50 + i * 45);
                }
            }
            else if (change.Variants.Count > 0)
            {
                // Единственный вариант (автоматически применяется)
                var variant = change.Variants[0];
                var autoLabel = new Label
                {
                    Text = $"Применяется автоматически: \"{TruncateText(variant.Text, 60)}\" (автор: {variant.Author})",
                    Location = new Point(10, 45),
                    Size = new Size(800, 20),
                    ForeColor = Color.DarkGreen,
                    Font = new Font(SystemFonts.DefaultFont, FontStyle.Italic)
                };
                panel.Controls.Add(autoLabel);
            }

            flowPanel.Controls.Add(panel);
        }

        private void CreateVariantControls(Panel parent, WordChange change, int variantIndex, int yPos)
        {
            var variant = change.Variants[variantIndex];
            
            // Радиокнопка для выбора варианта
            var radioButton = new RadioButton
            {
                Location = new Point(10, yPos),
                Size = new Size(15, 15),
                Checked = change.SelectedVariantIndex == variantIndex,
                Tag = new { Change = change, Index = variantIndex }
            };
            radioButton.CheckedChanged += (s, e) =>
            {
                if (((RadioButton)s).Checked)
                {
                    var tag = (dynamic)((RadioButton)s).Tag;
                    ((WordChange)tag.Change).SelectedVariantIndex = tag.Index;
                    UpdateStatus();
                }
            };

            // Текст варианта
            var variantLabel = new Label
            {
                Text = $"Вариант {variantIndex + 1} ({variant.Author}): \"{TruncateText(variant.Text, 50)}\"",
                Location = new Point(30, yPos),
                Size = new Size(500, 15)
            };

            // Комментарий к варианту
            var commentLabel = new Label
            {
                Text = string.IsNullOrEmpty(variant.Comment) ? "" : $"Комментарий: {TruncateText(variant.Comment, 40)}",
                Location = new Point(30, yPos + 18),
                Size = new Size(500, 15),
                ForeColor = Color.Gray,
                Font = new Font(SystemFonts.DefaultFont.FontFamily, 8)
            };

            parent.Controls.Add(radioButton);
            parent.Controls.Add(variantLabel);
            if (!string.IsNullOrEmpty(variant.Comment))
                parent.Controls.Add(commentLabel);
        }

        private void BtnAcceptAll_Click(object sender, EventArgs e)
        {
            foreach (var changeSet in allChanges)
            {
                foreach (var change in changeSet.Changes)
                {
                    if (change.Variants.Count > 0)
                        change.SelectedVariantIndex = 0; // Выбираем первый вариант
                }
            }
            RefreshDisplay();
            UpdateStatus();
        }

        private void BtnRejectAll_Click(object sender, EventArgs e)
        {
            foreach (var changeSet in allChanges)
            {
                foreach (var change in changeSet.Changes)
                {
                    change.SelectedVariantIndex = -1; // Отклоняем изменение
                }
            }
            RefreshDisplay();
            UpdateStatus();
        }

        private void RefreshDisplay()
        {
            InitializeReview();
        }

        private void UpdateStatus()
        {
            int totalChanges = 0;
            int resolvedChanges = 0;

            foreach (var changeSet in allChanges)
            {
                foreach (var change in changeSet.Changes)
                {
                    totalChanges++;
                    if (change.SelectedVariantIndex >= 0 && change.SelectedVariantIndex < change.Variants.Count)
                        resolvedChanges++;
                }
            }

            lblStatus.Text = $"Разрешено: {resolvedChanges} из {totalChanges}";
            lblStatus.ForeColor = (resolvedChanges == totalChanges) ? Color.Green : Color.Orange;
        }

        private string GetChangeTypeText(string type)
        {
            switch (type?.ToLower())
            {
                case "insert": return "Вставка";
                case "delete": return "Удаление";
                case "replace": return "Замена";
                case "comment": return "Комментарий";
                case "format": return "Форматирование";
                default: return "Неизвестно";
            }
        }

        private string TruncateText(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text))
                return "";
            
            text = text.Replace("\r", "").Replace("\n", " ").Replace("\t", " ");
            
            if (text.Length <= maxLength)
                return text;
            
            return text.Substring(0, maxLength - 3) + "...";
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            
            // ChangeReviewForm
            this.ClientSize = new Size(900, 700);
            this.Name = "ChangeReviewForm";
            this.Text = "Разрешение конфликтов изменений";
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MaximizeBox = true;
            this.MinimizeBox = true;
            this.ShowInTaskbar = true;
            
            this.ResumeLayout(false);
        }
    }
}
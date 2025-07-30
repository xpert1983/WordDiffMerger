using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Office.Interop.Word;

namespace WordDiffMerger
{
    public static class WordMerger
    {
        public static string ApplyChanges(string originalPath, List<ChangeSet> allChanges, string mergedPath)
        {
            Application wordApp = null;
            Document docOriginal = null;
            
            try
            {
                // Создаем копию оригинального файла
                File.Copy(originalPath, mergedPath, true);
                
                wordApp = new Application();
                wordApp.Visible = false;
                wordApp.DisplayAlerts = WdAlertLevel.wdAlertsNone;
                
                docOriginal = wordApp.Documents.Open(mergedPath, ReadOnly: false, Visible: false);

                // Собираем все изменения в один список
                var allWordChanges = new List<WordChange>();
                foreach (var cs in allChanges)
                {
                    allWordChanges.AddRange(cs.Changes);
                }

                // Фильтруем только выбранные изменения
                var selectedChanges = allWordChanges.Where(c => 
                    c.Variants.Count > 0 && 
                    c.SelectedVariantIndex >= 0 && 
                    c.SelectedVariantIndex < c.Variants.Count).ToList();

                // Сортируем изменения по убыванию позиции (чтобы не сбивать позиции при применении)
                selectedChanges.Sort((a, b) => b.StartPos.CompareTo(a.StartPos));

                // Применяем изменения
                foreach (var change in selectedChanges)
                {
                    try
                    {
                        ApplySingleChange(docOriginal, change);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Ошибка применения изменения: {ex.Message}");
                        // Продолжаем с другими изменениями
                    }
                }

                // Сохраняем документ
                docOriginal.Save();
                return mergedPath;
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при объединении изменений: {ex.Message}", ex);
            }
            finally
            {
                try
                {
                    docOriginal?.Close(false);
                    wordApp?.Quit(false);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Ошибка при закрытии Word: {ex.Message}");
                }
            }
        }

        private static void ApplySingleChange(Document document, WordChange change)
        {
            if (change.Variants.Count == 0 || 
                change.SelectedVariantIndex < 0 || 
                change.SelectedVariantIndex >= change.Variants.Count)
                return;

            var variant = change.Variants[change.SelectedVariantIndex];
            
            try
            {
                // Проверяем, что позиции корректны
                if (change.StartPos < 0 || change.EndPos < change.StartPos || 
                    change.EndPos > document.Range().End)
                {
                    System.Diagnostics.Debug.WriteLine($"Некорректные позиции: {change.StartPos}-{change.EndPos}");
                    return;
                }

                Range range = document.Range(change.StartPos, change.EndPos);
                
                switch (change.Type.ToLower())
                {
                    case "insert":
                        // Для вставки устанавливаем позицию в начало
                        range = document.Range(change.StartPos, change.StartPos);
                        range.Text = variant.Text ?? "";
                        break;
                        
                    case "delete":
                        // Удаляем текст
                        range.Text = "";
                        break;
                        
                    case "replace":
                        // Заменяем текст
                        range.Text = variant.Text ?? "";
                        break;
                        
                    case "comment":
                        // Добавляем комментарий
                        if (!string.IsNullOrEmpty(variant.Comment))
                        {
                            document.Comments.Add(range, variant.Comment);
                        }
                        break;
                        
                    case "format":
                        // Применяем форматирование (базовая реализация)
                        ApplyFormatting(range, variant);
                        break;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка применения изменения типа {change.Type}: {ex.Message}");
            }
        }

        private static void ApplyFormatting(Range range, ChangeVariant variant)
        {
            try
            {
                // Базовое форматирование - можно расширить по необходимости
                if (!string.IsNullOrEmpty(variant.Text))
                {
                    // Применяем основные стили форматирования
                    if (variant.Comment.Contains("bold"))
                        range.Font.Bold = 1;
                    if (variant.Comment.Contains("italic"))
                        range.Font.Italic = 1;
                    if (variant.Comment.Contains("underline"))
                        range.Font.Underline = WdUnderline.wdUnderlineSingle;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка применения форматирования: {ex.Message}");
            }
        }
    }
}
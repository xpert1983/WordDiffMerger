using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Office.Interop.Word;

namespace WordDiffMerger
{
    public static class WordComparer
    {
        public static ChangeSet CompareDocuments(string originalPath, string changedPath)
        {
            ChangeSet result = new ChangeSet();
            Application wordApp = null;
            Document docOriginal = null, docChanged = null, docDiff = null;

            try
            {
                wordApp = new Application();
                wordApp.Visible = false;
                wordApp.DisplayAlerts = WdAlertLevel.wdAlertsNone;

                docOriginal = wordApp.Documents.Open(originalPath, ReadOnly: true, Visible: false);
                docChanged = wordApp.Documents.Open(changedPath, ReadOnly: true, Visible: false);

                // Создаем документ сравнения
                docDiff = wordApp.CompareDocuments(
                    docOriginal, 
                    docChanged, 
                    WdCompareDestination.wdCompareDestinationNew,
                    WdGranularity.wdGranularityWordLevel,
                    true, true, true, true, true, true, true, true, true, true,
                    "", true);

                // Обрабатываем изменения (ревизии)
                ProcessRevisions(docDiff, result);

                // Обрабатываем комментарии
                ProcessComments(docDiff, result);

                // Находим конфликты
                DetectConflicts(result);
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при сравнении документов: {ex.Message}", ex);
            }
            finally
            {
                try
                {
                    docOriginal?.Close(false);
                    docChanged?.Close(false);
                    docDiff?.Close(false);
                    wordApp?.Quit(false);
                }
                catch (Exception ex)
                {
                    // Логируем ошибку закрытия, но не прерываем выполнение
                    System.Diagnostics.Debug.WriteLine($"Ошибка при закрытии Word: {ex.Message}");
                }
            }
            return result;
        }

        private static void ProcessRevisions(Document docDiff, ChangeSet result)
        {
            foreach (Revision rev in docDiff.Revisions)
            {
                try
                {
                    var change = new WordChange();
                    change.Type = GetChangeType(rev.Type);
                    
                    if (rev.Range != null)
                    {
                        change.OriginalText = rev.Range.Text ?? "";
                        change.StartPos = rev.Range.Start;
                        change.EndPos = rev.Range.End;
                    }

                    var variant = new ChangeVariant
                    {
                        Author = rev.Author ?? "Unknown",
                        Text = rev.Range?.Text ?? "",
                        Comment = GetCommentForRange(docDiff, rev.Range)
                    };

                    change.Variants.Add(variant);
                    result.Changes.Add(change);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Ошибка обработки ревизии: {ex.Message}");
                }
            }
        }

        private static void ProcessComments(Document docDiff, ChangeSet result)
        {
            foreach (Comment comment in docDiff.Comments)
            {
                try
                {
                    var change = new WordChange();
                    change.Type = "comment";
                    
                    if (comment.Scope != null)
                    {
                        change.OriginalText = comment.Scope.Text ?? "";
                        change.StartPos = comment.Scope.Start;
                        change.EndPos = comment.Scope.End;
                    }

                    var variant = new ChangeVariant
                    {
                        Author = comment.Author ?? "Unknown",
                        Text = comment.Scope?.Text ?? "",
                        Comment = comment.Range?.Text ?? ""
                    };

                    change.Variants.Add(variant);
                    result.Changes.Add(change);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Ошибка обработки комментария: {ex.Message}");
                }
            }
        }

        private static void DetectConflicts(ChangeSet result)
        {
            // Группируем изменения по позиции для обнаружения конфликтов
            var positionGroups = new Dictionary<int, List<WordChange>>();
            
            foreach (var change in result.Changes)
            {
                int key = change.StartPos;
                if (!positionGroups.ContainsKey(key))
                    positionGroups[key] = new List<WordChange>();
                positionGroups[key].Add(change);
            }

            // Отмечаем конфликты
            foreach (var group in positionGroups.Values)
            {
                if (group.Count > 1)
                {
                    foreach (var change in group)
                    {
                        change.IsConflict = true;
                    }
                }
            }
        }

        private static string GetChangeType(WdRevisionType revType)
        {
            switch (revType)
            {
                case WdRevisionType.wdRevisionInsert:
                    return "insert";
                case WdRevisionType.wdRevisionDelete:
                    return "delete";
                case WdRevisionType.wdRevisionReplace:
                    return "replace";
                case WdRevisionType.wdRevisionProperty:
                    return "format";
                default:
                    return "unknown";
            }
        }

        private static string GetCommentForRange(Document doc, Range range)
        {
            if (range == null) return "";
            
            try
            {
                foreach (Comment c in doc.Comments)
                {
                    if (c.Scope != null && 
                        c.Scope.Start <= range.Start && 
                        c.Scope.End >= range.End)
                    {
                        return c.Range?.Text ?? "";
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка поиска комментария: {ex.Message}");
            }
            return "";
        }
    }
}
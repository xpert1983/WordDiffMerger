using System.Collections.Generic;

namespace WordDiffMerger
{
    public class ChangeSet
    {
        public string ChangedFileName { get; set; }
        public List<WordChange> Changes { get; set; } = new List<WordChange>();
    }

    public class WordChange
    {
        public string Type { get; set; } // insert, delete, format, comment
        public int StartPos { get; set; }
        public int EndPos { get; set; }
        public string OriginalText { get; set; }
        public List<ChangeVariant> Variants { get; set; } = new List<ChangeVariant>();
        public int SelectedVariantIndex { get; set; }
        public bool IsConflict { get; set; }
    }

    public class ChangeVariant
    {
        public string Author { get; set; }
        public string Text { get; set; }
        public string Comment { get; set; }
    }
}
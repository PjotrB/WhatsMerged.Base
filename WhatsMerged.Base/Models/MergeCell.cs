using System.Diagnostics;

namespace WhatsMerged.Base.Models
{
    /// <summary>
    /// A cell that can be used in a MergeTable, with a Type and a Text.
    /// </summary>
    [DebuggerDisplay("{DebugText,nq}")]
    public class MergeCell
    {
        /// <summary>
        /// The cell type.
        /// </summary>
        public CellType Type;

        /// <summary>
        /// The cell text.
        /// </summary>
        public string Text;

        private string DebugText
        {
            get { return string.Format("{0}{{Type={1}, Text=\"{2}\"}}", nameof(MergeCell), Type, Text.Replace("\"", "\\\"")); }
        }
    }

    /// <summary>
    /// Enum for the Type of a MergeCell
    /// </summary>
    public enum CellType
    {
        /// <summary></summary>
        SubTitle,
        /// <summary></summary>
        BranchName,
        /// <summary></summary>
        BranchDate,
        /// <summary></summary>
        Merged,
        /// <summary></summary>
        NotMerged,
        /// <summary></summary>
        SelfJoin
    }
}
namespace WhatsMerged.Base.Models
{
    /// <summary>
    /// A header that can be used in a MergeTable, with a Type and a Text.
    /// </summary>
    public class MergeHeader
    {
        /// <summary>
        /// The header type.
        /// </summary>
        public HeaderCellType Type;

        /// <summary>
        /// The header text.
        /// </summary>
        public string Text;
    }

    /// <summary>
    /// Enum for the Type of a MergeHeader
    /// </summary>
    public enum HeaderCellType
    {
        /// <summary></summary>
        Regular,
        /// <summary></summary>
        Title,
        /// <summary></summary>
        BranchName
    }
}
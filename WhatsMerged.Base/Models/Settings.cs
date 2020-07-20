using Newtonsoft.Json;
using System.ComponentModel;

namespace WhatsMerged.Base.Models
{
    /// <summary>
    /// Settings contains 3 BindingLists. These are used by the WhatsMerged Engine to compare "Work" and/or "Merge" branches with each other, while ignoring the "Ignore" group of branches.
    /// </summary>
    public class Settings
    {
        /// <summary>
        /// The "Work" branches. Contains all branches that are not in the "Merge" or "Ignore" branch lists.
        /// </summary>
        [JsonIgnore]
        public BindingList<string> WorkBranches { get; set; }

        /// <summary>
        /// The "Merge" branches. This is where "Work" branches usually are merged into, and also from where Releases are created.
        /// </summary>
        public BindingList<string> MergeBranches { get; set; }

        /// <summary>
        /// The "Ignore" branches. Contains all branches that are old or obsolete. They still exist, bus are not relevant or interesing when doing branch compare actions.
        /// </summary>
        public BindingList<string> IgnoreBranches { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public Settings()
        {
            WorkBranches = new BindingList<string>();
            MergeBranches = new BindingList<string>();
            IgnoreBranches = new BindingList<string>();
        }
    }
}
using GitGetter.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using WhatsMerged.Base.Helpers;
using WhatsMerged.Base.Models;

namespace WhatsMerged.Base
{
    /// <summary>
    /// The WhatsMerged Engine offers analysis of the "merged/not merged" state of branches, using the IGitGetter of your choice. The output is a MergeTable with a 2-D array of MergeCells, that can be used to render a front-end grid to show the results.
    /// </summary>
    public class Engine
    {
        private const string SettingsFilename = "WhatsMerged.json";

        /// <summary>
        /// The "checkout" path on disk of the Git Project.
        /// </summary>
        public string ProjectPath { get; private set; }

        /// <summary>
        /// An Action that the client can set, allowing the Engine to save project settings if they were changed by the Engine.
        /// </summary>
        public Action SaveProjectSettings = () => { };

        /// <summary>
        /// ProjectSettings consists of 3 lists: WorkBranches, MergeBranches and IgnoreBranches. These are used to group branches together, so we can make meaningful comparison of groups of branches (e.g. which Work-branches have been merged into which Merge-branches, or any other combination). IgnoreBranches is used to put branches in that are old, obsolete or for any other reason not interesting, so they do not clutter the results.
        /// </summary>
        public Settings ProjectSettings;

        private readonly IGitGetter GitGetter;
        private readonly IErrorReporter Reporter;
        private const string CommitDateFormatString = "yyyy-MM-dd HH:mm";

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="gitGetter">The IGitGetter object of your choice. It connects the Engine to the branches stored in Git.</param>
        /// <param name="reporter">The IErrorReporter object that both the Engine and the IGitGetter will use to show any kinds of problems to the End User.</param>
        public Engine(IGitGetter gitGetter, IErrorReporter reporter)
        {
            GitGetter = gitGetter;
            Reporter = reporter;
        }

        /// <summary>
        /// Apply the Path and the ProjectSettings for the Engine to use.
        /// </summary>
        /// <param name="projectPath"></param>
        /// <param name="projectSettings"></param>
        public void ApplyProjectSettings(string projectPath, Settings projectSettings)
        {
            ProjectPath = projectPath;
            ProjectSettings = projectSettings;
        }

        /// <summary>
        /// Update cached data for the specified git project by connecting with the remote server, as configured in the git config. Report any errors using the specified reporter.
        /// </summary>
        public void GitRefresh()
        {
            GitGetter.Refresh(ProjectPath, Reporter);
        }

        /// <summary>
        /// Load remote branches for the path that has been set in property ProjectPath, then update ProjectSettings accordingly: (1) remove entries from MergeBranches and IgnoreBranches that no longer exist as remote branches; (2) load WorkBranches with all remote branches that are not present in MergeBranches or IgnoreBranches.
        /// </summary>
        public void GitLoadBranchesAndUpdateProjectSettings()
        {
            var remoteBranches = GitGetter.RemoteBranches(ProjectPath, Reporter);
            if (Reporter.HasError()) return;

            var changed = RemoveObsoleteBranches(ProjectSettings, remoteBranches);
            if (changed) SaveProjectSettings();

            ProjectSettings.WorkBranches = remoteBranches
                .Except(ProjectSettings.MergeBranches)
                .Except(ProjectSettings.IgnoreBranches)
                .ToBindingList();
        }

        /// <summary>
        /// Sort the branches in the list according their Cast Commit Dates as stored in Git.
        /// </summary>
        /// <param name="list"></param>
        public void SortByCommitDate(BindingList<string> list)
        {
            if (list.Count == 0) return;
            var sortedDatesAndBranches = GitGetter.BranchesByLastCommitDate(ProjectPath, Reporter);
            if (Reporter.HasError()) return;

            string[] newItems = sortedDatesAndBranches.Select(item => item.branch).Where(branch => list.Contains(branch)).ToArray();
            list.Clear();
            list.AddRange(newItems);

            if (IsListForSave(list)) SaveProjectSettings();
        }

        /// <summary>
        /// Return true if one or more of the specified lists are equal to any property of ProjectSettings that is saved in the project settings config file.
        /// </summary>
        /// <param name="lists"></param>
        /// <returns></returns>
        public bool IsListForSave(params BindingList<string>[] lists)
        {
            for (int i = 0; i < lists.Length; i++)
                if (lists[i] != ProjectSettings.WorkBranches) return true;

            return false;
        }

        /// <summary>
        /// Produce a MergeTable using branch info from Git, according to the specified from- and to-branches as kept in ProjectSettings.
        /// </summary>
        /// <param name="title"></param>
        /// <param name="fromWork"></param>
        /// <param name="fromMerge"></param>
        /// <param name="toWork"></param>
        /// <param name="toMerge"></param>
        /// <returns></returns>
        public MergeTable GetMergeTable(string title, bool fromWork = false, bool fromMerge = false, bool toWork = false, bool toMerge = false)
        {
            if (!fromWork && !fromMerge) { Reporter.ShowError("No 'From' collections specified."); return null; }
            if (!toWork && !toMerge) { Reporter.ShowError("No 'To' collections specified."); return null; }

            var from = fromWork && fromMerge ? WorkBranches.Concat(MergeBranches).ToBindingList()
                                  : fromWork ? WorkBranches
                                             : MergeBranches;

            var to = toWork && toMerge ? SortByCommitDate(WorkBranches.Concat(MergeBranches)).ToBindingList()
                              : toWork ? WorkBranches
                                       : MergeBranches;

            if (from.IsNullOrEmpty() || to.IsNullOrEmpty()) { Reporter.ShowError("Both 'From' and 'To' must have at least 1 branch."); return null; }

            var ignore = IgnoreBranches;
            if (!fromWork) ignore = ignore.Concat(WorkBranches).ToBindingList();
            if (!fromMerge) ignore = ignore.Concat(MergeBranches).ToBindingList();

            // Check for each entry in mergeBranches what branches have been merged into it, and which haven't.
            // The results are shown as grid rows. We make no rows for the branchesToHide, e.g. because these branches are merge
            // targets (meaning if all is well then no work is done in them), or because they are not of interest (e.g. very old).

            var merged = new List<string[]>();
            var notMerged = new List<string[]>();
            for (int i = 0; i < to.Count; i++)
            {
                merged.Add(GitGetter.MergedBranches(to[i], ignore, ProjectPath, Reporter));
                notMerged.Add(GitGetter.NotMergedBranches(to[i], ignore, ProjectPath, Reporter));
            }

            // Construct a distinct list of all work branches. We start with what we found for the last branch (it will contain the least merged work
            // branches), and then we proceed back to the first branch (it will contain the most merged work branches). This makes workBranches sorted
            // more or less from oldest-that-has-been-merged to newest-that-has-been-merged, followed by all branches that have not been merged.
            var fromBranchesOrderedByMergeStatus = new List<string>();
            for (int i = to.Count - 1; i >= 0; i--)
                fromBranchesOrderedByMergeStatus.AddRange(merged[i].Where(b => !fromBranchesOrderedByMergeStatus.Contains(b)));
            for (int i = to.Count - 1; i >= 0; i--)
                fromBranchesOrderedByMergeStatus.AddRange(notMerged[i].Where(b => !fromBranchesOrderedByMergeStatus.Contains(b)));

            var table = GetMergeTable(title, to, fromBranchesOrderedByMergeStatus, merged);
            return table;
        }

        private IEnumerable<string> SortByCommitDate(IEnumerable<string> branches)
        {
            var items = branches.ToArray();
            var branchesSorted = GitGetter.BranchesByLastCommitDate(ProjectPath, Reporter);
            return branchesSorted.Select(branchAndDate => branchAndDate.branch).Where(b => items.Contains(b));
        }

        // ********************* Private properties ********************* //

        private BindingList<string> WorkBranches => ProjectSettings.WorkBranches;
        private BindingList<string> MergeBranches => ProjectSettings.MergeBranches;
        private BindingList<string> IgnoreBranches => ProjectSettings.IgnoreBranches;

        // ********************* Private methods ************************ //

        private bool RemoveObsoleteBranches(Settings settings, string[] remoteBranches)
        {
            var changed = false;

            var obsoleteMerge = settings.MergeBranches.Where(b => !remoteBranches.Contains(b)).ToArray();
            if (obsoleteMerge.Length > 0)
            {
                foreach (var b in obsoleteMerge) Reporter.ShowError("Merge branch '" + b + "' no longer exists.");
                settings.MergeBranches = settings.MergeBranches.Except(obsoleteMerge).ToBindingList();
                changed = true;
            }

            var obsoleteIgnored = settings.IgnoreBranches.Where(b => !remoteBranches.Contains(b)).ToArray();
            if (obsoleteIgnored.Length > 0)
            {
                foreach (var b in obsoleteIgnored) Reporter.ShowError("Ignored branch '" + b + "' no longer exists.");
                settings.IgnoreBranches = settings.IgnoreBranches.Except(obsoleteIgnored).ToBindingList();
                changed = true;
            }

            return changed;
        }

        private MergeTable GetMergeTable(string title, IList<string> toBranches, IList<string> fromBranches, List<string[]> merged)
        {
            var lastCommitDates = GitGetter.BranchesByLastCommitDate(ProjectPath, Reporter);
            if (Reporter.HasError()) return null;

            var table = new MergeTable(fromBranches.Count + 1, 1, toBranches.Count + 2, 2, hasRowHeaders: false, hasColHeaders: true);
            int row;
            int col;

            // Set column headers:
            col = 0;
            table.ColHeaders[col++] = new MergeHeader { Type = HeaderCellType.Title, Text = title };
            table.ColHeaders[col++] = new MergeHeader { Type = HeaderCellType.Regular, Text = "" };
            for (int i = 0; i < toBranches.Count; i++)
                table.ColHeaders[col++] = new MergeHeader { Type = HeaderCellType.BranchName, Text = toBranches[i] };
            if (col != table.ColHeaders.Length) { Reporter.ShowError("Invalid column header count. Expected: " + table.ColHeaders.Length + ", found: " + col + "."); return null; }

            // Set row headers:
            // ********* Not present *********

            // Set cells for first row:
            row = 0;
            col = 0;
            table.Cells[row, col++] = new MergeCell { Type = CellType.SubTitle, Text = "Branch" };
            table.Cells[row, col++] = new MergeCell { Type = CellType.SubTitle, Text = "Last commit date" };
            for (int i = 0; i < toBranches.Count; i++)
                table.Cells[row, col++] = new MergeCell { Type = CellType.BranchDate, Text = GetDateString(lastCommitDates, toBranches[i]) };

            // Set cells for next rows:
            for (var j = 0; j < fromBranches.Count; j++)
            {
                row++;
                col = 0;
                table.Cells[row, col++] = new MergeCell { Type = CellType.BranchName, Text = fromBranches[j] };
                table.Cells[row, col++] = new MergeCell { Type = CellType.BranchDate, Text = GetDateString(lastCommitDates, fromBranches[j]) };
                for (int i = 0; i < toBranches.Count; i++)
                {
                    var cellType = toBranches[i] == fromBranches[j] ? CellType.SelfJoin : merged[i].Contains(fromBranches[j]) ? CellType.Merged : CellType.NotMerged;
                    var cellText = cellType == CellType.SelfJoin ? "-" : cellType == CellType.Merged ? "✔" : "✘";
                    table.Cells[row, col++] = new MergeCell { Type = cellType, Text = cellText };
                }
            }

            return table;
        }

        private string GetDateString((DateTime date, string branch)[] datesAndBranches, string branchName)
        {
            return datesAndBranches.FirstOrDefault(item => item.branch == branchName).date.ToString(CommitDateFormatString);
        }
    }
}
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WhatsMerged.Base.Helpers
{
    /// <summary>
    /// Utility class for scanning folders on a disk.
    /// </summary>
    public static class Disk
    {
        /// <summary>
        /// Scan folders recursively, starting from 'path'. For every 'folderToFind' that is found, return the folder that contains it (so excluding 'folderToFind') and don't scan any deeper.
        /// If 'folderToSkip' is not empty, and we find 'folderToSkip' during the recursive search then we don't scan any deeper down that path.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="folderToFind"></param>
        /// <param name="foldersToSkip"></param>
        /// <returns></returns>
        public static IEnumerable<string> LoadSubDirs(string path, string folderToFind, params string[] foldersToSkip)
        {
            foldersToSkip = foldersToSkip ?? new string[0];

            var doAllFolders = foldersToSkip.Length == 0;
            if (!doAllFolders)
            {
                for (int i = 0; i < foldersToSkip.Length; i++)
                {
                    foldersToSkip[i] = foldersToSkip[i].Trim();
                    if (foldersToSkip[i].Length > 0 && !foldersToSkip[i].StartsWith(@"\"))
                        foldersToSkip[i] = @"\" + foldersToSkip[i];
                }
            }

            return LoadSubDirs_private(path, folderToFind, doAllFolders, foldersToSkip);
        }

        private static IEnumerable<string> LoadSubDirs_private(string path, string folderToFind, bool doAllFolders, string[] foldersToSkip)
        {
            if (Directory.Exists(Path.Combine(path, folderToFind)))
            {
                yield return path;
            }
            else
            {
                foreach (string dir in Directory.GetDirectories(path))
                    if (doAllFolders || !foldersToSkip.Any(f => f.Length > 0 && dir.EndsWith(f, System.StringComparison.InvariantCultureIgnoreCase)))
                        foreach (var result in LoadSubDirs_private(dir, folderToFind, doAllFolders, foldersToSkip))
                            yield return result;
            }
        }
    }
}
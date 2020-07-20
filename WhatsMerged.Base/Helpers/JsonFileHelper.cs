using GitGetter.Interfaces;
using Newtonsoft.Json;
using System.IO;

namespace WhatsMerged.Base.Helpers
{
    /// <summary>
    /// Helper for saving/loading an object to/from disk as JSON.
    /// </summary>
    public static class JsonFileHelper
    {
        /// <summary>
        /// Try to load a JSON file from the specified path and deserialize it as an object of type T. If there is no such file, a default(T) object is returned. Errors are shown by means of the IErrorReporter object.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="filename"></param>
        /// <param name="reporter"></param>
        /// <returns></returns>
        public static T Load<T>(string path, string filename, IErrorReporter reporter)
        {
            if (path.HasValue())
            {
                try
                {
                    var json = File.ReadAllText(Path.Combine(path, filename));
                    return JsonConvert.DeserializeObject<T>(json);
                }
                catch (FileNotFoundException)
                {
                    // ignore, this is allowed to happen. We proceed to return a default object.
                }
                catch (System.Exception ex)
                {
                    reporter.ShowError("Error loading settings: " + ex.Message);
                }
            }
            return default(T);
        }

        /// <summary>
        /// Try to save the specified object as a file on the specified path. Errors are shown by means of the IErrorReporter object.
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="path"></param>
        /// <param name="filename"></param>
        /// <param name="reporter"></param>
        public static void Save<T>(T settings, string path, string filename, IErrorReporter reporter)
        {
            if (path.HasValue())
            {
                try
                {
                    var json = JsonConvert.SerializeObject(settings, Formatting.Indented);
                    File.WriteAllText(Path.Combine(path, filename), json);
                }
                catch (System.Exception ex)
                {
                    reporter.ShowError("Error saving settings: " + ex.Message);
                }
            }
        }
    }
}
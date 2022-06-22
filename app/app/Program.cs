using System;
using System.Collections.Generic;
using System.IO;

namespace app
{
    internal class Program
    {
        private static Github github = new Github();
        private static string path = @"C:\Users\user\Desktop\git_test\demo\";
        static void Main(string[] args)
        {
            Watcher(path);
            Console.ReadLine();
        }

        private static void Watcher(string path)
        {
            FileSystemWatcher watcher = new FileSystemWatcher();
            watcher.Path = path;
            watcher.NotifyFilter = NotifyFilters.LastWrite;
            watcher.Filter = "*.*";

            watcher.Changed += Changed;
            watcher.EnableRaisingEvents = true;
        }

        static DateTime lastRead = DateTime.MinValue;
        private async static void Changed(object sender, FileSystemEventArgs e)
        {
            Dictionary<string, string> project_files = new Dictionary<string, string>();
            //List<string> list = new List<string>();
            DateTime lastWrite = File.GetLastWriteTime(path);

            if (lastWrite != lastRead)
            {
                Console.WriteLine("path: " + e.Name);
                try
                {
                    string[] files = Directory.GetFiles(e.FullPath);

                    foreach (string file in files)
                    {
                        string[] str = file.Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);

                        if (str.Length != 0)
                        {
                            project_files.Add(str[str.Length - 1], file);
                        }
                    }
                        

                    var sha_file_list = await github.CreateBlob(project_files);
                    string current_commit_url = await github.GetCurrentCommit();
                    string base_tree_sha = await github.GetCurrentCommitTree(current_commit_url);

                    await github.CreateTree(base_tree_sha, sha_file_list);
                } 
                catch(IOException error) 
                { 
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(error); 
                }
                lastRead = lastWrite;
            }
        }

        public static string ConvertToBase64String(string file_path)
        {
            byte[] bytes = File.ReadAllBytes(file_path);
            return Convert.ToBase64String(bytes);
        }
    }
}

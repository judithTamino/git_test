﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace app
{
    internal class Program
    {
        private static Github github = new Github();
        private static ReadTestResult test = new ReadTestResult();

        private static DateTime lastRead = DateTime.MinValue;

        private static string path = @"C:\Users\user\Desktop\Demo_Project";
        private static string repo = "Demo_Project";

        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("WELCOME TO MY LITTLE CI");
            Console.ResetColor();

            Watcher(path);
        }

        private static void Watcher(string path)
        {
            FileSystemWatcher watcher = new FileSystemWatcher();
            watcher.Path = path;
            watcher.NotifyFilter = NotifyFilters.Attributes
                             | NotifyFilters.CreationTime
                             | NotifyFilters.DirectoryName
                             | NotifyFilters.FileName
                             | NotifyFilters.LastAccess
                             | NotifyFilters.LastWrite
                             | NotifyFilters.Security
                             | NotifyFilters.Size;

            watcher.Changed += Changed;

            watcher.Filter = "*.cs";
            watcher.IncludeSubdirectories = true;
            watcher.EnableRaisingEvents = true;

            Console.WriteLine("Press enter to exit.");
            Console.ReadLine();
        }

        
        private async static void Changed(object sender, FileSystemEventArgs e)
        {
            DateTime lastWrite = File.GetLastWriteTime(path);

            if (lastWrite != lastRead)
            {
                RunBuild();
                RunTest();

                UploadToGithub(e.FullPath);
                lastRead = lastWrite;
            }
        }

        private static void RunBuild()
        {
            string exeMSBulidPath = @"C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe";
            string solutionPath = @"C:\Users\user\Desktop\Demo_Project\DemoProject\DemoProject.sln";

            if(!RunProcess(exeMSBulidPath, solutionPath))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\nBuild Unsuccessfully");
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\nBuild Successfully");
                Console.ResetColor();
            }
        }

        private static void RunTest()
        {
            string testDllPath = @"C:\Users\user\Desktop\Demo_Project\DemoProject\TestProject\bin\Debug\net6.0\TestProject.dll";
            string exeFilePath = @"C:\Users\user\Desktop\git_test\app\packages\NUnit.ConsoleRunner.3.15.0\tools\nunit3-console.exe";

            RunProcess(exeFilePath, testDllPath);
        }

        private static bool RunProcess(string filename, string arguments)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = false;
            startInfo.UseShellExecute = false;
            startInfo.FileName = filename;
            startInfo.Arguments = arguments;

            using(Process p = Process.Start(startInfo))
            {
                p.WaitForExit();
                return (p.ExitCode == 0);
            }
        }

        private static async void UploadToGithub(string file)
        {
            Dictionary<string, string> encodedFiles = new Dictionary<string, string>();

            int repo_name_indx = file.IndexOf(repo);
            string[] repo_url = file.Substring(repo_name_indx).Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);

            encodedFiles.Add(ConstructGithubPath(repo_url), ConvertToBase64String(file));


            var blobFiles = await github.CreateBlob(encodedFiles);

            string current_commit_sha = await github.GetCurrentCommit();
            string current_base_tree_sha = await github.GetBaseTree(current_commit_sha);
            string new_tree_sha = await github.CreateTree(current_base_tree_sha, blobFiles);
            string new_commit_sha = await github.AddCommit(new_tree_sha, current_commit_sha);

            github.UpdateRef(new_commit_sha);
        }

        private static string ConstructGithubPath(string[] repoUrl)
        {
            string[] slicedUrl = new string[repoUrl.Length - 1];
            string path = "";

            Array.Copy(repoUrl, 1, slicedUrl, 0, slicedUrl.Length);
            for (int i = 0; i < slicedUrl.Length; i++)
                path += slicedUrl[i] + "/";

            return path.Remove(path.Length - 1);
        }

        public static string ConvertToBase64String(string file_path)
        {
            byte[] bytes = File.ReadAllBytes(file_path);
            return Convert.ToBase64String(bytes);
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace socket_udp
{
    class Repository
    {
        private readonly string filesPath = Path.GetTempPath() + "files_repository";

        internal void IniRepository()
        {
            Directory.CreateDirectory(filesPath);
        }

        internal void CreateDirectory(string subpath)
        {
            Directory.CreateDirectory(GetCompletePath(subpath));
        }

        internal void CreateFile(string filename, byte[] bytes)
        {
            string filePath = GetCompletePath(filename);

            FileInfo file = new FileInfo(filePath);
            if (!file.Directory.Exists)
            {
                Directory.CreateDirectory(file.Directory.FullName);
            }

            try
            {
                File.WriteAllBytes(filePath, bytes);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        internal string GetDirectoryFiles()
        {
            return string.Join(",", GetDirectoryList());
        }

        internal byte[] GetFile(string filePath)
        {
            try
            {
                return File.ReadAllBytes(GetCompletePath(filePath));
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine($"Arquivo '{filePath}' não encontrado");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return null;
        }

        internal void CheckDirectoryUpdate(string directoryFiles, UDPSocket response)
        {
            List<string> requestFiles = directoryFiles.Split(',').ToList();
            List<string> localFiles = GetDirectoryList();

            bool foundFile;
            foreach (string rqFile in requestFiles)
            {
                foundFile = false;
                foreach (string lcFile in localFiles)
                {
                    if (rqFile.Equals(lcFile))
                    {
                        foundFile = true;
                        break;
                    }
                }

                if (!foundFile)
                {
                    response.Send($"PAE;{rqFile}");
                    Thread.Sleep(100);
                }
            }
        }

        private string GetFilePath(string subpath, string fileName)
        {
            return GetCompletePath(subpath) + "\\" + fileName;
        }

        private string GetCompletePath(string subpath)
        {
            return $"{filesPath}\\{subpath}";
        }

        private List<string> GetDirectoryList()
        {
            List<string> filePaths = new List<string>();

            DirectoryInfo[] directories = new DirectoryInfo(filesPath).GetDirectories();
            foreach (DirectoryInfo directory in directories)
            {
                foreach (FileInfo file in directory.GetFiles())
                {
                    filePaths.Add(string.Format("{0}\\{1}", directory.Name, file.Name));
                }
            }

            return filePaths;
        }
    }
}

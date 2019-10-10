using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace socket_udp
{
    class Repository
    {
        private const string RootPath = "files_repository";
        private readonly string FilesPath = Path.GetTempPath() + RootPath;
        private string MyDir;

        internal void IniRepository(string myIp)
        {
            MyDir = myIp;
            Directory.CreateDirectory(GetCompletePath(MyDir));
        }

        private string GetFilePath(string subpath, string fileName)
        {
            return GetCompletePath(subpath) + "\\" + fileName;
        }

        private string GetCompletePath(string subpath)
        {
            return $"{FilesPath}\\{subpath}";
        }

        internal void CreateFile(string subpath, string filename, byte[] bytes)
        {
            string filePath = GetFilePath(subpath, filename);

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
            return string.Join(",", GetDirectoryList(MyDir));
        }

        internal byte[] GetFile(string filename)
        {
            try
            {
                return File.ReadAllBytes(GetFilePath(MyDir, filename));
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("Arquivo '{0}' não encontrado", filename);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return null;
        }

        internal List<string> CheckDirectoryUpdate(string directoryFiles, UDPSocket response, string subpath)
        {
            string[] requestFiles = directoryFiles.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            string[] localFiles = GetDirectoryList(subpath);

            foreach (string lcFile in localFiles.Except(requestFiles))
            {
                //se houver necessidade deleta arquivos do diretorio
                DeleteFile(subpath, lcFile);
            }

            return requestFiles.Except(localFiles).ToList();
        }

        private string[] GetDirectoryList(string subpath)
        {
            DirectoryInfo directory = new DirectoryInfo(GetCompletePath(subpath));
            if (!directory.Exists)
            {
                Directory.CreateDirectory(directory.FullName);
            }

            return directory.GetFiles().Select(x => x.Name).ToArray();
        }

        private void DeleteFile(string subpath, string filename)
        {
            try
            {
                File.Delete(GetFilePath(subpath, filename));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}

using System;
using System.IO;
using System.Linq;
using FluentFTP;

namespace FluentFtpTest
{
	internal static class Program
	{
		private static void Main(string[] args)
		{
			if (args.Length != 4)
			{
				Console.WriteLine("The following parameters are required: ftp host name, user name, password, local folder");
				return;
			}

			// подключиться к ftp серверу, вывести список файлов в каталоге
			try
			{
				var server = args[0];
				var user = args[1];
				var password = args[2];

				using (var ftp = new FtpClient(server, 21, user, password))
				{
					// Получить содержимое каталога и выбрать только архивные файлы
					var files = ftp.GetListing().Where(
						  item => item.Type == FtpFileSystemObjectType.File && item.Name.EndsWith(".ar")).ToList();

					Console.WriteLine("There are {0} zip files", files.Count);

					string fileWithLargestSize = null;
					long largestSize = -1;

					foreach (var f in files)
					{
						var name = f.Name;
						var size = f.Size;

						Console.WriteLine("File: {0}, size = {1}", name, size);

						if (size <= largestSize) continue;

						largestSize = size;
						fileWithLargestSize = name;
					}

					// скачать файл с наибольшим размером
					if (fileWithLargestSize != null)
					{
						try
						{
							ftp.DownloadFile(Path.Combine(args[3], fileWithLargestSize), fileWithLargestSize);
							Console.WriteLine("Downloaded {0}", fileWithLargestSize);

						}
						catch (FtpException ex)
						{
							Console.WriteLine("Failed to download file {0}. Error: {1}", fileWithLargestSize, ex.Message);
						}
					}
					else
						Console.WriteLine("Nothing to download.");
				}

			}
			catch (Exception ex)
			{
				Console.WriteLine("Error: {0}", ex.Message);
			}
		}
	}
}

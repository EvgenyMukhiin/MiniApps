using System;
using System.IO;
using System.Threading.Tasks;

namespace Id3Renamer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Для запуска добавление тэгов нажмите любую кнопку");
            Console.ReadKey();
            Console.Write("Ведите адрес папки с mp3: ");
            string? path = Console.ReadLine();
            Task.Run(() => Start(path)).Wait();
            Console.WriteLine("Добавление тэгов завешено");
            Console.ReadKey();
        }
        public static async Task Start(string path)
        {
            DirectoryInfo dir = new DirectoryInfo(path);
            try
            {
                int count = 1;
                foreach (FileInfo file in dir.GetFiles())
                {
                    if (file.Extension.ToUpperInvariant() == ".mp3" || file.Extension.ToUpperInvariant() == ".MP3")
                      await Task.Factory.StartNew(() => Renames(file, path, count));
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"{count} - {file} Файл пропущен");
                    }
                    count++;
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex);
            }
        }
        public static Task Renames(FileInfo file, string path, int count)
        {
            string art;
            string tit;
            try
            {
                art = file.Name.Substring(0, file.Name.IndexOf(" -"));
                tit = file.Name.Remove(0, file.Name.LastIndexOf("-") + 1).Replace(".mp3", "");
                //
                var tfile = TagLib.File.Create($"{path}/{file.Name}");
                string[] artist = { art };
                tfile.Tag.Title = tit;
                tfile.Tag.Artists = artist;
                //tfile.Tag.Album = "album";
                tfile.Save();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"{count} - {file.Name} - Ошибка!");
            }
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"{count} - {file.Name} - id3 тэги добавлены");
            return Task.CompletedTask;
        }
    }
}

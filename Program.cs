
using System;

namespace MonitorStudentsActivities
{
    class Program
    {
        static void Main(string[] args)
        {
            // Создание Базы данных
            var dbPath = "database.db";
            var dbHandler = new DatabaseHandler(dbPath);
            dbHandler.CreateTables();

            // Импорт данных из файлов в таблицы
            var csvHandler = new CsvHandler(dbPath);
            csvHandler.ImportCsvToTable("Students.csv", "Students");
            csvHandler.ImportCsvToTable("Works.csv", "Works");
            csvHandler.ImportCsvToTable("Results.csv", "Results");

            // Вызов метода генерации сводной таблицы
            dbHandler.GenerateCurrentResults();

            var summaryEditor = new SummaryEditor(dbPath);

            while (true)
            {
                Console.WriteLine("Menu:");
                Console.WriteLine("1. Display Current Results");
                Console.WriteLine("2. Edit Results");
                Console.WriteLine("3. Export Results");
                Console.WriteLine("4. Exit");
                Console.Write("Select an option: ");

                var input = Console.ReadLine();
                switch (input)
                {
                    case "1":
                        summaryEditor.DisplayCurrentResults();
                        break;
                    case "2":
                        summaryEditor.EditResultsFromConsole();
                        break;
                    case "3":
                        csvHandler.ExportResultsToCsv();
                        break;
                    case "4":
                        return;
                    default:
                        Console.WriteLine("Invalid option.");
                        break;
                }
            }
        }
    }

}

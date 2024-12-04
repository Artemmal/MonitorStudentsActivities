using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace MonitorStudentsActivities
{
    /// <summary>
    /// Класс для работы с файлами csv
    /// </summary>
    public class CsvHandler
    {
        private readonly string connectionString;

        public CsvHandler(string dbPath)
        {
            connectionString = $"Data Source={dbPath};";
        }

        public void ImportCsvToTable(string filePath, string tableName)
        {
            try
            {
                using (var reader = new StreamReader(filePath))
                {
                    var lines = new List<string>();

                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        lines.Add(line);
                    }

                    using (var connection = new SqliteConnection(connectionString))
                    {
                        connection.Open();

                        // Очистка таблицы перед вставкой новых данных
                        ExecuteQuery(connection, $"DELETE FROM {tableName};");

                        for (int i = 1; i < lines.Count; i++)
                        {
                            var columns = lines.First();
                            var splitedLine = lines[i].Split(',');
                            var values = string.Join(", ", splitedLine.Select(v => $"'{v}'"));
                            var query = $"INSERT INTO {tableName} ({columns}) VALUES ({values});";

                            ExecuteQuery(connection, query);
                        }
                    }

                    Console.WriteLine($"Data imported to {tableName} from {filePath} successfully.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error importing data: {ex.Message}");
                throw;
            }
        }

        public void ExportResultsToCsv()
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                var query = "SELECT r.StudentID, r.WorkID, r.Attempt, r.Score, r.Date, r.Note " +
                            "FROM Results r " +
                            "JOIN Students s ON r.StudentID = s.ID " +
                            "JOIN Works w ON r.WorkID = w.ID";

                var command = new SqliteCommand(query, connection);
                var reader = command.ExecuteReader();

                using (var writer = new StreamWriter("Results.csv"))
                {
                    // Запись заголовка
                    writer.WriteLine("StudentID,WorkID,Attempt,Score,Date,Note");

                    // Запись данных
                    while (reader.Read())
                    {
                        var score = (double)reader["Score"];
                        writer.WriteLine($"{reader["StudentID"]},{reader["WorkID"]},{reader["Attempt"]},{score.ToString("0.0", CultureInfo.InvariantCulture)},{reader["Date"]},{reader["Note"]}");
                    }
                }

                Console.WriteLine("Results saved to Results.csv successfully.");
            }
        }

        public void ExecuteQuery(SqliteConnection connection, string query)
        {
            using (var command = new SqliteCommand(query, connection))
            {
                command.ExecuteNonQuery();
            }
        }
    }
}
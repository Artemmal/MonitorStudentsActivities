using Microsoft.Data.Sqlite;
using System;

namespace MonitorStudentsActivities
{
    public class SummaryEditor
    {
        private readonly string connectionString;

        public SummaryEditor(string dbPath)
        {
            connectionString = $"Data Source={dbPath};";
        }

        /// <summary>
        /// Вывод на экран содержимого сводной таблицы
        /// </summary>
        public void DisplayCurrentResults()
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                var command = new SqliteCommand(
                    "SELECT Student, Work, TotalScore FROM CurrentResults;",
                    connection);
                using (var reader = command.ExecuteReader())
                {
                    Console.WriteLine("Current Results:");
                    Console.WriteLine("--------------------------------------------------");
                    while (reader.Read())
                    {
                        Console.WriteLine($"{reader["Student"]} - {reader["Work"]}: {reader["TotalScore"]}");
                    }
                    Console.WriteLine("--------------------------------------------------");
                }

                var resultsCommand = new SqliteCommand(
                    "SELECT * FROM Results;",
                    connection);
                using (var reader = resultsCommand.ExecuteReader())
                {
                    Console.WriteLine("Results:");
                    Console.WriteLine("--------------------------------------------------");
                    while (reader.Read())
                    {
                        Console.WriteLine($"{reader["StudentID"]} - {reader["Score"]}");
                    }
                    Console.WriteLine("--------------------------------------------------");
                }
            }
        }

        /// <summary>
        /// Обновление оценки студента в сводной таблице
        /// </summary>
        public void EditResultsFromConsole()
        {
            DisplayCurrentResults();

            Console.WriteLine("Enter the Student first name:");
            var firstName = Console.ReadLine();

            Console.WriteLine("Enter the Student last name:");
            var lastName = Console.ReadLine();

            Console.WriteLine("Enter the Work name:");
            var workName = Console.ReadLine();

            Console.WriteLine("Enter the new score:");
            if (!double.TryParse(Console.ReadLine(), out double newScore))
            {
                Console.WriteLine("Invalid score entered.");
                return;
            }

            UpdateResult(firstName, lastName, workName, newScore);
            Console.WriteLine("Record updated successfully!");
        }

        public void UpdateResult(string firstName, string lastName, string workName, double newScore)
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    // Получаем ID студента по имени
                    var studentIdCommand = new SqliteCommand("SELECT ID FROM Students WHERE TRIM(FirstName) = TRIM(@firstName) AND TRIM(LastName) = TRIM(@lastName);", connection, transaction);
                    studentIdCommand.Parameters.AddWithValue("@firstName", firstName);
                    studentIdCommand.Parameters.AddWithValue("@lastName", lastName);

                    var studentId = studentIdCommand.ExecuteScalar();
                    Console.WriteLine($"Student ID: {studentId}");

                    // Получаем ID работы по названию
                    var workIdCommand = new SqliteCommand("SELECT ID FROM Works WHERE TRIM(Name) = @workName;", connection, transaction);
                    workIdCommand.Parameters.AddWithValue("@workName", workName);
                    var workId = workIdCommand.ExecuteScalar();
                    Console.WriteLine($"Work ID: {workId}");

                    if (studentId == null || workId == null)
                    {
                        Console.WriteLine("Record not found.");
                        return;
                    }

                    // Получаем старую оценку
                    var oldScoreCommand = new SqliteCommand(
                        "SELECT TotalScore FROM CurrentResults WHERE Student = @studentName AND Work = @workName;",
                        connection, transaction);
                    oldScoreCommand.Parameters.AddWithValue("@studentName", $"{firstName} {lastName}");
                    oldScoreCommand.Parameters.AddWithValue("@workName", workName);

                    var oldScore = (double?)oldScoreCommand.ExecuteScalar();

                    /// Проверка, не ниже ли новая оценка, чем предыдущая
                    if (oldScore.HasValue && oldScore > newScore)
                    {
                        Console.WriteLine("Warning: The score cannot be reduced.");
                        return;
                    }

                    // Обновляем оценку
                    var updateCommand = new SqliteCommand("UPDATE CurrentResults SET TotalScore = @newScore WHERE Student = @studentName AND Work = @workName;",
                        connection, transaction);
                    updateCommand.Parameters.AddWithValue("@newScore", newScore);
                    updateCommand.Parameters.AddWithValue("@studentName", $"{firstName} {lastName}");
                    updateCommand.Parameters.AddWithValue("@workName", workName);
                    updateCommand.ExecuteNonQuery();

                    transaction.Commit();
                }
            }
        }
    }
}

using Microsoft.Data.Sqlite;
using System;

namespace MonitorStudentsActivities
{
    public class DatabaseHandler
    {
        private readonly string connectionString;

        public DatabaseHandler(string dbPath)
        {
            connectionString = $"Data Source={dbPath};";
        }

        /// <summary>
        /// Создание таблиц в БД
        /// </summary>
        public void CreateTables()
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                ExecuteQuery(connection, @"
                CREATE TABLE IF NOT EXISTS Students (
                    ID INTEGER PRIMARY KEY AUTOINCREMENT,
                    FirstName TEXT NOT NULL,
                    LastName TEXT NOT NULL
                );");

                ExecuteQuery(connection, @"
                CREATE TABLE IF NOT EXISTS Works (
                    ID INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    MaxScore REAL NOT NULL,
                    MustToDo BOOLEAN NOT NULL,
                    Sum BOOLEAN NOT NULL,
                    IncludeInFReport BOOLEAN NOT NULL
                );");

                ExecuteQuery(connection, @"
                CREATE TABLE IF NOT EXISTS Results (
                    ID INTEGER PRIMARY KEY AUTOINCREMENT,
                    StudentID INTEGER NOT NULL,
                    WorkID INTEGER NOT NULL,
                    Attempt INTEGER NOT NULL,
                    Score REAL NOT NULL,
                    Date TEXT NOT NULL,
                    Note TEXT,
                    FOREIGN KEY (StudentID) REFERENCES Students(ID) ON DELETE CASCADE,
                    FOREIGN KEY (WorkID) REFERENCES Works(ID) ON DELETE CASCADE
                );");
            }
        }

        /// <summary>
        /// Генерация временной сводной таблицы и создание триггера
        /// </summary>
        public void GenerateCurrentResults()
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                ExecuteQuery(connection, @"
                CREATE TEMP TABLE CurrentResults AS
                SELECT 
                    s.FirstName || ' ' || s.LastName AS Student,
                    w.Name AS Work,
                    SUM(r.Score) AS TotalScore
                FROM Results r
                JOIN Students s ON r.StudentID = s.ID
                JOIN Works w ON r.WorkID = w.ID
                WHERE w.IncludeInFReport = 1
                GROUP BY s.ID, w.ID;");

                // Создание триггера trg_UpdateResults
                ExecuteQuery(connection, @"
                CREATE TRIGGER trg_UpdateResults
                AFTER UPDATE ON CurrentResults
                FOR EACH ROW
                BEGIN
                    -- Вставка новой записи в Results
                    INSERT INTO Results (StudentID, WorkID, Attempt, Score, Date, Note)
                    VALUES (
                        (SELECT ID FROM Students WHERE FirstName || ' ' || LastName = NEW.Student),
                        (SELECT ID FROM Works WHERE Name = NEW.Work),
                        1,  -- Attempt (пока просто моковое значение)
                        NEW.TotalScore - OLD.TotalScore,
                        datetime('now'),
                        CASE
                            WHEN NEW.TotalScore < OLD.TotalScore THEN 'Score decreased'
                            ELSE 'Updated via trigger'
                        END
                    );
                END");

                Console.WriteLine("Temporary table CurrentResults and trigger trg_UpdateResults created successfully.");
            }
        }

        private void ExecuteQuery(SqliteConnection connection, string query)
        {
            using (var command = new SqliteCommand(query, connection))
            {
                command.ExecuteNonQuery();
            }
        }
    }
}

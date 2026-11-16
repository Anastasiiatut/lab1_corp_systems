using System;
using Microsoft.Data.Sqlite;

namespace Lab1_ADONET
{
    internal class Program
    {
        private const string ConnectionString = "Data Source=library.db;";

        static void Main(string[] args)
        {
            InitDatabase();

            while (true)
            {
                Console.WriteLine("\n=== Лабораторна робота №1, варіант №5, Бібліотека ===");
                Console.WriteLine("1 – Показати активні видачі");
                Console.WriteLine("2 – Показати список читачів");
                Console.WriteLine("3 – Показати список книг");
                Console.WriteLine("4 – Додати читача");
                Console.WriteLine("5 – Додати книгу");
                Console.WriteLine("6 – Видати книгу (додати видачу)");
                Console.WriteLine("7 – Повернути книгу");
                Console.WriteLine("8 – Історія всіх видач (з датами повернення)");
                Console.WriteLine("0 – Вихід");
                Console.Write("Вибір: ");

                string choice = Console.ReadLine()!;

                if (choice == "1") ShowLoans();
                else if (choice == "2") ShowReaders();
                else if (choice == "3") ShowBooks();
                else if (choice == "4") AddReader();
                else if (choice == "5") AddBook();
                else if (choice == "6") AddLoan();
                else if (choice == "7") ReturnBook();
                else if (choice == "8") ShowAllLoans();
                else if (choice == "0") break;
                else Console.WriteLine("Невірний вибір. Спробуйте ще раз.");
            }
        }

        static void InitDatabase()
        {
            using (var conn = new SqliteConnection(ConnectionString))
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Readers (
                        ReaderID INTEGER PRIMARY KEY AUTOINCREMENT,
                        LastName TEXT NOT NULL,
                        FirstName TEXT NOT NULL,
                        MiddleName TEXT,
                        HomeAddress TEXT,
                        PhoneNumber TEXT,
                        DateOfBirth TEXT,
                        LibraryCardNumber TEXT NOT NULL UNIQUE
                    );

                    CREATE TABLE IF NOT EXISTS Books (
                        BookID INTEGER PRIMARY KEY AUTOINCREMENT,
                        Title TEXT NOT NULL,
                        Author TEXT NOT NULL,
                        Price REAL NOT NULL
                    );

                    CREATE TABLE IF NOT EXISTS BookLoans (
                        LoanID INTEGER PRIMARY KEY AUTOINCREMENT,
                        ReaderID INTEGER NOT NULL,
                        BookID INTEGER NOT NULL,
                        BorrowDate TEXT NOT NULL,
                        ReturnDate TEXT,
                        FOREIGN KEY (ReaderID) REFERENCES Readers(ReaderID),
                        FOREIGN KEY (BookID) REFERENCES Books(BookID)
                    );";
                cmd.ExecuteNonQuery();
            }
        }

        // 1. Показати активні видачі
        static void ShowLoans()
        {
            string sql = @"SELECT l.LoanID, r.LastName || ' ' || r.FirstName AS ReaderName,
                                  b.Title, b.Author, l.BorrowDate
                           FROM BookLoans l
                           JOIN Readers r ON l.ReaderID = r.ReaderID
                           JOIN Books b ON l.BookID = b.BookID
                           WHERE l.ReturnDate IS NULL";

            using (var conn = new SqliteConnection(ConnectionString))
            {
                conn.Open();
                var cmd = new SqliteCommand(sql, conn);
                var reader = cmd.ExecuteReader();

                Console.WriteLine("\nID | Читач | Назва книги | Автор | Дата видачі");
                Console.WriteLine("---------------------------------------------------------");

                bool hasData = false;
                while (reader.Read())
                {
                    hasData = true;
                    Console.WriteLine($"{reader["LoanID"]} | {reader["ReaderName"]} | {reader["Title"]} | {reader["Author"]} | {reader["BorrowDate"]}");
                }

                if (!hasData) Console.WriteLine("Немає активних видач.");
            }
        }

        // 2. Показати список читачів
        static void ShowReaders()
        {
            string sql = "SELECT ReaderID, LastName, FirstName, LibraryCardNumber, PhoneNumber FROM Readers";

            using (var conn = new SqliteConnection(ConnectionString))
            {
                conn.Open();
                var cmd = new SqliteCommand(sql, conn);
                var reader = cmd.ExecuteReader();

                Console.WriteLine("\nID | Прізвище та Ім'я | № Квитка | Телефон");
                Console.WriteLine("---------------------------------------------------------");

                bool hasData = false;
                while (reader.Read())
                {
                    hasData = true;
                    Console.WriteLine($"{reader["ReaderID"]} | {reader["LastName"]} {reader["FirstName"]} | {reader["LibraryCardNumber"]} | {reader["PhoneNumber"]}");
                }

                if (!hasData) Console.WriteLine("Список читачів порожній.");
            }
        }

        // 3. Показати список книг
        static void ShowBooks()
        {
            string sql = "SELECT BookID, Title, Author, Price FROM Books";

            using (var conn = new SqliteConnection(ConnectionString))
            {
                conn.Open();
                var cmd = new SqliteCommand(sql, conn);
                var reader = cmd.ExecuteReader();

                Console.WriteLine("\nID | Назва книги | Автор | Ціна");
                Console.WriteLine("---------------------------------------------------------");

                bool hasData = false;
                while (reader.Read())
                {
                    hasData = true;
                    Console.WriteLine($"{reader["BookID"]} | {reader["Title"]} | {reader["Author"]} | {reader["Price"]} грн");
                }

                if (!hasData) Console.WriteLine("Список книг порожній.");
            }
        }

        // 4. Додати читача
        static void AddReader()
        {
            Console.Write("Прізвище: ");
            string lastName = Console.ReadLine()!;
            Console.Write("Ім'я: ");
            string firstName = Console.ReadLine()!;
            Console.Write("По батькові: ");
            string middleName = Console.ReadLine()!;
            Console.Write("Адреса: ");
            string address = Console.ReadLine()!;
            Console.Write("Телефон: ");
            string phone = Console.ReadLine()!;
            Console.Write("Дата народження (РРРР-ММ-ДД): ");
            string dob = Console.ReadLine()!;
            Console.Write("Номер читацького квитка: ");
            string card = Console.ReadLine()!;

            string sql = @"INSERT INTO Readers (LastName, FirstName, MiddleName, HomeAddress, PhoneNumber, DateOfBirth, LibraryCardNumber)
                           VALUES (@LN, @FN, @MN, @Addr, @Phone, @DOB, @Card)";

            using (var conn = new SqliteConnection(ConnectionString))
            {
                conn.Open();
                var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@LN", lastName);
                cmd.Parameters.AddWithValue("@FN", firstName);
                cmd.Parameters.AddWithValue("@MN", middleName);
                cmd.Parameters.AddWithValue("@Addr", address);
                cmd.Parameters.AddWithValue("@Phone", phone);
                cmd.Parameters.AddWithValue("@DOB", dob);
                cmd.Parameters.AddWithValue("@Card", card);

                cmd.ExecuteNonQuery();
                Console.WriteLine("Читача успішно додано!");
            }
        }

        // 5. Додати книгу
        static void AddBook()
        {
            Console.Write("Назва книги: ");
            string title = Console.ReadLine()!;
            Console.Write("Автор книги: ");
            string author = Console.ReadLine()!;
            Console.Write("Ціна книги (грн): ");
            if (!double.TryParse(Console.ReadLine(), out double price))
            {
                Console.WriteLine("Некоректне значення ціни.");
                return;
            }

            string sql = "INSERT INTO Books (Title, Author, Price) VALUES (@Title, @Author, @Price)";

            using (var conn = new SqliteConnection(ConnectionString))
            {
                conn.Open();
                var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Title", title);
                cmd.Parameters.AddWithValue("@Author", author);
                cmd.Parameters.AddWithValue("@Price", price);

                cmd.ExecuteNonQuery();
                Console.WriteLine("Книгу успішно додано!");
            }
        }

        // 6. Видати книгу (додати видачу)
        static void AddLoan()
        {
            Console.WriteLine("\n--- Доступні читачі ---");
            ShowReaders();

            Console.Write("\nВведіть ID читача зі списку: ");
            if (!int.TryParse(Console.ReadLine(), out int readerId))
            {
                Console.WriteLine("Некоректний ID читача.");
                return;
            }

            Console.WriteLine("\n--- Доступні книги ---");
            ShowBooks();

            Console.Write("\nВведіть ID книги зі списку: ");
            if (!int.TryParse(Console.ReadLine(), out int bookId))
            {
                Console.WriteLine("Некоректний ID книги.");
                return;
            }

            string borrowDate = DateTime.Now.ToString("yyyy-MM-dd");

            string sql = @"INSERT INTO BookLoans (ReaderID, BookID, BorrowDate, ReturnDate)
                           VALUES (@ReaderID, @BookID, @BorrowDate, NULL)";

            using (var conn = new SqliteConnection(ConnectionString))
            {
                conn.Open();
                var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@ReaderID", readerId);
                cmd.Parameters.AddWithValue("@BookID", bookId);
                cmd.Parameters.AddWithValue("@BorrowDate", borrowDate);

                cmd.ExecuteNonQuery();
                Console.WriteLine("\nВидачу книги успішно зареєстровано!");
            }
        }

        // 7. Повернути книгу з можливістю введення дати повернення
        static void ReturnBook()
        {
            Console.WriteLine("\n--- Активні видачі ---");
            ShowLoans();

            Console.Write("\nВведіть ID видачі (LoanID) для повернення: ");
            if (!int.TryParse(Console.ReadLine(), out int loanId))
            {
                Console.WriteLine("Некоректний ID видачі.");
                return;
            }

            Console.Write("Дата повернення (РРРР-ММ-ДД, натисніть Enter для сьогодні): ");
            string inputDate = Console.ReadLine()!;

            // Якщо користувач просто натиснув Enter — беремо поточну дату
            string returnDate = string.IsNullOrWhiteSpace(inputDate)
                ? DateTime.Now.ToString("yyyy-MM-dd")
                : inputDate;

            string sql = @"UPDATE BookLoans
                           SET ReturnDate = @ReturnDate
                           WHERE LoanID = @LoanID AND ReturnDate IS NULL";

            using (var conn = new SqliteConnection(ConnectionString))
            {
                conn.Open();
                var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@ReturnDate", returnDate);
                cmd.Parameters.AddWithValue("@LoanID", loanId);

                int rowsAffected = cmd.ExecuteNonQuery();
                if (rowsAffected > 0)
                {
                    Console.WriteLine($"\nКнигу успішно повернуто! (Дата повернення: {returnDate})");
                }
                else
                {
                    Console.WriteLine("\nВидачу з таким ID не знайдено або книгу вже було повернуто раніше.");
                }
            }
        }

        // 8. Показати історію всіх видач (включаючи повернуті)
        static void ShowAllLoans()
        {
            string sql = @"SELECT l.LoanID, r.LastName || ' ' || r.FirstName AS ReaderName,
                                  b.Title, l.BorrowDate, l.ReturnDate
                           FROM BookLoans l
                           JOIN Readers r ON l.ReaderID = r.ReaderID
                           JOIN Books b ON l.BookID = b.BookID
                           ORDER BY l.LoanID DESC";

            using (var conn = new SqliteConnection(ConnectionString))
            {
                conn.Open();
                var cmd = new SqliteCommand(sql, conn);
                var reader = cmd.ExecuteReader();

                Console.WriteLine("\nID | Читач | Назва книги | Дата видачі | Дата повернення");
                Console.WriteLine("------------------------------------------------------------------");

                bool hasData = false;
                while (reader.Read())
                {
                    hasData = true;
                    string returnDate = reader["ReturnDate"]?.ToString() is { Length: > 0 } s
                        ? s
                        : "Ще не повернуто";

                    Console.WriteLine($"{reader["LoanID"]} | {reader["ReaderName"]} | {reader["Title"]} | {reader["BorrowDate"]} | {returnDate}");
                }

                if (!hasData) Console.WriteLine("Історія видач порожня.");
            }
        }
    }
}

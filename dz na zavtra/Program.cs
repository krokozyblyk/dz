using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

// Абстрактный класс LibraryMember описывает общее поведение членов библиотеки
abstract class LibraryMember
{
    protected string name; // Имя члена библиотеки

    public LibraryMember(string name)
    {
        this.name = name;
    }

    public string Name => name; // Свойство для доступа к имени
    public abstract void DisplayActions(); // Абстрактный метод для отображения доступных действий
}

// Класс User описывает пользователя и его книги
class User : LibraryMember
{
    private List<Book> borrowedBooks = new List<Book>(); // Список взятых книг

    public User(string name) : base(name) { }

    // Метод для добавления книги в список взятых
    public void BorrowBook(Book book)
    {
        if (book.IsAvailable)
        {
            borrowedBooks.Add(book);
            book.IsAvailable = false; // Устанавливаем статус книги на "выдана"
            Console.WriteLine($"Вы взяли книгу: {book.Title}");
        }
        else
        {
            Console.WriteLine($"Книга '{book.Title}' уже выдана.");
        }
    }

    // Метод для возврата книги
    public void ReturnBook(Book book)
    {
        if (borrowedBooks.Contains(book))
        {
            borrowedBooks.Remove(book);
            book.IsAvailable = true; // Устанавливаем статус книги на "доступна"
            Console.WriteLine($"Вы вернули книгу: {book.Title}");
        }
        else
        {
            Console.WriteLine("Эта книга не была взята вами.");
        }
    }

    // Отображение списка взятых книг
    public void DisplayBorrowedBooks()
    {
        Console.WriteLine($"{name}, ваши книги:");
        if (borrowedBooks.Count == 0)
        {
            Console.WriteLine("У вас нет взятых книг.");
        }
        else
        {
            foreach (var book in borrowedBooks)
            {
                Console.WriteLine(book.ToString());
            }
        }
    }

    public override void DisplayActions()
    {
        Console.WriteLine("Доступные действия для пользователя:");
        Console.WriteLine("1. Просмотреть доступные книги");
        Console.WriteLine("2. Взять книгу");
        Console.WriteLine("3. Вернуть книгу");
        Console.WriteLine("4. Просмотреть список взятых книг");
        Console.WriteLine("Введите 'exit' для выхода в меню.");
    }

    // Метод для выполнения действий пользователя
    public void ExecuteAction(Librarian librarian)
    {
        string action;
        while (true)
        {
            DisplayActions();
            action = Console.ReadLine();

            if (action == "1")
            {
                librarian.DisplayBooks();
            }
            else if (action == "2")
            {
                Console.WriteLine("Введите название книги для взятия:");
                string title = Console.ReadLine();
                var book = librarian.FindBook(title);
                if (book != null)
                {
                    BorrowBook(book);
                }
                else
                {
                    Console.WriteLine("Книга не найдена.");
                }
            }
            else if (action == "3")
            {
                Console.WriteLine("Введите название книги для возврата:");
                string title = Console.ReadLine();
                var book = borrowedBooks.FirstOrDefault(b => b.Title.Equals(title, StringComparison.OrdinalIgnoreCase));
                if (book != null)
                {
                    ReturnBook(book);
                }
                else
                {
                    Console.WriteLine("Книга не найдена в вашем списке.");
                }
            }
            else if (action == "4")
            {
                DisplayBorrowedBooks();
            }
            else if (action.ToLower() == "exit")
            {
                break; // Выход в меню регистрации
            }
            else
            {
                Console.WriteLine("Некорректный ввод. Попробуйте снова.");
            }
        }
    }
}

// Класс Librarian описывает библиотекаря и его функции
class Librarian : LibraryMember
{
    private List<Book> books = new List<Book>(); // Список книг в библиотеке
    private List<User> users = new List<User>(); // Список пользователей

    public Librarian(string name) : base(name)
    {
        LoadBooks(); // Загружаем книги при создании библиотекаря
        LoadUsers(); // Загружаем пользователей при создании библиотекаря
    }

    // Метод для добавления новой книги
    public void AddBook(Book book)
    {
        books.Add(book);
        SaveBooks(); // Сохраняем изменения в файл
    }

    // Метод для удаления книги
    public void RemoveBook(string title)
    {
        var book = books.FirstOrDefault(b => b.Title.Equals(title, StringComparison.OrdinalIgnoreCase));
        if (book != null)
        {
            books.Remove(book);
            SaveBooks(); // Сохраняем изменения в файл
        }
        else
        {
            Console.WriteLine("Книга не найдена.");
        }
    }

    // Метод для регистрации нового пользователя
    public void RegisterUser(User user)
    {
        users.Add(user);
        SaveUsers(); // Сохраняем изменения в файл
    }

    // Метод для просмотра всех пользователей
    public void DisplayUsers()
    {
        Console.WriteLine("Список пользователей:");
        foreach (var user in users)
        {
            Console.WriteLine(user.Name);
        }
    }

    // Метод для просмотра всех книг
    public void DisplayBooks()
    {
        Console.WriteLine("Список всех книг в библиотеке:");
        foreach (var book in books)
        {
            Console.WriteLine(book.ToString());
        }
    }

    // Метод для поиска книги по названию
    public Book FindBook(string title)
    {
        return books.FirstOrDefault(b => b.Title.Equals(title, StringComparison.OrdinalIgnoreCase));
    }

    // Метод для сохранения книг в файл
    private void SaveBooks()
    {
        using (StreamWriter sw = new StreamWriter("books.txt"))
        {
            foreach (var book in books)
            {
                sw.WriteLine($"{book.Title}|{book.Author}|{book.IsAvailable}");
            }
        }
    }

    // Метод для сохранения пользователей в файл
    private void SaveUsers()
    {
        using (StreamWriter sw = new StreamWriter("users.txt"))
        {
            foreach (var user in users)
            {
                sw.WriteLine(user.Name);
            }
        }
    }

    // Метод для загрузки книг из файла
    private void LoadBooks()
    {
        if (File.Exists("books.txt"))
        {
            foreach (var line in File.ReadAllLines("books.txt"))
            {
                var parts = line.Split('|');
                if (parts.Length == 3 && bool.TryParse(parts[2], out bool isAvailable))
                {
                    books.Add(new Book(parts[0], parts[1], isAvailable));
                }
            }
        }
    }

    // Метод для загрузки пользователей из файла
    private void LoadUsers()
    {
        if (File.Exists("users.txt"))
        {
            foreach (var line in File.ReadAllLines("users.txt"))
            {
                users.Add(new User(line));
            }
        }
    }

    public override void DisplayActions()
    {
        Console.WriteLine("Доступные действия для библиотекаря:");
        Console.WriteLine("1. Добавить новую книгу");
        Console.WriteLine("2. Удалить книгу");
        Console.WriteLine("3. Зарегистрировать нового пользователя");
        Console.WriteLine("4. Просмотреть список всех пользователей");
        Console.WriteLine("5. Просмотреть список всех книг");
        Console.WriteLine("Введите 'exit' для выхода в меню.");
    }

    // Метод для выполнения действий библиотекаря
    public void ExecuteAction()
    {
        string action;
        while (true)
        {
            DisplayActions();
            action = Console.ReadLine();

            if (action == "1")
            {
                Console.WriteLine("Введите название книги:");
                string title = Console.ReadLine();
                Console.WriteLine("Введите автора книги:");
                string author = Console.ReadLine();
                AddBook(new Book(title, author));
                Console.WriteLine("Книга добавлена.");
            }
            else if (action == "2")
            {
                Console.WriteLine("Введите название книги для удаления:");
                string title = Console.ReadLine();
                RemoveBook(title);
            }
            else if (action == "3")
            {
                Console.WriteLine("Введите имя нового пользователя:");
                string userName = Console.ReadLine();
                RegisterUser(new User(userName));
                Console.WriteLine("Пользователь зарегистрирован.");
            }
            else if (action == "4")
            {
                DisplayUsers();
            }
            else if (action == "5")
            {
                DisplayBooks();
            }
            else if (action.ToLower() == "exit")
            {
                break; // Выход в меню регистрации
            }
            else
            {
                Console.WriteLine("Некорректный ввод. Попробуйте снова.");
            }
        }
    }
}

// Класс Book представляет книгу с её деталями и статусом
class Book
{
    public string Title { get; set; } // Название книги
    public string Author { get; set; } // Автор книги
    public bool IsAvailable { get; set; } // Статус доступности книги

    public Book(string title, string author, bool isAvailable = true)
    {
        Title = title;
        Author = author;
        IsAvailable = isAvailable;
    }

    public override string ToString()
    {
        return $"{Title} by {Author} - {(IsAvailable ? "Available" : "Checked Out")}";
    }
}

// Главный класс - точка входа в приложение
class Program
{
    static void Main(string[] args)
    {
        while (true) // Цикл для повторного входа в меню регистрации
        {
            Console.WriteLine("Выберите роль: 1. Пользователь 2. Библиотекарь");
            var role = Console.ReadLine();
            LibraryMember member;

            if (role == "1") // Если выбрана роль пользователя
            {
                Console.WriteLine("Введите ваше имя:");
                var userName = Console.ReadLine();
                member = new User(userName);
                User user = member as User;

                var librarian = new Librarian("Default Librarian"); // Создаем библиотекаря для поиска книг

                user.ExecuteAction(librarian); // Запуск действий пользователя
            }
            else if (role == "2") // Если выбрана роль библиотекаря
            {
                Console.WriteLine("Введите имя библиотекаря:");
                var librarianName = Console.ReadLine();
                member = new Librarian(librarianName);
                Librarian librarian = member as Librarian;

                librarian.ExecuteAction(); // Запуск действий библиотекаря
            }
            else
            {
                Console.WriteLine("Некорректный ввод. Попробуйте снова.");
                continue; // Продолжаем цикл, позволяя вводить повторно
            }

            Console.WriteLine("Хотите начать снова? (y/n)");
            if (Console.ReadLine().ToLower() != "y")
            {
                break; // Завершение программы
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;


namespace RMC {
    class Program
    {
        static string filePath = "db.json";
        static List<User> users = new List<User>();
        static List<Task> tasks = new List<Task>();
        static User currentUser = null;

        static void Main()
        {
            LoadData();
            Authenticate();
            if (currentUser.Role == Role.Manager)
                ManagerMenu();
            else
                EmployeeMenu();
            SaveData();
        }

        static void LoadData()
        {
            if (File.Exists(filePath))
            {
                var json = File.ReadAllText(filePath);
                var data = JsonSerializer.Deserialize<DataStore>(json);
                if (data != null)
                {
                    users = data.Users;
                    tasks = data.Tasks;
                }
            }
        }

        static void SaveData()
        {
            var data = new DataStore { Users = users, Tasks = tasks };
            File.WriteAllText(filePath, JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true }));
        }

        static void Authenticate()
        {
            Console.Write("Введите логин: ");
            string login = Console.ReadLine();
            Console.Write("Введите пароль: ");
            string password = Console.ReadLine();

            currentUser = users.Find(u => u.Login == login && u.Password == password);
            if (currentUser == null)
            {
                Console.WriteLine("Неверные учетные данные.");
                Environment.Exit(0);
            }
        }

        static void ManagerMenu()
        {
            while (true)
            {
                Console.WriteLine("1. Создать задачу");
                Console.WriteLine("2. Зарегистрировать сотрудника");
                Console.WriteLine("3. Выйти");
                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1": CreateTask(); break;
                    case "2": RegisterEmployee(); break;
                    case "3": return;
                    default: Console.WriteLine("Неверный ввод."); break;
                }
            }
        }

        static void EmployeeMenu()
        {
            while (true)
            {
                Console.WriteLine("1. Просмотреть задачи");
                Console.WriteLine("2. Обновить статус задачи");
                Console.WriteLine("3. Выйти");
                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1": ViewTasks(); break;
                    case "2": UpdateTaskStatus(); break;
                    case "3": return;
                    default: Console.WriteLine("Неверный ввод."); break;
                }
            }
        }

        static void CreateTask()
        {
            Console.Write("Введите название задачи: ");
            string title = Console.ReadLine();
            Console.Write("Введите описание: ");
            string description = Console.ReadLine();
            Console.Write("Введите логин сотрудника: ");
            string assignee = Console.ReadLine();

            if (users.Exists(u => u.Login == assignee && u.Role == Role.Employee))
            {
                tasks.Add(new Task { Id = tasks.Count + 1, Title = title, Description = description, Assignee = assignee, Status = "To Do" });
                Console.WriteLine("Задача создана.");
            }
            else
            {
                Console.WriteLine("Сотрудник не найден.");
            }
        }

        //Добавить шифрование
        static void RegisterEmployee()
        {
            Console.Write("Введите логин: ");
            string login = Console.ReadLine();
            Console.Write("Введите пароль: ");
            string password = Console.ReadLine();

            if (users.Exists(u => u.Login == login))
            {
                Console.WriteLine("Логин уже занят.");
                return;
            }

            users.Add(new User { Login = login, Password = password, Role = Role.Employee });
            Console.WriteLine("Сотрудник зарегистрирован.");
        }

        static void ViewTasks()
        {
            foreach (var task in tasks.FindAll(t => t.Assignee == currentUser.Login))
            {
                Console.WriteLine($"[{task.Id}] {task.Title} - {task.Description} (Статус: {task.Status})");
            }
        }

        static void UpdateTaskStatus()
        {
            Console.Write("Введите ID задачи: ");
            if (int.TryParse(Console.ReadLine(), out int id))
            {
                var task = tasks.Find(t => t.Id == id && t.Assignee == currentUser.Login);
                if (task != null)
                {
                    Console.Write("Введите новый статус (To Do / In Progress / Done): ");
                    //сделать проверку на корректность статуса
                    task.Status = Console.ReadLine();
                    Console.WriteLine("Статус обновлен.");
                }
                else
                {
                    Console.WriteLine("Задача не найдена.");
                }
            }
            else
            {
                Console.WriteLine("Неверный ввод.");
            }
        }
    }


    class User
    {
        public string Login { get; set; }
        public string Password { get; set; }
        public Role Role { get; set; }
    }


    class Task
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Assignee { get; set; }
        public string Status { get; set; }
    }

    enum Role
    {
        Manager,
        Employee
    }

    class DataStore
    {
        public List<User> Users { get; set; } = new List<User>();
        public List<Task> Tasks { get; set; } = new List<Task>();
    }
}
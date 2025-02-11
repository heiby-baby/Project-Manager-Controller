using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace PMC
{
    class Program
    {
        static string filePath = "db.json";
        static DataStore dataStore = new DataStore();

        static List<User> users = new List<User>();
        static List<Task> tasks = new List<Task>();
        static List<Log> logs = new List<Log>();
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
            try
            {
                var json = File.ReadAllText(filePath);
                var data = JsonSerializer.Deserialize<DataStore>(json);
                if (data != null)
                {
                    users = data.Users;
                    tasks = data.Tasks;
                    logs = data.Logs;
                }
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("Не обнаружен файл для работы программы");
                Console.WriteLine("Желаете ли вы создать новый файл (Значение по умолчанию N)? (Y/N)");
                string answer = Console.ReadLine();
                if (answer == "Y" || answer == "y" || answer == "Д" || answer == "д")
                {
                    User user = new User
                    {
                        Role = Role.Manager,
                        Login = GetUserInput("Пожалуйста, введите логин администратора", "Логин не может быть пустым"),
                        Password = Hash.HashString(GetUserInput("Пожалуйста, введите пароль администратора", "Пароль не может быть пустым"))
                    };
                    users.Add(user);
                }
                else
                {
                    Environment.Exit(0);
                }
            }
        }
       
        static void SaveData()
        {
            var data = new DataStore { Users = users, Tasks = tasks, Logs = logs };
            File.WriteAllText(filePath, JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true }));
        }

        static void Authenticate()
        {
            string login = GetUserInput("Введите логин", "Логин не может быть пустым");
            string password = GetUserInput("Введите пароль", "Пароль не может быть пустым");
            currentUser = users.Find(u => u.Login == login && Hash.VerifyHash(password, u.Password));
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
                Console.WriteLine("3. Просмотреть логи");
                Console.WriteLine("4. Выйти");
                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1": CreateTask(); break;
                    case "2": RegisterEmployee(); break;
                    case "3": CheckLogs(); break;
                    case "4": return;
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

        static void CheckLogs()
        {
            foreach (var log in logs)
            {
                Console.WriteLine($"[{log.taskId}] {log.Assignee} - {log.dateTime} (Статус: {log.newStatus})");
            }
        }

        static void CreateTask()
        {
            string title = GetUserInput("Введите название задачи", "Название не может быть пустым");
            string description = GetUserInput("Введите описание", "Описание не может быть пустым");
            string assignee = GetUserInput("Введите логин сотрудника", "Логин не может быть пустым");

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

        static void RegisterEmployee()
        {
            string login = GetUserInput("Введите логин", "Логин не может быть пустым");
            string password = GetUserInput("Введите пароль", "Пароль не может быть пустым");

            if (users.Exists(u => u.Login == login))
            {
                Console.WriteLine("Логин уже занят.");
                return;
            }

            users.Add(new User { Login = login, Password = Hash.HashString(password), Role = Role.Employee });
            Console.WriteLine("Сотрудник зарегистрирован.");
        }

        static void ViewTasks()
        {
            Console.WriteLine("1. Вывести все задачи");
            Console.WriteLine("2. Задачи со статусом \"To Do\"");
            Console.WriteLine("3. Задачи со статусом \"In Progress\"");
            Console.WriteLine("4. Задачи со статусом \"Done\"");
            string choice = Console.ReadLine();

            var userTasks = tasks.FindAll(t => t.Assignee == currentUser.Login);
            var filteredTasks = choice switch
            {
                "1" => userTasks,
                "2" => userTasks.FindAll(t => t.Status == "To Do"),
                "3" => userTasks.FindAll(t => t.Status == "In Progress"),
                "4" => userTasks.FindAll(t => t.Status == "Done"),
                _ => null
            };

            if (filteredTasks != null)
            {
                foreach (var task in filteredTasks)
                {
                    Console.WriteLine($"[{task.Id}] {task.Title} - {task.Description} (Статус: {task.Status})");
                }
            }
            else
            {
                Console.WriteLine("Неверный ввод.");
            }
        }

        static void UpdateTaskStatus()
        {
            Console.Write("Введите ID задачи: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("Неверный ввод.");
                return;
            }

            var task = tasks.Find(t => t.Id == id && t.Assignee == currentUser.Login);
            if (task == null)
            {
                Console.WriteLine("Задача не найдена.");
                return;
            }

            string newStatus = GetUserInput("Введите новый статус (To Do / In Progress / Done)", "Статус не может быть пустым");
            if (newStatus != "To Do" && newStatus != "In Progress" && newStatus != "Done")
            {
                Console.WriteLine("Неверный ввод.");
                return;
            }

            if (task.Status == newStatus)
            {
                Console.WriteLine("Ошибка: новый статус совпадает с текущим.");
                return;
            }

            task.Status = newStatus;
            logs.Add(new Log { dateTime = DateTime.Now, Assignee = task.Assignee, taskId = task.Id, newStatus = newStatus });
            Console.WriteLine("Статус обновлен.");
        }

        static string GetUserInput(string prompt, string errorMessage)
        {
            while (true)
            {
                Console.WriteLine(prompt);
                string input = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(input))
                {
                    return input;
                }
                Console.WriteLine(errorMessage);
            }
        }
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
        public List<Log> Logs { get; set; } = new List<Log>();

    }


}

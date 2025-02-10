# Проект: Система управления задачами (PMC)

Проект представляет собой консольное приложение для управления задачами и пользователями. Система позволяет менеджерам создавать задачи и регистрировать сотрудников, а сотрудникам — просматривать свои задачи и обновлять их статус.

## Основные функции

- **Аутентификация пользователей**: 
  - Менеджеры и сотрудники входят в систему с помощью логина и пароля.
  - Пароли хранятся в хэшированном виде для безопасности.
  
- **Управление задачами**:
  - Менеджеры могут создавать задачи и назначать их сотрудникам.
  - Сотрудники могут просматривать свои задачи и обновлять их статус (To Do, In Progress, Done).

- **Логирование изменений**:
  - Все изменения статуса задач записываются в лог с указанием времени, сотрудника и нового статуса.

- **Сохранение данных**:
  - Данные о пользователях, задачах и логах сохраняются в файл `db.json`.

## Установка и запуск

1. **Установка .NET**:
   - Убедитесь, что у вас установлен [.NET SDK](https://dotnet.microsoft.com/download).
   - Проверьте установку, выполнив команду:
     ```bash
     dotnet --version
     ```

2. **Клонирование репозитория**:
   ```bash
   https://github.com/heiby-baby/Project-Manager-Controller
   cd Project-Manager-Controller
   dotnet build
   dotnet run

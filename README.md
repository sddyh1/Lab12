# Лабораторная работа №12
## Взаимодействие с базами данных в .NET. ADO.NET vs ORM. Обратная инженерия

**Выполнил:** Евсеев В. А.  
**Группа:** 2307А1  
**Проверил:** Макаров М. С.  
**Новосибирск, 2026**

---

## Формулировка задания

**Тема:** Взаимодействие с базами данных в .NET. ADO.NET vs ORM. Обратная инженерия.  

**Цель работы:** изучить основные подходы к взаимодействию с базами данных в приложениях .NET, сравнить низкоуровневый подход ADO.NET с высокоуровневыми ORM-решениями. Освоить подход Database First (обратная инженерия) с использованием Entity Framework Core для генерации модели данных на основе готовой базы данных.

**Задание:**  
Выполнить интеграцию базы данных в существующее приложение «Телефонная книга» (из лабораторной работы №10–11).

Требования к доработке:
1. Подключить к проекту необходимые NuGet-пакеты для работы с Entity Framework Core и выбранной СУБД (SQLite).
2. Создать новую базу данных в выбранной СУБД с именем `PhoneBookDB_ФАМИЛИЯ_ГРУППА` (в работе – `PhoneBookDB_Evseev_2307a`).
3. Используя средства обратной инженерии (Scaffolding), восстановить классы сущностей и контекст данных по готовой базе данных.
4. Зарегистрировать `DbContext` в контейнере внедрения зависимостей (DI).
5. Обеспечить чтение списка контактов из базы данных при запуске приложения.

---

## Теоретическое обоснование

### ADO.NET и ORM
- **ADO.NET** – низкоуровневый интерфейс доступа к данным, требующий ручного написания SQL-запросов и маппинга. Обеспечивает максимальную производительность, но увеличивает объём шаблонного кода.
- **ORM (Object-Relational Mapping)** – высокоуровневая абстракция, позволяющая работать с базой данных через объекты C#. Entity Framework Core автоматически генерирует SQL, упрощает разработку и сопровождение.

### Подход Database First (Обратная инженерия)
При наличии готовой базы данных EF Core может сгенерировать классы сущностей и контекст данных. Это избавляет от ручного написания моделей и снижает риск ошибок при изменении схемы БД.

### DI и DbContext
Регистрация `DbContext` в DI-контейнере с временем жизни `Scoped` (или `Singleton` для SQLite) позволяет внедрять контекст в ViewModel и другие сервисы, обеспечивая централизованное управление подключением к БД.

---

## Описание выполненных действий

### 1. Установка NuGet-пакетов
Добавлены пакеты:
- `Microsoft.EntityFrameworkCore.Sqlite` – провайдер для SQLite.
- `Microsoft.EntityFrameworkCore.Design` – инструменты для Scaffolding.
- `Microsoft.EntityFrameworkCore.Tools` (опционально, для команд PMC).

Установка выполнена через `dotnet add package` и Package Manager Console.

### 2. Создание базы данных SQLite
С помощью **DB Browser for SQLite** создан файл `PhoneBookDB_Evseev_2307a.db` со следующей структурой:

```sql
CREATE TABLE Contacts (
    Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL,
    Phone TEXT NOT NULL
);

INSERT INTO Contacts (Name, Phone) VALUES
('Иванов Иван', '+79991234567'),
('Петрова Мария', '+79997654321'),
('Сидоров Алексей', '+79995558899');
Файл помещён в корень проекта.

3. Обратная инженерия (Scaffolding)
Выполнена команда .NET CLI (предварительно установлен глобальный инструмент dotnet-ef):

bash
dotnet ef dbcontext scaffold "Data Source=PhoneBookDB_Evseev_2307a.db" Microsoft.EntityFrameworkCore.Sqlite --output-dir Models --force
Результат: в папке Models сгенерированы файлы:

PhoneBookDbEvseev2307aContext.cs – контекст базы данных.

Contact.cs – класс сущности (свойства Id, Name, Phone).

4. Регистрация DbContext в DI-контейнере
В App.xaml.cs добавлена регистрация контекста с указанием строки подключения:

csharp
services.AddDbContext<PhoneBookDbEvseev2307aContext>(options =>
    options.UseSqlite("Data Source=PhoneBookDB_Evseev_2307a.db"));
5. Модификация ViewModel (MainViewModel)
В конструктор добавлен параметр PhoneBookDbEvseev2307aContext context (внедрение через DI).

Загрузка контактов реализована через _context.Contacts.Load() и присвоение Contacts = _context.Contacts.Local.ToObservableCollection().

Метод AddContact:

Проверяет дубликат через _context.Contacts.Any(...).

Добавляет новый объект Contact в DbSet и вызывает _context.SaveChanges().

Метод DeleteContact:

Удаляет контакт из DbSet и сохраняет изменения.

Все диалоговые окна (успех, ошибка, подтверждение) остались через IDialogService, реализованный в ЛР№10.

6. Настройка копирования файла базы данных
Для автоматической доставки .db в выходную папку в свойствах файла установлено «Копировать в выходной каталог: Копировать, если новее».

7. Тестирование
При запуске приложения в DataGrid отображаются три тестовых контакта.

Добавление нового контакта приводит к его сохранению в БД (проверено перезапуском).

Удаление контакта также сохраняется в БД.

При попытке добавить дубликат по номеру телефона выводится предупреждение.

Валидация полей (имя не пустое, телефон в формате +7... или 10 цифр) работает через CanAddContact.

Результат выполненной работы
Скриншоты (описательно)
Окно приложения при запуске: отображаются три контакта из БД.

Добавление нового контакта: после нажатия «Добавить» контакт появляется в таблице, появляется информационное сообщение.

Удаление: запрос подтверждения, после удаления контакт исчезает.

Сравнение с теоретической оценкой
Использование ORM (EF Core) позволило отказаться от ручного написания SQL и маппинга, сосредоточившись на объектной модели.

Database First ускорил разработку: модель данных сгенерирована автоматически из существующей БД.

DI обеспечил слабую связанность: MainViewModel не создаёт контекст самостоятельно, а получает его через конструктор. Это упрощает тестирование и замену провайдера БД.

SQLite выбран как встраиваемая СУБД, не требующая установки сервера и сложной настройки.

Исходный код модуля (ключевые фрагменты)
Models/PhoneBookDbEvseev2307aContext.cs (сгенерирован, с добавленным DbSet<Contact>)
csharp
using Microsoft.EntityFrameworkCore;
using PhoneBookDI.Models;

namespace PhoneBookDI.Models
{
    public partial class PhoneBookDbEvseev2307aContext : DbContext
    {
        public PhoneBookDbEvseev2307aContext(DbContextOptions<PhoneBookDbEvseev2307aContext> options)
            : base(options)
        {
        }

        public DbSet<Contact> Contacts { get; set; }

        // ... остальной код (OnConfiguring, OnModelCreating)
    }
}
ViewModels/MainViewModel.cs (фрагмент с работой с БД)
csharp
private readonly IDialogService _dialogService;
private readonly PhoneBookDbEvseev2307aContext _context;

public MainViewModel(IDialogService dialogService, PhoneBookDbEvseev2307aContext context)
{
    _dialogService = dialogService;
    _context = context;

    _context.Contacts.Load();
    Contacts = _context.Contacts.Local.ToObservableCollection();

    AddCommand = new RelayCommand(AddContact, CanAddContact);
    DeleteCommand = new RelayCommand<Contact>(DeleteContact, c => c != null);
}

private void AddContact()
{
    if (_context.Contacts.Any(c => c.Phone == Phone))
    {
        _dialogService.ShowWarning("Контакт с таким номером телефона уже существует!", "Дубликат");
        return;
    }

    try
    {
        var newContact = new Contact { Name = Name, Phone = Phone };
        _context.Contacts.Add(newContact);
        _context.SaveChanges();
        Name = string.Empty;
        Phone = string.Empty;
        _dialogService.ShowInfo("Контакт успешно добавлен.", "Успех");
    }
    catch (Exception ex)
    {
        _dialogService.ShowError($"Ошибка при добавлении: {ex.Message}", "Ошибка");
    }
}

private void DeleteContact(Contact? contact)
{
    if (contact == null) return;
    if (_dialogService.ShowConfirmation($"Удалить контакт \"{contact.Name}\"?", "Удаление"))
    {
        _context.Contacts.Remove(contact);
        _context.SaveChanges();
        _dialogService.ShowInfo("Контакт удалён.", "Успех");
    }
}
App.xaml.cs (регистрация DbContext)
csharp
services.AddDbContext<PhoneBookDbEvseev2307aContext>(options =>
    options.UseSqlite("Data Source=PhoneBookDB_Evseev_2307a.db"));
services.AddSingleton<IDialogService, DialogService>();
services.AddTransient<MainViewModel>();
services.AddSingleton<MainWindow>(provider =>
{
    var window = new MainWindow();
    window.DataContext = provider.GetRequiredService<MainViewModel>();
    return window;
});

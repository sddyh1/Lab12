# Лабораторная работа №10
## Dependency Injection и паттерн «Сервис» в MVVM-приложениях

**Выполнил:** Евсеев В. А.  
**Группа:** 2307А1  
**Проверил:** Макаров М. С.  
**Новосибирск, 2026**

---

## Формулировка задания

**Тема:** Dependency Injection и паттерн «Сервис» в MVVM-приложениях.  

**Цель работы:** изучить принцип внедрения зависимостей (Dependency Injection, DI) и паттерн «Сервис» для решения проблемы жёсткой связности компонентов в MVVM-приложениях.

**Задание:**  
Выполнить рефакторинг приложения «Телефонная книга» из Лабораторной работы №9 с применением паттернов Dependency Injection и Service.

Требования к доработке:
- Подключить NuGet-пакет `Microsoft.Extensions.DependencyInjection`.
- Разработать интерфейс `IDialogService` и его реализацию `DialogService`, поддерживающую вызов информационных, предупреждающих, ошибочных сообщений и запросов подтверждения (Да/Нет).
- Модифицировать класс ViewModel: внедрить `IDialogService` через конструктор. Использовать сервис в командах:
  - При удалении контакта запрашивать подтверждение (`ShowConfirmation`). Если пользователь отказывается - не удалять контакт.
  - При добавлении контакта проверять наличие дубликатов по номеру телефона. Если номер есть - предупреждать (`ShowWarning`) и отменять добавление.
  - При успешном добавлении выводить информационное сообщение (`ShowInfo`).
- Изменить точку входа приложения: убрать атрибут `StartupUri` из `App.xaml`. В `App.xaml.cs` настроить IoC-контейнер, зарегистрировать сервисы, ViewModel и главное окно. Инициализировать и запустить приложение вручную.
- Убрать жёсткую привязку `DataContext` из XAML-разметки `MainWindow.xaml`.
- Исходный код должен содержать комментарии, поясняющие назначение зарегистрированных сервисов и время их жизни (Lifetime).

---

## Теоретическое обоснование

### Проблема жёсткой связности в MVVM
В классической MVVM ViewModel не должна знать о конкретных элементах UI. Прямой вызов `MessageBox.Show()` из ViewModel приводит к:
- Жёсткой привязке к сборке `PresentationFramework`.
- Невозможности юнит-тестирования (тесты не могут запускать реальные диалоги).
- Нарушению принципа единственной ответственности.

### Dependency Injection (DI)
DI - это паттерн, при котором зависимости передаются объекту извне (чаще всего через конструктор) вместо того, чтобы объект создавал их сам. Это обеспечивает слабую связанность и упрощает тестирование.

### Паттерн «Сервис»
Сервис - это класс, инкапсулирующий вспомогательную функциональность (диалоги, работа с файлами, HTTP-запросы). Для сервиса определяется интерфейс, а его реализация может быть заменена без изменения клиентского кода.

### IoC-контейнер
IoC-контейнер (например, `Microsoft.Extensions.DependencyInjection`) автоматически создаёт объекты и разрешает их зависимости. Время жизни объектов задаётся при регистрации:
- `Singleton` - один экземпляр на всё приложение.
- `Transient` - новый экземпляр при каждом запросе.
- `Scoped` - один экземпляр на логическую область (редко в десктопах).

**Ожидаемый результат:** после рефакторинга ViewModel не будет содержать прямых вызовов WPF-диалогов, все сообщения будут проходить через сервис `IDialogService`. DI-контейнер будет управлять созданием окна, ViewModel и сервиса, а `DataContext` будет устанавливаться в коде, а не в XAML. Это повысит тестируемость и гибкость приложения.

---

## Описание выполненных действий

### 1. Подготовка проекта
- Открыто решение с приложением «Телефонная книга» (Лабораторная работа №9).
- Установлен NuGet-пакет `Microsoft.Extensions.DependencyInjection`.
- В проекте созданы папки: `Models`, `ViewModels`, `Services`.

### 2. Реализация сервиса диалогов
- В папке `Services` создан интерфейс `IDialogService` с методами:
  - `ShowInfo(string message, string title)`
  - `ShowWarning(string message, string title)`
  - `ShowError(string message, string title)`
  - `ShowConfirmation(string message, string title)` - возвращает `bool`.
- Создан класс `DialogService`, реализующий этот интерфейс через `System.Windows.MessageBox`.

### 3. Модификация ViewModel (`MainViewModel`)
- В конструктор `MainViewModel` добавлен параметр `IDialogService dialogService` (Constructor Injection).
- Приватное поле `_dialogService` сохранено для использования в командах.
- Метод `AddContact` дополнен:
  - Проверкой дубликатов номера телефона через `Contacts.Any(...)`.
  - При дубликате - вызов `_dialogService.ShowWarning`.
  - После успешного добавления - вызов `_dialogService.ShowInfo`.
  - При ошибке валидации - вызов `_dialogService.ShowError`.
- Метод `DeleteContact` дополнен:
  - Вызовом `_dialogService.ShowConfirmation` перед удалением.
  - Удаление только при подтверждении.
  - Информационное сообщение после удаления.
- Свойства и команды `AddCommand`, `DeleteCommand` оставлены без изменений, но их логика использует сервис.

### 4. Настройка DI-контейнера в `App.xaml.cs`
- Из файла `App.xaml` удалён атрибут `StartupUri="MainWindow.xaml"`.
- В `App.xaml.cs` переопределён метод `OnStartup`:
  - Создан `ServiceCollection`.
  - Зарегистрирован `IDialogService` как `Singleton` (сервис без состояния).
  - Зарегистрирован `MainViewModel` как `Transient` (для возможности создания новых экземпляров при навигации).
  - Зарегистрировано `MainWindow` как `Singleton` с фабричным методом, где вручную устанавливается `DataContext = provider.GetRequiredService<MainViewModel>()`.
  - Построен `ServiceProvider` и получено главное окно через `GetRequiredService<MainWindow>()`, затем вызван `Show()`.
- В `OnExit` добавлен вызов `_serviceProvider?.Dispose()` для освобождения ресурсов.

### 5. Изменение XAML-разметки
- Из `MainWindow.xaml` удалён блок `<Window.DataContext><local:MainViewModel/></Window.DataContext>`.
- Все привязки (`{Binding Name}`, `{Binding Phone}`, `{Binding AddCommand}` и т.д.) остались без изменений - они теперь работают с `DataContext`, установленным из кода.

### 6. Очистка кода behind
- В `MainWindow.xaml.cs` оставлен только конструктор с `InitializeComponent()`. Создание `DataContext` удалено.

### 7. Комментирование кода
- В `App.xaml.cs` и `MainViewModel.cs` добавлены комментарии, поясняющие выбор времени жизни сервисов и назначение каждого этапа.

### Аналогичные действия
- Для регистрации других возможных сервисов применяется тот же подход (интерфейс + реализация, регистрация в контейнере, внедрение через конструктор).

---

## Результат выполненной работы

### Тестирование
Приложение запущено. Проведены следующие проверки:

1. **Запуск:** открывается одно главное окно (нет дублирования окон, так как `StartupUri` удалён).
2. **Добавление нового контакта:**  
   - Ввод: Имя = "Иван", Телефон = "+79131234567".  
   - Кнопка "Добавить" активна. Нажатие → появляется информационное сообщение "Контакт успешно добавлен". Контакт отображается в `DataGrid`. Поля ввода очищаются.
3. **Добавление дубликата:**  
   - Попытка добавить контакт с тем же номером → появляется предупреждение "Контакт с таким номером телефона уже существует!"; добавление не выполняется.
4. **Неверный формат телефона / пустое имя:**  
   - Кнопка "Добавить" становится неактивной (благодаря `CanAddContact`).
5. **Удаление контакта:**  
   - Выбран контакт в `DataGrid`. Нажатие "Удалить" → появляется запрос подтверждения "Вы уверены, что хотите удалить контакт ...?".  
   - При выборе "Нет" - удаление не происходит. При выборе "Да" - контакт удаляется из списка, появляется информационное сообщение "Контакт удалён".

### Сравнение с теоретической оценкой
- **Слабая связность:** ViewModel не содержит прямых вызовов `MessageBox`. Все диалоги идут через `IDialogService`. Это соответствует теоретическим принципам DI.
- **Тестируемость:** при необходимости можно заменить `DialogService` на mock-объект, что позволит писать юнит-тесты.
- **Управление зависимостями:** контейнер сам создаёт и внедряет зависимости, что избавляет от ручного `new` и упрощает изменение архитектуры.
- **Lifetime:** выбор `Singleton` для сервиса диалогов и `Transient` для ViewModel обоснован и соответствует лучшим практикам.
- **XAML:** отсутствие жёсткой привязки `DataContext` делает представление независимым от конкретной ViewModel.

Таким образом, все требования задания выполнены, полученный результат полностью соответствует ожидаемому.

---

## Исходный код модуля

### Файл `Services/IDialogService.cs`
```csharp
namespace PhoneBookDI.Services
{
    /// <summary>
    /// Сервис для отображения диалоговых окон.
    /// Абстрагирует ViewModel от конкретной реализации.
    /// </summary>
    public interface IDialogService
    {
        void ShowInfo(string message, string title = "Информация");
        void ShowWarning(string message, string title = "Предупреждение");
        void ShowError(string message, string title = "Ошибка");
        bool ShowConfirmation(string message, string title = "Подтверждение");
    }
}

Файл Services/DialogService.cs
using System.Windows;

namespace PhoneBookDI.Services
{
    /// <summary>
    /// Реализация IDialogService через стандартный MessageBox WPF.
    /// </summary>
    public class DialogService : IDialogService
    {
        public void ShowInfo(string message, string title = "Информация")
            => MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);

        public void ShowWarning(string message, string title = "Предупреждение")
            => MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);

        public void ShowError(string message, string title = "Ошибка")
            => MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);

        public bool ShowConfirmation(string message, string title = "Подтверждение")
        {
            var result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question);
            return result == MessageBoxResult.Yes;
        }
    }
}

Файл ViewModels/MainViewModel.cs


using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Input;
using PhoneBookDI.Models;
using PhoneBookDI.Services;

namespace PhoneBookDI.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        private readonly IDialogService _dialogService;

        public ObservableCollection<Contact> Contacts { get; }

        private string _name = string.Empty;
        public string Name { get => _name; set => Set(ref _name, value); }

        private string _phone = string.Empty;
        public string Phone { get => _phone; set => Set(ref _phone, value); }

        private Contact? _selectedContact;
        public Contact? SelectedContact { get => _selectedContact; set => Set(ref _selectedContact, value); }

        public ICommand AddCommand { get; }
        public ICommand DeleteCommand { get; }

        // Constructor Injection: DI-контейнер передаёт реализацию IDialogService
        public MainViewModel(IDialogService dialogService)
        {
            _dialogService = dialogService;
            Contacts = new ObservableCollection<Contact>();

            AddCommand = new RelayCommand(AddContact, CanAddContact);
            DeleteCommand = new RelayCommand<Contact>(DeleteContact, c => c != null);
        }

        private bool CanAddContact() => !string.IsNullOrWhiteSpace(Name) && Regex.IsMatch(Phone, @"^(\+7\d{10}|\d{10})$");

        private void AddContact()
        {
            if (Contacts.Any(c => c.Phone == Phone))
            {
                _dialogService.ShowWarning("Контакт с таким номером телефона уже существует!", "Дубликат");
                return;
            }

            try
            {
                var newContact = new Contact(Name, Phone);
                Contacts.Add(newContact);
                Name = Phone = string.Empty;
                _dialogService.ShowInfo("Контакт успешно добавлен.", "Успех");
            }
            catch (ArgumentException ex)
            {
                _dialogService.ShowError(ex.Message, "Ошибка валидации");
            }
        }

        private void DeleteContact(Contact? contact)
        {
            if (contact == null) return;

            if (_dialogService.ShowConfirmation($"Вы уверены, что хотите удалить контакт \"{contact.Name}\"?", "Удаление"))
            {
                Contacts.Remove(contact);
                _dialogService.ShowInfo("Контакт удалён.", "Успех");
            }
        }
    }
}

Файл App.xaml.cs
using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using PhoneBookDI.Services;
using PhoneBookDI.ViewModels;

namespace PhoneBookDI
{
    public partial class App : Application
    {
        private ServiceProvider _serviceProvider;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var services = new ServiceCollection();

            // DialogService — Singleton, так как не хранит состояние пользователя
            services.AddSingleton<IDialogService, DialogService>();

            // MainViewModel — Transient, чтобы при навигации создавались новые экземпляры
            services.AddTransient<MainViewModel>();

            // Главное окно — Singleton с фабрикой, устанавливающей DataContext
            services.AddSingleton<MainWindow>(provider =>
            {
                var window = new MainWindow();
                window.DataContext = provider.GetRequiredService<MainViewModel>();
                return window;
            });

            _serviceProvider = services.BuildServiceProvider();

            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _serviceProvider?.Dispose();
            base.OnExit(e);
        }
    }
}

Файл MainWindow.xaml (фрагмент, без DataContext)

<Window x:Class="PhoneBookDI.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Телефонная книга" Height="450" Width="600">
    <Grid Margin="10">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
        </Grid.RowDefinitions>

        <TextBox Grid.Row="0" Margin="0,5" Text="{Binding Name, UpdateSourceTrigger=PropertyChanged}" ToolTip="Введите имя контакта"/>
        <TextBox Grid.Row="1" Margin="0,5" Text="{Binding Phone, UpdateSourceTrigger=PropertyChanged}" ToolTip="Формат: +7XXXXXXXXXX или 10 цифр"/>

        <StackPanel Grid.Row="2" Orientation="Horizontal" Margin="0,10">
            <Button Content="Добавить" Width="100" Margin="0,0,10,0" Command="{Binding AddCommand}"/>
            <Button Content="Удалить" Width="100" Command="{Binding DeleteCommand}" CommandParameter="{Binding SelectedContact}"/>
        </StackPanel>

        <DataGrid Grid.Row="3" AutoGenerateColumns="False" IsReadOnly="True"
                  ItemsSource="{Binding Contacts}" SelectedItem="{Binding SelectedContact, Mode=TwoWay}"
                  CanUserDeleteRows="False">
            <DataGrid.Columns>
                <DataGridTextColumn Header="Имя" Binding="{Binding Name}" Width="*"/>
                <DataGridTextColumn Header="Телефон" Binding="{Binding Phone}" Width="*"/>
            </DataGrid.Columns>
        </DataGrid>
    </Grid>
</Window>

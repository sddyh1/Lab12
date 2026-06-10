# Лабораторная работа №13
## Реализация CRUD-операций с использованием Entity Framework Core

**Выполнил:** Евсеев В. А.  
**Группа:** 2307А1  
**Проверил:** Макаров М. С.  
**Новосибирск, 2026**

---

## Формулировка задания

**Тема:** Взаимодействие с базами данных в .NET. CRUD операции.  

**Цель работы:** изучить внутреннее устройство класса `DbContext` в Entity Framework Core, механизмы отслеживания изменений сущностей (Change Tracker). Реализовать полный цикл CRUD-операций (Create, Read, Update, Delete) в приложении «Телефонная книга», работая с базой данных напрямую через контекст.

**Задание:**  
Модернизировать приложение «Телефонная книга» (из лабораторной работы №12) для полноценной работы с базой данных.

Требования к доработке:
1. Изучить структуру сгенерированного в предыдущей работе класса `ApplicationContext`.
2. Внедрить `ApplicationContext` в конструкторы ViewModels (`ContactsListViewModel`, `ContactEditViewModel`).
3. Реализовать логику добавления нового контакта через отдельную форму.
4. Реализовать сохранение изменений при редактировании контакта.
5. Реализовать удаление выбранного контакта из списка.
6. Обеспечить обновление интерфейса после каждой операции с БД.

---

## Теоретическое обоснование

### Класс DbContext и его роль
`DbContext` является центральным классом EF Core, отвечающим за:
- Управление соединением с базой данных.
- Маппинг объектов C# на таблицы БД.
- Отслеживание изменений (Change Tracker).

### Жизненный цикл сущностей и Change Tracker
EF Core отслеживает состояние каждой сущности, загруженной через контекст. Состояния:
- `Added` - новая запись (будет выполнена операция INSERT).
- `Modified` - изменённая запись (будет выполнен UPDATE).
- `Deleted` - удалённая запись (будет выполнен DELETE).
- `Unchanged` - без изменений.

При вызове `SaveChanges()` Change Tracker анализирует все отслеживаемые объекты и генерирует соответствующие SQL-команды.

### Реализация CRUD-операций в EF Core
- **Create**: `_context.Contacts.Add(newContact); _context.SaveChanges();`
- **Read**: `_context.Contacts.ToList();` или `Load()` с локальной коллекцией.
- **Update**: изменение свойств отслеживаемого объекта, затем `SaveChanges()`.
- **Delete**: `_context.Contacts.Remove(contact); _context.SaveChanges();`

### Подход Database First
В лабораторной работе №12 была выполнена обратная инженерия (Scaffolding) готовой базы данных, сгенерированы классы `Contact` и `PhoneBookDbEvseev2307aContext`. В данной работе этот контекст используется напрямую через внедрение зависимостей.

---

## Описание выполненных действий

### 1. Анализ сгенерированного класса DbContext
Файл `PhoneBookDbEvseev2307aContext.cs` содержит:
- Конструктор с параметром `DbContextOptions<...>`, что позволяет передавать параметры подключения из DI.
- Свойство `DbSet<Contact> Contacts`.
- Метод `OnModelCreating` для Fluent API (заполняется автоматически при scaffolding).

Было принято решение удалить метод `OnConfiguring`, чтобы избежать конфликта строк подключения и полностью положиться на DI.

### 2. Реорганизация ViewModels
Созданы две ViewModel:
- **ContactsListViewModel** - отвечает за отображение списка контактов и команды (Add, Edit, Delete).
- **ContactEditViewModel** - отвечает за форму добавления/редактирования одного контакта.

### 3. Реализация операции чтения (Read)
В `ContactsListViewModel` в конструкторе загружаются все контакты:
```csharp
_context.Contacts.Load();
Contacts = _context.Contacts.Local.ToObservableCollection();
После каждой операции (добавление, обновление, удаление) список перезагружается методом LoadContacts().

4. Реализация операции создания (Create)
При нажатии кнопки «Добавить» создаётся экземпляр ContactEditViewModel, вызывается метод InitializeForAdd() и открывается окно ContactEditWindow. Пользователь заполняет имя и телефон, нажимает «Сохранить». В методе Save() выполняется проверка на дубликат по номеру телефона, создаётся новый объект Contact, добавляется в DbSet и вызывается _context.SaveChanges().

5. Реализация операции редактирования (Update)
При выборе контакта в списке и нажатии «Редактировать» создаётся ContactEditViewModel, вызывается InitializeForEdit(selectedContact), открывается окно с предзаполненными полями. После изменения данных и нажатия «Сохранить» изменяются свойства существующего объекта _currentContact (который уже отслеживается контекстом) и вызывается SaveChanges(). Благодаря Change Tracker EF Core автоматически формирует SQL-запрос UPDATE.

6. Реализация операции удаления (Delete)
В ContactsListViewModel команда DeleteCommand вызывает метод DeleteContact(), который запрашивает подтверждение, затем удаляет выбранный контакт из DbSet и сохраняет изменения.

7. Обновление интерфейса после операций
После успешного сохранения в ContactEditViewModel вызывается событие RequestClose, которое закрывает окно с DialogResult = true. В ContactsListViewModel метод OpenEditWindow ожидает этот результат и перезагружает список через LoadContacts(). При этом используется _context.ChangeTracker.Clear() и повторная загрузка из БД, чтобы гарантировать актуальность данных.

8. Регистрация сервисов в DI-контейнере
В App.xaml.cs добавлены:

PhoneBookDbEvseev2307aContext - зарегистрирован с использованием SQLite.

IDialogService и DialogService.

ContactEditViewModel как Transient (новый экземпляр для каждого окна).

ContactsListViewModel как Singleton.

Фабрика Func<ContactEditViewModel> для создания ViewModel через DI.

9. Создание окна редактирования
Добавлено окно ContactEditWindow с двумя TextBox и кнопками «Сохранить» / «Отмена». DataContext привязывается к ContactEditViewModel. Команды SaveCommand и CancelCommand управляют логикой.

10. Обработка ошибок
Все вызовы SaveChanges() обёрнуты в try-catch с выводом сообщений через IDialogService. При ошибках пользователь получает информативное диалоговое окно.

11. Тестирование
При запуске отображаются все контакты из БД.

Добавление нового контакта через отдельную форму: после сохранения контакт появляется в таблице.

Редактирование: изменение данных контакта, после сохранения таблица обновляется без перезапуска.

Удаление: запрос подтверждения, контакт удаляется из БД и таблицы.

Валидация: проверка на пустое имя и формат телефона (+7XXXXXXXXXX или 10 цифр), кнопка «Сохранить» активна только при корректных данных.

Защита от дубликатов: при добавлении контакта с уже существующим номером выводится предупреждение.

Результат выполненной работы
Скриншоты (описательно):

Главное окно - список контактов, три кнопки (Добавить, Редактировать, Удалить).

Окно добавления - пустые поля, после ввода и сохранения - контакт появляется в списке.

Окно редактирования - поля предзаполнены данными выбранного контакта, после изменения и сохранения - запись обновляется.

Сравнение с теоретической оценкой:

Использование EF Core позволило реализовать CRUD-операции без ручного написания SQL, благодаря Change Tracker все изменения автоматически преобразуются в соответствующие команды базы данных.

Разделение ViewModel и использование DI обеспечило слабую связанность и упростило тестирование.

Отдельная форма редактирования соответствует принципу единой ответственности и улучшает UX.

Исходный код модуля (ключевые фрагменты)
ViewModels/ContactsListViewModel.cs
csharp
public class ContactsListViewModel : ObservableObject
{
    private readonly PhoneBookDbEvseev2307aContext _context;
    private readonly IDialogService _dialogService;
    private readonly Func<ContactEditViewModel> _editViewModelFactory;

    private ObservableCollection<Contact> _contacts = null!;
    public ObservableCollection<Contact> Contacts { get; set; }
    public Contact? SelectedContact { get; set; }

    public ICommand AddCommand { get; }
    public ICommand EditCommand { get; }
    public ICommand DeleteCommand { get; }

    public ContactsListViewModel(PhoneBookDbEvseev2307aContext context, IDialogService dialogService, Func<ContactEditViewModel> editViewModelFactory)
    {
        _context = context;
        _dialogService = dialogService;
        _editViewModelFactory = editViewModelFactory;
        LoadContacts();
        AddCommand = new RelayCommand(OpenAddContact);
        EditCommand = new RelayCommand(OpenEditContact, () => SelectedContact != null);
        DeleteCommand = new RelayCommand(DeleteContact, () => SelectedContact != null);
    }

    private void LoadContacts()
    {
        _context.ChangeTracker.Clear();
        var freshContacts = _context.Contacts.ToList();
        Contacts = new ObservableCollection<Contact>(freshContacts);
    }

    private void OpenAddContact()
    {
        var editVm = _editViewModelFactory();
        editVm.InitializeForAdd();
        OpenEditWindow(editVm);
    }

    private void OpenEditContact()
    {
        if (SelectedContact == null) return;
        var editVm = _editViewModelFactory();
        editVm.InitializeForEdit(SelectedContact);
        OpenEditWindow(editVm);
    }

    private void OpenEditWindow(ContactEditViewModel vm)
    {
        var window = new ContactEditWindow { DataContext = vm };
        vm.RequestClose += (s, e) => { window.DialogResult = true; window.Close(); };
        if (window.ShowDialog() == true)
            LoadContacts();
    }

    private void DeleteContact()
    {
        if (SelectedContact == null) return;
        if (_dialogService.ShowConfirmation($"Удалить контакт \"{SelectedContact.Name}\"?", "Удаление"))
        {
            _context.Contacts.Remove(SelectedContact);
            _context.SaveChanges();
            LoadContacts();
            _dialogService.ShowInfo("Контакт удалён.", "Успех");
        }
    }
}
ViewModels/ContactEditViewModel.cs
csharp
public class ContactEditViewModel : ObservableObject
{
    private readonly PhoneBookDbEvseev2307aContext _context;
    private readonly IDialogService _dialogService;
    private Contact? _currentContact;
    private bool _isEditMode;

    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;

    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }
    public event EventHandler? RequestClose;

    public ContactEditViewModel(PhoneBookDbEvseev2307aContext context, IDialogService dialogService)
    {
        _context = context;
        _dialogService = dialogService;
        SaveCommand = new RelayCommand(Save, CanSave);
        CancelCommand = new RelayCommand(() => RequestClose?.Invoke(this, EventArgs.Empty));
    }

    public void InitializeForAdd() { _isEditMode = false; _currentContact = null; Name = Phone = string.Empty; }
    public void InitializeForEdit(Contact contact) { _isEditMode = true; _currentContact = contact; Name = contact.Name; Phone = contact.Phone; }

    private bool CanSave() => !string.IsNullOrWhiteSpace(Name) && Regex.IsMatch(Phone, @"^(\+7\d{10}|\d{10})$");

    private void Save()
    {
        try
        {
            if (_isEditMode && _currentContact != null)
            {
                _currentContact.Name = Name;
                _currentContact.Phone = Phone;
                _context.SaveChanges();
                _dialogService.ShowInfo("Контакт обновлён.", "Успех");
            }
            else
            {
                if (_context.Contacts.Any(c => c.Phone == Phone)) { _dialogService.ShowWarning("Контакт с таким номером уже существует!", "Дубликат"); return; }
                _context.Contacts.Add(new Contact { Name = Name, Phone = Phone });
                _context.SaveChanges();
                _dialogService.ShowInfo("Контакт добавлен.", "Успех");
            }
            RequestClose?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex) { _dialogService.ShowError($"Ошибка сохранения: {ex.Message}", "Ошибка"); }
    }
}
App.xaml.cs (регистрация DI)
csharp
services.AddDbContext<PhoneBookDbEvseev2307aContext>(options =>
    options.UseSqlite("Data Source=PhoneBookDB_Evseev_2307a.db"));
services.AddSingleton<IDialogService, DialogService>();
services.AddTransient<ContactEditViewModel>();
services.AddSingleton<ContactsListViewModel>();
services.AddSingleton<Func<ContactEditViewModel>>(provider => () => provider.GetRequiredService<ContactEditViewModel>());
MainWindow.xaml
xml
<Grid Margin="10">
    <Grid.RowDefinitions>
        <RowDefinition Height="Auto"/>
        <RowDefinition Height="*"/>
    </Grid.RowDefinitions>
    <StackPanel Grid.Row="0" Orientation="Horizontal" Margin="0,10">
        <Button Content="Добавить" Command="{Binding AddCommand}" Margin="0,0,10,0"/>
        <Button Content="Редактировать" Command="{Binding EditCommand}" Margin="0,0,10,0"/>
        <Button Content="Удалить" Command="{Binding DeleteCommand}"/>
    </StackPanel>
    <DataGrid Grid.Row="1" ItemsSource="{Binding Contacts}" SelectedItem="{Binding SelectedContact}" AutoGenerateColumns="False">
        <DataGrid.Columns>
            <DataGridTextColumn Header="Имя" Binding="{Binding Name}" Width="*"/>
            <DataGridTextColumn Header="Телефон" Binding="{Binding Phone}" Width="*"/>
        </DataGrid.Columns>
    </DataGrid>
</Grid>

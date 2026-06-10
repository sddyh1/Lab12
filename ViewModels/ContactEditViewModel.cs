using PhoneBookDI.Models;
using PhoneBookDI.Services;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Input;

namespace PhoneBookDI.ViewModels
{
    public class ContactEditViewModel : ObservableObject
    {
        private readonly PhoneBookDbEvseev2307aContext _context;
        private readonly IDialogService _dialogService;

        private Contact? _currentContact;
        private bool _isEditMode;

        private string _name = string.Empty;
        public string Name
        {
            get => _name;
            set
            {
                Set(ref _name, value);
                (SaveCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        private string _phone = string.Empty;
        public string Phone
        {
            get => _phone;
            set
            {
                Set(ref _phone, value);
                (SaveCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

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

        public void InitializeForAdd()
        {
            _isEditMode = false;
            _currentContact = null;
            Name = string.Empty;
            Phone = string.Empty;
        }

        public void InitializeForEdit(Contact contact)
        {
            _isEditMode = true;
            _currentContact = contact;
            Name = contact.Name;
            Phone = contact.Phone;
        }

        private bool CanSave()
        {
            return !string.IsNullOrWhiteSpace(Name) && Regex.IsMatch(Phone, @"^(\+7\d{10}|\d{10})$");
        }

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
                    if (_context.Contacts.Any(c => c.Phone == Phone))
                    {
                        _dialogService.ShowWarning("Контакт с таким номером уже существует!", "Дубликат");
                        return;
                    }
                    var newContact = new Contact { Name = Name, Phone = Phone };
                    _context.Contacts.Add(newContact);
                    _context.SaveChanges();
                    _dialogService.ShowInfo("Контакт добавлен.", "Успех");
                }
                RequestClose?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Ошибка сохранения: {ex.Message}", "Ошибка");
            }
        }
    }
}
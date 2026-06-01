using Microsoft.EntityFrameworkCore;
using PhoneBookDI.Models;
using PhoneBookDI.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Input;

namespace PhoneBookDI.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        private readonly IDialogService _dialogService;
        private readonly PhoneBookDbEvseev2307aContext _context;

        private ObservableCollection<Contact> _contacts;
        public ObservableCollection<Contact> Contacts
        {
            get => _contacts;
            set => Set(ref _contacts, value);
        }

        private string _name = string.Empty;
        public string Name
        {
            get => _name;
            set => Set(ref _name, value);
        }

        private string _phone = string.Empty;
        public string Phone
        {
            get => _phone;
            set => Set(ref _phone, value);
        }

        private Contact? _selectedContact;
        public Contact? SelectedContact
        {
            get => _selectedContact;
            set => Set(ref _selectedContact, value);
        }

        public ICommand AddCommand { get; }
        public ICommand DeleteCommand { get; }

        public MainViewModel(IDialogService dialogService, PhoneBookDbEvseev2307aContext context)
        {
            _dialogService = dialogService;
            _context = context;

            _context.Contacts.Load();
            Contacts = _context.Contacts.Local.ToObservableCollection();

            AddCommand = new RelayCommand(AddContact, CanAddContact);
            DeleteCommand = new RelayCommand<Contact>(DeleteContact, c => c != null);
        }

        private bool CanAddContact()
        {
            return !string.IsNullOrWhiteSpace(Name) && Regex.IsMatch(Phone, @"^(\+7\d{10}|\d{10})$");
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

            bool confirmed = _dialogService.ShowConfirmation($"Вы уверены, что хотите удалить контакт \"{contact.Name}\"?", "Удаление");
            if (!confirmed) return;

            try
            {
                _context.Contacts.Remove(contact);
                _context.SaveChanges();
                _dialogService.ShowInfo("Контакт удалён.", "Успех");
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Ошибка при удалении: {ex.Message}", "Ошибка");
            }
        }
    }
}
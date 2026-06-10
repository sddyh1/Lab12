using Microsoft.EntityFrameworkCore;
using PhoneBookDI.Models;
using PhoneBookDI.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace PhoneBookDI.ViewModels
{
    public class ContactsListViewModel : ObservableObject
    {
        private readonly PhoneBookDbEvseev2307aContext _context;
        private readonly IDialogService _dialogService;
        private readonly Func<ContactEditViewModel> _editViewModelFactory;

        private ObservableCollection<Contact> _contacts = null!;
        public ObservableCollection<Contact> Contacts
        {
            get => _contacts;
            set => Set(ref _contacts, value);
        }

        private Contact? _selectedContact;
        public Contact? SelectedContact
        {
            get => _selectedContact;
            set => Set(ref _selectedContact, value);
        }

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

            if (Contacts == null)
            {
                Contacts = new ObservableCollection<Contact>(freshContacts);
            }
            else
            {
                Contacts.Clear();
                foreach (var contact in freshContacts)
                {
                    Contacts.Add(contact);
                }
            }
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
            vm.RequestClose += (s, e) =>
            {
                window.DialogResult = true;
                window.Close();
            };
            bool? result = window.ShowDialog();
            if (result == true)
            {
                LoadContacts();
            }
        }

        private void DeleteContact()
        {
            if (SelectedContact == null) return;
            if (!_dialogService.ShowConfirmation($"Удалить контакт \"{SelectedContact.Name}\"?", "Удаление"))
                return;

            try
            {
                _context.Contacts.Remove(SelectedContact);
                _context.SaveChanges();
                LoadContacts();
                _dialogService.ShowInfo("Контакт удалён.", "Успех");
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Ошибка удаления: {ex.Message}", "Ошибка");
            }
        }
    }
}
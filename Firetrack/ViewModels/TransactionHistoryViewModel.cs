using Firetrack.Models;
using Firetrack.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace Firetrack.ViewModels
{
    public class TransactionHistoryViewModel : ViewModelBase
    {
        private ObservableCollection<TransactionModel> _transactions = new();
        private EquipmentModel? _equipment;
        private string _statusMessage = string.Empty;
        private bool _isBusy;

        public ObservableCollection<TransactionModel> Transactions
        {
            get => _transactions;
            set { _transactions = value; OnPropertyChanged(); }
        }

        public EquipmentModel? Equipment
        {
            get => _equipment;
            set { _equipment = value; OnPropertyChanged(); }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); }
        }

        public bool IsBusy
        {
            get => _isBusy;
            set { _isBusy = value; OnPropertyChanged(); }
        }

        public ICommand GoBackCommand { get; }

        public TransactionHistoryViewModel(EquipmentModel equipment)
        {
            Equipment = equipment;
            GoBackCommand = new Command(async () => await Shell.Current.GoToAsync(".."));
            LoadTransactions();
        }

        private async void LoadTransactions()
        {
            if (App.Database == null || Equipment == null) return;

            IsBusy = true;
            try
            {
                var logs = await App.Database.GetTransactionsForEquipmentAsync(Equipment.QRCode);
                Transactions.Clear();
                foreach (var t in logs.OrderByDescending(t => t.Timestamp))
                    Transactions.Add(t);
                StatusMessage = $"📋 {Transactions.Count} transaction(s) found.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"❌ Error: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
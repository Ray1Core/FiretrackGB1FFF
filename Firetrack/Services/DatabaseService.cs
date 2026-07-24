using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SQLite;
using Firetrack.Models;

namespace Firetrack.Services
{
    public class DatabaseService
    {
        private readonly SQLiteAsyncConnection _database;

        public DatabaseService(string dbPath)
        {
            _database = new SQLiteAsyncConnection(dbPath);

            // Create tables
            _database.CreateTableAsync<EquipmentModel>().Wait();
            _database.CreateTableAsync<TransactionModel>().Wait();
            _database.CreateTableAsync<UserModel>().Wait();

            // Seed data
            SeedData();
        }

        private void SeedData()
        {
            // Force admin user
            var admin = GetUserByUsernameAsync("admin").Result;
            if (admin == null)
            {
                SaveUserAsync(new UserModel
                {
                    Username = "admin",
                    Password = "admin123",
                    FullName = "Admin Chief",
                    Role = "Admin"
                }).Wait();
                System.Diagnostics.Debug.WriteLine("✅ Admin created.");
            }
            else
            {
                // Ensure password is correct
                if (admin.Password != "admin123")
                {
                    admin.Password = "admin123";
                    SaveUserAsync(admin).Wait();
                    System.Diagnostics.Debug.WriteLine("✅ Admin password reset.");
                }
            }

            // Ensure user
            var user = GetUserByUsernameAsync("user").Result;
            if (user == null)
            {
                SaveUserAsync(new UserModel
                {
                    Username = "user",
                    Password = "user123",
                    FullName = "John Firefighter",
                    Role = "Personnel"
                }).Wait();
                System.Diagnostics.Debug.WriteLine("✅ User created.");
            }

            // Seed equipment if empty
            var equipments = GetEquipmentsAsync().Result;
            if (equipments.Count == 0)
            {
                SaveEquipmentAsync(new EquipmentModel
                {
                    QRCode = "HOSE001",
                    Name = "Fire Hose 1",
                    Type = "Hose",
                    Status = "Available",
                    LastUpdated = DateTime.Now
                }).Wait();
                SaveEquipmentAsync(new EquipmentModel
                {
                    QRCode = "NOZZLE001",
                    Name = "High Pressure Nozzle",
                    Type = "Nozzle",
                    Status = "Issued",
                    AssignedToUsername = "user",
                    LastUpdated = DateTime.Now
                }).Wait();
                System.Diagnostics.Debug.WriteLine("✅ Equipment seeded.");
            }
        }

        // ---------- Public methods ----------
        public Task<List<EquipmentModel>> GetEquipmentsAsync() =>
            _database.Table<EquipmentModel>().ToListAsync();

        public Task<List<EquipmentModel>> GetEquipmentsAssignedToUserAsync(string username) =>
            _database.Table<EquipmentModel>().Where(e => e.AssignedToUsername == username).ToListAsync();

        public Task<int> SaveEquipmentAsync(EquipmentModel equipment) =>
            _database.InsertOrReplaceAsync(equipment);

        public Task<int> DeleteEquipmentAsync(EquipmentModel equipment) =>
            _database.DeleteAsync(equipment);

        public Task<int> SaveTransactionAsync(TransactionModel transaction) =>
            _database.InsertAsync(transaction);

        public Task<List<TransactionModel>> GetTransactionsForEquipmentAsync(string qrCode) =>
            _database.Table<TransactionModel>().Where(t => t.EquipmentQR == qrCode).ToListAsync();

        public Task<UserModel> GetUserByUsernameAsync(string username) =>
            _database.Table<UserModel>().FirstOrDefaultAsync(u => u.Username == username);

        public Task<int> SaveUserAsync(UserModel user) =>
            _database.InsertOrReplaceAsync(user);

        public Task<List<UserModel>> GetUsersAsync() =>
            _database.Table<UserModel>().ToListAsync();

        public Task<EquipmentModel?> GetEquipmentByQRAsync(string qrCode) =>
            _database.Table<EquipmentModel>().FirstOrDefaultAsync(e => e.QRCode == qrCode)!;
    }
}
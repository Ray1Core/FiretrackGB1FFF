using System.Data;
using Microsoft.Data.SqlClient;   // <-- use Microsoft.Data.SqlClient
using Dapper;
using Firetrack.Models;

namespace Firetrack.Services
{
    public class DatabaseService
    {
        private readonly string _connectionString;

        public DatabaseService(string connectionString)
        {
            _connectionString = connectionString;
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            // Seed admin
            var admin = connection.QueryFirstOrDefault<UserModel>(
                "SELECT * FROM Users WHERE Username = @Username", new { Username = "admin" });
            if (admin == null)
            {
                connection.Execute(
                    "INSERT INTO Users (Username, Password, FullName, Role) VALUES (@Username, @Password, @FullName, @Role)",
                    new { Username = "admin", Password = "admin123", FullName = "Admin Chief", Role = "Admin" });
            }

            // Seed user
            var user = connection.QueryFirstOrDefault<UserModel>(
                "SELECT * FROM Users WHERE Username = @Username", new { Username = "user" });
            if (user == null)
            {
                connection.Execute(
                    "INSERT INTO Users (Username, Password, FullName, Role) VALUES (@Username, @Password, @FullName, @Role)",
                    new { Username = "user", Password = "user123", FullName = "John Firefighter", Role = "Personnel" });
            }

            // Seed equipment if empty
            var count = connection.ExecuteScalar<int>("SELECT COUNT(*) FROM Equipment");
            if (count == 0)
            {
                connection.Execute(
                    "INSERT INTO Equipment (QRCode, Name, Type, Status, AssignedToUsername, LastUpdated) VALUES (@QRCode, @Name, @Type, @Status, @AssignedToUsername, @LastUpdated)",
                    new { QRCode = "HOSE001", Name = "Fire Hose 1", Type = "Hose", Status = "Available", AssignedToUsername = (string?)null, LastUpdated = DateTime.Now });
                connection.Execute(
                    "INSERT INTO Equipment (QRCode, Name, Type, Status, AssignedToUsername, LastUpdated) VALUES (@QRCode, @Name, @Type, @Status, @AssignedToUsername, @LastUpdated)",
                    new { QRCode = "NOZZLE001", Name = "High Pressure Nozzle", Type = "Nozzle", Status = "Issued", AssignedToUsername = "user", LastUpdated = DateTime.Now });
            }
        }

        // ---------- Equipment ----------
        public async Task<List<EquipmentModel>> GetEquipmentsAsync()
        {
            using var connection = new SqlConnection(_connectionString);
            var result = await connection.QueryAsync<EquipmentModel>("SELECT * FROM Equipment");
            return result.ToList();
        }

        public async Task<List<EquipmentModel>> GetEquipmentsAssignedToUserAsync(string username)
        {
            using var connection = new SqlConnection(_connectionString);
            var result = await connection.QueryAsync<EquipmentModel>(
                "SELECT * FROM Equipment WHERE AssignedToUsername = @Username",
                new { Username = username });
            return result.ToList();
        }

        public async Task<int> SaveEquipmentAsync(EquipmentModel equipment)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"
                IF EXISTS (SELECT 1 FROM Equipment WHERE EquipmentId = @EquipmentId)
                    UPDATE Equipment SET QRCode = @QRCode, Name = @Name, Type = @Type, Status = @Status,
                        AssignedToUsername = @AssignedToUsername, PhotoPath = @PhotoPath, Remarks = @Remarks,
                        LastUpdated = @LastUpdated
                    WHERE EquipmentId = @EquipmentId
                ELSE
                    INSERT INTO Equipment (QRCode, Name, Type, Status, AssignedToUsername, PhotoPath, Remarks, LastUpdated)
                    VALUES (@QRCode, @Name, @Type, @Status, @AssignedToUsername, @PhotoPath, @Remarks, @LastUpdated);
                    SELECT CAST(SCOPE_IDENTITY() as int);";
            return await connection.ExecuteScalarAsync<int>(sql, equipment);
        }

        public async Task<int> DeleteEquipmentAsync(EquipmentModel equipment)
        {
            using var connection = new SqlConnection(_connectionString);
            return await connection.ExecuteAsync("DELETE FROM Equipment WHERE EquipmentId = @EquipmentId", equipment);
        }

        // ---------- Transactions ----------
        public async Task<int> SaveTransactionAsync(TransactionModel transaction)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"INSERT INTO Transactions (EquipmentQR, FromUser, ToUser, Timestamp, Action, Remarks)
                            VALUES (@EquipmentQR, @FromUser, @ToUser, @Timestamp, @Action, @Remarks);
                            SELECT CAST(SCOPE_IDENTITY() as int);";
            return await connection.ExecuteScalarAsync<int>(sql, transaction);
        }

        public async Task<List<TransactionModel>> GetTransactionsForEquipmentAsync(string qrCode)
        {
            using var connection = new SqlConnection(_connectionString);
            var result = await connection.QueryAsync<TransactionModel>(
                "SELECT * FROM Transactions WHERE EquipmentQR = @QRCode ORDER BY Timestamp DESC",
                new { QRCode = qrCode });
            return result.ToList();
        }

        // ---------- Users ----------
        public async Task<UserModel?> GetUserByUsernameAsync(string username)
        {
            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<UserModel>(
                "SELECT * FROM Users WHERE Username = @Username",
                new { Username = username });
        }

        public async Task<int> SaveUserAsync(UserModel user)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"
                IF EXISTS (SELECT 1 FROM Users WHERE UserId = @UserId)
                    UPDATE Users SET Username = @Username, Password = @Password, FullName = @FullName, Role = @Role
                    WHERE UserId = @UserId
                ELSE
                    INSERT INTO Users (Username, Password, FullName, Role)
                    VALUES (@Username, @Password, @FullName, @Role);
                    SELECT CAST(SCOPE_IDENTITY() as int);";
            return await connection.ExecuteScalarAsync<int>(sql, user);
        }

        public async Task<List<UserModel>> GetUsersAsync()
        {
            using var connection = new SqlConnection(_connectionString);
            var result = await connection.QueryAsync<UserModel>("SELECT * FROM Users");
            return result.ToList();
        }

        public async Task<EquipmentModel?> GetEquipmentByQRAsync(string qrCode)
        {
            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<EquipmentModel>(
                "SELECT * FROM Equipment WHERE QRCode = @QRCode",
                new { QRCode = qrCode });
        }

        // ---------- Notifications ----------
        public async Task<int> SaveNotificationAsync(NotificationModel notification)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"INSERT INTO Notifications (Username, Title, Message, IsRead, Timestamp)
                    VALUES (@Username, @Title, @Message, @IsRead, @Timestamp);
                    SELECT CAST(SCOPE_IDENTITY() as int);";
            return await connection.ExecuteScalarAsync<int>(sql, notification);
        }

        public async Task<List<NotificationModel>> GetNotificationsForUserAsync(string username)
        {
            using var connection = new SqlConnection(_connectionString);
            var result = await connection.QueryAsync<NotificationModel>(
                "SELECT * FROM Notifications WHERE Username = @Username ORDER BY Timestamp DESC",
                new { Username = username });
            return result.ToList();
        }

        public async Task<int> MarkNotificationAsReadAsync(int notificationId)
        {
            using var connection = new SqlConnection(_connectionString);
            return await connection.ExecuteAsync(
                "UPDATE Notifications SET IsRead = 1 WHERE NotificationId = @NotificationId",
                new { NotificationId = notificationId });
        }

        public async Task<int> MarkAllNotificationsAsReadAsync(string username)
        {
            using var connection = new SqlConnection(_connectionString);
            return await connection.ExecuteAsync(
                "UPDATE Notifications SET IsRead = 1 WHERE Username = @Username",
                new { Username = username });
        }

        public async Task SendNotificationAsync(string username, string title, string message)
        {
            await SaveNotificationAsync(new NotificationModel
            {
                Username = username,
                Title = title,
                Message = message,
                IsRead = false,
                Timestamp = DateTime.Now
            });
        }
    }
}
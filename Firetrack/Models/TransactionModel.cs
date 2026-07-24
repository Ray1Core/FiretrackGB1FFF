using System;
using SQLite;

namespace Firetrack.Models
{
    [Table("Transactions")]
    public class TransactionModel
    {
        [PrimaryKey, AutoIncrement]
        public int TransactionId { get; set; }

        [NotNull]
        public string EquipmentQR { get; set; } = string.Empty;

        [NotNull]
        public string FromUser { get; set; } = string.Empty;

        [NotNull]
        public string ToUser { get; set; } = string.Empty;

        [NotNull]
        public DateTime Timestamp { get; set; }

        [NotNull]
        public string Action { get; set; } = string.Empty;

        public string? Remarks { get; set; }
    }
}
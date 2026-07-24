using System;
using SQLite;

namespace Firetrack.Models
{
    [Table("Equipment")]
    public class EquipmentModel
    {
        [PrimaryKey, AutoIncrement]
        public int EquipmentId { get; set; }

        [Unique, NotNull]
        public string QRCode { get; set; } = string.Empty;

        [NotNull]
        public string Name { get; set; } = string.Empty;

        [NotNull]
        public string Type { get; set; } = string.Empty;

        [NotNull]
        public string Status { get; set; } = "Available";

        public string? AssignedToUsername { get; set; }

        public string? PhotoPath { get; set; }
        public string? Remarks { get; set; }
        public DateTime? LastUpdated { get; set; }
    }
}
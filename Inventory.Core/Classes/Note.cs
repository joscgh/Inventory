using System;
using System.Collections.Generic;

namespace Inventory.Core.Classes
{
    public enum NoteType
    {
        Entrega,
        Pedido,
        Credito
    }

    public class Note
    {
        public int Id { get; set; }
        public NoteType Type { get; set; }
        public string NoteNumber { get; set; } = string.Empty;
        public DateTime IssueDate { get; set; } = DateTime.Today;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerDocument { get; set; } = string.Empty;
        public string CustomerAddress { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public string? ValidityPeriod { get; set; }
        public string? Conditions { get; set; }
        public decimal Subtotal { get; set; }
        public decimal TotalTax { get; set; }
        public decimal Total { get; set; }
        public int? CurrencyId { get; set; }
        public Currency? Currency { get; set; }
        public decimal? ExchangeRate { get; set; }

        public int? CustomerAccountId { get; set; }
        public CustomerAccount? CustomerAccount { get; set; }

        public int? ConsumerCustomerId { get; set; }
        public ConsumerCustomer? ConsumerCustomer { get; set; }

        public int CreatedByUserId { get; set; }
        public CustomerAccountUser? CreatedByUser { get; set; }

        public int? ReferenceNoteId { get; set; }
        public Note? ReferenceNote { get; set; }
        public List<Note> ReferencedByNotes { get; set; } = new();

        public List<NoteLine> Lines { get; set; } = new();
    }
}

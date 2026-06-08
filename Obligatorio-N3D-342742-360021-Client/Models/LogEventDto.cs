namespace Obligatorio_N3D_342742_360021_Client.Models
{
    public class LogEventDto
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Operation { get; set; } = string.Empty;
        public string TableName { get; set; } = string.Empty;
        public bool IsError { get; set; }
        public string Date { get; set; } = string.Empty;
        public int? LoanTicketId { get; set; }
    }
}

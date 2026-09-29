namespace WebApplication1.Models
{
    /* Model: ErrorViewModel. Wordt gebruikt door Views/Shared/Error.cshtml. */
    public class ErrorViewModel
    {
        // Unieke request-id waarmee een fout technisch teruggevonden kan worden in logs.
        public string? RequestId { get; set; }

        // Computed property: true wanneer RequestId gevuld is.
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}

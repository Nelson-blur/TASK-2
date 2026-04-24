namespace GreenFieldWeb.Models
{
    public class ErrorViewModel// This model is used to pass error information to the error view when an unhandled exception occurs in the application
    {
        public string? RequestId { get; set; }// The unique identifier for the current request, used for tracking and debugging errors
        
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);// A helper property that indicates whether the RequestId should be displayed in the error view, which is true if RequestId is not null or empty
    }
}

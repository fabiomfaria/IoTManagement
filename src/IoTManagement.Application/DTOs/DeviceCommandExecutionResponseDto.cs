namespace IoTManagement.Application.DTOs
{
    public class DeviceCommandExecutionResponseDto
    {
        public string DeviceId { get; set; }
        public string CommandId { get; set; }
        public string CommandName { get; set; }
        public string RawResponse { get; set; }
        public object FormattedResponse { get; set; }
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
    }
}
using System.Collections.Generic;

namespace IoTManagement.UI.Blazor.Models
{
    public class DeviceModel
    {
        public string Identifier { get; set; }
        public string Description { get; set; }
        public string Manufacturer { get; set; }
        public string Url { get; set; }
        public List<CommandDescriptionModel> Commands { get; set; } = new List<CommandDescriptionModel>();
    }

    public class CommandDescriptionModel
    {
        public string Operation { get; set; }
        public string Description { get; set; }
        public CommandModel Command { get; set; }
        public string Result { get; set; }
        public string Format { get; set; }
    }

    public class CommandModel
    {
        public string CommandText { get; set; }
        public List<ParameterModel> Parameters { get; set; } = new List<ParameterModel>();
    }

    public class ParameterModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class ExecuteCommandModel
    {
        public string[] ParameterValues { get; set; }
    }

    public class CommandResultModel
    {
        public string Result { get; set; }
    }

    public class LoginModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class TokenResponse
    {
        public string AccessToken { get; set; }
        public string TokenType { get; set; }
        public int ExpiresIn { get; set; }
        public string Scope { get; set; }
    }
}
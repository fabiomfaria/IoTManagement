using System.Collections.Generic;
using System.Linq; // Added for Enumerable.Any check

namespace IoTManagement.Domain.Entities
{
    /// <summary>
    /// Represents a command configured in our system that can be executed on a device.
    /// </summary>
    public class DeviceCommand
    {
        public int Id { get; set; }

        /// <summary>
        /// Name of the command, should correspond to a CommandDescription.Operation on the Device.
        /// </summary>
        public string Name { get; set; }
        public string Description { get; set; }

        /// <summary>
        /// The command string template. Can be the same as Device.CommandDescription.Command.CommandText
        /// or a variation managed by our system.
        /// </summary>
        public string CommandText { get; set; }

        public string ResultDescription { get; set; } // Description of what the result represents
        public string ResponseFormat { get; set; }    // Pattern or type for formatting the raw response

        public List<DeviceCommandParameter> Parameters { get; set; } = new List<DeviceCommandParameter>();

        public int DeviceId { get; set; } // Foreign key to Device.Id
        public virtual Device Device { get; set; } // Navigation property

        /// <summary>
        /// Formats the command text with the provided parameter values.
        /// Adheres to requirement vii.ii: space separation, \r termination.
        /// </summary>
        /// <param name="parameterValues">Values for the parameters.</param>
        /// <returns>The formatted command string ready to be sent.</returns>
        public string FormatCommandWithParameters(IEnumerable<string> parameterValues)
        {
            var commandWithParams = CommandText; // Base command

            if (parameterValues != null && parameterValues.Any())
            {
                foreach (var paramValue in parameterValues)
                {
                    // Ensure parameter values are correctly encoded/escaped if necessary
                    commandWithParams += " " + paramValue;
                }
            }

            return commandWithParams + "\r"; // Add line terminator
        }
    }
}
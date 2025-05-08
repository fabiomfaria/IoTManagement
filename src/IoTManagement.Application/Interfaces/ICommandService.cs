using IoTManagement.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IoTManagement.Application.Interfaces
{
    public interface ICommandService
    {
        /// <summary>
        /// Gets a list of all commands available for a specific device
        /// </summary>
        /// <param name="deviceId">The ID of the device</param>
        /// <returns>List of device command DTOs</returns>
        Task<IEnumerable<DeviceCommandDto>> GetDeviceCommandsAsync(string deviceId);

        /// <summary>
        /// Gets a specific command for a device by ID
        /// </summary>
        /// <param name="deviceId">The ID of the device</param>
        /// <param name="commandId">The ID of the command</param>
        /// <returns>Device command DTO if found, otherwise null</returns>
        Task<DeviceCommandDto> GetDeviceCommandAsync(string deviceId, string commandId);

        /// <summary>
        /// Executes a command on a device with the specified parameters
        /// </summary>
        /// <param name="request">Command execution request DTO</param>
        /// <returns>Command execution response DTO</returns>
        Task<DeviceCommandExecutionResponseDto> ExecuteCommandAsync(DeviceCommandExecutionRequestDto request);
    }
}
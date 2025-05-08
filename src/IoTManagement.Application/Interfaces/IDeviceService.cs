using IoTManagement.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IoTManagement.Application.Interfaces
{
    public interface IDeviceService
    {
        /// <summary>
        /// Gets a list of all devices
        /// </summary>
        /// <returns>List of device DTOs</returns>
        Task<List<DeviceDto>> GetAllDevicesAsync();

        /// <summary>
        /// Gets a device by ID
        /// </summary>
        /// <param name="id">The ID of the device</param>
        /// <returns>Device DTO if found, otherwise null</returns>
        Task<DeviceDto> GetDeviceByIdAsync(string id);

        /// <summary>
        /// Gets all commands available for a device
        /// </summary>
        /// <param name="deviceId">The ID of the device</param>
        /// <returns>List of device command DTOs</returns>
        Task<List<DeviceCommandDto>> GetDeviceCommandsAsync(string deviceId);

        /// <summary>
        /// Gets a specific command by ID for a device
        /// </summary>
        /// <param name="deviceId">The ID of the device</param>
        /// <param name="commandId">The ID of the command</param>
        /// <returns>Device command DTO if found, otherwise null</returns>
        Task<DeviceCommandDto> GetDeviceCommandByIdAsync(string deviceId, string commandId);

        /// <summary>
        /// Executes a command on a device
        /// </summary>
        /// <param name="deviceId">The ID of the device</param>
        /// <param name="commandId">The ID of the command</param>
        /// <param name="requestDto">The command execution request data</param>
        /// <returns>Result of the command execution</returns>
        Task<DeviceCommandExecutionResponseDto> ExecuteCommandAsync(string deviceId, string commandId, DeviceCommandExecutionRequestDto requestDto);
    }
}
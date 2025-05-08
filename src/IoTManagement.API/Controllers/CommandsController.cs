using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IoTManagement.API.Models;
using IoTManagement.Application.Interfaces;
using IoTManagement.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IoTManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CommandsController : ControllerBase
    {
        private readonly IDeviceService _deviceService;
        private readonly ILogger<CommandsController> _logger;

        public CommandsController(IDeviceService deviceService, ILogger<CommandsController> logger)
        {
            _deviceService = deviceService ?? throw new ArgumentNullException(nameof(deviceService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpPost("execute")]
        public async Task<ActionResult<CommandExecutionResponse>> ExecuteCommand([FromBody] CommandExecutionRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var device = await _deviceService.GetDeviceByIdAsync(request.DeviceId);
                if (device == null)
                {
                    return NotFound($"Device with ID {request.DeviceId} not found");
                }

                var command = device.Commands.Find(c => c.Id == request.CommandId);
                if (command == null)
                {
                    return NotFound($"Command with ID {request.CommandId} not found for device {request.DeviceId}");
                }

                string result = await _deviceService.ExecuteDeviceCommandAsync(
                    request.DeviceId,
                    request.CommandId,
                    request.Parameters);

                // In a real implementation, we would parse the result based on the format
                // For now, just return the raw result
                var response = new CommandExecutionResponse
                {
                    Result = result,
                    FormattedResult = result, // In a real implementation, would format based on command.Format
                    FormatDescription = command.Format?.Description
                };

                return Ok(response);
            }
            catch (ValidationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (TelnetCommunicationException ex)
            {
                _logger.LogError(ex, "Error communicating with device");
                return StatusCode(500, "Error communicating with device: " + ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing command");
                return StatusCode(500, "An unexpected error occurred");
            }
        }

        [HttpGet("{deviceId}/commands")]
        public async Task<ActionResult<List<CommandModel>>> GetDeviceCommands(string deviceId)
        {
            try 
            {
                var device = await _deviceService.GetDeviceByIdAsync(deviceId);
                if (device == null)
                {
                    return NotFound($"Device with ID {deviceId} not found");
                }

                var commands = new List<CommandModel>();
                foreach (var command in device.Commands)
                {
                    commands.Add(new CommandModel
                    {
                        Id = command.Id,
                        Name = command.Name,
                        Description = command.Description,
                        ResultDescription = command.ResultDescription,
                        CommandDetails = new CommandDetailsModel
                        {
                            Command = command.CommandDetails.Command,
                            Parameters = command.CommandDetails.Parameters?.ConvertAll(p => new ParameterModel
                            {
                                Name = p.Name,
                                Description = p.Description,
                                Type = p.Type,
                                Required = p.Required
                            })
                        },
                        Format = new ResponseFormatModel
                        {
                            Description = command.Format?.Description,
                            Format = command.Format?.Format
                        }
                    });
                }

                return Ok(commands);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting device commands");
                return StatusCode(500, "An unexpected error occurred");
            }
        }

        [HttpGet("{deviceId}/commands/{commandId}")]
        public async Task<ActionResult<CommandModel>> GetCommandDetails(string deviceId, string commandId)
        {
            try
            {
                var device = await _deviceService.GetDeviceByIdAsync(deviceId);
                if (device == null)
                {
                    return NotFound($"Device with ID {deviceId} not found");
                }

                var command = device.Commands.Find(c => c.Id == commandId);
                if (command == null)
                {
                    return NotFound($"Command with ID {commandId} not found for device {deviceId}");
                }

                var commandModel = new CommandModel
                {
                    Id = command.Id,
                    Name = command.Name,
                    Description = command.Description,
                    ResultDescription = command.ResultDescription,
                    CommandDetails = new CommandDetailsModel
                    {
                        Command = command.CommandDetails.Command,
                        Parameters = command.CommandDetails.Parameters?.ConvertAll(p => new ParameterModel
                        {
                            Name = p.Name,
                            Description = p.Description,
                            Type = p.Type,
                            Required = p.Required
                        })
                    },
                    Format = new ResponseFormatModel
                    {
                        Description = command.Format?.Description,
                        Format = command.Format?.Format
                    }
                };

                return Ok(commandModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting command details");
                return StatusCode(500, "An unexpected error occurred");
            }
        }
    }
}
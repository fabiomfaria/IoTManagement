using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IoTManagement.Application.Services;
using IoTManagement.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IoTManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DevicesController : ControllerBase
    {
        private readonly IDeviceService _deviceService;

        public DevicesController(IDeviceService deviceService)
        {
            _deviceService = deviceService ?? throw new ArgumentNullException(nameof(deviceService));
        }

        /// <summary>
        /// Obtém a lista de identificadores de todos os dispositivos
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<string>>> GetAllDeviceIds()
        {
            var deviceIds = await _deviceService.GetAllDeviceIdsAsync();
            return Ok(deviceIds);
        }

        /// <summary>
        /// Obtém os detalhes de um dispositivo específico
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<DeviceDto>> GetDeviceById(string id)
        {
            var device = await _deviceService.GetDeviceDetailsAsync(id);
            
            if (device == null)
                return NotFound();
                
            // Mapear domínio para DTO
            var deviceDto = new DeviceDto
            {
                Identifier = device.Identifier,
                Description = device.Description,
                Manufacturer = device.Manufacturer,
                Url = device.Url,
                Commands = device.Commands.ConvertAll(c => new CommandDescriptionDto
                {
                    Operation = c.Operation,
                    Description = c.Description,
                    Result = c.Result,
                    Format = c.Format,
                    Command = new CommandDto
                    {
                        CommandText = c.Command.CommandText,
                        Parameters = c.Command.Parameters.ConvertAll(p => new ParameterDto
                        {
                            Name = p.Name,
                            Description = p.Description
                        })
                    }
                })
            };
            
            return Ok(deviceDto);
        }

        /// <summary>
        /// Executa um comando em um dispositivo
        /// </summary>
        [HttpPost("{id}/commands/{commandIndex}/execute")]
        public async Task<ActionResult<string>> ExecuteCommand(
            string id, 
            int commandIndex, 
            [FromBody] ExecuteCommandDto executeCommandDto)
        {
            try
            {
                var result = await _deviceService.ExecuteDeviceCommandAsync(
                    id, 
                    commandIndex, 
                    executeCommandDto.ParameterValues);
                    
                return Ok(new { Result = result });
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Device with ID {id} not found");
            }
            catch (ArgumentOutOfRangeException)
            {
                return BadRequest($"Invalid command index: {commandIndex}");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, $"Error executing command: {ex.Message}");
            }
        }
    }
}
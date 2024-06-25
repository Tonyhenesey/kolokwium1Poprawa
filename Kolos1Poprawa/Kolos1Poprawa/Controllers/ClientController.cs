using Microsoft.AspNetCore.Mvc;
using Kolos1Poprawa.Models;
using Kolos1Poprawa.Repository;
using System.Threading.Tasks;

namespace Kolos1Poprawa.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientsController : ControllerBase
    {
        private readonly IClientRepository _clientRepository;

        public ClientsController(IClientRepository clientRepository)
        {
            _clientRepository = clientRepository;
        }

        [HttpGet("{clientId}")]
        public async Task<ActionResult<ClientDTO>> GetClientById(int clientId)
        {
            var client = await _clientRepository.GetClientDataById(clientId);

            if (client == null)
            {
                return NotFound();
            }

            return Ok(client);
        }

        [HttpPost("")]
        public async Task<ActionResult> PostClient([FromBody] PostClientDTO postClientDTO)
        {
            if (postClientDTO == null)
            {
                return BadRequest();
            }

            var result = await _clientRepository.AddClientAndRental(postClientDTO);

            if (result == null)
            {
                return BadRequest("Error adding client and rental");
            }

            return Ok(result);
        }
    }
}
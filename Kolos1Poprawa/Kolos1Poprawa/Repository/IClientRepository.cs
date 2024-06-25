using Kolos1Poprawa.Models;

namespace Kolos1Poprawa.Repository
{
    public interface IClientRepository
    {
        Task<ClientDTO> GetClientDataById(int clientId);
        Task<PostClientDTO> AddClientAndRental(PostClientDTO postClientDTO);
    }
}
using System.Threading.Tasks;
using DatingApp.Api.Models;

namespace DatingApp.Api.Data
{
    public interface IAuthRepository
    {
         public Task<User> Register(User user, string password);
         public Task<User> Login(string username, string password);
         public Task<bool> UserExists(string username);
    }
}
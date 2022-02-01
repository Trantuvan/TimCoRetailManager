using System.Net.Http;
using System.Threading.Tasks;
using TRMDesktopUI.Models;

namespace TRMDesktopUI.Library.Api
{
    public interface IAPIHelper
    {
        Task<AuthenticatedUser> Authenticate(string userName, string password);
        Task GetLoggedInUserInfo(string token);
    }
}
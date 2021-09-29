using Microsoft.AspNetCore.Http;
using Snippet.EntityFrameworkCore.Audit.Extension;

namespace Snippet.EntityFrameworkCore.Audit.Sample.Data
{
    public class HttpContextUserInfoProvider : IUserInfoAccessor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HttpContextUserInfoProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetUserName()
        {
            return "管理员";
        }
    }
}
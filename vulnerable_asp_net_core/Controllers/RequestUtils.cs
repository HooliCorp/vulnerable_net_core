using Microsoft.AspNetCore.Http;

namespace vulnerable_asp_net_core.Controllers
{
    public static class RequestUtils
    {
        public static string GetIfDefined(HttpRequest req, string val)
        {
            if (req.Query.ContainsKey(val))
                return req.Query[val];

            return "";
        }
    }
}
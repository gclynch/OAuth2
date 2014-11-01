// hello controller with authorised controller action
// say hello to the authenticated user

// authentication is done in the host (IIS) - see Startup.Auth.cs
// /Token is token endpoint for OAuth, issues bearer tokens in JSON format - Authorization server
// a bearer token is an access token which anyone can use (no checks) so should be 
// used only over https to keep the token secret

using System.Web.Http;

namespace HelloWorldOAuth2.Controllers
{
    public class HelloController : ApiController
    {
        // GET /api/Hello
        [Authorize]                     // return 401 Unauthorized if not authorized
        public IHttpActionResult GetHelloGreeting()               
        {
            // user identity established via bearer token in authorization header
            return Ok("Hello there " + User.Identity.Name + " authenticated using " + User.Identity.AuthenticationType + " authentication type, welcome to the Web API");      // 200 OK
        }

        // to do: enable SSL and require it

        // [Authorize] can be applied to a controller action, a controller, or for all controllers
    }
}

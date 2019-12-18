using System;
using System.Collections.Generic;
//using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using FirebirdSql.Data.FirebirdClient;
using MerchantAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace MerchantAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private AppDbContext context;
        public UsersController(AppDbContext dbContext)
        {
            context = dbContext;
        }

        // GET: api/Users/bc3e77c2657c4374cfa9f23f226adf87
        [SwaggerOperation(Summary = "Получение кода клиента Макссипост по user account для web-api")]
        [HttpGet("{account}", Name = "Get")]
        public async Task<ActionResult<UserModel>> Get(string account)
        {   
            FbParameter account_param = new FbParameter("account", FbDbType.VarChar);
            account_param.Value = account;

            try
            {
                var users = await context.Query<UserModel>()
                    .FromSql(
                        "select suc.client_id from site_users su join site_users_clients suc on suc.site_user_id = su.id where su.is_active = 1 and su.md5_account = @account", 
                        account_param).FirstOrDefaultAsync();

                if (users != null)
                {
                  HttpContext.Session.SetString("ClientId", users.Client_Id.ToString());
                  return users;
                }
                else
                 return Ok("{}");

            }
            catch (Exception e)
            {
                ModelState.AddModelError("GetClientID", e.Message);
                return BadRequest(ModelState);
            }
            
        }
    }
}

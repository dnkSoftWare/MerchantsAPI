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
       /* [HttpGet("{account}", Name = "Get")]
        public ActionResult<UserModel> Get(string account)
        {
            FbParameter account_param = new FbParameter("account", FbDbType.VarChar);
            account_param.Value = account;
            // RawSqlString sql = new RawSqlString($"select suc.client_id from site_users su join site_users_clients suc on suc.site_user_id = su.id where  su.is_active = 1 and su.md5_account = '{account}'");
            var userModels = dbUsers.Query<UserModel>().FromSql("select suc.client_id from site_users su join site_users_clients suc on suc.site_user_id = su.id where  su.is_active = 1 and su.md5_account = @account",account_param).First();

            HttpContext.Session.SetString("ClientId", userModels.Client_Id.ToString());

            return userModels;
        }*/

        [HttpGet("{account}", Name = "Get")]
        public async Task<ActionResult<UserModel>> Get(string account)
        {   
            FbParameter account_param = new FbParameter("account", FbDbType.VarChar);
            account_param.Value = account;

            try
            {
                var users = await context.Query<UserModel>()
                    .FromSql(
                        "select suc.client_id \n from site_users su \n  join site_users_clients suc on suc.site_user_id = su.id \n where  \n      su.is_active = 1 \n  and su.md5_account = @account",
                        account_param).FirstAsync();

                if (users != null)
                {
                    HttpContext.Session.SetString("ClientId", users.Client_Id.ToString());
                    return users;
                }
                else
                {
                    ModelState.AddModelError("GetClientID", $"Код клиента не найден!");
                    return BadRequest(ModelState);
                }
            
            }
            catch (InvalidOperationException e)
            {
                
                if (e.Message.Contains("Source sequence doesn't contain any elements."))
                {
                    ModelState.AddModelError("GetClientID", $"Код клиента не найден!");
                    return BadRequest(ModelState);
                }
                else
                {
                    return BadRequest(e);
                }
            }
            
        }
    }
}

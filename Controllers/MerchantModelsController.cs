using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MerchantAPI.Models;

namespace MerchantAPI.Controllers
{
    [Route("api/MerchantModels")]
    [ApiController]
    public class MerchantModelsController : ControllerBase
    {
        private AppDbContext _context;
        public MerchantModelsController(AppDbContext context)
        {
            _context = context;
        }
       
        // GET: api/MerchantModels
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MerchantModel>>> GetMerchants()
        {
            //Debug.WriteLine(_context.Merchants.ToSql());
            var clientId = GetClientId();
            if (clientId > 0)
            {
                return await _context.Merchants.Where(m => m.Client_ID == GetClientId()).ToListAsync();
            }
            else
            {
                ModelState.AddModelError("MerchantModels", $"Код клиента [{clientId}] не определён! Для получения кода вызовите метод GET: api/Users/XXXX где XXXX ваш аккаунт в системе Максипост.");
                return BadRequest(ModelState);
            }
        }

        // GET: api/MerchantModels/load
        [HttpGet("load")]
        public async Task<ActionResult<int>> Load()
        {
            var clientId = GetClientId();
            if (clientId == 0)
            {
                ModelState.AddModelError("MerchantModels", $"Код клиента [{clientId}] не определён! Для получения кода вызовите метод GET: api/Users/XXXX где XXXX ваш аккаунт в системе Максипост.");
                return BadRequest(ModelState);
            }

            var record_affected =
                await _context.Database.ExecuteSqlCommandAsync($"execute procedure add_client_warehouses({clientId}, 10);");
            return record_affected;
        }
        
        // GET: api/MerchantModels/5
        [HttpGet("{id}")]
        public async Task<ActionResult<MerchantModel>> GetMerchantModel(int id)
        {
            var clientId = GetClientId();
            if (clientId == 0)
            {
                ModelState.AddModelError("MerchantModels", $"Код клиента [{clientId}] не определён! Для получения кода вызовите метод GET: api/Users/XXXX где XXXX ваш аккаунт в системе Максипост.");
                return BadRequest(ModelState);
            }
            
            var merchantModel = await _context.Merchants.FromSql($"select * from TMP_MERCHANTS where Client_Id = {clientId} and ID = {id}").FirstOrDefaultAsync(); 

            if (merchantModel == null)
            {
                return NotFound();
            }

            return merchantModel;
        }

        // PUT: api/MerchantModels/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMerchantModel([FromQuery]int id, [FromBody]MerchantModel merchantModel)
        {
            var clientId = GetClientId();
            if (clientId == 0)
            {
                ModelState.AddModelError("MerchantModels", $"Код клиента [{clientId}] не определён! Для получения кода вызовите метод GET: api/Users/XXXX где XXXX ваш аккаунт в системе Максипост.");
                return BadRequest(ModelState);
            }

            if (id != merchantModel.Id)
            {
                ModelState.AddModelError("MerchantModels", $"ID записи не соответствует ID из текущего объекта MerchantModels");
                return BadRequest();
            }

            _context.Entry(merchantModel).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MerchantModelExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/MerchantModels
        [HttpPost]
        public async Task<ActionResult<MerchantModel>> PostMerchantModel([FromBody]MerchantModel merchantModel)
        {
            var clientID = GetClientId();
            if (clientID == 0)
            {
                ModelState.AddModelError("MerchantModels", $"Код клиента [{clientID}] не определён! Для получения кода вызовите метод GET: api/Users/XXXX где XXXX ваш аккаунт в системе Максипост.");
                return BadRequest(ModelState);
            }


            var merchantModelHave =
                await _context.Merchants.AnyAsync(m => (m.Merch_Code == merchantModel.Merch_Code) &&
                              (m.Client_ID == clientID));


            
                if (merchantModelHave)
                {
                    ModelState.AddModelError("Merch_Code", $"Код мерчанта [{merchantModel.Merch_Code}] уже есть в базе !");
                    return BadRequest(ModelState);
                }
            
                merchantModel.Client_ID = clientID;
                merchantModel.Date_Create = DateTime.Now;
                merchantModel.Is_Actual = 1;

                _context.Merchants.Add(merchantModel);
                await _context.SaveChangesAsync();

                return CreatedAtAction("GetMerchantModel", new {id = merchantModel.Id}, merchantModel);
        }

        // DELETE: api/MerchantModels/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<MerchantModel>> DeleteMerchantModel([FromQuery]int id)
        {
            var clientId = GetClientId();
            if (clientId == 0)
            {
                ModelState.AddModelError("MerchantModels", $"Код клиента [{clientId}] не определён! Для получения кода вызовите метод GET: api/Users/XXXX где XXXX ваш аккаунт в системе Максипост.");
                return BadRequest(ModelState);
            }

            var merchantModel = await _context.Merchants.FromSql($"select * from TMP_MERCHANTS where Client_ID = {clientId} and ID = {id}").FirstOrDefaultAsync();

            // var merchantModel = await _context.Merchants.FindAsync(id);
            if (merchantModel == null)
            {
                return NotFound();
            }

            merchantModel.Is_Actual = 0; // Помечаем как не актуальный

            _context.Entry(merchantModel).State = EntityState.Modified;
                //_context.Merchants.Update(merchantModel); 

            await _context.SaveChangesAsync();

            return merchantModel;
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        private bool MerchantModelExists(int id)
        {
            return _context.Merchants.Any(e => e.Id == id);
        }

        private int GetClientId()
        {
            if (HttpContext.Session.Keys.Contains("ClientId"))
            {
               return Int32.Parse(HttpContext.Session.GetString("ClientId"));
            }
            else
            {
               return 0;
            }
        }
    }
}

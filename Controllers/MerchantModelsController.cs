using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MerchantAPI.Models;
using Swashbuckle.AspNetCore.Annotations;

namespace MerchantAPI.Controllers
{
    [SwaggerTag("Добавление, чтение, обновление и удаление(дезактивация) мерчантов.")]
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
        [SwaggerOperation(Summary = "Получение всего списка мерчантов")]
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
        /// <summary>
        /// Собственно загрузка данных из плоского списка в справочники Максипоста 
        /// </summary>
        /// <returns></returns>
        [SwaggerOperation(Summary = "Загрузка данных в БД Максипоста по изменённому списку мерчантов")]
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
                await _context.Database.ExecuteSqlCommandAsync($"execute procedure add_client_warehouses({clientId});");

            return record_affected;
        }
        
        // GET: api/MerchantModels/5
        [SwaggerOperation(Summary = "Получение данных мерчанта по ID")]
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
        [SwaggerOperation(Summary = "Обновление данных мерчанта по ID ", Description = "Данные по указанному мерчанту передаются в формате JSON")]
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
        [SwaggerOperation(Summary = "Добавление в список нового мерчанта")]
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
        [SwaggerOperation(Summary = "Дезактивация мерчанта по ID")]
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

        /// <summary>
        /// Проверка , что мерчант с ID существует в списке
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [ApiExplorerSettings(IgnoreApi = true)]
        private bool MerchantModelExists(int id)
        {
            return _context.Merchants.Any(e => e.Id == id);
        }

        /// <summary>
        /// ВытаСкайп? скиваем ClientId если она есть в сессионной переменной!
        /// </summary>
        /// <returns></returns>
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

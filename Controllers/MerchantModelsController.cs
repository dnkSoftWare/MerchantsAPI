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
        private readonly AppDbContext _context;
        public MerchantModelsController(AppDbContext context)
        {
            _context = context;
        }
       
        // GET: api/MerchantModels
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MerchantModel>>> GetMerchants()
        {
            //Debug.WriteLine(_context.Merchants.ToSql());
            return await _context.Merchants.Where(m => m.Client_ID == GetClientId()).ToListAsync();
        }

        // GET: api/MerchantModels/5
        [HttpGet("{id}")]
        public async Task<ActionResult<MerchantModel>> GetMerchantModel(int id)
        {
            var merchantModel = await _context.Merchants.FromSql($"select * from TMP_MERCHANTS where Client_Id = {GetClientId()} and ID = {id}").FirstAsync(); 

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
            if (id != merchantModel.Id)
            {
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
            var merchantModelHave =
                await _context.Merchants.AnyAsync(m => (m.Merch_Code == merchantModel.Merch_Code) &&
                              (m.Client_ID == merchantModel.Client_ID));


            
                if (merchantModelHave)
                {
                    ModelState.AddModelError("Merch_Code", $"Код мерчанта [{merchantModel.Merch_Code}] уже есть в базе !");
                    return BadRequest(ModelState);
                }
            
                merchantModel.Client_ID = GetClientId();
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
            var merchantModel = await _context.Merchants.FromSql($"select * from TMP_MERCHANTS where Client_ID = {GetClientId()} and ID = {id}").FirstAsync();

            // var merchantModel = await _context.Merchants.FindAsync(id);
            if (merchantModel == null)
            {
                return NotFound();
            }

            _context.Merchants.Remove(merchantModel);
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

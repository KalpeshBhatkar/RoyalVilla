using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoyalVilla_API.Data;
using RoyalVilla_API.Models;
using RoyalVilla_API.Models.DTO;

namespace RoyalVilla_API.Controllers
{
    [Route("api/villa")]
    [ApiController]
    public class VillaController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public VillaController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Villa>>> GetVillasById()
        {
            return Ok(await _db.Villa.ToListAsync());
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Villa>> GetVillaById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest("Invalid ID. ID must be greater than zero.");
                }

                var villa = await _db.Villa.FirstOrDefaultAsync(v => v.Id == id);
                if (villa == null)
                {
                    return NotFound($"Villa with ID {id} not found.");
                }
                else
                {
                    return Ok(villa);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error occurred while retrieving villa with ID {id} : {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<ActionResult<Villa>> GetVillaById(CreateVillaDTO villaDTO)
        {
            try
            {
                if (villaDTO == null)
                {
                    return BadRequest("villa data is required.");
                }

                Villa _villa = new()
                {
                    Name = villaDTO.Name,
                    Details = villaDTO.Details,
                    Rate = villaDTO.Rate,
                    Sqft = villaDTO.Sqft,
                    Occupancy = villaDTO.Occupancy,
                    ImageUrl = villaDTO.ImageUrl,
                    CreatedDate = DateTime.Now
                };

                await _db.Villa.AddAsync(_villa);
                await _db.SaveChangesAsync();
                return Ok(_villa);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error occurred while creating the villa : {ex.Message}");
            }
        }

    }
}

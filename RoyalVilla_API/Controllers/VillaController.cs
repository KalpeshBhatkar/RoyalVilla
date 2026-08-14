using AutoMapper;
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
        private readonly IMapper _mapper;

        public VillaController(ApplicationDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
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
        public async Task<ActionResult<Villa>> CreateVilla(CreateVillaDTO villaDTO)
        {
            try
            {
                if (villaDTO == null)
                {
                    return BadRequest("villa data is required.");
                }

                Villa _villa = _mapper.Map<Villa>(villaDTO);  

                await _db.Villa.AddAsync(_villa);
                await _db.SaveChangesAsync();
                return CreatedAtAction(nameof(GetVillaById), new { id = _villa.Id }, _villa);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error occurred while creating the villa : {ex.Message}");
            }
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<Villa>> UpdateVilla(int id, UpdateVillaDTO villaDTO)
        {
            try
            {
                if (villaDTO == null)
                {
                    return BadRequest("villa data is required.");
                }
                if (id != villaDTO.Id)
                {
                    return BadRequest("Villa ID in URL does not match Villa ID in request body");
                }

                var existingVilla = await _db.Villa.FirstOrDefaultAsync(v => v.Id == id);
                if (existingVilla == null)
                {
                    return NotFound($"Villa with ID {id} not found.");
                }

                _mapper.Map(villaDTO, existingVilla);
                existingVilla.UpdatedDate = DateTime.UtcNow;
                //await _db.Villa.AddAsync(existingVilla);
                await _db.SaveChangesAsync();
                return Ok(villaDTO);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error occurred while updating the villa : {ex.Message}");
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult<Villa>> DeleteVilla(int id)
        {
            try
            {
                var existingVilla = await _db.Villa.FirstOrDefaultAsync(v => v.Id == id);
                if (existingVilla == null)
                {
                    return NotFound($"Villa with ID {id} not found.");
                }
                _db.Villa.Remove(existingVilla);
                await _db.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error occurred while deleting the villa : {ex.Message}");
            }
        }
    }
}

using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoyalVilla.DTO;
using RoyalVilla_API.Data;
using RoyalVilla_API.Models;

namespace RoyalVilla_API.Controllers.v1
{
    [Route("api/v{version:apiVersion}/villa")]
    [ApiVersion("1.0")]
    [ApiController]
    //[Authorize]
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
        //[Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<VillaDTO>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<VillaDTO>>>> GetVillas()
        {
            var villas = await _db.Villa.ToListAsync();
            var dtoResponseVilla = _mapper.Map<List<VillaDTO>>(villas);
            return Ok(ApiResponse<IEnumerable<VillaDTO>>.Ok(dtoResponseVilla, "Records retrieved successfully"));
        }

        [HttpGet("{id:int}")]
        //[AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<VillaDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<VillaDTO>>> GetVillaById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return NotFound(ApiResponse<object>.NotFound("Invalid ID. ID must be greater than zero."));
                }

                var villa = await _db.Villa.FirstOrDefaultAsync(v => v.Id == id);
                if (villa == null)
                {
                    return NotFound(ApiResponse<object>.NotFound($"Villa with ID {id} not found."));
                }
                else
                {
                    return Ok(ApiResponse<VillaDTO>.Ok(_mapper.Map<VillaDTO>(villa), "Records retrieved successfully"));
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error occurred while retrieving villa with ID {id} : {ex.Message}");
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<VillaDTO>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<VillaDTO>>> CreateVilla(CreateVillaDTO villaDTO)
        {
            try
            {
                if (villaDTO == null)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("villa data is required."));
                }

                Villa _villa = _mapper.Map<Villa>(villaDTO);
                var duplicateVilla = await _db.Villa.FirstOrDefaultAsync(v => v.Name.ToLower() == villaDTO.Name.ToLower());
                if (duplicateVilla != null)
                {
                    return Conflict(ApiResponse<object>.Conflict($"Villa with name '{villaDTO.Name}' already exists."));
                }
                await _db.Villa.AddAsync(_villa);
                await _db.SaveChangesAsync();
                var dtoResponseVilla = _mapper.Map<VillaDTO>(_villa);
                return CreatedAtAction(nameof(GetVillaById), new { id = _villa.Id }, ApiResponse<VillaDTO>.CreatedAt(dtoResponseVilla, "Villa created successfully"));
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<object>.Error(StatusCodes.Status500InternalServerError, $"Error occurred while creating the villa", ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
            }
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<VillaDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<VillaDTO>>> UpdateVilla(int id, UpdateVillaDTO villaDTO)
        {
            try
            {
                if (villaDTO == null)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("villa data is required."));
                }
                if (id != villaDTO.Id)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Villa ID in URL does not match Villa ID in request body"));
                }

                var existingVilla = await _db.Villa.FirstOrDefaultAsync(v => v.Id == id);
                if (existingVilla == null)
                {
                    return NotFound(ApiResponse<object>.NotFound($"Villa with ID {id} not found."));
                }

                var duplicateVilla = await _db.Villa.FirstOrDefaultAsync(v => v.Name.ToLower() == villaDTO.Name.ToLower() && v.Id != id);
                if (duplicateVilla != null)
                {
                    return Conflict(ApiResponse<object>.Conflict($"Villa with name '{villaDTO.Name}' already exists."));
                }

                _mapper.Map(villaDTO, existingVilla);
                existingVilla.UpdatedDate = DateTime.UtcNow;
                //await _db.Villa.AddAsync(existingVilla);
                await _db.SaveChangesAsync();
                return Ok(ApiResponse<VillaDTO>.Ok(_mapper.Map<VillaDTO>(villaDTO), "Villa updated successfully"));
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<object>.Error(StatusCodes.Status500InternalServerError, $"Error occurred while updating the villa", ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
            }
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<object>>> DeleteVilla(int id)
        {
            try
            {
                var existingVilla = await _db.Villa.FirstOrDefaultAsync(v => v.Id == id);
                if (existingVilla == null)
                {
                    return NotFound(ApiResponse<object>.NotFound($"Villa with ID {id} not found."));
                }
                _db.Villa.Remove(existingVilla);
                await _db.SaveChangesAsync();
                return Ok(ApiResponse<object>.NotContent("Villa deleted successfully"));
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<object>.Error(StatusCodes.Status500InternalServerError, $"Error occurred while deleting the villa", ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
            }
        }
    }
}

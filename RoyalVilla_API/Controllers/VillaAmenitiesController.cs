using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoyalVilla_API.Data;
using RoyalVilla_API.Models;
using RoyalVilla.DTO;

namespace RoyalVilla_API.Controllers
{
    [Route("api/villa-amenities")]
    [ApiController]
    public class VillaAmenitiesController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;

        public VillaAmenitiesController(ApplicationDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        [HttpGet]
        //[Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<VillaAmenitiesDTO>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<VillaAmenitiesDTO>>>> GetVillaAmenities()
        {
            var villaAmenities = await _db.VillaAmenities.ToListAsync();
            var dtoResponseVilla = _mapper.Map<List<VillaAmenitiesDTO>>(villaAmenities);
            return Ok(ApiResponse<IEnumerable<VillaAmenitiesDTO>>.Ok(dtoResponseVilla, "Records retrieved successfully"));
        }

        [HttpGet("{id:int}")]
        //[AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<VillaAmenitiesDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<VillaAmenitiesDTO>>> GetVillaAmenitiesById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return NotFound(ApiResponse<object>.NotFound("Invalid ID. ID must be greater than zero."));
                }

                var villa = await _db.VillaAmenities.FirstOrDefaultAsync(v => v.Id == id);
                if (villa == null)
                {
                    return NotFound(ApiResponse<object>.NotFound($"Villa Amenities with ID {id} not found."));
                }
                else
                {
                    return Ok(ApiResponse<VillaAmenitiesDTO>.Ok(_mapper.Map<VillaAmenitiesDTO>(villa), "Records retrieved successfully"));
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error occurred while retrieving villa with ID {id} : {ex.Message}");
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<VillaAmenitiesDTO>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<VillaAmenitiesDTO>>> CreateVillaAmenities(VillaAmenitiesCreateDTO villaAmenitiesCreateDTO)
        {
            try
            {
                if (villaAmenitiesCreateDTO == null)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("villa Amenities data is required."));
                }

                var villaExists = await _db.Villa.FirstOrDefaultAsync(v => v.Id == villaAmenitiesCreateDTO.VillaId);
                if (villaExists == null)
                {
                    return Conflict(ApiResponse<object>.Conflict($"Villa with ID '{villaAmenitiesCreateDTO.VillaId}' does not exists."));
                }
                VillaAmenities _villaAmenities = _mapper.Map<VillaAmenities>(villaAmenitiesCreateDTO);
                _villaAmenities.CreatedDate = DateTime.UtcNow;
                await _db.VillaAmenities.AddAsync(_villaAmenities);
                await _db.SaveChangesAsync();
                var response = ApiResponse<VillaAmenitiesDTO>.CreatedAt(_mapper.Map<VillaAmenitiesDTO>(_villaAmenities), "Villa Amenities created successfully");
                return CreatedAtAction(nameof(CreateVillaAmenities), new { id = _villaAmenities.Id }, response);

                var dtoResponseVilla = _mapper.Map<VillaAmenitiesDTO>(_villaAmenities);
                return CreatedAtAction(nameof(GetVillaAmenitiesById), new { id = _villaAmenities.Id }, ApiResponse<VillaAmenitiesDTO>.CreatedAt(dtoResponseVilla, "Villa Amenities created successfully"));
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<object>.Error(StatusCodes.Status500InternalServerError, $"Error occurred while creating the villa Amenities", ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
            }
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<VillaAmenitiesDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<VillaAmenitiesDTO>>> UpdateVillaAmenities(int id, VillaAmenitiesUpdateDTO villaAmenitiesUpdateDTO)
        {
            try
            {
                if (villaAmenitiesUpdateDTO == null)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("villa Amenities data is required."));
                }
                if (id != villaAmenitiesUpdateDTO.Id)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Villa Amenities ID in URL does not match Villa ID in request body"));
                }
                var villaExists = await _db.Villa.FirstOrDefaultAsync(v => v.Id == villaAmenitiesUpdateDTO.VillaId);
                if (villaExists == null)
                {
                    return Conflict(ApiResponse<object>.Conflict($"Villa with ID '{villaAmenitiesUpdateDTO.VillaId}' does not exists."));
                }

                var existingVillaAmenities = await _db.VillaAmenities.FirstOrDefaultAsync(v => v.Id == id);
                if (existingVillaAmenities == null)
                {
                    return NotFound(ApiResponse<object>.NotFound($"Villa Amenities with ID {id} not found."));
                }

                _mapper.Map(villaAmenitiesUpdateDTO, existingVillaAmenities);
                existingVillaAmenities.UpdatedDate = DateTime.UtcNow;
                await _db.SaveChangesAsync();
                var responseDto = _mapper.Map<VillaAmenitiesDTO>(existingVillaAmenities);
                return Ok(ApiResponse<VillaAmenitiesDTO>.Ok(responseDto, "Villa Amenities updated successfully"));
                //return Ok(ApiResponse<VillaAmenitiesDTO>.Ok(_mapper.Map<VillaAmenitiesDTO>(villaAmenitiesUpdateDTO), "Villa Amenities updated successfully"));
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<object>.Error(StatusCodes.Status500InternalServerError, $"Error occurred while updating the villa Amenities", ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
            }
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<object>>> DeleteVillaAmenities(int id)
        {
            try
            {
                var existingVillaAmenities = await _db.VillaAmenities.FirstOrDefaultAsync(v => v.Id == id);
                if (existingVillaAmenities == null)
                {
                    return NotFound(ApiResponse<object>.NotFound($"Villa Amenities with ID {id} not found."));
                }
                _db.VillaAmenities.Remove(existingVillaAmenities);
                await _db.SaveChangesAsync();
                return Ok(ApiResponse<object>.NotContent("Villa Amenities deleted successfully"));
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<object>.Error(StatusCodes.Status500InternalServerError, $"Error occurred while deleting the villa Amenities", ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
            }
        }
    }
}

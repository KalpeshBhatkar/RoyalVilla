using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoyalVilla.DTO;
using RoyalVilla_API.Data;
using RoyalVilla_API.Models;

namespace RoyalVilla_API.Controllers.v2
{
    [Route("api/v{version:apiVersion}/villa")]
    [ApiVersion("2.0")]
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
        public async Task<ActionResult<string>> GetVillas()
        {
            return "This is V2";
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<string>> GetVillaById(int id)
        {
            return "This is V2 " + id;
        }
    }
}

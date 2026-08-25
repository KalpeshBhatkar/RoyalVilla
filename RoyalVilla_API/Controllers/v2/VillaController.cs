using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoyalVilla_API.Data;
using RoyalVilla_API.Models;
using RoyalVilla.DTO;

namespace RoyalVilla_API.Controllers.v2
{
    [Route("api/v2/villa")]
    [ApiExplorerSettings(GroupName = "v2")]
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

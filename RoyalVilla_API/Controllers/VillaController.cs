using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace RoyalVilla_API.Controllers
{
    [Route("api/villa")]
    [ApiController]
    public class VillaController : ControllerBase
    {
        //[HttpGet]
        //public string GetVillas()
        //{
        //    return "Get All Villas";
        //}

        [HttpGet("{id:int}")]
        public string GetVillasById(int id)
        {
            return "Get All Villas - " + id.ToString();
        }

        //[HttpGet("{id:int}/{name}")]
        //public string GetVillasById([FromRoute] int id,[FromRoute] string name)
        //{
        //    return "Get All Villas - " + id.ToString() + " : " + name;
        //}

        //[HttpGet]
        //public string GetVillasById([FromQuery] int id, [FromQuery] string name)
        //{
        //    return "Get All Villas - " + id.ToString() + " : " + name;
        //}

        [HttpGet()]
        public string GetVillasById([FromQuery] int id, [FromHeader] string name)
        {
            return "Get All Villas - " + id.ToString() + " : " + name;
        }
    }
}

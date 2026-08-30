using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoyalVilla.DTO;
using RoyalVillaWeb.Services.IServices;

namespace RoyalVillaWeb.Controllers
{
    public class VillaController : Controller
    {
        private readonly IVillaService _villaService;
        private readonly IMapper _mapper;
        public VillaController(IVillaService villaService, IMapper mapper)
        {
            _villaService = villaService;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index()
        {
            List<VillaDTO> villaList = new();
            try
            {
                var response = await _villaService.GetAllAsync<ApiResponse<List<VillaDTO>>>();
                if (response != null && response.Success && response.Data != null)
                {
                    villaList = response.Data;
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = $"An error occurred: {ex.Message}";
            }
            return View(villaList);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateVillaDTO createVillaDTO)
        {
            if (!ModelState.IsValid) { return View(createVillaDTO); }

            try
            {
                var response = await _villaService.CreateAsync<ApiResponse<VillaDTO>>(createVillaDTO);
                if (response != null && response.Success && response.Data != null)
                {
                    TempData["success"] = "Villa created successfully";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = $"An error occurred: {ex.Message}";
            }
            return View(createVillaDTO);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
            {
                TempData["error"] = $"Invalid villa ID";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var response = await _villaService.GetAsync<ApiResponse<VillaDTO>>(id);
                if (response != null && response.Success && response.Data != null)
                {
                    View(response.Data);
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = $"An error occurred: {ex.Message}";
            }
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(VillaDTO villaDTO)
        {
            try
            {
                var response = await _villaService.DeleteAsync<ApiResponse<object>>(villaDTO.Id);
                if (response != null && response.Success && response.Data != null)
                {
                    TempData["success"] = "Villa deleted successfully";
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = $"An error occurred: {ex.Message}";
            }
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            if (id <= 0)
            {
                TempData["error"] = $"Invalid villa ID";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var response = await _villaService.GetAsync<ApiResponse<VillaDTO>>(id);
                if (response != null && response.Success && response.Data != null)
                {
                    View(_mapper.Map<UpdateVillaDTO>(response.Data));
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = $"An error occurred: {ex.Message}";
            }
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UpdateVillaDTO villaUpdateDTO)
        {
            try
            {
                var response = await _villaService.UpdateAsync<ApiResponse<object>>(villaUpdateDTO);
                if (response != null && response.Success && response.Data != null)
                {
                    TempData["success"] = "Villa updated successfully";
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = $"An error occurred: {ex.Message}";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}

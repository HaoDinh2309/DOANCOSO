using Microsoft.AspNetCore.Mvc;
using DOANCOSO.Models;
using DOANCOSO.Repositories;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;

namespace DOANCOSO.Controllers
{
    public class FlashCardController : Controller
    {
        private readonly IFlashCardRepository _flashCardRepository;

        public FlashCardController(IFlashCardRepository flashCardRepository)
        {
            _flashCardRepository = flashCardRepository;
        }

        public async Task<IActionResult> Index()
        {
            var flashCards = await _flashCardRepository.GetAllFlashCardsAsync();
            return View(flashCards);  // Trả về view với danh sách FlashCards
        }

        // Action để xem chi tiết một FlashCard
        public async Task<IActionResult> Details(int id)
        {
            var flashCard = await _flashCardRepository.GetFlashCardByIdAsync(id);
            if (flashCard == null)
            {
                return NotFound();  // Nếu không tìm thấy FlashCard, trả về lỗi 404
            }
            return View(flashCard);  // Trả về view với FlashCard chi tiết
        }

        // Action để hiển thị form tạo mới FlashCard
        [HttpGet]
        public IActionResult Create()
        {
            return View();  // Trả về view tạo mới
        }

        // Action để thêm FlashCard mới
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FlashCard flashCard, IFormFile imageFile)
        {
            if (ModelState.IsValid)
            {
                if (imageFile != null)
                {
                    // Đảm bảo thư mục tồn tại
                    var imagesPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                    if (!Directory.Exists(imagesPath))
                    {
                        Directory.CreateDirectory(imagesPath);
                    }

                    // Lưu ảnh vào thư mục wwwroot/images
                    var filePath = Path.Combine(imagesPath, imageFile.FileName);
                    // Lưu file vào thư mục
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }
                    // Lưu đường dẫn ảnh vào FlashCard
                    flashCard.ImageUrl = "/images/" + imageFile.FileName;  // Lưu đường dẫn ảnh
                }
                // Thêm FlashCard vào cơ sở dữ liệu
                await _flashCardRepository.AddFlashCardAsync(flashCard);
                return RedirectToAction(nameof(Index));  // Quay lại danh sách FlashCard
            }
            return View(flashCard);  // Nếu có lỗi, trả về form
        }

        // Action để hiển thị form chỉnh sửa FlashCard
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var flashCard = await _flashCardRepository.GetFlashCardByIdAsync(id);
            if (flashCard == null)
            {
                return NotFound();
            }
            return View(flashCard);
        }

        // Action để cập nhật FlashCard
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, FlashCard flashCard, IFormFile imageFile)
        {
            if (id != flashCard.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Nếu có file ảnh mới, thì cập nhật đường dẫn ảnh
                if (imageFile != null)
                {
                    // Đảm bảo thư mục tồn tại
                    var imagesPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                    if (!Directory.Exists(imagesPath))
                    {
                        Directory.CreateDirectory(imagesPath);
                    }

                    var filePath = Path.Combine(imagesPath, imageFile.FileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }
                    // Cập nhật đường dẫn ảnh
                    flashCard.ImageUrl = "/images/" + imageFile.FileName;
                }
                // Không thì giữ nguyên đường dẫn ảnh cũ

                try
                {
                    await _flashCardRepository.UpdateFlashCardAsync(flashCard);
                }
                catch
                {
                    if (!await FlashCardExists(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(flashCard);
        }

        // Action để hiển thị trang xác nhận xóa
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var flashCard = await _flashCardRepository.GetFlashCardByIdAsync(id);
            if (flashCard == null)
            {
                return NotFound();
            }
            return View(flashCard);
        }

        // Action để xóa FlashCard
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _flashCardRepository.DeleteFlashCardAsync(id);
            return RedirectToAction(nameof(Index));
        }

        // Phương thức kiểm tra flashcard có tồn tại không
        private async Task<bool> FlashCardExists(int id)
        {
            var flashCard = await _flashCardRepository.GetFlashCardByIdAsync(id);
            return flashCard != null;
        }
    }
}
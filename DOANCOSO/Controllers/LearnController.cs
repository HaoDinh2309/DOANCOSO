using Microsoft.AspNetCore.Mvc;
using DOANCOSO.Models;
using DOANCOSO.Repositories;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace DOANCOSO.Controllers
{
    public class LearnController : Controller
    {
        private readonly IFlashCardRepository _flashCardRepository;

        public LearnController(IFlashCardRepository flashCardRepository)
        {
            _flashCardRepository = flashCardRepository;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> StartLearning()
        {
            // Reset điểm số
            HttpContext.Session.SetInt32("CorrectAnswers", 0);
            HttpContext.Session.SetInt32("IncorrectAnswers", 0);

            // Xóa TempData
            TempData.Clear();

            var flashCards = await _flashCardRepository.GetAllFlashCardsAsync();

            if (flashCards == null || !flashCards.Any())
            {
                return View("NoCards");
            }

            // Sắp xếp ngẫu nhiên
            var randomizedCards = flashCards.OrderBy(c => Guid.NewGuid()).ToList();

            // Mặc định bắt đầu với chế độ Definition
            HttpContext.Session.SetString("ViewMode", "Definition");
            HttpContext.Session.SetString("CardIds", string.Join(",", randomizedCards.Select(c => c.Id)));
            HttpContext.Session.SetInt32("CurrentIndex", 0);

            // Đặt lại ViewData
            ViewData["ShowAnswer"] = false;
            ViewData["IsCorrect"] = false;
            ViewData["UserAnswer"] = "";

            return View("Learn", randomizedCards.First());
        }

        [HttpGet]
        public async Task<IActionResult> ToggleViewMode(int id)
        {
            var flashCard = await _flashCardRepository.GetFlashCardByIdAsync(id);

            if (flashCard == null)
            {
                return NotFound();
            }

            // Lấy chế độ xem hiện tại
            var currentMode = HttpContext.Session.GetString("ViewMode") ?? "Definition";

            // Chuyển đổi chế độ xem
            var newMode = currentMode == "Definition" ? "Image" : "Definition";
            HttpContext.Session.SetString("ViewMode", newMode);

            // Giữ lại trạng thái hiển thị đáp án nếu có
            if (TempData["ShowAnswer"] != null)
            {
                ViewData["ShowAnswer"] = true;
                ViewData["IsCorrect"] = TempData["IsCorrect"];
                ViewData["UserAnswer"] = TempData["UserAnswer"];

                // Để duy trì giá trị sau khi đọc
                TempData.Keep("ShowAnswer");
                TempData.Keep("IsCorrect");
                TempData.Keep("UserAnswer");
            }

            return View("Learn", flashCard);
        }

        public async Task<IActionResult> NextCard()
        {
            // Xóa TempData từ thẻ trước
            TempData.Remove("ShowAnswer");
            TempData.Remove("IsCorrect");
            TempData.Remove("UserAnswer");

            // Đặt lại ViewData
            ViewData["ShowAnswer"] = false;
            ViewData["IsCorrect"] = false;
            ViewData["UserAnswer"] = "";

            var cardIdsString = HttpContext.Session.GetString("CardIds");
            var currentIndex = HttpContext.Session.GetInt32("CurrentIndex") ?? 0;

            if (string.IsNullOrEmpty(cardIdsString))
            {
                return RedirectToAction("Index");
            }

            var cardIds = cardIdsString.Split(',').Select(int.Parse).ToList();

            // Tăng chỉ số hiện tại
            currentIndex++;
            HttpContext.Session.SetInt32("CurrentIndex", currentIndex);

            // Kiểm tra nếu đã học hết tất cả flashcard
            if (currentIndex >= cardIds.Count)
            {
                return View("Completed");
            }

            // Lấy flashcard tiếp theo
            var nextCardId = cardIds[currentIndex];
            var nextCard = await _flashCardRepository.GetFlashCardByIdAsync(nextCardId);

            if (nextCard == null)
            {
                return NotFound();
            }

            return View("Learn", nextCard);
        }

        [HttpPost]
        public async Task<IActionResult> ShowAnswer(int id)
        {
            var flashCard = await _flashCardRepository.GetFlashCardByIdAsync(id);

            if (flashCard == null)
            {
                return NotFound();
            }

            ViewData["ShowAnswer"] = true;

            return View("Learn", flashCard);
        }

        // Action để kiểm tra đáp án người dùng nhập vào
        [HttpPost]
        public async Task<IActionResult> CheckAnswer(int id, string userAnswer)
        {
            var flashCard = await _flashCardRepository.GetFlashCardByIdAsync(id);

            if (flashCard == null)
            {
                return NotFound();
            }

            // So sánh đáp án của người dùng với đáp án đúng
            // Sử dụng StringComparison.OrdinalIgnoreCase để không phân biệt hoa thường
            bool isCorrect = flashCard.Word.Equals(userAnswer, StringComparison.OrdinalIgnoreCase);

            // Cập nhật điểm số trong session
            int correctAnswers = HttpContext.Session.GetInt32("CorrectAnswers") ?? 0;
            int incorrectAnswers = HttpContext.Session.GetInt32("IncorrectAnswers") ?? 0;

            if (isCorrect)
            {
                correctAnswers++;
                HttpContext.Session.SetInt32("CorrectAnswers", correctAnswers);
            }
            else
            {
                incorrectAnswers++;
                HttpContext.Session.SetInt32("IncorrectAnswers", incorrectAnswers);

                // Lưu ID của flashcard trả lời sai
                var incorrectCardIds = HttpContext.Session.GetString("IncorrectCardIds") ?? "";
                var idList = incorrectCardIds.Split(',').Where(i => !string.IsNullOrEmpty(i)).ToList();
                if (!idList.Contains(id.ToString()))
                {
                    idList.Add(id.ToString());
                }
                HttpContext.Session.SetString("IncorrectCardIds", string.Join(",", idList));
            }

            // Đánh dấu là đã hiển thị đáp án
            ViewData["ShowAnswer"] = true;
            ViewData["IsCorrect"] = isCorrect;
            ViewData["UserAnswer"] = userAnswer;

            // Lưu trạng thái vào TempData để sử dụng khi chuyển chế độ xem
            TempData["ShowAnswer"] = true;
            TempData["IsCorrect"] = isCorrect;
            TempData["UserAnswer"] = userAnswer;

            return View("Learn", flashCard);
        }

        public async Task<IActionResult> ReviewMistakes()
        {
            // Xóa TempData
            TempData.Clear();

            // Đặt lại ViewData
            ViewData["ShowAnswer"] = false;
            ViewData["IsCorrect"] = false;
            ViewData["UserAnswer"] = "";

            // Lấy danh sách ID các flashcard đã trả lời sai
            var incorrectCardIds = HttpContext.Session.GetString("IncorrectCardIds")?.Split(',').Where(id => !string.IsNullOrEmpty(id)).Select(int.Parse).ToList();

            if (incorrectCardIds == null || !incorrectCardIds.Any())
            {
                return View("NoMistakes");
            }

            // Lấy danh sách flashcard từ ID
            var incorrectCards = new List<FlashCard>();
            foreach (var id in incorrectCardIds)
            {
                var card = await _flashCardRepository.GetFlashCardByIdAsync(id);
                if (card != null)
                {
                    incorrectCards.Add(card);
                }
            }

            // Sắp xếp ngẫu nhiên
            var randomizedCards = incorrectCards.OrderBy(c => Guid.NewGuid()).ToList();

            // Mặc định bắt đầu với chế độ Definition
            HttpContext.Session.SetString("ViewMode", "Definition");
            HttpContext.Session.SetString("CardIds", string.Join(",", randomizedCards.Select(c => c.Id)));
            HttpContext.Session.SetInt32("CurrentIndex", 0);

            // Reset điểm số
            HttpContext.Session.SetInt32("CorrectAnswers", 0);
            HttpContext.Session.SetInt32("IncorrectAnswers", 0);

            if (randomizedCards.Any())
            {
                return View("Learn", randomizedCards.First());
            }

            return View("NoCards");
        }
    }
}
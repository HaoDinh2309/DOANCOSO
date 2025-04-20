using DOANCOSO.Models;
using Microsoft.EntityFrameworkCore;

namespace DOANCOSO.Repositories
{
    public class FlashCardRepository : IFlashCardRepository
    {
        private readonly ApplicationDbContext _context;

        public FlashCardRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<FlashCard>> GetAllFlashCardsAsync()
        {
            return await _context.FlashCards.ToListAsync();
        }

        public async Task<FlashCard> GetFlashCardByIdAsync(int id)
        {
            return await _context.FlashCards.FindAsync(id);
        }

        public async Task AddFlashCardAsync(FlashCard flashCard)
        {
            await _context.FlashCards.AddAsync(flashCard);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateFlashCardAsync(FlashCard flashCard)
        {
            _context.FlashCards.Update(flashCard);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteFlashCardAsync(int id)
        {
            var flashCard = await _context.FlashCards.FindAsync(id);
            if (flashCard != null)
            {
                _context.FlashCards.Remove(flashCard);
                await _context.SaveChangesAsync();
            }
        }
    }
}

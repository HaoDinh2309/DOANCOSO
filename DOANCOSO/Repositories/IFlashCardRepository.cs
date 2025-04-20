using DOANCOSO.Models;

namespace DOANCOSO.Repositories
{
    public interface IFlashCardRepository
    {
        Task<IEnumerable<FlashCard>> GetAllFlashCardsAsync();
        Task<FlashCard> GetFlashCardByIdAsync(int id);
        Task AddFlashCardAsync(FlashCard flashCard);
        Task UpdateFlashCardAsync(FlashCard flashCard);
        Task DeleteFlashCardAsync(int id);
    }
}

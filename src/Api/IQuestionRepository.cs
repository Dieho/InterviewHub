using InterviewHub.Api.Contracts;
using InterviewHub.Domain;
using Microsoft.EntityFrameworkCore;

namespace InterviewHub.Data;

public interface IQuestionRepository
{
    Task<Question?> GetQuestion(Guid id);
    Task<List<Question>> GetQuestions();
    Task<Category?> GetCategory(Guid id);
    Task<List<Category>> GetCategories();
    Task AddQuestion(Question question);
    Task AddCategory(Category category);
}

public class QuestionRepository(AppDbContext context) : IQuestionRepository
{
    // Implement repository methods here, e.g., Add, Get, Update, Delete
    public async Task<Question?> GetQuestion(Guid id)
    {
        return await context.Questions
            .AsNoTracking() // read-only запит — не потрібен change tracking, і уникаємо fixup-циклу
            .Include(q => q.Category)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<List<Question>> GetQuestions()
    {
        return await context.Questions
            .AsNoTracking() // read-only запит — не потрібен change tracking, і уникаємо fixup-циклу
            .Include(q => q.Category)
            .ToListAsync();
    }

    public async Task<Category?> GetCategory(Guid id)
    {
        return await context.Categories
            .AsNoTracking() // read-only запит — не потрібен change tracking, і уникаємо fixup-циклу
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<List<Category>> GetCategories()
    {
        return await context.Categories
            .AsNoTracking() // read-only запит — не потрібен change tracking, і уникаємо fixup-циклу
            .ToListAsync();
    }

    public async Task AddQuestion(Question question)
    {
        context.Entry(question.Category).State = EntityState.Unchanged;

        await context.Questions.AddAsync(question);
        await context.SaveChangesAsync();
    }

    public async Task AddCategory(Category category)
    {
        await context.Categories.AddAsync(category);
        await context.SaveChangesAsync();
    }
}
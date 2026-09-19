using InterviewHub.Api.Contracts;
using InterviewHub.Domain;
using Microsoft.EntityFrameworkCore;

namespace InterviewHub.Data;

public interface IAuthRepository
{
    Task<User?> GetUserByEmail(string email);
    Task AddUser(User user);
}

public class AuthRepository(AppDbContext context) : IAuthRepository
{
    public async Task<User?> GetUserByEmail(string email)
    {
        return await context.Users
            .AsNoTracking() // read-only запит — не потрібен change tracking, і уникаємо fixup-циклу
            .FirstOrDefaultAsync(x=> x.Email == email);
    }

    public async Task AddUser(User user)
    {
        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();
    }
}
using Microsoft.AspNetCore.Identity;

namespace InterviewHub.Domain;

public enum Role
{
    User, Admin
}

public class User
{
    public Guid Id { get; private set; }
    public string Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public Role Role { get; private set; }
    
    private User() { } // для EF Core
    
    public User(string email, string password, Role role = Role.User)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email не може бути пустим", nameof(email));
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password не може бути пустим", nameof(password));
        Id = Guid.NewGuid();
        Email = email.Trim().ToLowerInvariant();
        SetPasswordHash(password);
        Role = role;
    }

    public void SetPasswordHash(string password)
    {
        var passwordHasher = new PasswordHasher<User>(); //passwordHasher.VerifyHashedPassword to check
        PasswordHash = passwordHasher.HashPassword(this, password);
    }
}
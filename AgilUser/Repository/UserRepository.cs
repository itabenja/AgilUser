using UsersApi.Models;

namespace UsersApi.Repositories;

public class UserRepository : IUserRepository
{
    // In-memory storage (acceptabel ifølge assignment)
    private static readonly List<User> _users = new();

    public Task<User?> GetByEmailAsync(string email)
    {
        var user = _users
            .FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));

        return Task.FromResult(user);
    }

    public Task<User?> GetByIdAsync(Guid id)
    {
        var user = _users.FirstOrDefault(u => u.Id == id);
        return Task.FromResult(user);
    }

    public Task<List<User>> GetAllAsync()
    {
        return Task.FromResult(_users.ToList());
    }

    public Task AddAsync(User user)
    {
        _users.Add(user);
        return Task.CompletedTask;
    }
}
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Enums;
using TaskManagement.Domain.Interfaces;

namespace TaskManagement.Infrastructure.Data
{
    public class DbInitializer
    {
        private readonly IUserRepository _userRepository;
        private readonly ITaskRepository _taskRepository;

        public DbInitializer(IUserRepository userRepository, ITaskRepository taskRepository)
        {
            _userRepository = userRepository;
            _taskRepository = taskRepository;
        }

        public async Task SeedAsync()
        {
            var existingUser = await _userRepository.GetByUsernameAsync("user_alpha");
            if (existingUser != null)
            {
                return;
            }

            string genericPasswordHash = GenerateSeedPasswordHash("Password123!");

            var fakeUsers = new List<User>
            {
                new User { Id = Guid.NewGuid(), Username = "user_alpha", PasswordHash = genericPasswordHash, CreatedAt = DateTime.UtcNow.AddDays(-5) },
                new User { Id = Guid.NewGuid(), Username = "user_beta", PasswordHash = genericPasswordHash, CreatedAt = DateTime.UtcNow.AddDays(-3) },
                new User { Id = Guid.NewGuid(), Username = "user_gamma", PasswordHash = genericPasswordHash, CreatedAt = DateTime.UtcNow.AddDays(-1) }
            };

            // 3. Procesar cada usuario y sembrar sus respectivas 3 tareas
            foreach (var user in fakeUsers)
            {
                await _userRepository.AddAsync(user);

                var tasksForUser = new List<TaskEntity>
                {
                    new TaskEntity
                    {
                        Id = Guid.NewGuid(),
                        UserId = user.Id,
                        Title = $"Strategic Planning - {user.Username}",
                        Description = "Define initial technical requirements and scope for the upcoming iteration.",
                        Status = TodoStatus.Pending,
                        DueDate = DateTime.UtcNow.AddDays(2)
                    },
                    new TaskEntity
                    {
                        Id = Guid.NewGuid(),
                        UserId = user.Id,
                        Title = $"Core API Development - {user.Username}",
                        Description = "Write native CRUD endpoints in .NET 8 without ORM intermediates.",
                        Status = TodoStatus.InProgress,
                        DueDate = DateTime.UtcNow.AddDays(5)
                    },
                    new TaskEntity
                    {
                        Id = Guid.NewGuid(),
                        UserId = user.Id,
                        Title = $"Quality Assurance - {user.Username}",
                        Description = "Verify the system stability by executing the full unit test suite with Mongo2Go.",
                        Status = TodoStatus.Completed,
                        DueDate = DateTime.UtcNow.AddDays(-1)
                    }
                };

                foreach (var task in tasksForUser)
                {
                    await _taskRepository.AddAsync(task);
                }
            }
        }

        private string GenerateSeedPasswordHash(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 11);
        }
    }
}

using MongoDB.Driver;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Interfaces;
using TaskManagement.Infrastructure.Documents;

namespace TaskManagement.Infrastructure.Repositories
{
    public class MongoUserRepository : IUserRepository
    {
        private readonly IMongoCollection<UserDocument> _usersCollection;

        public MongoUserRepository(IMongoDatabase database)
        {
            if (database == null)
            {
                throw new ArgumentNullException(nameof(database), "La instancia de la base de datos de MongoDB no puede ser nula.");
            }
            _usersCollection = database.GetCollection<UserDocument>("Users");
        }

        public async Task<User?> GetByIdAsync(Guid id)
        {
            var userDocument = await _usersCollection.Find(u => u.Id == id).FirstOrDefaultAsync();

            if (userDocument == null) return null;

            return new User
            {
                Id = userDocument.Id,
                Username = userDocument.Username,
                PasswordHash = userDocument.PasswordHash,
                CreatedAt = userDocument.CreatedAt
            };
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username)) return null;

            var filter = Builders<UserDocument>.Filter.Regex(
                u => u.Username,
                new MongoDB.Bson.BsonRegularExpression($"^{username}$", "i")
            );

            var userDocument = await _usersCollection.Find(filter).FirstOrDefaultAsync();

            if (userDocument == null) return null;

            return new User
            {
                Id = userDocument.Id,
                Username = userDocument.Username,
                PasswordHash = userDocument.PasswordHash,
                CreatedAt = userDocument.CreatedAt
            };
        }

        public async Task AddAsync(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user), "No se puede registrar un usuario nulo en el contenedor.");
            }

            if (user.Id == Guid.Empty)
            {
                user.Id = Guid.NewGuid();
            }

            if (user.CreatedAt == default)
            {
                user.CreatedAt = DateTime.UtcNow;
            }

            var userDocument = new UserDocument
            {
                Id = user.Id,
                Username = user.Username,
                PasswordHash = user.PasswordHash,
                CreatedAt = user.CreatedAt
            };

            await _usersCollection.InsertOneAsync(userDocument);
        }
    }
}

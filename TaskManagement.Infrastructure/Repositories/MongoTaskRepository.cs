using MongoDB.Driver;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Enums;
using TaskManagement.Domain.Interfaces;
using TaskManagement.Infrastructure.Documents;

namespace TaskManagement.Infrastructure.Repositories
{
    public class MongoTaskRepository : ITaskRepository
    {
        private readonly IMongoCollection<TaskDocument> _tasks;

        public MongoTaskRepository(IMongoDatabase database)
        {
            if (database == null)
            {
                throw new ArgumentNullException(nameof(database), "La instancia de la base de datos de MongoDB no puede ser nula.");
            }

            _tasks = database.GetCollection<TaskDocument>("Tasks");
        }

        public async Task<TaskEntity?> GetByIdAsync(Guid id)
        {
            var mongoTask = await _tasks.Find(t => t.Id == id).FirstOrDefaultAsync();

            if (mongoTask == null) return null;

            return new TaskEntity
            {
                Id = mongoTask.Id,
                Title = mongoTask.Title,
                Description = mongoTask.Description,
                Status = (TodoStatus)mongoTask.Status,
                DueDate = mongoTask.DueDate,
                UserId = mongoTask.UserId
            };
        }

        public async Task<IEnumerable<TaskEntity>> GetByUserIdAsync(Guid userId)
        {
            var mongoTasks = await _tasks.Find(t => t.UserId == userId).ToListAsync();

            return mongoTasks.Select(t => new TaskEntity
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                Status = (TodoStatus)t.Status,
                DueDate = t.DueDate,
                UserId = t.UserId
            });
        }

        public async Task AddAsync(TaskEntity task)
        {
            if (task.Id == Guid.Empty)
            {
                task.Id = Guid.NewGuid();
            }

            var mongoTask = new TaskDocument
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                Status = (int)task.Status,
                DueDate = task.DueDate,
                UserId = task.UserId
            };

            await _tasks.InsertOneAsync(mongoTask);
        }

        public async Task UpdateAsync(TaskEntity task)
        {
            var mongoTask = new TaskDocument
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                Status = (int)task.Status,
                DueDate = task.DueDate,
                UserId = task.UserId
            };

            var result = await _tasks.ReplaceOneAsync(t => t.Id == task.Id, mongoTask);

            if (result.MatchedCount == 0)
            {
                throw new KeyNotFoundException($"Task with ID {task.Id} was not found in the storage container.");
            }
        }

        public async Task DeleteAsync(Guid id)
        {
            var result = await _tasks.DeleteOneAsync(t => t.Id == id);

            if (result.DeletedCount == 0)
            {
                throw new KeyNotFoundException($"Task with ID {id} could not be deleted because it does not exist.");
            }
        }
    }
}
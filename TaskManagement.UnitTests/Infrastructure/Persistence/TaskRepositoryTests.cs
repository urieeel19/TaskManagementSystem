using Mongo2Go;
using MongoDB.Driver;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Enums;
using TaskManagement.Domain.Interfaces;
using TaskManagement.Infrastructure.Documents;
using TaskManagement.Infrastructure.Repositories;

namespace TaskManagement.UnitTests.Infrastructure.Persistence
{
    public class TaskRepositoryTests : IDisposable
    {
        private readonly MongoDbRunner _mongoRunner;
        private readonly IMongoDatabase _database;
        private readonly ITaskRepository _repository;
        private readonly IMongoCollection<TaskDocument> _rawCollection;

        public TaskRepositoryTests()
        {
            _mongoRunner = MongoDbRunner.Start();

            var client = new MongoClient(_mongoRunner.ConnectionString);
            _database = client.GetDatabase("TestTaskManagementDb");

            _repository = new MongoTaskRepository(_database);

            _rawCollection = _database.GetCollection<TaskDocument>("Tasks");
        }

        public void Dispose()
        {
            _mongoRunner.Dispose();
        }

        [Fact]
        public void Constructor_Should_Throw_ArgumentNullException_When_Database_Is_Null()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => new MongoTaskRepository(null!));
            Assert.Equal("database", exception.ParamName);
        }

        [Fact]
        public async Task AddAsync_Should_Write_Task_Correctly_To_Storage_With_Numeric_Enums()
        {
            // Arrange
            var task = new TaskEntity
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Title = "Test Numeric Enum Task",
                Description = "Verifying that enums are stored as integer digits",
                Status = TodoStatus.InProgress,
                DueDate = DateTime.UtcNow.AddDays(2)
            };

            // Act
            await _repository.AddAsync(task);

            // Assert - Comprobación mediante la API del Repositorio
            var persistedTask = await _repository.GetByIdAsync(task.Id);
            Assert.NotNull(persistedTask);
            Assert.Equal(task.Id, persistedTask.Id);
            Assert.Equal(TodoStatus.InProgress, persistedTask.Status);

            // Assert - Comprobación estricta de caja blanca (Inspeccionando el documento crudo en MongoDB BSON)
            var rawDocument = await _rawCollection.Find(t => t.Id == task.Id).FirstOrDefaultAsync();
            Assert.NotNull(rawDocument);

            // Verificamos que el Enum se guardó como entero puro (int) cumpliendo la regla técnica de la base de datos
            int expectedNumericValue = (int)TodoStatus.InProgress;
            Assert.Equal(expectedNumericValue, rawDocument.Status);
        }

        [Fact]
        public async Task GetByUserIdAsync_Should_Return_Only_Matching_User_Tasks()
        {
            // Arrange
            var targetUserId = Guid.NewGuid();
            var foreignUserId = Guid.NewGuid();

            var task1 = new TaskEntity { Id = Guid.NewGuid(), UserId = targetUserId, Title = "Task 1", Status = TodoStatus.Pending, DueDate = DateTime.UtcNow };
            var task2 = new TaskEntity { Id = Guid.NewGuid(), UserId = targetUserId, Title = "Task 2", Status = TodoStatus.Pending, DueDate = DateTime.UtcNow };
            var foreignTask = new TaskEntity { Id = Guid.NewGuid(), UserId = foreignUserId, Title = "Foreign", Status = TodoStatus.Pending, DueDate = DateTime.UtcNow };

            await _repository.AddAsync(task1);
            await _repository.AddAsync(task2);
            await _repository.AddAsync(foreignTask);

            // Act
            var result = await _repository.GetByUserIdAsync(targetUserId);

            // Assert
            var resultList = result.ToList();
            Assert.Equal(2, resultList.Count);
            Assert.All(resultList, t => Assert.Equal(targetUserId, t.UserId));
            Assert.Contains(resultList, t => t.Title == "Task 1");
            Assert.Contains(resultList, t => t.Title == "Task 2");
            Assert.DoesNotContain(resultList, t => t.Title == "Foreign");
        }

        [Fact]
        public async Task UpdateAsync_Should_Modify_Existing_Task_Payload_Data()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var originalTask = new TaskEntity
            {
                Id = taskId,
                UserId = Guid.NewGuid(),
                Title = "Old Title",
                Description = "Old Description",
                Status = TodoStatus.Pending,
                DueDate = DateTime.UtcNow
            };

            await _repository.AddAsync(originalTask);

            // Modificamos las propiedades en memoria
            originalTask.Title = "Updated Title";
            originalTask.Status = TodoStatus.Completed;

            // Act
            await _repository.UpdateAsync(originalTask);

            // Assert
            var updatedTask = await _repository.GetByIdAsync(taskId);
            Assert.NotNull(updatedTask);
            Assert.Equal("Updated Title", updatedTask.Title);
            Assert.Equal(TodoStatus.Completed, updatedTask.Status);
        }

        [Fact]
        public async Task UpdateAsync_Should_Throw_KeyNotFoundException_When_Task_DoesNotExist()
        {
            // Arrange
            var nonExistentTask = new TaskEntity
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Title = "Ghost Task"
            };

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _repository.UpdateAsync(nonExistentTask));
        }

        [Fact]
        public async Task DeleteAsync_Should_Remove_Record_From_Storage_Container()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var task = new TaskEntity { Id = taskId, UserId = Guid.NewGuid(), Title = "To Delete", Status = TodoStatus.Pending, DueDate = DateTime.UtcNow };
            await _repository.AddAsync(task);

            // Act
            await _repository.DeleteAsync(taskId);

            // Assert
            var deletedTask = await _repository.GetByIdAsync(taskId);
            Assert.Null(deletedTask);

            // Verificamos que el contenedor crudo esté vacío
            var totalDocuments = await _rawCollection.CountDocumentsAsync(Builders<TaskDocument>.Filter.Empty);
            Assert.Equal(0, totalDocuments);
        }

        [Fact]
        public async Task DeleteAsync_Should_Throw_KeyNotFoundException_When_Id_DoesNotExist()
        {
            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _repository.DeleteAsync(Guid.NewGuid()));
        }
    }
}

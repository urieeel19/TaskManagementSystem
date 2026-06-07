using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Domain.Interfaces
{
    public interface ITaskRepository
    {
        Task<TaskEntity?> GetByIdAsync(Guid id);
        Task<IEnumerable<TaskEntity>> GetByUserIdAsync(Guid userId);
        Task AddAsync(TaskEntity task);
        Task UpdateAsync(TaskEntity task);
        Task DeleteAsync(Guid id);
    }
}

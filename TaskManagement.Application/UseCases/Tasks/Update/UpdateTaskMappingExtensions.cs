using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Enums;

namespace TaskManagement.Application.UseCases.Tasks.Update
{
    public static class UpdateTaskMappingExtensions
    {
        public static TaskEntity ToEntity(this UpdateTaskCommand request, Guid id, Guid userId)
        {
            return new TaskEntity
            {
                Id = id,
                UserId = userId,
                Title = request.Title,
                Description = request.Description,
                DueDate = request.DueDate,
                Status = (TodoStatus)request.Status
            };
        }
        public static UpdateTaskResponse ToResponse(this TaskEntity entity)
        {
            return new UpdateTaskResponse(
                entity.Id,
                entity.Title,
                entity.Description,
                (int)entity.Status,
                entity.DueDate
            );
        }
    }
}

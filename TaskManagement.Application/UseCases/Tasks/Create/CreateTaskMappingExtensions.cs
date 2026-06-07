using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Enums;

namespace TaskManagement.Application.UseCases.Tasks.Create
{
    public static class CreateTaskMappingExtensions
    {
        public static TaskEntity ToEntity(this CreateTaskCommand request, Guid userId)
        {
            return new TaskEntity
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Title = request.Title,
                Description = request.Description,
                DueDate = request.DueDate,
                Status = (TodoStatus)request.Status
            };
        }
        public static CreateTaskResponse ToCreateResponse(this TaskEntity entity)
        {
            return new CreateTaskResponse(
                entity.Id,
                entity.Title,
                entity.Description,                
                (int)entity.Status,
                entity.DueDate
            );
        }
    }
}

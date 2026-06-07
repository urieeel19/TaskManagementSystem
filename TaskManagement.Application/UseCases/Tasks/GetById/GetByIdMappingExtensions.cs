using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Application.UseCases.Tasks.GetById
{
    public static class GetByIdMappingExtensions
    {
        /// <summary>
        /// Maps a single Task domain entity into the clean Output DTO for the GetById query.
        /// </summary>
        public static GetByIdResponse ToResponse(this TaskEntity task)
        {
            return new GetByIdResponse(
                task.Id,
                task.Title,
                task.Description,
                (int)task.Status,
                task.DueDate
            );
        }
    }
}

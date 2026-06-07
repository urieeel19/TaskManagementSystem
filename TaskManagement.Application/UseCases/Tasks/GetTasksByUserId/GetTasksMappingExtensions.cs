using TaskManagement.Domain.Entities;

namespace TaskManagement.Application.UseCases.Tasks.GetTasksByUserId
{
    public static class GetTasksMappingExtensions
    {
        public static GetTaskResponse ToResponse(this TaskEntity task)
        {
            return new GetTaskResponse(
                task.Id,
                task.Title,
                task.Description,
                (int)task.Status,
                task.DueDate
            );
        }

        public static IEnumerable<GetTaskResponse> ToResponseList(this IEnumerable<TaskEntity> tasks)
        {
            return tasks.Select(task => task.ToResponse());
        }
    }
}

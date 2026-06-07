namespace TaskManagement.Application.UseCases.Tasks.Create
{
    public record CreateTaskCommand(string Title, string Description, int Status, DateTime DueDate);
    public record CreateTaskResponse(Guid Id, string Title, string Description, int Status, DateTime DueDate);
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManagement.Application.UseCases.Auth.Register
{
    public record RegisterUserCommand(string Username,string Password,string ConfirmPassword);
    public record RegisterUserResponse;
}

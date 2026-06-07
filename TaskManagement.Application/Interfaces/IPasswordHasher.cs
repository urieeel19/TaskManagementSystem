using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManagement.Application.Interfaces
{
    public interface IPasswordHasher
    {
        /// <summary>
        /// Hashes a plain text password using a secure cryptographic algorithm
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        string HashPassword(string password);
        /// <summary>
        /// Verifies if a plain text password matches an existing secure hash
        /// </summary>
        /// <param name="password"></param>
        /// <param name="passwordHash"></param>
        /// <returns></returns>         
        bool VerifyPassword(string password, string passwordHash);
    }
}

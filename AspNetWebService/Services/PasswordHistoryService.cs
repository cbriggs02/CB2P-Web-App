using AspNetWebService.Data;
using AspNetWebService.Interfaces;
using AspNetWebService.Models.Entities;
using AspNetWebService.Models.Request_Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AspNetWebService.Services
{
    /// <summary>
    ///     Service responsible for interacting with passwordHistory-related data and business logic.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    /// </remarks>
    public class PasswordHistoryService : IPasswordHistoryService
    {
        private readonly ApplicationDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;

        /// <summary>
        ///     Constructor for the <see cref="PasswordHistoryService"/> class.
        /// </summary>
        /// <param name="context">
        ///     The application database context used for accessing password history data.
        /// </param>
        /// <param name="passwordHasher">
        ///     This is used for comparing hashed passwords and ensuring password security.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown if the <paramref name="context"/> parameter is null.
        /// </exception>
        public PasswordHistoryService(ApplicationDbContext context, IPasswordHasher<User> passwordHasher)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        }


        /// <summary>
        ///     Records the current password hash of the specified user in the password history.
        ///     This method is called whenever a user successfully changes their password, ensuring
        ///     a record of the old password is kept for security and compliance purposes.
        /// </summary>
        /// <param name="request">
        ///     The expected request object containing the password and user id whose password is being record.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation of saving the password history
        ///     record to the database.
        /// </returns>
        public async Task AddPasswordHistory(StorePasswordHistoryRequest request)
        {
            var passwordHistory = new PasswordHistory
            {
                UserId = request.UserId,
                PasswordHash = request.PasswordHash,
                CreatedDate = DateTime.UtcNow
            };

            _context.PasswordHistories.Add(passwordHistory);

            await RemoveOldPasswordHistories(passwordHistory.UserId);
            await _context.SaveChangesAsync();
        }


        /// <summary>
        ///     Checks a user's history for passwords that may be re-used.
        /// </summary>
        /// <param name="request">
        ///     A model object that contains required data for checking a user's password history, including user ID and password.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation. The task result is a boolean value indicating whether the provided password hash is found in the user's password history.
        ///     - <c>true</c> if the password hash is found in the user's history, indicating a potential re-use.
        ///     - <c>false</c> if the password hash is not found in the user's history, indicating that the password is not a re-used one.
        /// </returns>
        public async Task<bool> FindPasswordHash(SearchPasswordHistoryRequest request)
        {
            var passwordHistories = await _context.PasswordHistories
                .Where(x => x.UserId == request.UserId)
                .Select(x => x.PasswordHash)
                .AsNoTracking()
                .ToListAsync();

            // Create a dummy user to use for verification
            var dummyUser = new User { Id = request.UserId };

            // Compare the plain text password against each stored hash
            foreach (var storedHash in passwordHistories)
            {
                if (_passwordHasher.VerifyHashedPassword(dummyUser, storedHash, request.Password) == PasswordVerificationResult.Success)
                {
                    return true;
                }
            }
            return false;
        }


        /// <summary>
        ///     Removes old password entries for a user, keeping only the most recent 5.
        /// </summary>
        /// <param name="id">
        ///     The ID of the user whose password history is being cleaned up.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation of removing old password histories.
        /// </returns>
        private async Task RemoveOldPasswordHistories(string id)
        {
            var totalCount = await _context.PasswordHistories.CountAsync(x => x.UserId == id);
            var recordsToTake = Math.Max(totalCount - 5, 0);

            var oldPasswordHistories = await _context.PasswordHistories
                .Where(x => x.UserId == id)
                .OrderBy(x => x.CreatedDate)
                .Take(recordsToTake)
                .Select(x => x.Id)
                .AsNoTracking()
                .ToListAsync();

            if (oldPasswordHistories.Count > 0)
            {
                _context.PasswordHistories
                    .RemoveRange(_context.PasswordHistories
                    .Where(x => oldPasswordHistories.Contains(x.Id)));
            }
        }
    }
}

using AspNetWebService.Data;
using AspNetWebService.Interfaces;
using AspNetWebService.Models;
using AspNetWebService.Models.Request_Models;
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

        /// <summary>
        ///     Constructor for the <see cref="PasswordHistoryService"/> class.
        /// </summary>
        /// <param name="context">
        ///     The application database context used for accessing password history data.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown if the <paramref name="context"/> parameter is null.
        /// </exception>
        public PasswordHistoryService(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }


        /// <summary>
        ///     Records the current password hash of the specified user in the password history.
        ///     This method is called whenever a user successfully changes their password, ensuring
        ///     a record of the old password is kept for security and compliance purposes.
        /// </summary>
        /// <param name="request">
        ///     The expected request object containing the password and user id whos password is being record.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation of saving the password history
        ///     record to the database.
        /// </returns>
        public async Task AddPasswordHistory(PasswordHistoryRequest request)
        {
            var passwordHistory = new PasswordHistory
            {
                UserId = request.UserId,
                PasswordHash = request.PasswordHash,
                CreatedDate = DateTime.UtcNow
            };

            _context.PasswordHistories.Add(passwordHistory);

            // Clean up old password entries to keep only the most recent 5
            await RemoveOldPasswordHistories(request.UserId);

            await _context.SaveChangesAsync();
        }


        /// <summary>
        ///     Checks a user's history for passwords that may be re-used.
        /// </summary>
        /// <param name="request">
        ///     A model object that contains required data for checking a user's password history, including user ID and hashed password.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation. The task result is a boolean value indicating whether the provided password hash is found in the user's password history.
        ///     - <c>true</c> if the password hash is found in the user's history, indicating a potential re-use.
        ///     - <c>false</c> if the password hash is not found in the user's history, indicating that the password is not a re-used one.
        /// </returns>
        public async Task<bool> FindPasswordHash(PasswordHistoryRequest request)
        {
            var passwordHistory = await _context.PasswordHistories
                .Where(x => x.UserId == request.UserId)
                .Select(x => x.PasswordHash)
                .AsNoTracking()
                .ToListAsync();

            return passwordHistory.Contains(request.PasswordHash);
        }


        /// <summary>
        ///     Removes old password entries for a user, keeping only the most recent 5.
        /// </summary>
        /// <param name="userId">
        ///     The ID of the user whose password history is being cleaned up.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation of removing old password histories.
        /// </returns>
        public async Task RemoveOldPasswordHistories(string userId)
        {
            var oldPasswordHistories = await _context.PasswordHistories
                .Where(x => x.UserId == userId)
                .OrderBy(x => x.CreatedDate)
                .Take(await _context.PasswordHistories.CountAsync(x => x.UserId == userId) - 5)
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

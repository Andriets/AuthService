using System.Text.RegularExpressions;
using AuthService.Web.Core.Interfaces;
using AuthService.Web.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Web.Core.Services;

public partial class PasswordService(AppDbContext db) : IPasswordService
{
    private const int WorkFactor = 12;
    private const int HistoryDepth = 5;

    public string Hash(string plaintext) =>
        BCrypt.Net.BCrypt.HashPassword(plaintext, WorkFactor);

    public bool Verify(string plaintext, string hash) =>
        BCrypt.Net.BCrypt.Verify(plaintext, hash);

    public bool MeetsComplexity(string plaintext) =>
        plaintext.Length >= 8 &&
        UpperCaseRegex().IsMatch(plaintext) &&
        LowerCaseRegex().IsMatch(plaintext) &&
        DigitRegex().IsMatch(plaintext) &&
        SpecialCharRegex().IsMatch(plaintext);

    public async Task<bool> IsReusedAsync(string plaintext, Guid userId, CancellationToken cancellationToken = default)
    {
        var recentHashes = await db.UserPasswordHistory
            .Where(h => h.UserId == userId)
            .OrderByDescending(h => h.CreatedAt)
            .Take(HistoryDepth)
            .Select(h => h.PasswordHash)
            .ToListAsync(cancellationToken);

        return recentHashes.Any(hash => BCrypt.Net.BCrypt.Verify(plaintext, hash));
    }

    [GeneratedRegex("[A-Z]")]
    private static partial Regex UpperCaseRegex();

    [GeneratedRegex("[a-z]")]
    private static partial Regex LowerCaseRegex();

    [GeneratedRegex("[0-9]")]
    private static partial Regex DigitRegex();

    [GeneratedRegex(@"[^a-zA-Z0-9]")]
    private static partial Regex SpecialCharRegex();
}

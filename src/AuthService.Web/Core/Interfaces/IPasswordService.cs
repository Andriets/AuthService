namespace AuthService.Web.Core.Interfaces;

public interface IPasswordService
{
    string Hash(string plaintext);
    bool Verify(string plaintext, string hash);
    bool MeetsComplexity(string plaintext);
    Task<bool> IsReusedAsync(string plaintext, Guid userId, CancellationToken cancellationToken = default);
}

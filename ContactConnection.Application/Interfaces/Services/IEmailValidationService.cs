namespace ContactConnection.Application.Interfaces.Services;

public interface IEmailValidationService
{
    Task<EmailValidationResult> ValidateAsync(
        string email,
        bool checkARecord,
        bool checkMX,
        bool checkDisposable,
        CancellationToken ct = default);
}

public record EmailValidationResult(
    bool IsFormatValid,
    bool? DomainExists,   // null = not checked
    bool? MXExists,       // null = not checked
    bool? IsDisposable,   // null = not checked
    bool IsDeliverable    // format valid + MX exists + NOT disposable
);

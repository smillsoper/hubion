using System.Text.RegularExpressions;
using ContactConnection.Application.Interfaces.Services;
using DnsClient;

namespace ContactConnection.Infrastructure.FlowEngine.Services;

public partial class EmailValidationService(ILookupClient dns) : IEmailValidationService
{
    // Common disposable / throwaway email providers
    private static readonly HashSet<string> DisposableDomains = new(StringComparer.OrdinalIgnoreCase)
    {
        "mailinator.com", "guerrillamail.com", "guerrillamail.net", "guerrillamail.org",
        "guerrillamail.biz", "guerrillamail.de", "guerrillamail.info",
        "trashmail.com", "trashmail.net", "trashmail.org", "trashmail.at",
        "trashmail.io", "trashmail.me", "trashmail.xyz",
        "yopmail.com", "yopmail.fr", "yopmail.net",
        "tempmail.com", "temp-mail.org", "tempmail.net",
        "throwam.com", "throwam.net", "throwaway.email",
        "dispostable.com", "discard.email",
        "sharklasers.com", "guerrillamailblock.com", "spam4.me",
        "maildrop.cc", "fakeinbox.com", "fakeinbox.net",
        "getnada.com", "10minutemail.com", "10minutemail.net",
        "10minutemail.org", "minuteinbox.com",
        "mailnull.com", "spamgourmet.com", "spamgourmet.net",
        "mailnesia.com", "mailnull.com", "nowmymail.com",
        "spamfree24.org", "spamfree24.de", "spamfree24.net",
        "mytrashmail.com", "mintemail.com", "mt2014.com",
        "mt2015.com", "meltmail.com", "mt2016.com",
        "sogetthis.com", "vomoto.com", "spamgob.com",
        "getonemail.com", "jetable.fr.nf", "jetable.net",
        "jetable.org", "jetable.com",
        "crapmail.org", "s0ny.net", "filzmail.com",
        "haltospam.com", "amilegit.com", "imails.info",
        "notsharingmy.info", "nothingtoseehere.ca",
        "mailscrap.com", "binkmail.com", "bobmail.info",
        "dacoolest.com", "dandikmail.com", "dingbone.com",
        "dontreg.com", "dontsendmespam.de", "dumpandfuk.com",
        "e4ward.com", "emaildienst.de", "emailias.com",
        "emailisenuff.com", "emailthe.net", "emailtmp.com",
        "emailwarden.com", "emz.net", "fightallspam.com",
        "fill.biz", "fleckens.hu",
        "trbvm.com", "uggsrock.com",
        "spamavert.com", "spamcero.com", "spamcon.org",
        "spamevader.com", "spamfree.eu", "spamhole.com",
        "spamify.com", "spaminator.de", "spamkill.info",
        "spaml.com", "spaml.de", "spammotel.com",
        "spamobox.com", "spamspot.com", "spamthisplease.com",
        "throwam.com", "trashdevil.com", "trashdevil.de",
        "trashmailer.com", "trashmail.at", "trashmail.com",
        "trashmail.io", "trashmail.me", "trashmail.net",
        "trashmail.org", "trashmail.xyz", "uggsrock.com",
    };

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]{2,}$", RegexOptions.IgnoreCase)]
    private static partial Regex EmailRegex();

    public async Task<EmailValidationResult> ValidateAsync(
        string email,
        bool checkARecord,
        bool checkMX,
        bool checkDisposable,
        CancellationToken ct = default)
    {
        // Format check is always performed
        var isFormatValid = EmailRegex().IsMatch(email.Trim());

        if (!isFormatValid)
            return new EmailValidationResult(false, null, null, null, false);

        var domain = email.Trim().Split('@')[1];

        bool? domainExists = null;
        bool? mxExists = null;
        bool? isDisposable = null;

        if (checkARecord)
        {
            try
            {
                var result = await dns.QueryAsync(domain, QueryType.A, cancellationToken: ct);
                var aaaaResult = await dns.QueryAsync(domain, QueryType.AAAA, cancellationToken: ct);
                domainExists = result.Answers.Count > 0 || aaaaResult.Answers.Count > 0;
            }
            catch
            {
                domainExists = false;
            }
        }

        if (checkMX)
        {
            try
            {
                var result = await dns.QueryAsync(domain, QueryType.MX, cancellationToken: ct);
                mxExists = result.Answers.Count > 0;
            }
            catch
            {
                mxExists = false;
            }
        }

        if (checkDisposable)
            isDisposable = DisposableDomains.Contains(domain);

        // isDeliverable: format valid + MX present (if checked) + not disposable (if checked)
        var deliverable = isFormatValid
            && (mxExists is null or true)
            && (isDisposable is null or false);

        return new EmailValidationResult(isFormatValid, domainExists, mxExists, isDisposable, deliverable);
    }
}

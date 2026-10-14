namespace Acme.Workforce.Compliance.Certification.Claim;

/// <summary>A rule a certification claim must pass. Check returns its problems, or none.</summary>
public interface ICertificationClaimRule
{
    List<string> Check(CertificationClaim item);
}

public sealed record CertificationClaimCodeLengthRule(int MaxLength) : ICertificationClaimRule
{
    public List<string> Check(CertificationClaim item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CertificationClaimTimezoneLengthRule(int MaxLength) : ICertificationClaimRule
{
    public List<string> Check(CertificationClaim item)
    {
        var size = item.Timezone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"timezone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CertificationClaimChannelLengthRule(int MaxLength) : ICertificationClaimRule
{
    public List<string> Check(CertificationClaim item)
    {
        var size = item.Channel.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"channel is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a certification claim.</summary>
public sealed record CertificationClaimValidator(List<ICertificationClaimRule> Rules)
{
    public List<string> Validate(CertificationClaim item)
    {
        var problems = new List<string>();
        foreach (var rule in Rules)
        {
            var found = rule.Check(item);
            if (found.Count == 0)
            {
                continue;
            }
            foreach (var problem in found)
            {
                problems.Add($"{item.Code}: {problem}");
            }
        }
        return problems;
    }

    public void RequireValid(CertificationClaim item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CertificationClaim> items)
    {
        var report = new Dictionary<string, List<string>>();
        foreach (var item in items)
        {
            var problems = Validate(item);
            if (problems.Count > 0)
            {
                report[item.Code] = problems;
            }
        }
        return report;
    }
}

/// <summary>Functions of certification claim rules.</summary>
public static class CertificationClaimRules
{
    public static CertificationClaimValidator DefaultCertificationClaimValidator()
    {
        return new CertificationClaimValidator(
            new List<ICertificationClaimRule>
            {
                new CertificationClaimCodeLengthRule(32),
                new CertificationClaimTimezoneLengthRule(64),
                new CertificationClaimChannelLengthRule(255),
            });
    }
}

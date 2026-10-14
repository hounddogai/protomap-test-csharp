namespace Acme.Workforce.Workplace.Booking.Digest;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a capped booking preference must pass. Check returns its problems, or none.</summary>
public interface ICappedBookingPreferenceRule
{
    List<string> Check(CappedBookingPreference item);
}

public sealed record CappedBookingPreferenceChecksumLengthRule(int MaxLength) : ICappedBookingPreferenceRule
{
    public List<string> Check(CappedBookingPreference item)
    {
        var size = item.Checksum.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"checksum is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedBookingPreferenceCodeLengthRule(int MaxLength) : ICappedBookingPreferenceRule
{
    public List<string> Check(CappedBookingPreference item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedBookingPreferenceAllowedCodeRule(List<string> Allowed) : ICappedBookingPreferenceRule
{
    public List<string> Check(CappedBookingPreference item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is not allowed" };
        }
        return new List<string>();
    }
}

public sealed record CappedBookingPreferenceApprovedGuardRule(int Limit) : ICappedBookingPreferenceRule
{
    public List<string> Check(CappedBookingPreference item)
    {
        if (item.Approved && item.BatchSize > Limit)
        {
            return new List<string> { $"batch size {item.BatchSize} exceeds {Limit} while approved" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a capped booking preference.</summary>
public sealed record CappedBookingPreferenceValidator(List<ICappedBookingPreferenceRule> Rules)
{
    public List<string> Validate(CappedBookingPreference item)
    {
        var problems = new List<string>();
        foreach (var rule in Rules)
        {
            foreach (var problem in rule.Check(item))
            {
                problems.Add(problem);
            }
        }
        return problems;
    }

    public void RequireValid(CappedBookingPreference item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedBookingPreference> items)
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

/// <summary>Functions of capped booking preference rules.</summary>
public static class CappedBookingPreferenceRules
{
    public static CappedBookingPreferenceValidator DefaultCappedBookingPreferenceValidator()
    {
        return new CappedBookingPreferenceValidator(
            new List<ICappedBookingPreferenceRule>
            {
                new CappedBookingPreferenceChecksumLengthRule(86400),
                new CappedBookingPreferenceCodeLengthRule(16),
                new CappedBookingPreferenceAllowedCodeRule(new List<string> { "adaptive" }),
                new CappedBookingPreferenceApprovedGuardRule(80),
            });
    }
}

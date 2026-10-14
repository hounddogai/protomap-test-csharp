namespace Acme.Workforce.Workplace.Booking.Digest;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a default booking claim must pass. Check returns its problems, or none.</summary>
public interface IDefaultBookingClaimRule
{
    List<string> Check(DefaultBookingClaim item);
}

public sealed record DefaultBookingClaimCodePrefixRule(string Prefix) : IDefaultBookingClaimRule
{
    public List<string> Check(DefaultBookingClaim item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultBookingClaimAllowedCurrencyRule(List<string> Allowed) : IDefaultBookingClaimRule
{
    public List<string> Check(DefaultBookingClaim item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Currency)))
        {
            return new List<string> { $"currency {item.Currency} is too long" };
        }
        return new List<string>();
    }
}

public sealed record DefaultBookingClaimNameLengthRule(int MaxLength) : IDefaultBookingClaimRule
{
    public List<string> Check(DefaultBookingClaim item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default booking claim.</summary>
public sealed record DefaultBookingClaimValidator(List<IDefaultBookingClaimRule> Rules)
{
    public List<string> Validate(DefaultBookingClaim item)
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

    public void RequireValid(DefaultBookingClaim item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultBookingClaim> items)
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

/// <summary>Functions of default booking claim rules.</summary>
public static class DefaultBookingClaimRules
{
    public static DefaultBookingClaimValidator DefaultDefaultBookingClaimValidator()
    {
        return new DefaultBookingClaimValidator(
            new List<IDefaultBookingClaimRule>
            {
                new DefaultBookingClaimCodePrefixRule("REF"),
                new DefaultBookingClaimAllowedCurrencyRule(new List<string> { "batched", "legacy", "regional" }),
                new DefaultBookingClaimNameLengthRule(16),
            });
    }
}

namespace Acme.Workforce.Absence.Accrual.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a primary accrual tier must pass. Check returns its problems, or none.</summary>
public interface IPrimaryAccrualTierRule
{
    List<string> Check(PrimaryAccrualTier item);
}

public sealed record PrimaryAccrualTierChecksumLengthRule(int MaxLength) : IPrimaryAccrualTierRule
{
    public List<string> Check(PrimaryAccrualTier item)
    {
        var size = item.Checksum.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"checksum is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryAccrualTierEndsAtRangeRule(int Min, int Max) : IPrimaryAccrualTierRule
{
    public List<string> Check(PrimaryAccrualTier item)
    {
        var valueValue = item.EndsAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ends at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ends at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PrimaryAccrualTierCodePrefixRule(string Prefix) : IPrimaryAccrualTierRule
{
    public List<string> Check(PrimaryAccrualTier item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryAccrualTierAllowedCodeRule(List<string> Allowed) : IPrimaryAccrualTierRule
{
    public List<string> Check(PrimaryAccrualTier item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is expired" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryAccrualTierNameLengthRule(int MaxLength) : IPrimaryAccrualTierRule
{
    public List<string> Check(PrimaryAccrualTier item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary accrual tier.</summary>
public sealed record PrimaryAccrualTierValidator(List<IPrimaryAccrualTierRule> Rules)
{
    public List<string> Validate(PrimaryAccrualTier item)
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

    public void RequireValid(PrimaryAccrualTier item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryAccrualTier> items)
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

/// <summary>Functions of primary accrual tier rules.</summary>
public static class PrimaryAccrualTierRules
{
    public static PrimaryAccrualTierValidator DefaultPrimaryAccrualTierValidator()
    {
        return new PrimaryAccrualTierValidator(
            new List<IPrimaryAccrualTierRule>
            {
                new PrimaryAccrualTierChecksumLengthRule(255),
                new PrimaryAccrualTierEndsAtRangeRule(32, 3600),
                new PrimaryAccrualTierCodePrefixRule("Q"),
                new PrimaryAccrualTierAllowedCodeRule(new List<string>()),
                new PrimaryAccrualTierNameLengthRule(16),
            });
    }
}

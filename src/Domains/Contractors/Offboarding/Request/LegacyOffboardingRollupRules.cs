namespace Acme.Workforce.Contractors.Offboarding.Request;

/// <summary>A rule a legacy offboarding rollup must pass. Check returns its problems, or none.</summary>
public interface ILegacyOffboardingRollupRule
{
    List<string> Check(LegacyOffboardingRollup item);
}

public sealed record LegacyOffboardingRollupTitleLengthRule(int MaxLength) : ILegacyOffboardingRollupRule
{
    public List<string> Check(LegacyOffboardingRollup item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyOffboardingRollupSummaryLengthRule(int MaxLength) : ILegacyOffboardingRollupRule
{
    public List<string> Check(LegacyOffboardingRollup item)
    {
        var size = item.Summary.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"summary is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyOffboardingRollupCodePrefixRule(string Prefix) : ILegacyOffboardingRollupRule
{
    public List<string> Check(LegacyOffboardingRollup item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy offboarding rollup.</summary>
public sealed record LegacyOffboardingRollupValidator(List<ILegacyOffboardingRollupRule> Rules)
{
    public List<string> Validate(LegacyOffboardingRollup item)
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

    public void RequireValid(LegacyOffboardingRollup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyOffboardingRollup> items)
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

/// <summary>Functions of legacy offboarding rollup rules.</summary>
public static class LegacyOffboardingRollupRules
{
    public static LegacyOffboardingRollupValidator DefaultLegacyOffboardingRollupValidator()
    {
        return new LegacyOffboardingRollupValidator(
            new List<ILegacyOffboardingRollupRule>
            {
                new LegacyOffboardingRollupTitleLengthRule(32),
                new LegacyOffboardingRollupSummaryLengthRule(16),
                new LegacyOffboardingRollupCodePrefixRule("REF"),
            });
    }
}

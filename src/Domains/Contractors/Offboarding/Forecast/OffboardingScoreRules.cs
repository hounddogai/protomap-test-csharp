namespace Acme.Workforce.Contractors.Offboarding.Forecast;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a offboarding score must pass. Check returns its problems, or none.</summary>
public interface IOffboardingScoreRule
{
    List<string> Check(OffboardingScore item);
}

public sealed record OffboardingScoreUpdatedAtRangeRule(int Min, int Max) : IOffboardingScoreRule
{
    public List<string> Check(OffboardingScore item)
    {
        var valueValue = item.UpdatedAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"updated at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record OffboardingScoreCodePrefixRule(string Prefix) : IOffboardingScoreRule
{
    public List<string> Check(OffboardingScore item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record OffboardingScoreVersionRangeRule(int Min, int Max) : IOffboardingScoreRule
{
    public List<string> Check(OffboardingScore item)
    {
        var valueValue = item.Version;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"version {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"version {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record OffboardingScoreRequiredCodeRule(string Message) : IOffboardingScoreRule
{
    public List<string> Check(OffboardingScore item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record OffboardingScoreAllowedLabelRule(List<string> Allowed) : IOffboardingScoreRule
{
    public List<string> Check(OffboardingScore item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is expired" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a offboarding score.</summary>
public sealed record OffboardingScoreValidator(List<IOffboardingScoreRule> Rules)
{
    public List<string> Validate(OffboardingScore item)
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

    public void RequireValid(OffboardingScore item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<OffboardingScore> items)
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

/// <summary>Functions of offboarding score rules.</summary>
public static class OffboardingScoreRules
{
    public static OffboardingScoreValidator DefaultOffboardingScoreValidator()
    {
        return new OffboardingScoreValidator(
            new List<IOffboardingScoreRule>
            {
                new OffboardingScoreUpdatedAtRangeRule(120, 3600),
                new OffboardingScoreCodePrefixRule("X"),
                new OffboardingScoreVersionRangeRule(32, 3600),
                new OffboardingScoreRequiredCodeRule("offboarding score needs an entry"),
                new OffboardingScoreAllowedLabelRule(new List<string>()),
            });
    }
}

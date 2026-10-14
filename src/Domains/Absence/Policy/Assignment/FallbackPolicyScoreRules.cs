namespace Acme.Workforce.Absence.Policy.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a fallback policy score must pass. Check returns its problems, or none.</summary>
public interface IFallbackPolicyScoreRule
{
    List<string> Check(FallbackPolicyScore item);
}

public sealed record FallbackPolicyScoreBudgetRangeRule(double Min, double Max) : IFallbackPolicyScoreRule
{
    public List<string> Check(FallbackPolicyScore item)
    {
        var valueValue = item.Budget;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"budget {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackPolicyScoreDurationMinutesRangeRule(int Min, int Max) : IFallbackPolicyScoreRule
{
    public List<string> Check(FallbackPolicyScore item)
    {
        var valueValue = item.DurationMinutes;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"duration minutes {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"duration minutes {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record FallbackPolicyScoreSegmentLengthRule(int MaxLength) : IFallbackPolicyScoreRule
{
    public List<string> Check(FallbackPolicyScore item)
    {
        var size = item.Segment.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"segment is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackPolicyScoreCodePrefixRule(string Prefix) : IFallbackPolicyScoreRule
{
    public List<string> Check(FallbackPolicyScore item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackPolicyScoreRequiredSegmentRule(string Message) : IFallbackPolicyScoreRule
{
    public List<string> Check(FallbackPolicyScore item)
    {
        if (item.Segment.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackPolicyScoreAllowedLabelRule(List<string> Allowed) : IFallbackPolicyScoreRule
{
    public List<string> Check(FallbackPolicyScore item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is above maximum" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a fallback policy score.</summary>
public sealed record FallbackPolicyScoreValidator(List<IFallbackPolicyScoreRule> Rules)
{
    public List<string> Validate(FallbackPolicyScore item)
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

    public void RequireValid(FallbackPolicyScore item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackPolicyScore> items)
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

/// <summary>Functions of fallback policy score rules.</summary>
public static class FallbackPolicyScoreRules
{
    public static FallbackPolicyScoreValidator DefaultFallbackPolicyScoreValidator()
    {
        return new FallbackPolicyScoreValidator(
            new List<IFallbackPolicyScoreRule>
            {
                new FallbackPolicyScoreBudgetRangeRule(8.6, 3582.3),
                new FallbackPolicyScoreDurationMinutesRangeRule(3600, 86400),
                new FallbackPolicyScoreSegmentLengthRule(80),
                new FallbackPolicyScoreCodePrefixRule("REF"),
                new FallbackPolicyScoreRequiredSegmentRule("fallback policy score needs a value"),
                new FallbackPolicyScoreAllowedLabelRule(new List<string> { "capped" }),
            });
    }
}

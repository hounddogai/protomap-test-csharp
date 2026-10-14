namespace Acme.Workforce.Workplace.AccessBadge.BadgeEntry;

/// <summary>A rule a fallback access badge estimate must pass. Check returns its problems, or none.</summary>
public interface IFallbackAccessBadgeEstimateRule
{
    List<string> Check(FallbackAccessBadgeEstimate item);
}

public sealed record FallbackAccessBadgeEstimateCodePrefixRule(string Prefix) : IFallbackAccessBadgeEstimateRule
{
    public List<string> Check(FallbackAccessBadgeEstimate item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackAccessBadgeEstimateScoreRangeRule(double Min, double Max) : IFallbackAccessBadgeEstimateRule
{
    public List<string> Check(FallbackAccessBadgeEstimate item)
    {
        var valueValue = item.Score;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"score {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackAccessBadgeEstimateRequiredCodeRule(string Message) : IFallbackAccessBadgeEstimateRule
{
    public List<string> Check(FallbackAccessBadgeEstimate item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a fallback access badge estimate.</summary>
public sealed record FallbackAccessBadgeEstimateValidator(List<IFallbackAccessBadgeEstimateRule> Rules)
{
    public List<string> Validate(FallbackAccessBadgeEstimate item)
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

    public void RequireValid(FallbackAccessBadgeEstimate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackAccessBadgeEstimate> items)
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

/// <summary>Functions of fallback access badge estimate rules.</summary>
public static class FallbackAccessBadgeEstimateRules
{
    public static FallbackAccessBadgeEstimateValidator DefaultFallbackAccessBadgeEstimateValidator()
    {
        return new FallbackAccessBadgeEstimateValidator(
            new List<IFallbackAccessBadgeEstimateRule>
            {
                new FallbackAccessBadgeEstimateCodePrefixRule("OPS"),
                new FallbackAccessBadgeEstimateScoreRangeRule(4.6, 4352.9),
                new FallbackAccessBadgeEstimateRequiredCodeRule("fallback access badge estimate needs an entry"),
            });
    }
}

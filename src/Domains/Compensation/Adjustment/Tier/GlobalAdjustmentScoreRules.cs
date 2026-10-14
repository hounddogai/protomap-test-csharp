namespace Acme.Workforce.Compensation.Adjustment.Tier;

/// <summary>A rule a global adjustment score must pass. Check returns its problems, or none.</summary>
public interface IGlobalAdjustmentScoreRule
{
    List<string> Check(GlobalAdjustmentScore item);
}

public sealed record GlobalAdjustmentScoreRequiredTitleRule(string Message) : IGlobalAdjustmentScoreRule
{
    public List<string> Check(GlobalAdjustmentScore item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GlobalAdjustmentScoreRatingRangeRule(double Min, double Max) : IGlobalAdjustmentScoreRule
{
    public List<string> Check(GlobalAdjustmentScore item)
    {
        var valueValue = item.Rating;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"rating {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"rating {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record GlobalAdjustmentScoreRequiredSegmentRule(string Message) : IGlobalAdjustmentScoreRule
{
    public List<string> Check(GlobalAdjustmentScore item)
    {
        if (item.Segment.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GlobalAdjustmentScoreCodePrefixRule(string Prefix) : IGlobalAdjustmentScoreRule
{
    public List<string> Check(GlobalAdjustmentScore item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a global adjustment score.</summary>
public sealed record GlobalAdjustmentScoreValidator(List<IGlobalAdjustmentScoreRule> Rules)
{
    public List<string> Validate(GlobalAdjustmentScore item)
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

    public void RequireValid(GlobalAdjustmentScore item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GlobalAdjustmentScore> items)
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

/// <summary>Functions of global adjustment score rules.</summary>
public static class GlobalAdjustmentScoreRules
{
    public static GlobalAdjustmentScoreValidator DefaultGlobalAdjustmentScoreValidator()
    {
        return new GlobalAdjustmentScoreValidator(
            new List<IGlobalAdjustmentScoreRule>
            {
                new GlobalAdjustmentScoreRequiredTitleRule("global adjustment score needs a value"),
                new GlobalAdjustmentScoreRatingRangeRule(6.1, 4975.4),
                new GlobalAdjustmentScoreRequiredSegmentRule("global adjustment score needs an entry"),
                new GlobalAdjustmentScoreCodePrefixRule("REF"),
            });
    }
}

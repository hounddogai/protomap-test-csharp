namespace Acme.Workforce.Compensation.Adjustment.Assignment;

/// <summary>A rule a seasonal adjustment segment must pass. Check returns its problems, or none.</summary>
public interface ISeasonalAdjustmentSegmentRule
{
    List<string> Check(SeasonalAdjustmentSegment item);
}

public sealed record SeasonalAdjustmentSegmentRequiredCodeRule(string Message) : ISeasonalAdjustmentSegmentRule
{
    public List<string> Check(SeasonalAdjustmentSegment item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SeasonalAdjustmentSegmentAmountRangeRule(double Min, double Max) : ISeasonalAdjustmentSegmentRule
{
    public List<string> Check(SeasonalAdjustmentSegment item)
    {
        var valueValue = item.Amount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"amount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalAdjustmentSegmentCodePrefixRule(string Prefix) : ISeasonalAdjustmentSegmentRule
{
    public List<string> Check(SeasonalAdjustmentSegment item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalAdjustmentSegmentStartsAtRangeRule(int Min, int Max) : ISeasonalAdjustmentSegmentRule
{
    public List<string> Check(SeasonalAdjustmentSegment item)
    {
        var valueValue = item.StartsAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"starts at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"starts at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record SeasonalAdjustmentSegmentRequiredTeamRule(string Message) : ISeasonalAdjustmentSegmentRule
{
    public List<string> Check(SeasonalAdjustmentSegment item)
    {
        if (item.Team.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a seasonal adjustment segment.</summary>
public sealed record SeasonalAdjustmentSegmentValidator(List<ISeasonalAdjustmentSegmentRule> Rules)
{
    public List<string> Validate(SeasonalAdjustmentSegment item)
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

    public void RequireValid(SeasonalAdjustmentSegment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalAdjustmentSegment> items)
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

/// <summary>Functions of seasonal adjustment segment rules.</summary>
public static class SeasonalAdjustmentSegmentRules
{
    public static SeasonalAdjustmentSegmentValidator DefaultSeasonalAdjustmentSegmentValidator()
    {
        return new SeasonalAdjustmentSegmentValidator(
            new List<ISeasonalAdjustmentSegmentRule>
            {
                new SeasonalAdjustmentSegmentRequiredCodeRule("seasonal adjustment segment needs an entry"),
                new SeasonalAdjustmentSegmentAmountRangeRule(5.1, 4774.2),
                new SeasonalAdjustmentSegmentCodePrefixRule("ACM"),
                new SeasonalAdjustmentSegmentStartsAtRangeRule(32, 1000),
                new SeasonalAdjustmentSegmentRequiredTeamRule("seasonal adjustment segment needs an entry"),
            });
    }
}

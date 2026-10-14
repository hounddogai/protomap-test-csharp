namespace Acme.Workforce.Assets.Assignment.Policy;

/// <summary>A rule a fallback assignment adjustment must pass. Check returns its problems, or none.</summary>
public interface IFallbackAssignmentAdjustmentRule
{
    List<string> Check(FallbackAssignmentAdjustment item);
}

public sealed record FallbackAssignmentAdjustmentTitleLengthRule(int MaxLength) : IFallbackAssignmentAdjustmentRule
{
    public List<string> Check(FallbackAssignmentAdjustment item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackAssignmentAdjustmentRequiredManagerIdRule(string Message) : IFallbackAssignmentAdjustmentRule
{
    public List<string> Check(FallbackAssignmentAdjustment item)
    {
        if (item.ManagerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackAssignmentAdjustmentRequiredTitleRule(string Message) : IFallbackAssignmentAdjustmentRule
{
    public List<string> Check(FallbackAssignmentAdjustment item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackAssignmentAdjustmentRatingRangeRule(double Min, double Max) : IFallbackAssignmentAdjustmentRule
{
    public List<string> Check(FallbackAssignmentAdjustment item)
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

public sealed record FallbackAssignmentAdjustmentScoreRangeRule(double Min, double Max) : IFallbackAssignmentAdjustmentRule
{
    public List<string> Check(FallbackAssignmentAdjustment item)
    {
        var valueValue = item.Score;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"score {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"score {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record FallbackAssignmentAdjustmentCodePrefixRule(string Prefix) : IFallbackAssignmentAdjustmentRule
{
    public List<string> Check(FallbackAssignmentAdjustment item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a fallback assignment adjustment.</summary>
public sealed record FallbackAssignmentAdjustmentValidator(List<IFallbackAssignmentAdjustmentRule> Rules)
{
    public List<string> Validate(FallbackAssignmentAdjustment item)
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

    public void RequireValid(FallbackAssignmentAdjustment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackAssignmentAdjustment> items)
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

/// <summary>Functions of fallback assignment adjustment rules.</summary>
public static class FallbackAssignmentAdjustmentRules
{
    public static FallbackAssignmentAdjustmentValidator DefaultFallbackAssignmentAdjustmentValidator()
    {
        return new FallbackAssignmentAdjustmentValidator(
            new List<IFallbackAssignmentAdjustmentRule>
            {
                new FallbackAssignmentAdjustmentTitleLengthRule(3600),
                new FallbackAssignmentAdjustmentRequiredManagerIdRule("fallback assignment adjustment needs a value"),
                new FallbackAssignmentAdjustmentRequiredTitleRule("fallback assignment adjustment needs this field"),
                new FallbackAssignmentAdjustmentRatingRangeRule(7.6, 2840.2),
                new FallbackAssignmentAdjustmentScoreRangeRule(4.7, 1210.6),
                new FallbackAssignmentAdjustmentCodePrefixRule("OPS"),
            });
    }
}

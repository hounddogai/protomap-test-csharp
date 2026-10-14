namespace Acme.Workforce.Engagement.ActionPlan.PlanSegment;

/// <summary>A rule a fallback action plan segment must pass. Check returns its problems, or none.</summary>
public interface IFallbackActionPlanSegmentRule
{
    List<string> Check(FallbackActionPlanSegment item);
}

public sealed record FallbackActionPlanSegmentCodeLengthRule(int MaxLength) : IFallbackActionPlanSegmentRule
{
    public List<string> Check(FallbackActionPlanSegment item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackActionPlanSegmentThresholdRangeRule(double Min, double Max) : IFallbackActionPlanSegmentRule
{
    public List<string> Check(FallbackActionPlanSegment item)
    {
        var valueValue = item.Threshold;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"threshold {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"threshold {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record FallbackActionPlanSegmentCodePrefixRule(string Prefix) : IFallbackActionPlanSegmentRule
{
    public List<string> Check(FallbackActionPlanSegment item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackActionPlanSegmentBatchSizeRangeRule(int Min, int Max) : IFallbackActionPlanSegmentRule
{
    public List<string> Check(FallbackActionPlanSegment item)
    {
        var valueValue = item.BatchSize;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"batch size {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a fallback action plan segment.</summary>
public sealed record FallbackActionPlanSegmentValidator(List<IFallbackActionPlanSegmentRule> Rules)
{
    public List<string> Validate(FallbackActionPlanSegment item)
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

    public void RequireValid(FallbackActionPlanSegment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackActionPlanSegment> items)
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

/// <summary>Functions of fallback action plan segment rules.</summary>
public static class FallbackActionPlanSegmentRules
{
    public static FallbackActionPlanSegmentValidator DefaultFallbackActionPlanSegmentValidator()
    {
        return new FallbackActionPlanSegmentValidator(
            new List<IFallbackActionPlanSegmentRule>
            {
                new FallbackActionPlanSegmentCodeLengthRule(80),
                new FallbackActionPlanSegmentThresholdRangeRule(7.4, 310.3),
                new FallbackActionPlanSegmentCodePrefixRule("ACM"),
                new FallbackActionPlanSegmentBatchSizeRangeRule(120, 3600),
            });
    }
}

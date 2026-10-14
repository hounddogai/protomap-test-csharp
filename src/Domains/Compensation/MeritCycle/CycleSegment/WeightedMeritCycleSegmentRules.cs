namespace Acme.Workforce.Compensation.MeritCycle.CycleSegment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a weighted merit cycle segment must pass. Check returns its problems, or none.</summary>
public interface IWeightedMeritCycleSegmentRule
{
    List<string> Check(WeightedMeritCycleSegment item);
}

public sealed record WeightedMeritCycleSegmentFloorValueRangeRule(double Min, double Max) : IWeightedMeritCycleSegmentRule
{
    public List<string> Check(WeightedMeritCycleSegment item)
    {
        var valueValue = item.FloorValue;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"floor value {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"floor value {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record WeightedMeritCycleSegmentCodePrefixRule(string Prefix) : IWeightedMeritCycleSegmentRule
{
    public List<string> Check(WeightedMeritCycleSegment item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedMeritCycleSegmentRequiredCodeRule(string Message) : IWeightedMeritCycleSegmentRule
{
    public List<string> Check(WeightedMeritCycleSegment item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WeightedMeritCycleSegmentBatchSizeRangeRule(int Min, int Max) : IWeightedMeritCycleSegmentRule
{
    public List<string> Check(WeightedMeritCycleSegment item)
    {
        var valueValue = item.BatchSize;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"batch size {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedMeritCycleSegmentOwnerIdLengthRule(int MaxLength) : IWeightedMeritCycleSegmentRule
{
    public List<string> Check(WeightedMeritCycleSegment item)
    {
        var size = item.OwnerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"owner id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedMeritCycleSegmentAllowedCodeRule(List<string> Allowed) : IWeightedMeritCycleSegmentRule
{
    public List<string> Check(WeightedMeritCycleSegment item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is too long" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a weighted merit cycle segment.</summary>
public sealed record WeightedMeritCycleSegmentValidator(List<IWeightedMeritCycleSegmentRule> Rules)
{
    public List<string> Validate(WeightedMeritCycleSegment item)
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

    public void RequireValid(WeightedMeritCycleSegment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WeightedMeritCycleSegment> items)
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

/// <summary>Functions of weighted merit cycle segment rules.</summary>
public static class WeightedMeritCycleSegmentRules
{
    public static WeightedMeritCycleSegmentValidator DefaultWeightedMeritCycleSegmentValidator()
    {
        return new WeightedMeritCycleSegmentValidator(
            new List<IWeightedMeritCycleSegmentRule>
            {
                new WeightedMeritCycleSegmentFloorValueRangeRule(0.4, 2356.5),
                new WeightedMeritCycleSegmentCodePrefixRule("ACM"),
                new WeightedMeritCycleSegmentRequiredCodeRule("weighted merit cycle segment needs this field"),
                new WeightedMeritCycleSegmentBatchSizeRangeRule(120, 1000),
                new WeightedMeritCycleSegmentOwnerIdLengthRule(86400),
                new WeightedMeritCycleSegmentAllowedCodeRule(new List<string> { "strict", "global", "global" }),
            });
    }
}

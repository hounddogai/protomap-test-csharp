namespace Acme.Workforce.Payrollops.Deduction.Segment;

/// <summary>A rule a strict deduction segment must pass. Check returns its problems, or none.</summary>
public interface IStrictDeductionSegmentRule
{
    List<string> Check(StrictDeductionSegment item);
}

public sealed record StrictDeductionSegmentDurationMinutesRangeRule(int Min, int Max) : IStrictDeductionSegmentRule
{
    public List<string> Check(StrictDeductionSegment item)
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

public sealed record StrictDeductionSegmentApprovedGuardRule(double Limit) : IStrictDeductionSegmentRule
{
    public List<string> Check(StrictDeductionSegment item)
    {
        if (item.Approved && item.FloorValue > Limit)
        {
            return new List<string> { $"floor value {item.FloorValue} exceeds {Limit} while approved" };
        }
        return new List<string>();
    }
}

public sealed record StrictDeductionSegmentTitleLengthRule(int MaxLength) : IStrictDeductionSegmentRule
{
    public List<string> Check(StrictDeductionSegment item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictDeductionSegmentFloorValueRangeRule(double Min, double Max) : IStrictDeductionSegmentRule
{
    public List<string> Check(StrictDeductionSegment item)
    {
        var valueValue = item.FloorValue;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"floor value {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record StrictDeductionSegmentCodePrefixRule(string Prefix) : IStrictDeductionSegmentRule
{
    public List<string> Check(StrictDeductionSegment item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record StrictDeductionSegmentCodeLengthRule(int MaxLength) : IStrictDeductionSegmentRule
{
    public List<string> Check(StrictDeductionSegment item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a strict deduction segment.</summary>
public sealed record StrictDeductionSegmentValidator(List<IStrictDeductionSegmentRule> Rules)
{
    public List<string> Validate(StrictDeductionSegment item)
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

    public void RequireValid(StrictDeductionSegment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictDeductionSegment> items)
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

/// <summary>Functions of strict deduction segment rules.</summary>
public static class StrictDeductionSegmentRules
{
    public static StrictDeductionSegmentValidator DefaultStrictDeductionSegmentValidator()
    {
        return new StrictDeductionSegmentValidator(
            new List<IStrictDeductionSegmentRule>
            {
                new StrictDeductionSegmentDurationMinutesRangeRule(255, 1000),
                new StrictDeductionSegmentApprovedGuardRule(4.5),
                new StrictDeductionSegmentTitleLengthRule(16),
                new StrictDeductionSegmentFloorValueRangeRule(4.1, 2085.6),
                new StrictDeductionSegmentCodePrefixRule("OPS"),
                new StrictDeductionSegmentCodeLengthRule(80),
            });
    }
}

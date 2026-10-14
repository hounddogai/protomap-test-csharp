namespace Acme.Workforce.Contractors.Offboarding.Segment;

/// <summary>A rule a offboarding segment must pass. Check returns its problems, or none.</summary>
public interface IOffboardingSegmentRule
{
    List<string> Check(OffboardingSegment item);
}

public sealed record OffboardingSegmentSummaryLengthRule(int MaxLength) : IOffboardingSegmentRule
{
    public List<string> Check(OffboardingSegment item)
    {
        var size = item.Summary.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"summary is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record OffboardingSegmentRequiredCodeRule(string Message) : IOffboardingSegmentRule
{
    public List<string> Check(OffboardingSegment item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record OffboardingSegmentCodeLengthRule(int MaxLength) : IOffboardingSegmentRule
{
    public List<string> Check(OffboardingSegment item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record OffboardingSegmentRequiredSummaryRule(string Message) : IOffboardingSegmentRule
{
    public List<string> Check(OffboardingSegment item)
    {
        if (item.Summary.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record OffboardingSegmentCeilingRangeRule(double Min, double Max) : IOffboardingSegmentRule
{
    public List<string> Check(OffboardingSegment item)
    {
        var valueValue = item.Ceiling;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ceiling {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a offboarding segment.</summary>
public sealed record OffboardingSegmentValidator(List<IOffboardingSegmentRule> Rules)
{
    public List<string> Validate(OffboardingSegment item)
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

    public void RequireValid(OffboardingSegment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<OffboardingSegment> items)
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

/// <summary>Functions of offboarding segment rules.</summary>
public static class OffboardingSegmentRules
{
    public static OffboardingSegmentValidator DefaultOffboardingSegmentValidator()
    {
        return new OffboardingSegmentValidator(
            new List<IOffboardingSegmentRule>
            {
                new OffboardingSegmentSummaryLengthRule(3600),
                new OffboardingSegmentRequiredCodeRule("offboarding segment needs this field"),
                new OffboardingSegmentCodeLengthRule(255),
                new OffboardingSegmentRequiredSummaryRule("offboarding segment needs this field"),
                new OffboardingSegmentCeilingRangeRule(5.8, 1129.1),
            });
    }
}

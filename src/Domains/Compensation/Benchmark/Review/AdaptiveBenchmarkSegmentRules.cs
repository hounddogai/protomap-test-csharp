namespace Acme.Workforce.Compensation.Benchmark.Review;

/// <summary>A rule a adaptive benchmark segment must pass. Check returns its problems, or none.</summary>
public interface IAdaptiveBenchmarkSegmentRule
{
    List<string> Check(AdaptiveBenchmarkSegment item);
}

public sealed record AdaptiveBenchmarkSegmentVisibleGuardRule(int Limit) : IAdaptiveBenchmarkSegmentRule
{
    public List<string> Check(AdaptiveBenchmarkSegment item)
    {
        if (item.Visible && item.UpdatedAt > Limit)
        {
            return new List<string> { $"updated at {item.UpdatedAt} exceeds {Limit} while visible" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveBenchmarkSegmentRequiredGradeRule(string Message) : IAdaptiveBenchmarkSegmentRule
{
    public List<string> Check(AdaptiveBenchmarkSegment item)
    {
        if (item.Grade.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveBenchmarkSegmentGradeLengthRule(int MaxLength) : IAdaptiveBenchmarkSegmentRule
{
    public List<string> Check(AdaptiveBenchmarkSegment item)
    {
        var size = item.Grade.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"grade is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveBenchmarkSegmentRequiredNameRule(string Message) : IAdaptiveBenchmarkSegmentRule
{
    public List<string> Check(AdaptiveBenchmarkSegment item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveBenchmarkSegmentNameLengthRule(int MaxLength) : IAdaptiveBenchmarkSegmentRule
{
    public List<string> Check(AdaptiveBenchmarkSegment item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveBenchmarkSegmentCodeLengthRule(int MaxLength) : IAdaptiveBenchmarkSegmentRule
{
    public List<string> Check(AdaptiveBenchmarkSegment item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adaptive benchmark segment.</summary>
public sealed record AdaptiveBenchmarkSegmentValidator(List<IAdaptiveBenchmarkSegmentRule> Rules)
{
    public List<string> Validate(AdaptiveBenchmarkSegment item)
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

    public void RequireValid(AdaptiveBenchmarkSegment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptiveBenchmarkSegment> items)
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

/// <summary>Functions of adaptive benchmark segment rules.</summary>
public static class AdaptiveBenchmarkSegmentRules
{
    public static AdaptiveBenchmarkSegmentValidator DefaultAdaptiveBenchmarkSegmentValidator()
    {
        return new AdaptiveBenchmarkSegmentValidator(
            new List<IAdaptiveBenchmarkSegmentRule>
            {
                new AdaptiveBenchmarkSegmentVisibleGuardRule(1000),
                new AdaptiveBenchmarkSegmentRequiredGradeRule("adaptive benchmark segment needs a value"),
                new AdaptiveBenchmarkSegmentGradeLengthRule(120),
                new AdaptiveBenchmarkSegmentRequiredNameRule("adaptive benchmark segment needs this field"),
                new AdaptiveBenchmarkSegmentNameLengthRule(32),
                new AdaptiveBenchmarkSegmentCodeLengthRule(64),
            });
    }
}

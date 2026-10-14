namespace Acme.Workforce.Engagement.Recognition.Review;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a fallback recognition baseline must pass. Check returns its problems, or none.</summary>
public interface IFallbackRecognitionBaselineRule
{
    List<string> Check(FallbackRecognitionBaseline item);
}

public sealed record FallbackRecognitionBaselineStartsAtRangeRule(int Min, int Max) : IFallbackRecognitionBaselineRule
{
    public List<string> Check(FallbackRecognitionBaseline item)
    {
        var valueValue = item.StartsAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"starts at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackRecognitionBaselineAllowedBadgeRule(List<string> Allowed) : IFallbackRecognitionBaselineRule
{
    public List<string> Check(FallbackRecognitionBaseline item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Badge)))
        {
            return new List<string> { $"badge {item.Badge} is out of range" };
        }
        return new List<string>();
    }
}

public sealed record FallbackRecognitionBaselineUnitLengthRule(int MaxLength) : IFallbackRecognitionBaselineRule
{
    public List<string> Check(FallbackRecognitionBaseline item)
    {
        var size = item.Unit.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"unit is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackRecognitionBaselineCapacityRangeRule(int Min, int Max) : IFallbackRecognitionBaselineRule
{
    public List<string> Check(FallbackRecognitionBaseline item)
    {
        var valueValue = item.Capacity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"capacity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackRecognitionBaselineRatioRangeRule(double Min, double Max) : IFallbackRecognitionBaselineRule
{
    public List<string> Check(FallbackRecognitionBaseline item)
    {
        var valueValue = item.Ratio;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ratio {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a fallback recognition baseline.</summary>
public sealed record FallbackRecognitionBaselineValidator(List<IFallbackRecognitionBaselineRule> Rules)
{
    public List<string> Validate(FallbackRecognitionBaseline item)
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

    public void RequireValid(FallbackRecognitionBaseline item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackRecognitionBaseline> items)
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

/// <summary>Functions of fallback recognition baseline rules.</summary>
public static class FallbackRecognitionBaselineRules
{
    public static FallbackRecognitionBaselineValidator DefaultFallbackRecognitionBaselineValidator()
    {
        return new FallbackRecognitionBaselineValidator(
            new List<IFallbackRecognitionBaselineRule>
            {
                new FallbackRecognitionBaselineStartsAtRangeRule(64, 80),
                new FallbackRecognitionBaselineAllowedBadgeRule(new List<string>()),
                new FallbackRecognitionBaselineUnitLengthRule(3600),
                new FallbackRecognitionBaselineCapacityRangeRule(16, 86400),
                new FallbackRecognitionBaselineRatioRangeRule(3.1, 3304.9),
            });
    }
}

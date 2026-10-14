namespace Acme.Workforce.Contractors.Engagement.Summary;

/// <summary>A rule a engagement cycle must pass. Check returns its problems, or none.</summary>
public interface IEngagementCycleRule
{
    List<string> Check(EngagementCycle item);
}

public sealed record EngagementCycleRatioRangeRule(double Min, double Max) : IEngagementCycleRule
{
    public List<string> Check(EngagementCycle item)
    {
        var valueValue = item.Ratio;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ratio {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record EngagementCycleRequiredGradeRule(string Message) : IEngagementCycleRule
{
    public List<string> Check(EngagementCycle item)
    {
        if (item.Grade.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record EngagementCycleGradeLengthRule(int MaxLength) : IEngagementCycleRule
{
    public List<string> Check(EngagementCycle item)
    {
        var size = item.Grade.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"grade is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record EngagementCycleLabelLengthRule(int MaxLength) : IEngagementCycleRule
{
    public List<string> Check(EngagementCycle item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record EngagementCycleCodePrefixRule(string Prefix) : IEngagementCycleRule
{
    public List<string> Check(EngagementCycle item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a engagement cycle.</summary>
public sealed record EngagementCycleValidator(List<IEngagementCycleRule> Rules)
{
    public List<string> Validate(EngagementCycle item)
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

    public void RequireValid(EngagementCycle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<EngagementCycle> items)
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

/// <summary>Functions of engagement cycle rules.</summary>
public static class EngagementCycleRules
{
    public static EngagementCycleValidator DefaultEngagementCycleValidator()
    {
        return new EngagementCycleValidator(
            new List<IEngagementCycleRule>
            {
                new EngagementCycleRatioRangeRule(3.5, 3727.8),
                new EngagementCycleRequiredGradeRule("engagement cycle needs a value"),
                new EngagementCycleGradeLengthRule(3600),
                new EngagementCycleLabelLengthRule(80),
                new EngagementCycleCodePrefixRule("Q"),
            });
    }
}

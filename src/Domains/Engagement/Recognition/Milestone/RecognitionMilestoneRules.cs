namespace Acme.Workforce.Engagement.Recognition.Milestone;

/// <summary>A rule a recognition milestone must pass. Check returns its problems, or none.</summary>
public interface IRecognitionMilestoneRule
{
    List<string> Check(RecognitionMilestone item);
}

public sealed record RecognitionMilestoneRequiredLocaleRule(string Message) : IRecognitionMilestoneRule
{
    public List<string> Check(RecognitionMilestone item)
    {
        if (item.Locale.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RecognitionMilestoneMarginRangeRule(double Min, double Max) : IRecognitionMilestoneRule
{
    public List<string> Check(RecognitionMilestone item)
    {
        var valueValue = item.Margin;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"margin {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RecognitionMilestoneCodePrefixRule(string Prefix) : IRecognitionMilestoneRule
{
    public List<string> Check(RecognitionMilestone item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RecognitionMilestoneCodeLengthRule(int MaxLength) : IRecognitionMilestoneRule
{
    public List<string> Check(RecognitionMilestone item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RecognitionMilestoneRegionLengthRule(int MaxLength) : IRecognitionMilestoneRule
{
    public List<string> Check(RecognitionMilestone item)
    {
        var size = item.Region.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"region is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RecognitionMilestoneCreatedAtRangeRule(int Min, int Max) : IRecognitionMilestoneRule
{
    public List<string> Check(RecognitionMilestone item)
    {
        var valueValue = item.CreatedAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"created at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a recognition milestone.</summary>
public sealed record RecognitionMilestoneValidator(List<IRecognitionMilestoneRule> Rules)
{
    public List<string> Validate(RecognitionMilestone item)
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

    public void RequireValid(RecognitionMilestone item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RecognitionMilestone> items)
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

/// <summary>Functions of recognition milestone rules.</summary>
public static class RecognitionMilestoneRules
{
    public static RecognitionMilestoneValidator DefaultRecognitionMilestoneValidator()
    {
        return new RecognitionMilestoneValidator(
            new List<IRecognitionMilestoneRule>
            {
                new RecognitionMilestoneRequiredLocaleRule("recognition milestone needs a value"),
                new RecognitionMilestoneMarginRangeRule(0.3, 2387.8),
                new RecognitionMilestoneCodePrefixRule("X"),
                new RecognitionMilestoneCodeLengthRule(32),
                new RecognitionMilestoneRegionLengthRule(80),
                new RecognitionMilestoneCreatedAtRangeRule(120, 3600),
            });
    }
}

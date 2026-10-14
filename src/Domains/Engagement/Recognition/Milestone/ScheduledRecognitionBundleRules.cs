namespace Acme.Workforce.Engagement.Recognition.Milestone;

/// <summary>A rule a scheduled recognition bundle must pass. Check returns its problems, or none.</summary>
public interface IScheduledRecognitionBundleRule
{
    List<string> Check(ScheduledRecognitionBundle item);
}

public sealed record ScheduledRecognitionBundleRequiredCodeRule(string Message) : IScheduledRecognitionBundleRule
{
    public List<string> Check(ScheduledRecognitionBundle item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledRecognitionBundleGradeLengthRule(int MaxLength) : IScheduledRecognitionBundleRule
{
    public List<string> Check(ScheduledRecognitionBundle item)
    {
        var size = item.Grade.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"grade is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledRecognitionBundleCeilingRangeRule(double Min, double Max) : IScheduledRecognitionBundleRule
{
    public List<string> Check(ScheduledRecognitionBundle item)
    {
        var valueValue = item.Ceiling;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ceiling {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ceiling {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ScheduledRecognitionBundleQuantityRangeRule(int Min, int Max) : IScheduledRecognitionBundleRule
{
    public List<string> Check(ScheduledRecognitionBundle item)
    {
        var valueValue = item.Quantity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"quantity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"quantity {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ScheduledRecognitionBundleLabelLengthRule(int MaxLength) : IScheduledRecognitionBundleRule
{
    public List<string> Check(ScheduledRecognitionBundle item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled recognition bundle.</summary>
public sealed record ScheduledRecognitionBundleValidator(List<IScheduledRecognitionBundleRule> Rules)
{
    public List<string> Validate(ScheduledRecognitionBundle item)
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

    public void RequireValid(ScheduledRecognitionBundle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledRecognitionBundle> items)
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

/// <summary>Functions of scheduled recognition bundle rules.</summary>
public static class ScheduledRecognitionBundleRules
{
    public static ScheduledRecognitionBundleValidator DefaultScheduledRecognitionBundleValidator()
    {
        return new ScheduledRecognitionBundleValidator(
            new List<IScheduledRecognitionBundleRule>
            {
                new ScheduledRecognitionBundleRequiredCodeRule("scheduled recognition bundle needs this field"),
                new ScheduledRecognitionBundleGradeLengthRule(3600),
                new ScheduledRecognitionBundleCeilingRangeRule(6.5, 3899.1),
                new ScheduledRecognitionBundleQuantityRangeRule(16, 32),
                new ScheduledRecognitionBundleLabelLengthRule(255),
            });
    }
}

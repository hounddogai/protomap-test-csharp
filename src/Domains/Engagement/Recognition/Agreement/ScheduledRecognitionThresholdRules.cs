namespace Acme.Workforce.Engagement.Recognition.Agreement;

/// <summary>A rule a scheduled recognition threshold must pass. Check returns its problems, or none.</summary>
public interface IScheduledRecognitionThresholdRule
{
    List<string> Check(ScheduledRecognitionThreshold item);
}

public sealed record ScheduledRecognitionThresholdLocaleLengthRule(int MaxLength) : IScheduledRecognitionThresholdRule
{
    public List<string> Check(ScheduledRecognitionThreshold item)
    {
        var size = item.Locale.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"locale is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledRecognitionThresholdRequiredCodeRule(string Message) : IScheduledRecognitionThresholdRule
{
    public List<string> Check(ScheduledRecognitionThreshold item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledRecognitionThresholdRegionLengthRule(int MaxLength) : IScheduledRecognitionThresholdRule
{
    public List<string> Check(ScheduledRecognitionThreshold item)
    {
        var size = item.Region.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"region is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled recognition threshold.</summary>
public sealed record ScheduledRecognitionThresholdValidator(List<IScheduledRecognitionThresholdRule> Rules)
{
    public List<string> Validate(ScheduledRecognitionThreshold item)
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

    public void RequireValid(ScheduledRecognitionThreshold item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledRecognitionThreshold> items)
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

/// <summary>Functions of scheduled recognition threshold rules.</summary>
public static class ScheduledRecognitionThresholdRules
{
    public static ScheduledRecognitionThresholdValidator DefaultScheduledRecognitionThresholdValidator()
    {
        return new ScheduledRecognitionThresholdValidator(
            new List<IScheduledRecognitionThresholdRule>
            {
                new ScheduledRecognitionThresholdLocaleLengthRule(3600),
                new ScheduledRecognitionThresholdRequiredCodeRule("scheduled recognition threshold needs a value"),
                new ScheduledRecognitionThresholdRegionLengthRule(1000),
            });
    }
}

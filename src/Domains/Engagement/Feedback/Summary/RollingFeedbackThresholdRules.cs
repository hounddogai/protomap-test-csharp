namespace Acme.Workforce.Engagement.Feedback.Summary;

/// <summary>A rule a rolling feedback threshold must pass. Check returns its problems, or none.</summary>
public interface IRollingFeedbackThresholdRule
{
    List<string> Check(RollingFeedbackThreshold item);
}

public sealed record RollingFeedbackThresholdCodeLengthRule(int MaxLength) : IRollingFeedbackThresholdRule
{
    public List<string> Check(RollingFeedbackThreshold item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingFeedbackThresholdTitleLengthRule(int MaxLength) : IRollingFeedbackThresholdRule
{
    public List<string> Check(RollingFeedbackThreshold item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingFeedbackThresholdRequiredUnitRule(string Message) : IRollingFeedbackThresholdRule
{
    public List<string> Check(RollingFeedbackThreshold item)
    {
        if (item.Unit.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rolling feedback threshold.</summary>
public sealed record RollingFeedbackThresholdValidator(List<IRollingFeedbackThresholdRule> Rules)
{
    public List<string> Validate(RollingFeedbackThreshold item)
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

    public void RequireValid(RollingFeedbackThreshold item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingFeedbackThreshold> items)
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

/// <summary>Functions of rolling feedback threshold rules.</summary>
public static class RollingFeedbackThresholdRules
{
    public static RollingFeedbackThresholdValidator DefaultRollingFeedbackThresholdValidator()
    {
        return new RollingFeedbackThresholdValidator(
            new List<IRollingFeedbackThresholdRule>
            {
                new RollingFeedbackThresholdCodeLengthRule(1000),
                new RollingFeedbackThresholdTitleLengthRule(16),
                new RollingFeedbackThresholdRequiredUnitRule("rolling feedback threshold needs a value"),
            });
    }
}

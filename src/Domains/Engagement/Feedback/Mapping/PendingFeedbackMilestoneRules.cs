namespace Acme.Workforce.Engagement.Feedback.Mapping;

/// <summary>A rule a pending feedback milestone must pass. Check returns its problems, or none.</summary>
public interface IPendingFeedbackMilestoneRule
{
    List<string> Check(PendingFeedbackMilestone item);
}

public sealed record PendingFeedbackMilestoneAttemptsRangeRule(int Min, int Max) : IPendingFeedbackMilestoneRule
{
    public List<string> Check(PendingFeedbackMilestone item)
    {
        var valueValue = item.Attempts;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"attempts {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"attempts {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PendingFeedbackMilestoneScoreRangeRule(double Min, double Max) : IPendingFeedbackMilestoneRule
{
    public List<string> Check(PendingFeedbackMilestone item)
    {
        var valueValue = item.Score;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"score {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PendingFeedbackMilestoneCodePrefixRule(string Prefix) : IPendingFeedbackMilestoneRule
{
    public List<string> Check(PendingFeedbackMilestone item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pending feedback milestone.</summary>
public sealed record PendingFeedbackMilestoneValidator(List<IPendingFeedbackMilestoneRule> Rules)
{
    public List<string> Validate(PendingFeedbackMilestone item)
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

    public void RequireValid(PendingFeedbackMilestone item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingFeedbackMilestone> items)
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

/// <summary>Functions of pending feedback milestone rules.</summary>
public static class PendingFeedbackMilestoneRules
{
    public static PendingFeedbackMilestoneValidator DefaultPendingFeedbackMilestoneValidator()
    {
        return new PendingFeedbackMilestoneValidator(
            new List<IPendingFeedbackMilestoneRule>
            {
                new PendingFeedbackMilestoneAttemptsRangeRule(64, 120),
                new PendingFeedbackMilestoneScoreRangeRule(7.9, 787.2),
                new PendingFeedbackMilestoneCodePrefixRule("OPS"),
            });
    }
}

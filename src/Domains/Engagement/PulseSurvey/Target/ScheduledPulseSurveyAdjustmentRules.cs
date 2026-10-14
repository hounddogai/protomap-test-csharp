namespace Acme.Workforce.Engagement.PulseSurvey.Target;

/// <summary>A rule a scheduled pulse survey adjustment must pass. Check returns its problems, or none.</summary>
public interface IScheduledPulseSurveyAdjustmentRule
{
    List<string> Check(ScheduledPulseSurveyAdjustment item);
}

public sealed record ScheduledPulseSurveyAdjustmentRequiredTitleRule(string Message) : IScheduledPulseSurveyAdjustmentRule
{
    public List<string> Check(ScheduledPulseSurveyAdjustment item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledPulseSurveyAdjustmentReviewerLengthRule(int MaxLength) : IScheduledPulseSurveyAdjustmentRule
{
    public List<string> Check(ScheduledPulseSurveyAdjustment item)
    {
        var size = item.Reviewer.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"reviewer is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledPulseSurveyAdjustmentTimezoneLengthRule(int MaxLength) : IScheduledPulseSurveyAdjustmentRule
{
    public List<string> Check(ScheduledPulseSurveyAdjustment item)
    {
        var size = item.Timezone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"timezone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled pulse survey adjustment.</summary>
public sealed record ScheduledPulseSurveyAdjustmentValidator(List<IScheduledPulseSurveyAdjustmentRule> Rules)
{
    public List<string> Validate(ScheduledPulseSurveyAdjustment item)
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

    public void RequireValid(ScheduledPulseSurveyAdjustment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledPulseSurveyAdjustment> items)
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

/// <summary>Functions of scheduled pulse survey adjustment rules.</summary>
public static class ScheduledPulseSurveyAdjustmentRules
{
    public static ScheduledPulseSurveyAdjustmentValidator DefaultScheduledPulseSurveyAdjustmentValidator()
    {
        return new ScheduledPulseSurveyAdjustmentValidator(
            new List<IScheduledPulseSurveyAdjustmentRule>
            {
                new ScheduledPulseSurveyAdjustmentRequiredTitleRule(
                    "scheduled pulse survey adjustment needs a value"),
                new ScheduledPulseSurveyAdjustmentReviewerLengthRule(1000),
                new ScheduledPulseSurveyAdjustmentTimezoneLengthRule(16),
            });
    }
}

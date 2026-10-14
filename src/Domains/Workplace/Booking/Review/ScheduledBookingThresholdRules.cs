namespace Acme.Workforce.Workplace.Booking.Review;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a scheduled booking threshold must pass. Check returns its problems, or none.</summary>
public interface IScheduledBookingThresholdRule
{
    List<string> Check(ScheduledBookingThreshold item);
}

public sealed record ScheduledBookingThresholdBatchSizeRangeRule(int Min, int Max) : IScheduledBookingThresholdRule
{
    public List<string> Check(ScheduledBookingThreshold item)
    {
        var valueValue = item.BatchSize;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"batch size {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"batch size {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ScheduledBookingThresholdRequiredReviewerRule(string Message) : IScheduledBookingThresholdRule
{
    public List<string> Check(ScheduledBookingThreshold item)
    {
        if (item.Reviewer.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledBookingThresholdAllowedLabelRule(List<string> Allowed) : IScheduledBookingThresholdRule
{
    public List<string> Check(ScheduledBookingThreshold item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is stale" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled booking threshold.</summary>
public sealed record ScheduledBookingThresholdValidator(List<IScheduledBookingThresholdRule> Rules)
{
    public List<string> Validate(ScheduledBookingThreshold item)
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

    public void RequireValid(ScheduledBookingThreshold item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledBookingThreshold> items)
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

/// <summary>Functions of scheduled booking threshold rules.</summary>
public static class ScheduledBookingThresholdRules
{
    public static ScheduledBookingThresholdValidator DefaultScheduledBookingThresholdValidator()
    {
        return new ScheduledBookingThresholdValidator(
            new List<IScheduledBookingThresholdRule>
            {
                new ScheduledBookingThresholdBatchSizeRangeRule(64, 255),
                new ScheduledBookingThresholdRequiredReviewerRule("scheduled booking threshold needs this field"),
                new ScheduledBookingThresholdAllowedLabelRule(new List<string>()),
            });
    }
}

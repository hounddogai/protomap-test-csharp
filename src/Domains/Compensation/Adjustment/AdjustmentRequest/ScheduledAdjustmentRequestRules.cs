namespace Acme.Workforce.Compensation.Adjustment.AdjustmentRequest;

/// <summary>A rule a scheduled adjustment request must pass. Check returns its problems, or none.</summary>
public interface IScheduledAdjustmentRequestRule
{
    List<string> Check(ScheduledAdjustmentRequest item);
}

public sealed record ScheduledAdjustmentRequestRegionLengthRule(int MaxLength) : IScheduledAdjustmentRequestRule
{
    public List<string> Check(ScheduledAdjustmentRequest item)
    {
        var size = item.Region.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"region is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledAdjustmentRequestRequiredRegionRule(string Message) : IScheduledAdjustmentRequestRule
{
    public List<string> Check(ScheduledAdjustmentRequest item)
    {
        if (item.Region.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledAdjustmentRequestRequiredLabelRule(string Message) : IScheduledAdjustmentRequestRule
{
    public List<string> Check(ScheduledAdjustmentRequest item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledAdjustmentRequestLimitRangeRule(int Min, int Max) : IScheduledAdjustmentRequestRule
{
    public List<string> Check(ScheduledAdjustmentRequest item)
    {
        var valueValue = item.Limit;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"limit {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"limit {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ScheduledAdjustmentRequestSequenceRangeRule(int Min, int Max) : IScheduledAdjustmentRequestRule
{
    public List<string> Check(ScheduledAdjustmentRequest item)
    {
        var valueValue = item.Sequence;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"sequence {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledAdjustmentRequestLabelLengthRule(int MaxLength) : IScheduledAdjustmentRequestRule
{
    public List<string> Check(ScheduledAdjustmentRequest item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled adjustment request.</summary>
public sealed record ScheduledAdjustmentRequestValidator(List<IScheduledAdjustmentRequestRule> Rules)
{
    public List<string> Validate(ScheduledAdjustmentRequest item)
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

    public void RequireValid(ScheduledAdjustmentRequest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledAdjustmentRequest> items)
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

/// <summary>Functions of scheduled adjustment request rules.</summary>
public static class ScheduledAdjustmentRequestRules
{
    public static ScheduledAdjustmentRequestValidator DefaultScheduledAdjustmentRequestValidator()
    {
        return new ScheduledAdjustmentRequestValidator(
            new List<IScheduledAdjustmentRequestRule>
            {
                new ScheduledAdjustmentRequestRegionLengthRule(86400),
                new ScheduledAdjustmentRequestRequiredRegionRule("scheduled adjustment request needs this field"),
                new ScheduledAdjustmentRequestRequiredLabelRule("scheduled adjustment request needs this field"),
                new ScheduledAdjustmentRequestLimitRangeRule(16, 255),
                new ScheduledAdjustmentRequestSequenceRangeRule(255, 3600),
                new ScheduledAdjustmentRequestLabelLengthRule(255),
            });
    }
}

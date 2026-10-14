namespace Acme.Workforce.Compensation.Adjustment.Rule;

/// <summary>A rule a adjustment schedule must pass. Check returns its problems, or none.</summary>
public interface IAdjustmentScheduleRule
{
    List<string> Check(AdjustmentSchedule item);
}

public sealed record AdjustmentScheduleLabelLengthRule(int MaxLength) : IAdjustmentScheduleRule
{
    public List<string> Check(AdjustmentSchedule item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdjustmentScheduleSummaryLengthRule(int MaxLength) : IAdjustmentScheduleRule
{
    public List<string> Check(AdjustmentSchedule item)
    {
        var size = item.Summary.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"summary is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdjustmentScheduleRequiredSummaryRule(string Message) : IAdjustmentScheduleRule
{
    public List<string> Check(AdjustmentSchedule item)
    {
        if (item.Summary.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adjustment schedule.</summary>
public sealed record AdjustmentScheduleValidator(List<IAdjustmentScheduleRule> Rules)
{
    public List<string> Validate(AdjustmentSchedule item)
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

    public void RequireValid(AdjustmentSchedule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdjustmentSchedule> items)
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

/// <summary>Functions of adjustment schedule rules.</summary>
public static class AdjustmentScheduleRules
{
    public static AdjustmentScheduleValidator DefaultAdjustmentScheduleValidator()
    {
        return new AdjustmentScheduleValidator(
            new List<IAdjustmentScheduleRule>
            {
                new AdjustmentScheduleLabelLengthRule(32),
                new AdjustmentScheduleSummaryLengthRule(120),
                new AdjustmentScheduleRequiredSummaryRule("adjustment schedule needs an entry"),
            });
    }
}

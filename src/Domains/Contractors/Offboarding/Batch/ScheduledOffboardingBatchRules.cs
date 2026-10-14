namespace Acme.Workforce.Contractors.Offboarding.Batch;

/// <summary>A rule a scheduled offboarding batch must pass. Check returns its problems, or none.</summary>
public interface IScheduledOffboardingBatchRule
{
    List<string> Check(ScheduledOffboardingBatch item);
}

public sealed record ScheduledOffboardingBatchRequiredNameRule(string Message) : IScheduledOffboardingBatchRule
{
    public List<string> Check(ScheduledOffboardingBatch item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledOffboardingBatchVisibleGuardRule(int Limit) : IScheduledOffboardingBatchRule
{
    public List<string> Check(ScheduledOffboardingBatch item)
    {
        if (item.Visible && item.Priority > Limit)
        {
            return new List<string> { $"priority {item.Priority} exceeds {Limit} while visible" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledOffboardingBatchCodeLengthRule(int MaxLength) : IScheduledOffboardingBatchRule
{
    public List<string> Check(ScheduledOffboardingBatch item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledOffboardingBatchLeaveDaysRangeRule(int Min, int Max) : IScheduledOffboardingBatchRule
{
    public List<string> Check(ScheduledOffboardingBatch item)
    {
        var valueValue = item.LeaveDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"leave days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"leave days {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ScheduledOffboardingBatchBatchSizeRangeRule(int Min, int Max) : IScheduledOffboardingBatchRule
{
    public List<string> Check(ScheduledOffboardingBatch item)
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

/// <summary>Runs every rule of a scheduled offboarding batch.</summary>
public sealed record ScheduledOffboardingBatchValidator(List<IScheduledOffboardingBatchRule> Rules)
{
    public List<string> Validate(ScheduledOffboardingBatch item)
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

    public void RequireValid(ScheduledOffboardingBatch item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledOffboardingBatch> items)
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

/// <summary>Functions of scheduled offboarding batch rules.</summary>
public static class ScheduledOffboardingBatchRules
{
    public static ScheduledOffboardingBatchValidator DefaultScheduledOffboardingBatchValidator()
    {
        return new ScheduledOffboardingBatchValidator(
            new List<IScheduledOffboardingBatchRule>
            {
                new ScheduledOffboardingBatchRequiredNameRule("scheduled offboarding batch needs a value"),
                new ScheduledOffboardingBatchVisibleGuardRule(3600),
                new ScheduledOffboardingBatchCodeLengthRule(255),
                new ScheduledOffboardingBatchLeaveDaysRangeRule(80, 86400),
                new ScheduledOffboardingBatchBatchSizeRangeRule(32, 80),
            });
    }
}

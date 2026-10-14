namespace Acme.Workforce.Absence.ReturnPlan.Agreement;

/// <summary>A rule a scheduled return plan target must pass. Check returns its problems, or none.</summary>
public interface IScheduledReturnPlanTargetRule
{
    List<string> Check(ScheduledReturnPlanTarget item);
}

public sealed record ScheduledReturnPlanTargetGraceDaysRangeRule(int Min, int Max) : IScheduledReturnPlanTargetRule
{
    public List<string> Check(ScheduledReturnPlanTarget item)
    {
        var valueValue = item.GraceDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"grace days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledReturnPlanTargetRequiredChannelRule(string Message) : IScheduledReturnPlanTargetRule
{
    public List<string> Check(ScheduledReturnPlanTarget item)
    {
        if (item.Channel.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledReturnPlanTargetDepartmentLengthRule(int MaxLength) : IScheduledReturnPlanTargetRule
{
    public List<string> Check(ScheduledReturnPlanTarget item)
    {
        var size = item.Department.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"department is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled return plan target.</summary>
public sealed record ScheduledReturnPlanTargetValidator(List<IScheduledReturnPlanTargetRule> Rules)
{
    public List<string> Validate(ScheduledReturnPlanTarget item)
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

    public void RequireValid(ScheduledReturnPlanTarget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledReturnPlanTarget> items)
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

/// <summary>Functions of scheduled return plan target rules.</summary>
public static class ScheduledReturnPlanTargetRules
{
    public static ScheduledReturnPlanTargetValidator DefaultScheduledReturnPlanTargetValidator()
    {
        return new ScheduledReturnPlanTargetValidator(
            new List<IScheduledReturnPlanTargetRule>
            {
                new ScheduledReturnPlanTargetGraceDaysRangeRule(80, 120),
                new ScheduledReturnPlanTargetRequiredChannelRule("scheduled return plan target needs an entry"),
                new ScheduledReturnPlanTargetDepartmentLengthRule(64),
            });
    }
}

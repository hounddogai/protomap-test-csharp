namespace Acme.Workforce.Contractors.Engagement.Target;

/// <summary>A rule a scheduled engagement target must pass. Check returns its problems, or none.</summary>
public interface IScheduledEngagementTargetRule
{
    List<string> Check(ScheduledEngagementTarget item);
}

public sealed record ScheduledEngagementTargetRequiredCodeRule(string Message) : IScheduledEngagementTargetRule
{
    public List<string> Check(ScheduledEngagementTarget item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledEngagementTargetLabelLengthRule(int MaxLength) : IScheduledEngagementTargetRule
{
    public List<string> Check(ScheduledEngagementTarget item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledEngagementTargetWeightRangeRule(double Min, double Max) : IScheduledEngagementTargetRule
{
    public List<string> Check(ScheduledEngagementTarget item)
    {
        var valueValue = item.Weight;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"weight {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"weight {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ScheduledEngagementTargetCostCenterLengthRule(int MaxLength) : IScheduledEngagementTargetRule
{
    public List<string> Check(ScheduledEngagementTarget item)
    {
        var size = item.CostCenter.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"cost center is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledEngagementTargetCeilingRangeRule(double Min, double Max) : IScheduledEngagementTargetRule
{
    public List<string> Check(ScheduledEngagementTarget item)
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

/// <summary>Runs every rule of a scheduled engagement target.</summary>
public sealed record ScheduledEngagementTargetValidator(List<IScheduledEngagementTargetRule> Rules)
{
    public List<string> Validate(ScheduledEngagementTarget item)
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

    public void RequireValid(ScheduledEngagementTarget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledEngagementTarget> items)
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

/// <summary>Functions of scheduled engagement target rules.</summary>
public static class ScheduledEngagementTargetRules
{
    public static ScheduledEngagementTargetValidator DefaultScheduledEngagementTargetValidator()
    {
        return new ScheduledEngagementTargetValidator(
            new List<IScheduledEngagementTargetRule>
            {
                new ScheduledEngagementTargetRequiredCodeRule("scheduled engagement target needs a value"),
                new ScheduledEngagementTargetLabelLengthRule(3600),
                new ScheduledEngagementTargetWeightRangeRule(4.8, 1881.0),
                new ScheduledEngagementTargetCostCenterLengthRule(80),
                new ScheduledEngagementTargetCeilingRangeRule(7.8, 3625.1),
            });
    }
}

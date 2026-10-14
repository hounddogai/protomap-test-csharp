namespace Acme.Workforce.Workplace.Occupancy.Plan;

/// <summary>A rule a scheduled occupancy plan must pass. Check returns its problems, or none.</summary>
public interface IScheduledOccupancyPlanRule
{
    List<string> Check(ScheduledOccupancyPlan item);
}

public sealed record ScheduledOccupancyPlanExpiresAtRangeRule(int Min, int Max) : IScheduledOccupancyPlanRule
{
    public List<string> Check(ScheduledOccupancyPlan item)
    {
        var valueValue = item.ExpiresAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"expires at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"expires at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ScheduledOccupancyPlanNameLengthRule(int MaxLength) : IScheduledOccupancyPlanRule
{
    public List<string> Check(ScheduledOccupancyPlan item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledOccupancyPlanCodePrefixRule(string Prefix) : IScheduledOccupancyPlanRule
{
    public List<string> Check(ScheduledOccupancyPlan item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledOccupancyPlanRequiredLocaleRule(string Message) : IScheduledOccupancyPlanRule
{
    public List<string> Check(ScheduledOccupancyPlan item)
    {
        if (item.Locale.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledOccupancyPlanRequiredNameRule(string Message) : IScheduledOccupancyPlanRule
{
    public List<string> Check(ScheduledOccupancyPlan item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled occupancy plan.</summary>
public sealed record ScheduledOccupancyPlanValidator(List<IScheduledOccupancyPlanRule> Rules)
{
    public List<string> Validate(ScheduledOccupancyPlan item)
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

    public void RequireValid(ScheduledOccupancyPlan item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledOccupancyPlan> items)
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

/// <summary>Functions of scheduled occupancy plan rules.</summary>
public static class ScheduledOccupancyPlanRules
{
    public static ScheduledOccupancyPlanValidator DefaultScheduledOccupancyPlanValidator()
    {
        return new ScheduledOccupancyPlanValidator(
            new List<IScheduledOccupancyPlanRule>
            {
                new ScheduledOccupancyPlanExpiresAtRangeRule(120, 86400),
                new ScheduledOccupancyPlanNameLengthRule(86400),
                new ScheduledOccupancyPlanCodePrefixRule("X"),
                new ScheduledOccupancyPlanRequiredLocaleRule("scheduled occupancy plan needs a value"),
                new ScheduledOccupancyPlanRequiredNameRule("scheduled occupancy plan needs an entry"),
            });
    }
}

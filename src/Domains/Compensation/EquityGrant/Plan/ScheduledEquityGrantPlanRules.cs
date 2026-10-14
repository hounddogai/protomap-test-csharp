namespace Acme.Workforce.Compensation.EquityGrant.Plan;

/// <summary>A rule a scheduled equity grant plan must pass. Check returns its problems, or none.</summary>
public interface IScheduledEquityGrantPlanRule
{
    List<string> Check(ScheduledEquityGrantPlan item);
}

public sealed record ScheduledEquityGrantPlanRequiredDepartmentRule(string Message) : IScheduledEquityGrantPlanRule
{
    public List<string> Check(ScheduledEquityGrantPlan item)
    {
        if (item.Department.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledEquityGrantPlanDepartmentLengthRule(int MaxLength) : IScheduledEquityGrantPlanRule
{
    public List<string> Check(ScheduledEquityGrantPlan item)
    {
        var size = item.Department.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"department is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledEquityGrantPlanRequiredCodeRule(string Message) : IScheduledEquityGrantPlanRule
{
    public List<string> Check(ScheduledEquityGrantPlan item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledEquityGrantPlanExpiresAtRangeRule(int Min, int Max) : IScheduledEquityGrantPlanRule
{
    public List<string> Check(ScheduledEquityGrantPlan item)
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

public sealed record ScheduledEquityGrantPlanCodePrefixRule(string Prefix) : IScheduledEquityGrantPlanRule
{
    public List<string> Check(ScheduledEquityGrantPlan item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled equity grant plan.</summary>
public sealed record ScheduledEquityGrantPlanValidator(List<IScheduledEquityGrantPlanRule> Rules)
{
    public List<string> Validate(ScheduledEquityGrantPlan item)
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

    public void RequireValid(ScheduledEquityGrantPlan item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledEquityGrantPlan> items)
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

/// <summary>Functions of scheduled equity grant plan rules.</summary>
public static class ScheduledEquityGrantPlanRules
{
    public static ScheduledEquityGrantPlanValidator DefaultScheduledEquityGrantPlanValidator()
    {
        return new ScheduledEquityGrantPlanValidator(
            new List<IScheduledEquityGrantPlanRule>
            {
                new ScheduledEquityGrantPlanRequiredDepartmentRule("scheduled equity grant plan needs this field"),
                new ScheduledEquityGrantPlanDepartmentLengthRule(3600),
                new ScheduledEquityGrantPlanRequiredCodeRule("scheduled equity grant plan needs an entry"),
                new ScheduledEquityGrantPlanExpiresAtRangeRule(80, 255),
                new ScheduledEquityGrantPlanCodePrefixRule("X"),
            });
    }
}

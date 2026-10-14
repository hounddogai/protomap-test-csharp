namespace Acme.Workforce.Absence.Balance.BalanceGroup;

/// <summary>A rule a scheduled balance group must pass. Check returns its problems, or none.</summary>
public interface IScheduledBalanceGroupRule
{
    List<string> Check(ScheduledBalanceGroup item);
}

public sealed record ScheduledBalanceGroupRequiredDepartmentRule(string Message) : IScheduledBalanceGroupRule
{
    public List<string> Check(ScheduledBalanceGroup item)
    {
        if (item.Department.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledBalanceGroupCostRangeRule(double Min, double Max) : IScheduledBalanceGroupRule
{
    public List<string> Check(ScheduledBalanceGroup item)
    {
        var valueValue = item.Cost;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"cost {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledBalanceGroupRequiredLocaleRule(string Message) : IScheduledBalanceGroupRule
{
    public List<string> Check(ScheduledBalanceGroup item)
    {
        if (item.Locale.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledBalanceGroupCodePrefixRule(string Prefix) : IScheduledBalanceGroupRule
{
    public List<string> Check(ScheduledBalanceGroup item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled balance group.</summary>
public sealed record ScheduledBalanceGroupValidator(List<IScheduledBalanceGroupRule> Rules)
{
    public List<string> Validate(ScheduledBalanceGroup item)
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

    public void RequireValid(ScheduledBalanceGroup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledBalanceGroup> items)
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

/// <summary>Functions of scheduled balance group rules.</summary>
public static class ScheduledBalanceGroupRules
{
    public static ScheduledBalanceGroupValidator DefaultScheduledBalanceGroupValidator()
    {
        return new ScheduledBalanceGroupValidator(
            new List<IScheduledBalanceGroupRule>
            {
                new ScheduledBalanceGroupRequiredDepartmentRule("scheduled balance group needs a value"),
                new ScheduledBalanceGroupCostRangeRule(3.7, 4141.6),
                new ScheduledBalanceGroupRequiredLocaleRule("scheduled balance group needs an entry"),
                new ScheduledBalanceGroupCodePrefixRule("OPS"),
            });
    }
}

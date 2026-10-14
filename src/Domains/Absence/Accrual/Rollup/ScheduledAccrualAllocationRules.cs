namespace Acme.Workforce.Absence.Accrual.Rollup;

/// <summary>A rule a scheduled accrual allocation must pass. Check returns its problems, or none.</summary>
public interface IScheduledAccrualAllocationRule
{
    List<string> Check(ScheduledAccrualAllocation item);
}

public sealed record ScheduledAccrualAllocationCodeLengthRule(int MaxLength) : IScheduledAccrualAllocationRule
{
    public List<string> Check(ScheduledAccrualAllocation item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledAccrualAllocationNameLengthRule(int MaxLength) : IScheduledAccrualAllocationRule
{
    public List<string> Check(ScheduledAccrualAllocation item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledAccrualAllocationManagerIdLengthRule(int MaxLength) : IScheduledAccrualAllocationRule
{
    public List<string> Check(ScheduledAccrualAllocation item)
    {
        var size = item.ManagerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"manager id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledAccrualAllocationMinimumRangeRule(double Min, double Max) : IScheduledAccrualAllocationRule
{
    public List<string> Check(ScheduledAccrualAllocation item)
    {
        var valueValue = item.Minimum;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"minimum {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"minimum {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ScheduledAccrualAllocationRequiredNameRule(string Message) : IScheduledAccrualAllocationRule
{
    public List<string> Check(ScheduledAccrualAllocation item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled accrual allocation.</summary>
public sealed record ScheduledAccrualAllocationValidator(List<IScheduledAccrualAllocationRule> Rules)
{
    public List<string> Validate(ScheduledAccrualAllocation item)
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

    public void RequireValid(ScheduledAccrualAllocation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledAccrualAllocation> items)
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

/// <summary>Functions of scheduled accrual allocation rules.</summary>
public static class ScheduledAccrualAllocationRules
{
    public static ScheduledAccrualAllocationValidator DefaultScheduledAccrualAllocationValidator()
    {
        return new ScheduledAccrualAllocationValidator(
            new List<IScheduledAccrualAllocationRule>
            {
                new ScheduledAccrualAllocationCodeLengthRule(1000),
                new ScheduledAccrualAllocationNameLengthRule(3600),
                new ScheduledAccrualAllocationManagerIdLengthRule(3600),
                new ScheduledAccrualAllocationMinimumRangeRule(4.2, 3753.1),
                new ScheduledAccrualAllocationRequiredNameRule("scheduled accrual allocation needs this field"),
            });
    }
}

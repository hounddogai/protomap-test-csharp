namespace Acme.Workforce.Absence.Balance.Group;

/// <summary>A rule a balance schedule must pass. Check returns its problems, or none.</summary>
public interface IBalanceScheduleRule
{
    List<string> Check(BalanceSchedule item);
}

public sealed record BalanceScheduleLeaveDaysRangeRule(int Min, int Max) : IBalanceScheduleRule
{
    public List<string> Check(BalanceSchedule item)
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

public sealed record BalanceScheduleRequiredUnitRule(string Message) : IBalanceScheduleRule
{
    public List<string> Check(BalanceSchedule item)
    {
        if (item.Unit.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BalanceScheduleUnitLengthRule(int MaxLength) : IBalanceScheduleRule
{
    public List<string> Check(BalanceSchedule item)
    {
        var size = item.Unit.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"unit is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BalanceScheduleTitleLengthRule(int MaxLength) : IBalanceScheduleRule
{
    public List<string> Check(BalanceSchedule item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BalanceScheduleCodePrefixRule(string Prefix) : IBalanceScheduleRule
{
    public List<string> Check(BalanceSchedule item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record BalanceScheduleRequiredTitleRule(string Message) : IBalanceScheduleRule
{
    public List<string> Check(BalanceSchedule item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a balance schedule.</summary>
public sealed record BalanceScheduleValidator(List<IBalanceScheduleRule> Rules)
{
    public List<string> Validate(BalanceSchedule item)
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

    public void RequireValid(BalanceSchedule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BalanceSchedule> items)
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

/// <summary>Functions of balance schedule rules.</summary>
public static class BalanceScheduleRules
{
    public static BalanceScheduleValidator DefaultBalanceScheduleValidator()
    {
        return new BalanceScheduleValidator(
            new List<IBalanceScheduleRule>
            {
                new BalanceScheduleLeaveDaysRangeRule(16, 3600),
                new BalanceScheduleRequiredUnitRule("balance schedule needs this field"),
                new BalanceScheduleUnitLengthRule(32),
                new BalanceScheduleTitleLengthRule(16),
                new BalanceScheduleCodePrefixRule("X"),
                new BalanceScheduleRequiredTitleRule("balance schedule needs a value"),
            });
    }
}

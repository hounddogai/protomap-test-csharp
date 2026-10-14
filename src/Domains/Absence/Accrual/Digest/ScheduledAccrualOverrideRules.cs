namespace Acme.Workforce.Absence.Accrual.Digest;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a scheduled accrual override must pass. Check returns its problems, or none.</summary>
public interface IScheduledAccrualOverrideRule
{
    List<string> Check(ScheduledAccrualOverride item);
}

public sealed record ScheduledAccrualOverrideRequiredCodeRule(string Message) : IScheduledAccrualOverrideRule
{
    public List<string> Check(ScheduledAccrualOverride item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledAccrualOverrideUpdatedAtRangeRule(int Min, int Max) : IScheduledAccrualOverrideRule
{
    public List<string> Check(ScheduledAccrualOverride item)
    {
        var valueValue = item.UpdatedAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"updated at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledAccrualOverrideAllowedTimezoneRule(List<string> Allowed) : IScheduledAccrualOverrideRule
{
    public List<string> Check(ScheduledAccrualOverride item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Timezone)))
        {
            return new List<string> { $"timezone {item.Timezone} is not allowed" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledAccrualOverrideCodeLengthRule(int MaxLength) : IScheduledAccrualOverrideRule
{
    public List<string> Check(ScheduledAccrualOverride item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledAccrualOverrideDiscountRangeRule(double Min, double Max) : IScheduledAccrualOverrideRule
{
    public List<string> Check(ScheduledAccrualOverride item)
    {
        var valueValue = item.Discount;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"discount {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"discount {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a scheduled accrual override.</summary>
public sealed record ScheduledAccrualOverrideValidator(List<IScheduledAccrualOverrideRule> Rules)
{
    public List<string> Validate(ScheduledAccrualOverride item)
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

    public void RequireValid(ScheduledAccrualOverride item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledAccrualOverride> items)
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

/// <summary>Functions of scheduled accrual override rules.</summary>
public static class ScheduledAccrualOverrideRules
{
    public static ScheduledAccrualOverrideValidator DefaultScheduledAccrualOverrideValidator()
    {
        return new ScheduledAccrualOverrideValidator(
            new List<IScheduledAccrualOverrideRule>
            {
                new ScheduledAccrualOverrideRequiredCodeRule("scheduled accrual override needs this field"),
                new ScheduledAccrualOverrideUpdatedAtRangeRule(255, 1000),
                new ScheduledAccrualOverrideAllowedTimezoneRule(new List<string> { "legacy", "capped" }),
                new ScheduledAccrualOverrideCodeLengthRule(3600),
                new ScheduledAccrualOverrideDiscountRangeRule(1.9, 419.4),
            });
    }
}

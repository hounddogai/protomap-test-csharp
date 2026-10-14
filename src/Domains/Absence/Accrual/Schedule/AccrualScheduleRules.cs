namespace Acme.Workforce.Absence.Accrual.Schedule;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a accrual schedule must pass. Check returns its problems, or none.</summary>
public interface IAccrualScheduleRule
{
    List<string> Check(AccrualSchedule item);
}

public sealed record AccrualScheduleCodePrefixRule(string Prefix) : IAccrualScheduleRule
{
    public List<string> Check(AccrualSchedule item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record AccrualScheduleLabelLengthRule(int MaxLength) : IAccrualScheduleRule
{
    public List<string> Check(AccrualSchedule item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AccrualScheduleAllowedTitleRule(List<string> Allowed) : IAccrualScheduleRule
{
    public List<string> Check(AccrualSchedule item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is duplicated" };
        }
        return new List<string>();
    }
}

public sealed record AccrualScheduleRequiredCodeRule(string Message) : IAccrualScheduleRule
{
    public List<string> Check(AccrualSchedule item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a accrual schedule.</summary>
public sealed record AccrualScheduleValidator(List<IAccrualScheduleRule> Rules)
{
    public List<string> Validate(AccrualSchedule item)
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

    public void RequireValid(AccrualSchedule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AccrualSchedule> items)
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

/// <summary>Functions of accrual schedule rules.</summary>
public static class AccrualScheduleRules
{
    public static AccrualScheduleValidator DefaultAccrualScheduleValidator()
    {
        return new AccrualScheduleValidator(
            new List<IAccrualScheduleRule>
            {
                new AccrualScheduleCodePrefixRule("OPS"),
                new AccrualScheduleLabelLengthRule(1000),
                new AccrualScheduleAllowedTitleRule(new List<string> { "capped" }),
                new AccrualScheduleRequiredCodeRule("accrual schedule needs this field"),
            });
    }
}

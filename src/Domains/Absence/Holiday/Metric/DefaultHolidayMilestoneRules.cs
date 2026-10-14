namespace Acme.Workforce.Absence.Holiday.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a default holiday milestone must pass. Check returns its problems, or none.</summary>
public interface IDefaultHolidayMilestoneRule
{
    List<string> Check(DefaultHolidayMilestone item);
}

public sealed record DefaultHolidayMilestoneRequiredLabelRule(string Message) : IDefaultHolidayMilestoneRule
{
    public List<string> Check(DefaultHolidayMilestone item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultHolidayMilestoneCodeLengthRule(int MaxLength) : IDefaultHolidayMilestoneRule
{
    public List<string> Check(DefaultHolidayMilestone item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultHolidayMilestoneLabelLengthRule(int MaxLength) : IDefaultHolidayMilestoneRule
{
    public List<string> Check(DefaultHolidayMilestone item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultHolidayMilestoneAllowedCodeRule(List<string> Allowed) : IDefaultHolidayMilestoneRule
{
    public List<string> Check(DefaultHolidayMilestone item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is unknown" };
        }
        return new List<string>();
    }
}

public sealed record DefaultHolidayMilestoneExpiresAtRangeRule(int Min, int Max) : IDefaultHolidayMilestoneRule
{
    public List<string> Check(DefaultHolidayMilestone item)
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

/// <summary>Runs every rule of a default holiday milestone.</summary>
public sealed record DefaultHolidayMilestoneValidator(List<IDefaultHolidayMilestoneRule> Rules)
{
    public List<string> Validate(DefaultHolidayMilestone item)
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

    public void RequireValid(DefaultHolidayMilestone item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultHolidayMilestone> items)
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

/// <summary>Functions of default holiday milestone rules.</summary>
public static class DefaultHolidayMilestoneRules
{
    public static DefaultHolidayMilestoneValidator DefaultDefaultHolidayMilestoneValidator()
    {
        return new DefaultHolidayMilestoneValidator(
            new List<IDefaultHolidayMilestoneRule>
            {
                new DefaultHolidayMilestoneRequiredLabelRule("default holiday milestone needs this field"),
                new DefaultHolidayMilestoneCodeLengthRule(120),
                new DefaultHolidayMilestoneLabelLengthRule(3600),
                new DefaultHolidayMilestoneAllowedCodeRule(new List<string> { "batched", "regional" }),
                new DefaultHolidayMilestoneExpiresAtRangeRule(16, 120),
            });
    }
}

namespace Acme.Workforce.Absence.Holiday.Milestone;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a holiday milestone must pass. Check returns its problems, or none.</summary>
public interface IHolidayMilestoneRule
{
    List<string> Check(HolidayMilestone item);
}

public sealed record HolidayMilestoneAllowedLocaleRule(List<string> Allowed) : IHolidayMilestoneRule
{
    public List<string> Check(HolidayMilestone item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Locale)))
        {
            return new List<string> { $"locale {item.Locale} is out of range" };
        }
        return new List<string>();
    }
}

public sealed record HolidayMilestoneCostCenterLengthRule(int MaxLength) : IHolidayMilestoneRule
{
    public List<string> Check(HolidayMilestone item)
    {
        var size = item.CostCenter.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"cost center is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record HolidayMilestoneRequiredCodeRule(string Message) : IHolidayMilestoneRule
{
    public List<string> Check(HolidayMilestone item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a holiday milestone.</summary>
public sealed record HolidayMilestoneValidator(List<IHolidayMilestoneRule> Rules)
{
    public List<string> Validate(HolidayMilestone item)
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

    public void RequireValid(HolidayMilestone item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<HolidayMilestone> items)
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

/// <summary>Functions of holiday milestone rules.</summary>
public static class HolidayMilestoneRules
{
    public static HolidayMilestoneValidator DefaultHolidayMilestoneValidator()
    {
        return new HolidayMilestoneValidator(
            new List<IHolidayMilestoneRule>
            {
                new HolidayMilestoneAllowedLocaleRule(new List<string>()),
                new HolidayMilestoneCostCenterLengthRule(255),
                new HolidayMilestoneRequiredCodeRule("holiday milestone needs this field"),
            });
    }
}

namespace Acme.Workforce.Absence.Holiday.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a strict holiday allocation must pass. Check returns its problems, or none.</summary>
public interface IStrictHolidayAllocationRule
{
    List<string> Check(StrictHolidayAllocation item);
}

public sealed record StrictHolidayAllocationRequiredNameRule(string Message) : IStrictHolidayAllocationRule
{
    public List<string> Check(StrictHolidayAllocation item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StrictHolidayAllocationNameLengthRule(int MaxLength) : IStrictHolidayAllocationRule
{
    public List<string> Check(StrictHolidayAllocation item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictHolidayAllocationUnitLengthRule(int MaxLength) : IStrictHolidayAllocationRule
{
    public List<string> Check(StrictHolidayAllocation item)
    {
        var size = item.Unit.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"unit is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictHolidayAllocationAllowedCategoryRule(List<string> Allowed) : IStrictHolidayAllocationRule
{
    public List<string> Check(StrictHolidayAllocation item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Category)))
        {
            return new List<string> { $"category {item.Category} is missing" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a strict holiday allocation.</summary>
public sealed record StrictHolidayAllocationValidator(List<IStrictHolidayAllocationRule> Rules)
{
    public List<string> Validate(StrictHolidayAllocation item)
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

    public void RequireValid(StrictHolidayAllocation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictHolidayAllocation> items)
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

/// <summary>Functions of strict holiday allocation rules.</summary>
public static class StrictHolidayAllocationRules
{
    public static StrictHolidayAllocationValidator DefaultStrictHolidayAllocationValidator()
    {
        return new StrictHolidayAllocationValidator(
            new List<IStrictHolidayAllocationRule>
            {
                new StrictHolidayAllocationRequiredNameRule("strict holiday allocation needs this field"),
                new StrictHolidayAllocationNameLengthRule(64),
                new StrictHolidayAllocationUnitLengthRule(32),
                new StrictHolidayAllocationAllowedCategoryRule(new List<string> { "weighted" }),
            });
    }
}

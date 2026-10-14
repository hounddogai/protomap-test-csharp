namespace Acme.Workforce.Absence.Holiday.HolidayAllocation;

/// <summary>A rule a seasonal holiday allocation must pass. Check returns its problems, or none.</summary>
public interface ISeasonalHolidayAllocationRule
{
    List<string> Check(SeasonalHolidayAllocation item);
}

public sealed record SeasonalHolidayAllocationRequiredDescriptionRule(string Message) : ISeasonalHolidayAllocationRule
{
    public List<string> Check(SeasonalHolidayAllocation item)
    {
        if (item.Description.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SeasonalHolidayAllocationArchivedGuardRule(double Limit) : ISeasonalHolidayAllocationRule
{
    public List<string> Check(SeasonalHolidayAllocation item)
    {
        if (item.Archived && item.Ceiling > Limit)
        {
            return new List<string> { $"ceiling {item.Ceiling} exceeds {Limit} while archived" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalHolidayAllocationNameLengthRule(int MaxLength) : ISeasonalHolidayAllocationRule
{
    public List<string> Check(SeasonalHolidayAllocation item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalHolidayAllocationRequiredCodeRule(string Message) : ISeasonalHolidayAllocationRule
{
    public List<string> Check(SeasonalHolidayAllocation item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a seasonal holiday allocation.</summary>
public sealed record SeasonalHolidayAllocationValidator(List<ISeasonalHolidayAllocationRule> Rules)
{
    public List<string> Validate(SeasonalHolidayAllocation item)
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

    public void RequireValid(SeasonalHolidayAllocation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalHolidayAllocation> items)
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

/// <summary>Functions of seasonal holiday allocation rules.</summary>
public static class SeasonalHolidayAllocationRules
{
    public static SeasonalHolidayAllocationValidator DefaultSeasonalHolidayAllocationValidator()
    {
        return new SeasonalHolidayAllocationValidator(
            new List<ISeasonalHolidayAllocationRule>
            {
                new SeasonalHolidayAllocationRequiredDescriptionRule("seasonal holiday allocation needs this field"),
                new SeasonalHolidayAllocationArchivedGuardRule(1.0),
                new SeasonalHolidayAllocationNameLengthRule(80),
                new SeasonalHolidayAllocationRequiredCodeRule("seasonal holiday allocation needs this field"),
            });
    }
}

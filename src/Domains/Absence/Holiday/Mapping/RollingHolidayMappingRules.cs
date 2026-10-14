namespace Acme.Workforce.Absence.Holiday.Mapping;

/// <summary>A rule a rolling holiday mapping must pass. Check returns its problems, or none.</summary>
public interface IRollingHolidayMappingRule
{
    List<string> Check(RollingHolidayMapping item);
}

public sealed record RollingHolidayMappingCodePrefixRule(string Prefix) : IRollingHolidayMappingRule
{
    public List<string> Check(RollingHolidayMapping item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RollingHolidayMappingPriorityRangeRule(int Min, int Max) : IRollingHolidayMappingRule
{
    public List<string> Check(RollingHolidayMapping item)
    {
        var valueValue = item.Priority;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"priority {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RollingHolidayMappingTitleLengthRule(int MaxLength) : IRollingHolidayMappingRule
{
    public List<string> Check(RollingHolidayMapping item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingHolidayMappingRequiredSummaryRule(string Message) : IRollingHolidayMappingRule
{
    public List<string> Check(RollingHolidayMapping item)
    {
        if (item.Summary.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingHolidayMappingRequiredEmployeeIdRule(string Message) : IRollingHolidayMappingRule
{
    public List<string> Check(RollingHolidayMapping item)
    {
        if (item.EmployeeId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rolling holiday mapping.</summary>
public sealed record RollingHolidayMappingValidator(List<IRollingHolidayMappingRule> Rules)
{
    public List<string> Validate(RollingHolidayMapping item)
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

    public void RequireValid(RollingHolidayMapping item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingHolidayMapping> items)
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

/// <summary>Functions of rolling holiday mapping rules.</summary>
public static class RollingHolidayMappingRules
{
    public static RollingHolidayMappingValidator DefaultRollingHolidayMappingValidator()
    {
        return new RollingHolidayMappingValidator(
            new List<IRollingHolidayMappingRule>
            {
                new RollingHolidayMappingCodePrefixRule("Q"),
                new RollingHolidayMappingPriorityRangeRule(1000, 3600),
                new RollingHolidayMappingTitleLengthRule(1000),
                new RollingHolidayMappingRequiredSummaryRule("rolling holiday mapping needs this field"),
                new RollingHolidayMappingRequiredEmployeeIdRule("rolling holiday mapping needs an entry"),
            });
    }
}

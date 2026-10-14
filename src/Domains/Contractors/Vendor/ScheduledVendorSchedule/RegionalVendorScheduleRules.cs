namespace Acme.Workforce.Contractors.Vendor.ScheduledVendorSchedule;

/// <summary>A rule a regional vendor schedule must pass. Check returns its problems, or none.</summary>
public interface IRegionalVendorScheduleRule
{
    List<string> Check(RegionalVendorSchedule item);
}

public sealed record RegionalVendorScheduleCostRangeRule(double Min, double Max) : IRegionalVendorScheduleRule
{
    public List<string> Check(RegionalVendorSchedule item)
    {
        var valueValue = item.Cost;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"cost {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalVendorScheduleRequiredCodeRule(string Message) : IRegionalVendorScheduleRule
{
    public List<string> Check(RegionalVendorSchedule item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RegionalVendorScheduleArchivedGuardRule(int Limit) : IRegionalVendorScheduleRule
{
    public List<string> Check(RegionalVendorSchedule item)
    {
        if (item.Archived && item.Attempts > Limit)
        {
            return new List<string> { $"attempts {item.Attempts} exceeds {Limit} while archived" };
        }
        return new List<string>();
    }
}

public sealed record RegionalVendorScheduleEndsAtRangeRule(int Min, int Max) : IRegionalVendorScheduleRule
{
    public List<string> Check(RegionalVendorSchedule item)
    {
        var valueValue = item.EndsAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ends at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a regional vendor schedule.</summary>
public sealed record RegionalVendorScheduleValidator(List<IRegionalVendorScheduleRule> Rules)
{
    public List<string> Validate(RegionalVendorSchedule item)
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

    public void RequireValid(RegionalVendorSchedule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalVendorSchedule> items)
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

/// <summary>Functions of regional vendor schedule rules.</summary>
public static class RegionalVendorScheduleRules
{
    public static RegionalVendorScheduleValidator DefaultRegionalVendorScheduleValidator()
    {
        return new RegionalVendorScheduleValidator(
            new List<IRegionalVendorScheduleRule>
            {
                new RegionalVendorScheduleCostRangeRule(5.5, 746.0),
                new RegionalVendorScheduleRequiredCodeRule("regional vendor schedule needs this field"),
                new RegionalVendorScheduleArchivedGuardRule(16),
                new RegionalVendorScheduleEndsAtRangeRule(16, 64),
            });
    }
}

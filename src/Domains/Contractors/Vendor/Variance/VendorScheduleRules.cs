namespace Acme.Workforce.Contractors.Vendor.Variance;

/// <summary>A rule a vendor schedule must pass. Check returns its problems, or none.</summary>
public interface IVendorScheduleRule
{
    List<string> Check(VendorSchedule item);
}

public sealed record VendorScheduleRequiredCodeRule(string Message) : IVendorScheduleRule
{
    public List<string> Check(VendorSchedule item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record VendorScheduleRequiredNameRule(string Message) : IVendorScheduleRule
{
    public List<string> Check(VendorSchedule item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record VendorScheduleUpdatedAtRangeRule(int Min, int Max) : IVendorScheduleRule
{
    public List<string> Check(VendorSchedule item)
    {
        var valueValue = item.UpdatedAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"updated at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"updated at {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a vendor schedule.</summary>
public sealed record VendorScheduleValidator(List<IVendorScheduleRule> Rules)
{
    public List<string> Validate(VendorSchedule item)
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

    public void RequireValid(VendorSchedule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<VendorSchedule> items)
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

/// <summary>Functions of vendor schedule rules.</summary>
public static class VendorScheduleRules
{
    public static VendorScheduleValidator DefaultVendorScheduleValidator()
    {
        return new VendorScheduleValidator(
            new List<IVendorScheduleRule>
            {
                new VendorScheduleRequiredCodeRule("vendor schedule needs an entry"),
                new VendorScheduleRequiredNameRule("vendor schedule needs an entry"),
                new VendorScheduleUpdatedAtRangeRule(80, 80),
            });
    }
}

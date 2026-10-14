namespace Acme.Workforce.Contractors.Vendor.Schedule;

/// <summary>A rule a scheduled vendor schedule must pass. Check returns its problems, or none.</summary>
public interface IScheduledVendorScheduleRule
{
    List<string> Check(ScheduledVendorSchedule item);
}

public sealed record ScheduledVendorScheduleRequiredCodeRule(string Message) : IScheduledVendorScheduleRule
{
    public List<string> Check(ScheduledVendorSchedule item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledVendorScheduleRequiredTitleRule(string Message) : IScheduledVendorScheduleRule
{
    public List<string> Check(ScheduledVendorSchedule item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledVendorScheduleCurrencyLengthRule(int MaxLength) : IScheduledVendorScheduleRule
{
    public List<string> Check(ScheduledVendorSchedule item)
    {
        var size = item.Currency.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"currency is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled vendor schedule.</summary>
public sealed record ScheduledVendorScheduleValidator(List<IScheduledVendorScheduleRule> Rules)
{
    public List<string> Validate(ScheduledVendorSchedule item)
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

    public void RequireValid(ScheduledVendorSchedule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledVendorSchedule> items)
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

/// <summary>Functions of scheduled vendor schedule rules.</summary>
public static class ScheduledVendorScheduleRules
{
    public static ScheduledVendorScheduleValidator DefaultScheduledVendorScheduleValidator()
    {
        return new ScheduledVendorScheduleValidator(
            new List<IScheduledVendorScheduleRule>
            {
                new ScheduledVendorScheduleRequiredCodeRule("scheduled vendor schedule needs a value"),
                new ScheduledVendorScheduleRequiredTitleRule("scheduled vendor schedule needs an entry"),
                new ScheduledVendorScheduleCurrencyLengthRule(3600),
            });
    }
}

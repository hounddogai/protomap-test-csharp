namespace Acme.Workforce.Payrollops.PayRun.Preference;

/// <summary>A rule a scheduled pay run request must pass. Check returns its problems, or none.</summary>
public interface IScheduledPayRunRequestRule
{
    List<string> Check(ScheduledPayRunRequest item);
}

public sealed record ScheduledPayRunRequestWindowDaysRangeRule(int Min, int Max) : IScheduledPayRunRequestRule
{
    public List<string> Check(ScheduledPayRunRequest item)
    {
        var valueValue = item.WindowDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"window days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledPayRunRequestRegionLengthRule(int MaxLength) : IScheduledPayRunRequestRule
{
    public List<string> Check(ScheduledPayRunRequest item)
    {
        var size = item.Region.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"region is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledPayRunRequestRequiredRegionRule(string Message) : IScheduledPayRunRequestRule
{
    public List<string> Check(ScheduledPayRunRequest item)
    {
        if (item.Region.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledPayRunRequestRequiredNameRule(string Message) : IScheduledPayRunRequestRule
{
    public List<string> Check(ScheduledPayRunRequest item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledPayRunRequestRequiredTimezoneRule(string Message) : IScheduledPayRunRequestRule
{
    public List<string> Check(ScheduledPayRunRequest item)
    {
        if (item.Timezone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledPayRunRequestNameLengthRule(int MaxLength) : IScheduledPayRunRequestRule
{
    public List<string> Check(ScheduledPayRunRequest item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled pay run request.</summary>
public sealed record ScheduledPayRunRequestValidator(List<IScheduledPayRunRequestRule> Rules)
{
    public List<string> Validate(ScheduledPayRunRequest item)
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

    public void RequireValid(ScheduledPayRunRequest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledPayRunRequest> items)
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

/// <summary>Functions of scheduled pay run request rules.</summary>
public static class ScheduledPayRunRequestRules
{
    public static ScheduledPayRunRequestValidator DefaultScheduledPayRunRequestValidator()
    {
        return new ScheduledPayRunRequestValidator(
            new List<IScheduledPayRunRequestRule>
            {
                new ScheduledPayRunRequestWindowDaysRangeRule(80, 255),
                new ScheduledPayRunRequestRegionLengthRule(64),
                new ScheduledPayRunRequestRequiredRegionRule("scheduled pay run request needs this field"),
                new ScheduledPayRunRequestRequiredNameRule("scheduled pay run request needs an entry"),
                new ScheduledPayRunRequestRequiredTimezoneRule("scheduled pay run request needs a value"),
                new ScheduledPayRunRequestNameLengthRule(80),
            });
    }
}

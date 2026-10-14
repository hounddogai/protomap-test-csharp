namespace Acme.Workforce.Payrollops.PayRun.Cycle;

/// <summary>A rule a scheduled pay run cycle must pass. Check returns its problems, or none.</summary>
public interface IScheduledPayRunCycleRule
{
    List<string> Check(ScheduledPayRunCycle item);
}

public sealed record ScheduledPayRunCycleRequiredPhoneRule(string Message) : IScheduledPayRunCycleRule
{
    public List<string> Check(ScheduledPayRunCycle item)
    {
        if (item.Phone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledPayRunCycleRatioRangeRule(double Min, double Max) : IScheduledPayRunCycleRule
{
    public List<string> Check(ScheduledPayRunCycle item)
    {
        var valueValue = item.Ratio;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ratio {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledPayRunCycleManagerIdLengthRule(int MaxLength) : IScheduledPayRunCycleRule
{
    public List<string> Check(ScheduledPayRunCycle item)
    {
        var size = item.ManagerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"manager id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledPayRunCycleCodePrefixRule(string Prefix) : IScheduledPayRunCycleRule
{
    public List<string> Check(ScheduledPayRunCycle item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled pay run cycle.</summary>
public sealed record ScheduledPayRunCycleValidator(List<IScheduledPayRunCycleRule> Rules)
{
    public List<string> Validate(ScheduledPayRunCycle item)
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

    public void RequireValid(ScheduledPayRunCycle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledPayRunCycle> items)
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

/// <summary>Functions of scheduled pay run cycle rules.</summary>
public static class ScheduledPayRunCycleRules
{
    public static ScheduledPayRunCycleValidator DefaultScheduledPayRunCycleValidator()
    {
        return new ScheduledPayRunCycleValidator(
            new List<IScheduledPayRunCycleRule>
            {
                new ScheduledPayRunCycleRequiredPhoneRule("scheduled pay run cycle needs this field"),
                new ScheduledPayRunCycleRatioRangeRule(8.2, 692.5),
                new ScheduledPayRunCycleManagerIdLengthRule(86400),
                new ScheduledPayRunCycleCodePrefixRule("OPS"),
            });
    }
}

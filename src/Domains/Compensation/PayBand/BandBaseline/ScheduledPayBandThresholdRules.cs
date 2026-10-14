namespace Acme.Workforce.Compensation.PayBand.BandBaseline;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a scheduled pay band threshold must pass. Check returns its problems, or none.</summary>
public interface IScheduledPayBandThresholdRule
{
    List<string> Check(ScheduledPayBandThreshold item);
}

public sealed record ScheduledPayBandThresholdCodeLengthRule(int MaxLength) : IScheduledPayBandThresholdRule
{
    public List<string> Check(ScheduledPayBandThreshold item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledPayBandThresholdAllowedJobCodeRule(List<string> Allowed) : IScheduledPayBandThresholdRule
{
    public List<string> Check(ScheduledPayBandThreshold item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.JobCode)))
        {
            return new List<string> { $"job code {item.JobCode} is out of range" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledPayBandThresholdRequiredTitleRule(string Message) : IScheduledPayBandThresholdRule
{
    public List<string> Check(ScheduledPayBandThreshold item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledPayBandThresholdRequiredManagerIdRule(string Message) : IScheduledPayBandThresholdRule
{
    public List<string> Check(ScheduledPayBandThreshold item)
    {
        if (item.ManagerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled pay band threshold.</summary>
public sealed record ScheduledPayBandThresholdValidator(List<IScheduledPayBandThresholdRule> Rules)
{
    public List<string> Validate(ScheduledPayBandThreshold item)
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

    public void RequireValid(ScheduledPayBandThreshold item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledPayBandThreshold> items)
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

/// <summary>Functions of scheduled pay band threshold rules.</summary>
public static class ScheduledPayBandThresholdRules
{
    public static ScheduledPayBandThresholdValidator DefaultScheduledPayBandThresholdValidator()
    {
        return new ScheduledPayBandThresholdValidator(
            new List<IScheduledPayBandThresholdRule>
            {
                new ScheduledPayBandThresholdCodeLengthRule(120),
                new ScheduledPayBandThresholdAllowedJobCodeRule(
                    new List<string> { "scheduled", "manual", "lenient" }),
                new ScheduledPayBandThresholdRequiredTitleRule("scheduled pay band threshold needs a value"),
                new ScheduledPayBandThresholdRequiredManagerIdRule("scheduled pay band threshold needs an entry"),
            });
    }
}

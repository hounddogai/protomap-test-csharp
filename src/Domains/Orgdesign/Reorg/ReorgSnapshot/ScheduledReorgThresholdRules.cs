namespace Acme.Workforce.Orgdesign.Reorg.ReorgSnapshot;

/// <summary>A rule a scheduled reorg threshold must pass. Check returns its problems, or none.</summary>
public interface IScheduledReorgThresholdRule
{
    List<string> Check(ScheduledReorgThreshold item);
}

public sealed record ScheduledReorgThresholdLabelLengthRule(int MaxLength) : IScheduledReorgThresholdRule
{
    public List<string> Check(ScheduledReorgThreshold item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledReorgThresholdCodeLengthRule(int MaxLength) : IScheduledReorgThresholdRule
{
    public List<string> Check(ScheduledReorgThreshold item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledReorgThresholdCodePrefixRule(string Prefix) : IScheduledReorgThresholdRule
{
    public List<string> Check(ScheduledReorgThreshold item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled reorg threshold.</summary>
public sealed record ScheduledReorgThresholdValidator(List<IScheduledReorgThresholdRule> Rules)
{
    public List<string> Validate(ScheduledReorgThreshold item)
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

    public void RequireValid(ScheduledReorgThreshold item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledReorgThreshold> items)
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

/// <summary>Functions of scheduled reorg threshold rules.</summary>
public static class ScheduledReorgThresholdRules
{
    public static ScheduledReorgThresholdValidator DefaultScheduledReorgThresholdValidator()
    {
        return new ScheduledReorgThresholdValidator(
            new List<IScheduledReorgThresholdRule>
            {
                new ScheduledReorgThresholdLabelLengthRule(3600),
                new ScheduledReorgThresholdCodeLengthRule(255),
                new ScheduledReorgThresholdCodePrefixRule("Q"),
            });
    }
}

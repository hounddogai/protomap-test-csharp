namespace Acme.Workforce.Contractors.Engagement.EngagementSummary;

/// <summary>A rule a engagement checkpoint must pass. Check returns its problems, or none.</summary>
public interface IEngagementCheckpointRule
{
    List<string> Check(EngagementCheckpoint item);
}

public sealed record EngagementCheckpointRequiredCodeRule(string Message) : IEngagementCheckpointRule
{
    public List<string> Check(EngagementCheckpoint item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record EngagementCheckpointCodePrefixRule(string Prefix) : IEngagementCheckpointRule
{
    public List<string> Check(EngagementCheckpoint item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record EngagementCheckpointRequiredJobCodeRule(string Message) : IEngagementCheckpointRule
{
    public List<string> Check(EngagementCheckpoint item)
    {
        if (item.JobCode.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record EngagementCheckpointPriorityRangeRule(int Min, int Max) : IEngagementCheckpointRule
{
    public List<string> Check(EngagementCheckpoint item)
    {
        var valueValue = item.Priority;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"priority {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record EngagementCheckpointManagerIdLengthRule(int MaxLength) : IEngagementCheckpointRule
{
    public List<string> Check(EngagementCheckpoint item)
    {
        var size = item.ManagerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"manager id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record EngagementCheckpointJobCodeLengthRule(int MaxLength) : IEngagementCheckpointRule
{
    public List<string> Check(EngagementCheckpoint item)
    {
        var size = item.JobCode.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"job code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a engagement checkpoint.</summary>
public sealed record EngagementCheckpointValidator(List<IEngagementCheckpointRule> Rules)
{
    public List<string> Validate(EngagementCheckpoint item)
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

    public void RequireValid(EngagementCheckpoint item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<EngagementCheckpoint> items)
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

/// <summary>Functions of engagement checkpoint rules.</summary>
public static class EngagementCheckpointRules
{
    public static EngagementCheckpointValidator DefaultEngagementCheckpointValidator()
    {
        return new EngagementCheckpointValidator(
            new List<IEngagementCheckpointRule>
            {
                new EngagementCheckpointRequiredCodeRule("engagement checkpoint needs this field"),
                new EngagementCheckpointCodePrefixRule("OPS"),
                new EngagementCheckpointRequiredJobCodeRule("engagement checkpoint needs a value"),
                new EngagementCheckpointPriorityRangeRule(32, 255),
                new EngagementCheckpointManagerIdLengthRule(80),
                new EngagementCheckpointJobCodeLengthRule(86400),
            });
    }
}

namespace Acme.Workforce.Workplace.Occupancy.Checkpoint;

/// <summary>A rule a occupancy checkpoint must pass. Check returns its problems, or none.</summary>
public interface IOccupancyCheckpointRule
{
    List<string> Check(OccupancyCheckpoint item);
}

public sealed record OccupancyCheckpointCodePrefixRule(string Prefix) : IOccupancyCheckpointRule
{
    public List<string> Check(OccupancyCheckpoint item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record OccupancyCheckpointCodeLengthRule(int MaxLength) : IOccupancyCheckpointRule
{
    public List<string> Check(OccupancyCheckpoint item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record OccupancyCheckpointRequiredLocaleRule(string Message) : IOccupancyCheckpointRule
{
    public List<string> Check(OccupancyCheckpoint item)
    {
        if (item.Locale.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record OccupancyCheckpointRequiredCodeRule(string Message) : IOccupancyCheckpointRule
{
    public List<string> Check(OccupancyCheckpoint item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a occupancy checkpoint.</summary>
public sealed record OccupancyCheckpointValidator(List<IOccupancyCheckpointRule> Rules)
{
    public List<string> Validate(OccupancyCheckpoint item)
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

    public void RequireValid(OccupancyCheckpoint item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<OccupancyCheckpoint> items)
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

/// <summary>Functions of occupancy checkpoint rules.</summary>
public static class OccupancyCheckpointRules
{
    public static OccupancyCheckpointValidator DefaultOccupancyCheckpointValidator()
    {
        return new OccupancyCheckpointValidator(
            new List<IOccupancyCheckpointRule>
            {
                new OccupancyCheckpointCodePrefixRule("ACM"),
                new OccupancyCheckpointCodeLengthRule(1000),
                new OccupancyCheckpointRequiredLocaleRule("occupancy checkpoint needs an entry"),
                new OccupancyCheckpointRequiredCodeRule("occupancy checkpoint needs an entry"),
            });
    }
}
